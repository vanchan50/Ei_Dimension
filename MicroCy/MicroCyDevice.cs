using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Collections.Concurrent;
using MicroCy.InstrumentParameters;

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

namespace MicroCy
{

  public class MicroCyDevice
  {
    public ResultsPublisher Publisher { get; }
    public MapController MapCtroller { get; }
    public WorkOrder WorkOrder { get; set; }
    public ConcurrentQueue<CommandStruct> Commands { get; } = new ConcurrentQueue<CommandStruct>();
    public ConcurrentQueue<BeadInfoStruct> DataOut { get; } = new ConcurrentQueue<BeadInfoStruct>();
    public WellController WellController { get; } = new WellController();
    public BitArray SystemActivity { get; } = new BitArray(16, false);
    public List<WellResult> WellResults { get; } = new List<WellResult>();
    public event EventHandler<ReadingWellEventArgs> StartingToReadWell;
    public event EventHandler<ReadingWellEventArgs> FinishedReadingWell;
    public event EventHandler FinishedMeasurement;
    public event EventHandler<StatsEventArgs> NewStatsAvailable;
    public OperationMode Mode { get; set; }
    public SystemControl Control { get; set; }
    public Gate ScatterGate { get; set; }
    public Termination TerminationType { get; set; }
    public int BoardVersion { get; internal set; }
    public float ReporterScaling { get; set; }
    public int BeadsToCapture { get; set; }
    public int BeadCount { get; internal set; }
    public int TotalBeads { get; internal set; }
    public int MinPerRegion { get; set; }
    public bool IsMeasurementGoing { get; private set; }
    public bool Everyevent { get; set; }
    public bool RMeans { get; set; }
    public bool PlateReportActive { get; set; }
    public bool OnlyClassified { get; set; }
    public bool Reg0stats { get; set; }
    public bool ChannelBIsHiSensitivity { get; set; }
    public DirectoryInfo RootDirectory { get; private set; }

    private bool _chkRegionCount;
    private bool _readingA;
    private readonly DataController _dataController;
    private readonly StateMachine _stateMach;
    internal readonly BeadProcessor _beadProcessor;
    private ICollection<int> _regionsToOutput;

    public MicroCyDevice(Type connectionType = null)
    {
      SetSystemDirectories();
      _dataController = new DataController(this, connectionType);
      _stateMach = new StateMachine(this, true);
      Publisher = new ResultsPublisher(this);
      MapCtroller = new MapController(this);
      _beadProcessor = new BeadProcessor(this);
      MainCommand("Sync");
      TotalBeads = 0;
      Mode = OperationMode.Normal;
      MapCtroller.MoveMaps();
      MapCtroller.LoadMaps();
      Reg0stats = false;
      IsMeasurementGoing = false;
      ReporterScaling = 1;
    }

    public void InitBeadRead()
    {
      Publisher.GetNewFileName();
      ResultsPublisher.StartNewWellReport();
      _chkRegionCount = false;
      BeadCount = 0;
      OnStartingToReadWell();
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

    internal void SetupRead()
    {
      SetReadingParamsForWell();
      WellController.Advance();

      if (WellController.IsLastWell)
      {
        if (_readingA)
          MainCommand("Read B");
        else
          MainCommand("Read A");
      }
      else
      {
        SetAspirateParamsForWell();
        if (_readingA)
          MainCommand("Read B Aspirate A");
        else
          MainCommand("Read A Aspirate B");
      }
    }

    public void StartOperation(ICollection<int> regionsToOutput = null)
    {
      MainCommand("Set Property", code: 0xce, parameter: MapCtroller.ActiveMap.calParams.minmapssc);  //set ssc gates for this map
      MainCommand("Set Property", code: 0xcf, parameter: MapCtroller.ActiveMap.calParams.maxmapssc);
      _beadProcessor.ConstructClassificationMap(MapCtroller.ActiveMap);
      //read section of plate
      MainCommand("Get FProperty", code: 0x58);
      MainCommand("Get FProperty", code: 0x68);
      ResultsPublisher.StartNewPlateReport();
      MainCommand("Get FProperty", code: 0x20); //get high dnr property
      SetAspirateParamsForWell();  //setup for first read
      _regionsToOutput = regionsToOutput;
      SetReadingParamsForWell();
      MainCommand("Set Property", code: 0x19, parameter: 1); //bubble detect on
      MainCommand("Position Well Plate"); //move motors. next position is set in properties 0xad and 0xae
      MainCommand("Aspirate Syringe A"); //handles down and pickup sample
      WellController.Advance();
      InitBeadRead(); //gets output file ready
      ResultsPublisher.ClearSummary();
      TotalBeads = 0;

      if (WellController.IsLastWell)
        MainCommand("Read A");
      else
      {
        SetAspirateParamsForWell();
        MainCommand("Read A Aspirate B");
      }

      if (TerminationType != Termination.TotalBeadsCaptured) //set some limit for running to eos or if regions are wrong
        BeadsToCapture = 100000;
    }

    private void SetReadingParamsForWell()
    {
      MainCommand("Set Property", code: 0xaa, parameter: (ushort)WellController.NextWell.runSpeed);
      MainCommand("Set Property", code: 0xc2, parameter: (ushort)WellController.NextWell.chanConfig);
      MakeNewWellResults();
    }

    private void SetAspirateParamsForWell()
    {
      MainCommand("Set Property", code: 0xad, parameter: (ushort)WellController.NextWell.rowIdx);
      MainCommand("Set Property", code: 0xae, parameter: (ushort)WellController.NextWell.colIdx);
      MainCommand("Set Property", code: 0xaf, parameter: (ushort)WellController.NextWell.sampVol);
      MainCommand("Set Property", code: 0xac, parameter: (ushort)WellController.NextWell.washVol);
      MainCommand("Set Property", code: 0xc4, parameter: (ushort)WellController.NextWell.agitateVol);
    }

    internal bool EndBeadRead()
    {
      if (_readingA)
        MainCommand("End Bead Read A");
      else
        MainCommand("End Bead Read B");
      return WellController.IsLastWell;
    }

    private void MakeNewWellResults()
    {
      WellResults.Clear();
      if (_regionsToOutput != null && _regionsToOutput.Count != 0)
      {
        foreach (var region in MapCtroller.ActiveMap.regions)
        {
          if (_regionsToOutput.Contains(region.Number))
            WellResults.Add(new WellResult { regionNumber = (ushort)region.Number });
        }
      }
      if (Reg0stats)
        WellResults.Add(new WellResult { regionNumber = 0 });
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
          _readingA = true;
          break;
        case "Read A Aspirate B":
          _readingA = true;
          break;
        case "Read B":
          _readingA = false;
          break;
        case "Read B Aspirate A":
          _readingA = false;
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
      DataController.OutCommands.Enqueue((command, cs));
      #if DEBUG
      Console.Error.WriteLine($"{DateTime.Now.ToString()} Enqueued [{command}]: {cs.ToString()}");
      #endif
      _dataController.NotifyCommandReceived();

      //RunCmd(command, cs);
    }

    public void InitSTab(string tabname)
    {
      //Removing this can lead to unforseen crucial bugs in instrument operation. If so - do with extra care
      //one example is a check in CommandLists.Readertab for changed plate parameter,which could happen in manual well selection in motors tab
      List<byte> list;
      switch (tabname)
      {
        case "readertab":
          list = CommandLists.Readertab;
          break;
        case "reportingtab":
          list = CommandLists.Reportingtab;
          break;
        case "calibtab":
          list = CommandLists.Calibtab;
          break;
        case "channeltab":
          list = CommandLists.Channeltab;
          break;
        case "motorstab":
          list = CommandLists.Motorstab;
          break;
        case "componentstab":
          list = CommandLists.Componentstab;
          break;
        default:
          return;
      }
      foreach (byte Code in list)
      {
        MainCommand("Get Property", code: Code);
      }
    }

    private void SetSystemDirectories()
    {
      RootDirectory = new DirectoryInfo(Path.Combine(@"C:\Emissioninc", Environment.MachineName));
      List<string> subDirectories = new List<string>(7) { "Config", "WorkOrder", "SavedImages", "Archive", "Result", "Status", "AcquisitionData", "SystemLogs" };
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

    internal void TerminationReadyCheck()
    {
      switch (TerminationType)
      {
        case Termination.MinPerRegion:
                //do statistical magic
          if (_chkRegionCount)  //a region made it, are there more that haven't
          {
            byte IsDone = 1;   //assume all region have enough beads
            foreach (WellResult region in WellResults)
            {
              if (region.RP1vals.Count < MinPerRegion)
              {
                IsDone = 0; //not done yet
                break;
              }
            }
            if (IsDone == 1)
              StartStateMachine();
            _chkRegionCount = false;
          }
          break;
        case Termination.TotalBeadsCaptured:
          if (BeadCount >= BeadsToCapture)
          {
            StartStateMachine();
          }
          break;
        case Termination.EndOfSample:
          break;
      }
    }

    internal void FillActiveWellResults(in BeadInfoStruct outBead)
    {
      //WellResults is a list of region numbers that are active
      //each entry has a list of rp1 values from each bead in that region
      ushort region = outBead.region;
      int index = WellResults.FindIndex(w => w.regionNumber == region);
      if (index >= 0)
      {
        WellResults[index].RP1vals.Add(outBead.reporter);
        WellResults[index].RP1bgnd.Add(outBead.greenC_bg);
        if (!_chkRegionCount)
          _chkRegionCount = WellResults[index].RP1vals.Count == MinPerRegion;  //see if assay is done via sufficient beads in each region
      }
    }

    private void OnStartingToReadWell()
    {
      IsMeasurementGoing = true;
      StartingToReadWell?.Invoke(this, new ReadingWellEventArgs(WellController.CurrentWell.rowIdx, WellController.CurrentWell.colIdx,
        ResultsPublisher.FullFileName));
    }

    internal void OnFinishedReadingWell()
    {
      FinishedReadingWell?.Invoke(this, new ReadingWellEventArgs(WellController.CurrentWell.rowIdx, WellController.CurrentWell.colIdx));
    }

    internal void OnFinishedMeasurement()
    {
      IsMeasurementGoing = false;
      MainCommand("Set Property", code: 0x19);  //bubble detect off
      ResultsPublisher.StartNewSummaryReport();
      if (Mode ==  OperationMode.Verification)
        Verificator.CalculateResults();
      FinishedMeasurement?.Invoke(this, EventArgs.Empty);
    }

    internal void OnNewStatsAvailable()
    {
      NewStatsAvailable?.Invoke(this, new StatsEventArgs(_beadProcessor.Stats, _beadProcessor.AvgBg));
    }

    #if DEBUG
    public void SetBoardVersion(int v)
    {
      BoardVersion = v;
    }
    #endif
  }
}