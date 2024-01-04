﻿using System.Text;
using DIOS.Core.HardwareIntercom;
using DIOS.Core.MainMeasurementScript;

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
  public SystemActivity SystemMonitor { get; } = new();
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
  public bool IsPlateEjected { get; internal set; }
  internal bool SingleSyringeMode { get; set; }
  internal readonly WellController _wellController = new();
  private readonly DataController _dataController;
  private readonly MeasurementScript _script;
  private readonly ILogger _logger;
  #if DEBUG
  public Action ContinueScript { get; }
  #endif

  public Device(ISerial connection, ILogger logger)
  {
    _logger = logger;
    var scriptTracker = new HardwareScriptTracker();
    #if DEBUG
    ContinueScript = scriptTracker.SignalScriptEnd;
    #endif
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
    _logger.Log($"Operation Mode: {Mode}");
    _dataController.BeadEventSink = beadEventSink;
    StringBuilder wellreport = new StringBuilder("[");
    foreach (var well in wells)
    {
      wellreport.Append($"{well.CoordinatesString()},");
    }
    wellreport.Replace(",", "]", wellreport.Length -1, 1);
    _logger.Log($"Number of wells to read: {wells.Count} {wellreport.ToString()}");
    _script.Start();
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