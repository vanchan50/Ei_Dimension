using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DIOS.Core.HardwareIntercom;
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
    public ResultsPublisher Publisher { get; }
    public RunResults Results { get; }
    public WorkOrder WorkOrder { get; set; }
    public ConcurrentQueue<ProcessedBead> DataOut { get; } = new ConcurrentQueue<ProcessedBead>();
    public WellController WellController { get; } = new WellController();
    public BitArray SystemActivity { get; } = new BitArray(16, false);
    public HardwareInterface Hardware { get; }
    public object SystemActivityNotBusyNotificationLock { get; } = new object();
    public object ScriptF9FinishedLock { get; } = new object();
    public event EventHandler<ReadingWellEventArgs> StartingToReadWell;
    public event EventHandler<ReadingWellEventArgs> FinishedReadingWell;
    public event EventHandler FinishedMeasurement;
    public event EventHandler<StatsEventArgs> NewStatsAvailable;
    public event EventHandler<ParameterUpdateEventArgs> ParameterUpdate;
    public OperationMode Mode { get; set; } = OperationMode.Normal;
    public SystemControl Control { get; set; } = SystemControl.Manual;
    public Gate ScatterGate
    {
      get
      {
        return _scatterGate;
      }
      set
      {
        _scatterGate = value;
        Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.ScatterGate, (ushort)_scatterGate);
      }
    }

    public Termination TerminationType { get; set; } = Termination.MinPerRegion;
    public int BoardVersion { get; internal set; }
    public string FirmwareVersion { get; internal set; }
    public float ReporterScaling { get; set; } = 1;
    public int BeadsToCapture { get; set; }
    public int BeadCount { get; internal set; }
    public int TotalBeads { get; internal set; }
    public int MinPerRegion { get; set; }
    public bool IsMeasurementGoing { get; private set; }
    public bool SaveIndividualBeadEvents { get; set; }
    public bool RMeans { get; set; }
    public bool OnlyClassifiedInBeadEventFile { get; set; }
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
    public float HdnrTrans
    {
      get
      {
        return _hdnrTrans;
      }
      set
      {
        _hdnrTrans = value;
        Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.DNRTransition, _hdnrTrans);
      }
    }
    public float HDnrCoef
    {
      get
      {
        return _hdnrCoef;
      }
      set
      {
        _hdnrCoef = value;
        Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.DNRCoefficient, _hdnrCoef);
      }
    }
    public float Compensation { get; set; }
    public NormalizationSettings Normalization
    {
      get { return _beadProcessor.Normalization; }
    }
    public float MaxPressure { get; set; }
    public bool IsPlateEjected { get; internal set; }
    internal bool _singleSyringeMode;
    public static bool IncludeReg0InPlateSummary { get; set; }  //TODO: crutch for filesaving

    internal bool _isReadingA;
    private Gate _scatterGate;
    private HiSensitivityChannel _sensitivityChannel;
    private float _hdnrTrans;
    private float _hdnrCoef;
    private readonly DataController _dataController;
    private readonly StateMachine _stateMach;
    internal SelfTester SelfTester { get; }
    internal readonly BeadProcessor _beadProcessor;

    public Device(ISerial connection, string folderPath)
    {
      SelfTester = new SelfTester(this);
      Results = new RunResults(this);
      _beadProcessor = new BeadProcessor(this);
      _dataController = new DataController(this, Results, connection);
      _stateMach = new StateMachine(this, true);
      Publisher = new ResultsPublisher(this, folderPath);
      Hardware = new HardwareInterface(this, _dataController);
    }

    public void Init()
    {
      if (_dataController.Run())
      {
        Hardware.SendCommand(DeviceCommandType.Synchronize);
        Hardware.RequestParameter(DeviceParameterType.BoardVersion);
      }
    }

    public void UpdateStateMachine()
    {
      _stateMach.Action();
    }

    public void PrematureStop()
    {
      WellController.PreparePrematureStop();
      _stateMach.Start();
    }

    /// <summary>
    /// Starts a sequence of commands to finalize well measurement. The sequence is in a form of state machine that takes several timer ticks
    /// </summary>
    public void StartStateMachine()
    {
      _stateMach.Start();
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
        Console.WriteLine("\nSelfTest Finished\n\n");
        #endif
        return data;
      });
      t.Start();
      return await t;
    }

    internal void SetupRead()
    {
      SetReadingParamsForWell();
      if (_singleSyringeMode)
      {
        SetAspirateParamsForWell();
      }
      WellController.Advance();
      Results.StartNewWell(WellController.CurrentWell);
      Publisher.StartNewBeadEventReport();
      BeadCount = 0;

      OnStartingToReadWell();
      StartBeadRead();
    }

    public void StartOperation()
    {
      if (Mode != OperationMode.Normal)
      {
        Normalization.SuspendForTheRun();
      }
      Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.MinSSC, _beadProcessor._map.calParams.minmapssc);
      Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.MaxSSC, _beadProcessor._map.calParams.maxmapssc);
      //read section of plate
      Hardware.RequestParameter(DeviceParameterType.MotorStepsX, MotorStepsX.Plate96Column1);
      Hardware.RequestParameter(DeviceParameterType.MotorStepsY, MotorStepsY.Plate96RowA);
      Results.StartNewPlateReport();
      Publisher.ResetResultData();
      SetAspirateParamsForWell();  //setup for first read
      SetReadingParamsForWell();
      WellController.Advance();
      Results.StartNewWell(WellController.CurrentWell);
      Publisher.StartNewBeadEventReport();
      BeadCount = 0;
      OnStartingToReadWell();
      Hardware.SetParameter(DeviceParameterType.IsBubbleDetectionActive, 1);
      Hardware.SendCommand(DeviceCommandType.PositionWellPlate);

      if(!_singleSyringeMode)
        Hardware.SendCommand(DeviceCommandType.AspirateA);

      TotalBeads = 0;

      _isReadingA = false;
      StartBeadRead();

      if (TerminationType != Termination.TotalBeadsCaptured) //set some limit for running to eos or if regions are wrong
        BeadsToCapture = 100000;
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

    private void SetReadingParamsForWell()
    {
      Hardware.SetParameter(DeviceParameterType.WellReadingSpeed,     (ushort)WellController.NextWell.RunSpeed);
      Hardware.SetParameter(DeviceParameterType.ChannelConfiguration, (ushort)WellController.NextWell.ChanConfig);
      BeadsToCapture = WellController.NextWell.BeadsToCapture;
      MinPerRegion = WellController.NextWell.MinPerRegion;
      TerminationType = WellController.NextWell.TermType;
    }

    private void SetAspirateParamsForWell()
    {
      Hardware.SetParameter(DeviceParameterType.WellRowIndex,    WellController.NextWell.RowIdx);
      Hardware.SetParameter(DeviceParameterType.WellColumnIndex, WellController.NextWell.ColIdx);
      Hardware.SetParameter(DeviceParameterType.Volume, VolumeType.Sample,  (ushort)WellController.NextWell.SampVol);
      Hardware.SetParameter(DeviceParameterType.Volume, VolumeType.Wash,    (ushort)WellController.NextWell.WashVol);
      Hardware.SetParameter(DeviceParameterType.Volume, VolumeType.Agitate, (ushort)WellController.NextWell.AgitateVol);
    }

    internal bool EndBeadRead()
    {
      if (_singleSyringeMode)
      {
        Hardware.SendCommand(DeviceCommandType.EndReadA);
        if(!WellController.IsLastWell)
          Hardware.SendCommand(DeviceCommandType.PositionWellPlate);
        return WellController.IsLastWell;
      }

      if (_isReadingA)
        Hardware.SendCommand(DeviceCommandType.EndReadA);
      else
        Hardware.SendCommand(DeviceCommandType.EndReadB);
      return WellController.IsLastWell;
    }

    private void StartBeadRead()
    {
      if (_singleSyringeMode)
      {
        Hardware.SendCommand(DeviceCommandType.AspirateA);
        Hardware.SendCommand(DeviceCommandType.ReadA);
        return;
      }

      if (WellController.IsLastWell)
      {
        if (_isReadingA)
          Hardware.SendCommand(DeviceCommandType.ReadB);
        else
          Hardware.SendCommand(DeviceCommandType.ReadA);
        return;
      }

      SetAspirateParamsForWell();
      if (_isReadingA)
        Hardware.SendCommand(DeviceCommandType.ReadBAspirateA);
      else
        Hardware.SendCommand(DeviceCommandType.ReadAAspirateB);
    }
    private void OnStartingToReadWell()
    {
      IsMeasurementGoing = true;
      StartingToReadWell?.Invoke(this, new ReadingWellEventArgs(WellController.CurrentWell.RowIdx, WellController.CurrentWell.ColIdx,
        Publisher.FullBeadEventFileName));
      Hardware.SetParameter(DeviceParameterType.TotalBeadsInFirmware); //reset totalbeads in firmware
      Console.WriteLine("Starting to read well with Params:");
      Console.WriteLine($"Termination: {(int)TerminationType}");
      Console.WriteLine($"TotalBeads: {TotalBeads}");
      Console.WriteLine($"BeadCount: {BeadCount}");
      Console.WriteLine($"BeadsToCapture: {BeadsToCapture}");
    }

    internal void OnFinishedReadingWell()
    {
      Hardware.SendCommand(DeviceCommandType.EndSampling);
      FinishedReadingWell?.Invoke(this, new ReadingWellEventArgs(WellController.CurrentWell.RowIdx, WellController.CurrentWell.ColIdx,
        Publisher.FullBeadEventFileName));
      Hardware.RequestParameter(DeviceParameterType.TotalBeadsInFirmware);
    }

    internal void OnFinishedMeasurement()
    {
      IsMeasurementGoing = false;
      BeadCount = 0;
      Results.PlateReport.completedDateTime = DateTime.Now;
      _ = Task.Run(() => { Publisher.OutputPlateReport(); });
      Results.EndOfOperationReset();
      Hardware.SetParameter(DeviceParameterType.IsBubbleDetectionActive, 0);
      if (Mode ==  OperationMode.Verification)
        Verificator.CalculateResults(_beadProcessor._map);

      if (Mode != OperationMode.Normal)
        Normalization.Restore();

      FinishedMeasurement?.Invoke(this, EventArgs.Empty);
    }

    internal void OnNewStatsAvailable()
    {
      var stats = Results.WellResults.GetStats();
      var averageBackgrounds = Results.WellResults.GetBackgroundAverages();
      NewStatsAvailable?.Invoke(this, new StatsEventArgs(stats, averageBackgrounds));
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

    public void DEBUGJBeadADD()
    {
      var r = new Random();
      var kek = new RawBead
      {
        Header = 0xadbeadbe,
        fsc = 2.36f,
        redssc = r.Next(1000,20000),
        cl0 = r.Next(1050, 1300),
        cl1 = r.Next(1450, 1700),
        cl2 = r.Next(1500, 1650),
        greenB = (ushort)r.Next(9, 12),
        greenC = 48950
      };
      var pek = new RawBead
      {
        Header = 0xadbeadbe,
        fsc = 15.82f,
        redssc = r.Next(1000, 20000),
        cl0 = 250f,
        cl1 = 500f,
        cl2 = 500f,
        greenB = (ushort)r.Next(80,150),
        greenC = 65212
      };
      var rek0 = new RawBead
      {
        Header = 0xadbeadbe,
        fsc = 2.36f,
        redssc = r.Next(1000, 20000),
        cl0 = r.Next(1050, 1300),
        cl1 = 35000,
        cl2 = 200,
        greenB = (ushort)r.Next(9, 12),
        greenC = 48950
      };
      var choose = r.Next(0, 3);
      switch (choose)
      {
        case 0:
          Results.AddRawBeadEvent(in kek);
          break;
        case 1:
          Results.AddRawBeadEvent(in pek);
          break;
        case 2:
          Results.AddRawBeadEvent(in rek0);
          break;
      }
      Results.TerminationReadyCheck();
    }

    public void DEBUGCommandTest(CommandStruct cs)
    {
      _dataController.DEBUGGetCommandFromBuffer(cs);
    }
    #endif
  }
}