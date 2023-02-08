using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DIOS.Core.HardwareIntercom;
using DIOS.Core.MainMeasurementScript;
using DIOS.Core.SelfTests;

/*
 * Most commands on the host side parallel the Properties and Methods document fo QB-1000
 * The only complex action is reading a region of wells defined by the READ SECTION parameters and button
 * They define an rectangular area of the plate that may include the entire plate. The complexity is
 * furthered by the fact that a read can be terminated in many different ways:
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
 * 5. If regioncount is 1-- == 0 the last sample is read, if > 0 the next well is also aspirated
 * 
 * When the host initiates an end condition (isDone== true)
 * This version is being used at MRBM
 */

namespace DIOS.Core
{
  public class Device
  {
    public WellController WellController { get; } = new WellController();
    public SystemActivity SystemMonitor { get; } = new SystemActivity();
    /// <summary>
    /// Abstractions on the hardware commands and parameters
    /// </summary>
    public HardwareInterface Hardware { get; }
    public event EventHandler<ReadingWellEventArgs> StartingToReadWell;
    public event EventHandler<ReadingWellEventArgs> FinishedReadingWell;
    public event EventHandler FinishedMeasurement;
    public event EventHandler<ParameterUpdateEventArgs> ParameterUpdate;
    public OperationMode Mode { get; set; } = OperationMode.Normal;
    public int BoardVersion { get; internal set; }
    public string FirmwareVersion { get; internal set; }
    /// <summary>
    /// An inverse coefficient for the bead Reporter<br></br>
    /// Applied before Reporter Normalization
    /// </summary>
    public float ReporterScaling { get; set; } = 1;
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
    public HiSensitivityChannel SensitivityChannel
    {
      get
      {
        return _sensitivityChannel;
      }
      set
      {
        _sensitivityChannel = value;
        Hardware.SetParameter(DeviceParameterType.HiSensitivityChannel, (ushort)_sensitivityChannel);
      }
    }
    /// <summary>
    /// A coefficient for high dynamic range channel
    /// </summary>
    public float Compensation { get; set; }
    public NormalizationSettings Normalization
    {
      get { return _beadProcessor.Normalization; }
    }
    public float MaxPressure { get; set; }
    public bool IsPlateEjected { get; internal set; }
    public readonly Verificator Verificator;
    internal bool _singleSyringeMode;

    internal bool _isReadingA;
    internal float HdnrTrans;
    internal float HDnrCoef;
    internal readonly SelfTester SelfTester;
    internal readonly BeadProcessor _beadProcessor;
    private HiSensitivityChannel _sensitivityChannel;
    private readonly DataController _dataController;
    private readonly MeasurementScript _script;
    private readonly ILogger _logger;

    public Device(ISerial connection, ILogger logger)
    {
      SelfTester = new SelfTester(this, logger);
      _beadProcessor = new BeadProcessor(this);
      _dataController = new DataController(this, connection, logger);
      Hardware = new HardwareInterface(this, _dataController, logger);
      _script = new MeasurementScript(this, logger);
      _logger = logger;
      Verificator = new Verificator(_logger);
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
      WellController.PreparePrematureStop(_singleSyringeMode);
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
    /// The operation is conducted on the other thread, while this function quickly returns
    /// </summary>
    public void StartOperation(IBeadEventSink beadEventSink)
    {
      if (Mode != OperationMode.Normal)
      {
        Normalization.SuspendForTheRun();
      }
      _dataController.BeadEventSink = beadEventSink;
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

    internal void OnStartingToReadWell()
    {
      WellController.Advance();
      BeadCount = 0;
      _dataController.IsMeasurementGoing = true;
      StartingToReadWell?.Invoke(this, new ReadingWellEventArgs(WellController.CurrentWell));
      Hardware.SetParameter(DeviceParameterType.TotalBeadsInFirmware); //reset totalbeads in firmware
    }

    internal void OnFinishedReadingWell()
    {
      _logger.Log("Finished Reading Well");
      Hardware.SendCommand(DeviceCommandType.EndSampling);
      Hardware.RequestParameter(DeviceParameterType.TotalBeadsInFirmware);
      FinishedReadingWell?.Invoke(this, new ReadingWellEventArgs(WellController.CurrentWell));
    }

    internal void OnFinishedMeasurement()
    {
      _dataController.IsMeasurementGoing = false;
      _dataController.BeadEventSink = null;
      Hardware.SetParameter(DeviceParameterType.IsBubbleDetectionActive, 0);
      if (Mode ==  OperationMode.Verification)
        Verificator.CalculateResults(_beadProcessor._map);

      if (Mode != OperationMode.Normal)
        Normalization.Restore();

      FinishedMeasurement?.Invoke(this, EventArgs.Empty);
    }

    internal void OnParameterUpdate(ParameterUpdateEventArgs param)
    {
      ParameterUpdate?.Invoke(this, param);
    }

    public void SetMap(CustomMap map)
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

#if DEBUG
    public void DEBUGSetBoardVersion(int v)
    {
      BoardVersion = v;
    }

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
}