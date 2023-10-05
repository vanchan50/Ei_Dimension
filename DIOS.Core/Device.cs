using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DIOS.Core.HardwareIntercom;
using DIOS.Core.MainMeasurementScript;
using DIOS.Core.SelfTests;

/* a read can be terminated in many different ways:
 * 1. Manually with the END SECTION READ button
 * 2. Manually with the END READ button which just ends the current well read and goes on the the next well
 * 3. OUT OF SHEATH condition, the sheath syringe is a position 0
 * 4. OUT OF SAMPLE condition, the sample syringe (either A or B) is at position 0
 * 5. Required number of beads read
 * 6. Required number of beads read in each region.
 * 7. Instrument fault (bubbles, plunger overload, clog, low laser power, etc)
 * 
 * When the instrument detects one of these end conditions: QB_cmd_proc.c / SyringeEmpty()
 * 1. The command Queue is cleared (the queue is only holding instructions for the currently read well)
 * 2. The sync token is cleared allowing new commands to execute immediately
 * 3. An FD or FE is sent to the host to tell it to save the data file and then it sends an EE or EF
 * 4. An EE or EF sequence is executed in the instrument, flushing the remaining sample and resetting syringes
 * 
 */

namespace DIOS.Core;

/// <summary>
/// Main class that provides access to all of the system functions
/// </summary>
public class Device
{
  public SystemActivity SystemMonitor { get; } = new SystemActivity();
  /// <summary>
  /// Abstractions on the hardware commands and parameters
  /// </summary>
  public HardwareInterface Hardware { get; }
  /// <summary>
  /// Fired on reading start of every Well/Tube
  /// </summary>
  public event EventHandler<ReadingWellEventArgs> StartingToReadWell;
  /// <summary>
  /// Fired on reading end of every Well/Tube
  /// </summary>
  public event EventHandler<ReadingWellEventArgs> FinishedReadingWell;
  /// <summary>
  /// Fired when every Well/Tube are read.
  /// </summary>
  public event EventHandler FinishedMeasurement;
  /// <summary>
  /// Fired on every request/change of the Hardware parameter
  /// </summary>
  public event EventHandler<ParameterUpdateEventArgs> ParameterUpdate;
  public OperationMode Mode { get; set; } = OperationMode.Normal; //TODO: move to app layer?
  public int BoardVersion { get; internal set; }
  public string FirmwareVersion { get; internal set; }

  /// <summary>
  /// An inverse coefficient for the bead Reporter<br></br>
  /// Applied before Reporter Normalization
  /// </summary>
  public float ReporterScaling
  {
    get
    {
      return _reporterScaling;
    }
    set
    {
      _reporterScaling = value;
      _beadProcessor._inverseReporterScaling = 1 / value;
    }
  }
  /// <summary>
  /// Amount of bead events for the current well<br></br>
  /// Resets on the start of new Well
  /// </summary>
  public int BeadCount { get; internal set; }
  /// <summary>
  /// The flag is set when the Device is in the measurement process;<br></br>
  /// Reset when the plate measurement is complete
  /// </summary>
  public bool IsMeasurementGoing
  {
    get
    {
      return _dataController.IsMeasurementGoing;
    }
  }
  /// <summary>
  /// A coefficient for high dynamic range channel
  /// </summary>
  public float Compensation
  {
    get
    {
      return _compensation;
    }
    set
    {
      _compensation = value;
      _beadProcessor._actualCompensation = value / 100f;
    }
  }
  public NormalizationSettings Normalization
  {
    get { return _beadProcessor.Normalization; }
  }
  /// <summary>
  /// Max allowed pressure of sheath in a flow cell
  /// </summary>
  public float MaxPressure { get; set; }
  public bool IsPlateEjected { get; internal set; }
  public float ExtendedRangeCL1Threshold
  {
    get
    {
      return _beadProcessor._extendedRangeCL1Threshold;
    }
    set
    {
      _beadProcessor._extendedRangeCL1Threshold = value;
    }
  } //Put it into hardwareInterface just for consistency? no matter there is no actual device parameter
  public float ExtendedRangeCL2Threshold
  {
    get
    {
      return _beadProcessor._extendedRangeCL2Threshold;
    }
    set
    {
      _beadProcessor._extendedRangeCL2Threshold = value;
    }
  }
  public float ExtendedRangeCL1Multiplier
  {
    get
    {
      return _beadProcessor._extendedRangeCL1Multiplier;
    }
    set
    {
      _beadProcessor._extendedRangeCL1Multiplier = value;
    }
  }
  public float ExtendedRangeCL2Multiplier
  {
    get
    {
      return _beadProcessor._extendedRangeCL2Multiplier;
    }
    set
    {
      _beadProcessor._extendedRangeCL2Multiplier = value;
    }
  }
  internal bool SingleSyringeMode { get; set; }
  internal float HdnrTrans;
  internal float HDnrCoef;
  internal readonly SelfTester SelfTester;
  internal readonly BeadProcessor _beadProcessor;
  internal readonly WellController _wellController = new();
  private readonly DataController _dataController;
  private readonly MeasurementScript _script;
  private readonly ILogger _logger;
  private float _reporterScaling = 1;
  private float _compensation = 1;

  public Device(ISerial connection, ILogger logger)
  {
    _logger = logger;
    SelfTester = new SelfTester(this, logger);
    _beadProcessor = new BeadProcessor(this);
    var scriptTracker = new HardwareScriptTracker();
    _dataController = new DataController(this, connection, scriptTracker, logger);
    Hardware = new HardwareInterface(this, _dataController, scriptTracker, logger);
    _script = new MeasurementScript(this, logger);
  }

  public void Init()
  {
    if (_dataController.Run())
    {
      Hardware.SendCommand(DeviceCommandType.Synchronize);
      Hardware.RequestParameter(DeviceParameterType.BoardVersion);
    }
  }

  public void PrematureStop()
  {
    _wellController.PreparePrematureStop(SingleSyringeMode);
    _script.FinalizeWellReading();
  }

  /// <summary>
  /// Starts a sequence of commands to finalize well measurement
  /// <br>The sequence is in a form of state machine and is NOT instant</br>
  /// </summary>
  public void StopOperation()
  {
    _script.FinalizeWellReading();
  }

  /// <summary>
  /// Sends a sequence of commands to startup a measurement.
  /// <br>The operation is conducted on the other thread, while this function quickly returns</br>
  /// </summary>
  public void StartOperation(IReadOnlyCollection<Well> wells, IBeadEventSink beadEventSink)
  {
    if (wells.Count == 0)
    {
      throw new ArgumentException("Wells count = 0");
    }
    _wellController.Init(wells);
    if (Mode != OperationMode.Normal)
    {
      Normalization.SuspendForTheRun();
      _logger.Log("Normalization: Suspended;");
    }
    else
    {
      if (Normalization.IsEnabled)
        _logger.Log("Normalization: Enabled");
      else
        _logger.Log("Normalization: Disabled");
    }
    _logger.Log($"Operation Mode: {Mode}");
    _dataController.BeadEventSink = beadEventSink;
    _logger.Log("Extended Range calculation: " + (_beadProcessor._extendedRangeEnabled ? "enabled" : "disabled"));
    StringBuilder wellreport = new StringBuilder("[");
    foreach (var well in wells)
    {
      wellreport.Append($"{well.CoordinatesString()},");
    }
    wellreport.Replace(",", "]", wellreport.Length -1, 1);
    _logger.Log($"Number of wells to read: {wells.Count} {wellreport.ToString()}");
    _script.Start();
  }

  public void StartSelfTest()
  {
    SelfTester.FluidicsTest();
  }

  /// <summary>
  /// Polls the device once per 100 ms until the test result is ready
  /// </summary>
  /// <returns></returns>
  public async Task<SelfTestData> GetSelfTestResultAsync()
  {
    var t = new Task<SelfTestData>(() =>
    {
      SelfTestData data;
      while (!SelfTester.GetResult(out data))
      {
        System.Threading.Thread.Sleep(100);
      }
#if DEBUG
      _logger.Log("\nSelfTest Finished\n\n");
#endif
      return data;
    });
    t.Start();
    return await t;
  }

  public void EjectPlate()
  {
    Hardware.SendCommand(DeviceCommandType.EjectPlate);
    IsPlateEjected = true;
  }

  public void LoadPlate()
  {
    Hardware.SendCommand(DeviceCommandType.LoadPlate);
    IsPlateEjected = false;
  }

  public void SetMap(MapModel map)
  {
    _beadProcessor.SetMap(map);
  }

  public void ReconnectUSB()
  {
    _dataController.ReconnectUSB();
    Hardware.SetParameter(DeviceParameterType.SystemActivityStatus);
    Hardware.SetToken(HardwareToken.Synchronization);
  }

  public void DisconnectedUSB()
  {
    _dataController.DisconnectedUSB();
  }

  internal void OnStartingToReadWell()
  {
    _wellController.Advance();
    BeadCount = 0;
    _dataController.IsMeasurementGoing = true;
    StartingToReadWell?.Invoke(this, new ReadingWellEventArgs(_wellController.CurrentWell));
    Hardware.SetParameter(DeviceParameterType.TotalBeadsInFirmware); //reset totalbeads in firmware
  }

  internal void OnFinishedReadingWell()
  {
    Hardware.SendCommand(DeviceCommandType.EndSampling);
    Hardware.RequestParameter(DeviceParameterType.TotalBeadsInFirmware);
    FinishedReadingWell?.Invoke(this, new ReadingWellEventArgs(_wellController.CurrentWell));
  }

  internal void OnFinishedMeasurement()
  {
    _dataController.IsMeasurementGoing = false;
    _dataController.BeadEventSink = null;
    Hardware.SetParameter(DeviceParameterType.IsBubbleDetectionActive, 0);

    if (Mode != OperationMode.Normal)
      Normalization.Restore();

    FinishedMeasurement?.Invoke(this, EventArgs.Empty);
  }

  internal void OnParameterUpdate(ParameterUpdateEventArgs param)
  {
    ParameterUpdate?.Invoke(this, param);
  }

#if DEBUG

  public void DEBUGOnParameterUpdate(DeviceParameterType type, int intparam = -1, float floatparam = -1F)
  {
    ParameterUpdate?.Invoke(this, new ParameterUpdateEventArgs(type, intparam, floatparam));
  }

  public void DEBUGCommandTest(CommandStruct cs)
  {
    _dataController.DEBUGGetCommandFromBuffer(cs);
  }
#endif
}