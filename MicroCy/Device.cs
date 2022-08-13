using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DIOS.Core.InstrumentParameters;
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
    public MapController MapCtroller { get; }
    public RunResults Results { get; }
    public WorkOrder WorkOrder { get; set; }
    public ConcurrentQueue<CommandStruct> Commands { get; } = new ConcurrentQueue<CommandStruct>();
    public ConcurrentQueue<ProcessedBead> DataOut { get; } = new ConcurrentQueue<ProcessedBead>();
    public WellController WellController { get; } = new WellController();
    public BitArray SystemActivity { get; } = new BitArray(16, false);
    public object SystemActivityNotBusyNotificationLock { get; } = new object();
    public event EventHandler<ReadingWellEventArgs> StartingToReadWell;
    public event EventHandler<ReadingWellEventArgs> FinishedReadingWell;
    public event EventHandler FinishedMeasurement;
    public event EventHandler<StatsEventArgs> NewStatsAvailable;
    public event EventHandler<int> BeadConcentrationStatusUpdate;
    public OperationMode Mode { get; set; } = OperationMode.Normal;
    public SystemControl Control { get; set; }
    public Gate ScatterGate
    {
      get
      {
        return _scatterGate;
      }
      set
      {
        _scatterGate = value;
        MainCommand("Set Property", code: 0xCA, parameter: (ushort)_scatterGate);
      }
    }
    public Termination TerminationType { get; set; }
    public int BoardVersion { get; internal set; }
    public string FirmwareVersion { get; internal set; }
    public float ReporterScaling { get; set; }
    public int BeadsToCapture { get; set; }
    public int BeadCount { get; internal set; }
    public int TotalBeads { get; internal set; }
    public int MinPerRegion { get; set; }
    public bool IsMeasurementGoing { get; private set; }
    public bool SaveIndividualBeadEvents { get; set; }
    public bool RMeans { get; set; }
    public bool OnlyClassified { get; set; }
    public HiSensitivityChannel SensitivityChannel
    {
      get
      {
        return _sensitivityChannel;
      }
      set
      {
        _sensitivityChannel = value;
        MainCommand("Set Property", code: 0x1E, parameter: (ushort)_sensitivityChannel);
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
        MainCommand("Set FProperty", code: 0x0A, fparameter: _hdnrTrans);
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
        MainCommand("Set FProperty", code: 0x20, fparameter: _hdnrCoef);
      }
    }
    public float Compensation { get; set; }
    public NormalizationSettings Normalization
    {
      get { return _beadProcessor.Normalization; }
    }
    public static DirectoryInfo RootDirectory { get; private set; }
    public float MaxPressure { get; set; }
    public bool IsPlateEjected { get; internal set; }

    private bool _isReadingA;
    private Gate _scatterGate;
    private HiSensitivityChannel _sensitivityChannel;
    private float _hdnrTrans;
    private float _hdnrCoef;
    private readonly DataController _dataController;
    private readonly StateMachine _stateMach;
    internal SelfTester SelfTester { get; }
    internal readonly BeadProcessor _beadProcessor;

    public Device(ISerial connection)
    {
      SetSystemDirectories();
      Results = new RunResults(this);
      _beadProcessor = new BeadProcessor(this);
      _dataController = new DataController(this, Results, connection);
      _stateMach = new StateMachine(this, true);
      Publisher = new ResultsPublisher(this);
      MapCtroller = new MapController();
      MapCtroller.ChangedActiveMap += MapChangedEventHandler;
      SelfTester = new SelfTester(this);
      MainCommand("Sync");
      TotalBeads = 0;
      IsMeasurementGoing = false;
      ReporterScaling = 1;
      MainCommand("Get Property", code: 0x01);  //get board version
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
      MainCommand("Set Property", code: 0xce, parameter: MapCtroller.ActiveMap.calParams.minmapssc);  //set ssc gates for this map
      MainCommand("Set Property", code: 0xcf, parameter: MapCtroller.ActiveMap.calParams.maxmapssc);
      //read section of plate
      MainCommand("Get FProperty", code: 0x58);
      MainCommand("Get FProperty", code: 0x68);
      Results.StartNewPlateReport();
      Publisher.ResetResultData();
      SetAspirateParamsForWell();  //setup for first read
      SetReadingParamsForWell();
      WellController.Advance();
      Results.StartNewWell(WellController.CurrentWell);
      Publisher.StartNewBeadEventReport();
      BeadCount = 0;
      OnStartingToReadWell();
      MainCommand("Set Property", code: 0x19, parameter: 1); //bubble detect on
      MainCommand("Position Well Plate"); //move motors. next position is set in properties 0xad and 0xae
      MainCommand("Aspirate Syringe A"); //handles down and pickup sample
      TotalBeads = 0;

      _isReadingA = false;
      StartBeadRead();

      if (TerminationType != Termination.TotalBeadsCaptured) //set some limit for running to eos or if regions are wrong
        BeadsToCapture = 100000;
    }

    public void EjectPlate()
    {
      MainCommand("Eject Plate");
      IsPlateEjected = true;
    }

    public void LoadPlate()
    {
      MainCommand("Load Plate");
      IsPlateEjected = false;
    }

    private void SetReadingParamsForWell()
    {
      MainCommand("Set Property", code: 0xaa, parameter: (ushort)WellController.NextWell.RunSpeed);
      MainCommand("Set Property", code: 0xc2, parameter: (ushort)WellController.NextWell.ChanConfig);
      BeadsToCapture = WellController.NextWell.BeadsToCapture;
      MinPerRegion = WellController.NextWell.MinPerRegion;
      TerminationType = WellController.NextWell.TermType;
    }

    private void SetAspirateParamsForWell()
    {
      MainCommand("Set Property", code: 0xad, parameter: (ushort)WellController.NextWell.RowIdx);
      MainCommand("Set Property", code: 0xae, parameter: (ushort)WellController.NextWell.ColIdx);
      MainCommand("Set Property", code: 0xaf, parameter: (ushort)WellController.NextWell.SampVol);
      MainCommand("Set Property", code: 0xac, parameter: (ushort)WellController.NextWell.WashVol);
      MainCommand("Set Property", code: 0xc4, parameter: (ushort)WellController.NextWell.AgitateVol);
    }

    internal bool EndBeadRead()
    {
      if (_isReadingA)
        MainCommand("End Bead Read A");
      else
        MainCommand("End Bead Read B");
      return WellController.IsLastWell;
    }

    private void StartBeadRead()
    {
      if (WellController.IsLastWell)
      {
        if (_isReadingA)
          MainCommand("Read B");
        else
          MainCommand("Read A");
      }
      else
      {
        SetAspirateParamsForWell();
        if (_isReadingA)
          MainCommand("Read B Aspirate A");
        else
          MainCommand("Read A Aspirate B");
      }
    }

    public void MainCommand(string command, byte? cmd = null, byte? code = null, ushort? parameter = null, float? fparameter = null)
    {
      CommandStruct cs = CommandLists.MainCmdTemplatesDict[command];
      cs.Command = cmd ?? cs.Command;
      cs.Code = code ?? cs.Code;
      cs.Parameter = parameter ?? cs.Parameter;
      cs.FParameter = fparameter ?? cs.FParameter;
      switch (command)
      {
        case "Read A":
          _isReadingA = true;
          break;
        case "Read A Aspirate B":
          _isReadingA = true;
          break;
        case "Read B":
          _isReadingA = false;
          break;
        case "Read B Aspirate A":
          _isReadingA = false;
          break;
        case "FlushCmdQueue":
          cs.Command = 0x02;
          break;
        case "Idex":
          cs.Command = Idex.Pos;
          cs.Parameter = Idex.Steps;
          cs.FParameter = Idex.Dir;
          break;
      }
      _dataController.AddCommand(command, cs);
    }

    private void SetSystemDirectories()
    {
      RootDirectory = new DirectoryInfo(Path.Combine(@"C:\Emissioninc", Environment.MachineName));
      List<string> subDirectories = new List<string>(8) { "Config", "WorkOrder", "SavedImages", "Archive", "Result", "Status", "AcquisitionData", "SystemLogs" };
      try
      {
        foreach (var d in subDirectories)
        {
          RootDirectory.CreateSubdirectory(d);
        }
        Directory.CreateDirectory(RootDirectory.FullName + @"\Result" + @"\Summary");
        Directory.CreateDirectory(RootDirectory.FullName + @"\Result" + @"\Detail");
      }
      catch
      {
        Console.WriteLine("Directory Creation Failed");
      }
    }

    private void OnStartingToReadWell()
    {
      IsMeasurementGoing = true;
      StartingToReadWell?.Invoke(this, new ReadingWellEventArgs(WellController.CurrentWell.RowIdx, WellController.CurrentWell.ColIdx,
        Publisher.FullBeadEventFileName));
      MainCommand("Set FProperty", code: 0x06);  //reset totalbeads in firmware
    }

    internal void OnFinishedReadingWell()
    {
      MainCommand("End Sampling");    //sends message to instrument to stop sampling
      FinishedReadingWell?.Invoke(this, new ReadingWellEventArgs(WellController.CurrentWell.RowIdx, WellController.CurrentWell.ColIdx,
        Publisher.FullBeadEventFileName));
      MainCommand("Get FProperty", code: 0x06);  //get totalbeads from firmware
    }

    internal void OnFinishedMeasurement()
    {
      IsMeasurementGoing = false;
      Results.PlateReport.completedDateTime = DateTime.Now;
      _ = Task.Run(() => { Publisher.OutputPlateReport(); });
      Results.EndOfOperationReset();
      MainCommand("Set Property", code: 0x19);  //bubble detect off
      if (Mode ==  OperationMode.Verification)
        Verificator.CalculateResults(MapCtroller);

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

    internal void MapChangedEventHandler(object sender, CustomMap map)
    {
      _beadProcessor.SetMap(map);
    }

    internal void OnBeadConcentrationStatusUpdate(int value)
    {
      BeadConcentrationStatusUpdate?.Invoke(this, value);
    }

    #if DEBUG
    public void SetBoardVersion(int v)
    {
      BoardVersion = v;
    }

    public void JKBeadADD(int p)
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
        if (p == 1)
        {
          Results.AddRawBeadEvent(in kek);
        }
        if (p == 2)
        {
          Results.AddRawBeadEvent(in pek);
        }
    }
    #endif
  }
}