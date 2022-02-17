using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using IronBarCode;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using MicroCy.InstrumentParameters;
using System.Threading;

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
    public MapController MapCtroller { get; private set; }
    public static WorkOrder WorkOrder { get; set; }
    public static ConcurrentQueue<CommandStruct> Commands { get; } = new ConcurrentQueue<CommandStruct>();
    public static ConcurrentQueue<BeadInfoStruct> DataOut { get; } = new ConcurrentQueue<BeadInfoStruct>();
    public static List<Wells> WellsInOrder { get; set; } = new List<Wells>();
    public static ICollection<int> RegionsToOutput { get; set; }
    public BitArray SystemActivity { get; } = new BitArray(16, false);
    public static List<WellResults> WellResults { get; } = new List<WellResults>();
    public event EventHandler<ReadingWellEventArgs> StartingToReadWell;
    public event EventHandler<ReadingWellEventArgs> FinishedReadingWell;
    public event EventHandler FinishedMeasurement;
    public event EventHandler<StatsEventArgs> NewStatsAvailable;
    public static OperationMode Mode { get; set; }
    public int BoardVersion { get; internal set; }
    public float ReporterScaling { get; set; }
    public static int WellsToRead { get; set; }
    public static int BeadsToCapture { get; set; }
    public static int BeadCount { get; internal set; }
    public static int TotalBeads { get; internal set; }
    public static int CurrentWellIdx { get; set; }
    public int ScatterGate { get; set; }
    public static int MinPerRegion { get; set; }
    public static bool IsMeasurementGoing { get; private set; }
    public static bool Everyevent { get; set; }
    public static bool RMeans { get; set; }
    public static bool PlateReportActive { get; set; }
    public static bool OnlyClassified { get; set; }
    public static bool Reg0stats { get; set; }
    public static bool ChannelBIsHiSensitivity { get; set; }
    public static byte PlateRow { get; set; }
    public static byte PlateCol { get; set; }
    public static byte TerminationType { get; set; }
    public static byte ReadingRow { get; set; }
    public static byte ReadingCol { get; set; }
    public static byte SystemControl { get; set; }
    public static DirectoryInfo RootDirectory { get; private set; }
    private static bool _chkRegionCount;
    private static bool _readingA;
    private DataController _dataController;
    private readonly StateMachine _stateMach;

    public MicroCyDevice(Type connectionType = null)
    {
      _dataController = new DataController(this, connectionType);
      _stateMach = new StateMachine(this, true);
      MapCtroller = new MapController();
      MainCommand("Sync");
      TotalBeads = 0;
      Mode = OperationMode.Normal;
      SetSystemDirectories();
      MapCtroller.MoveMaps();
      MapCtroller.LoadMaps();
      Reg0stats = false;
      IsMeasurementGoing = false;
      ReporterScaling = 1;
    }

    public void InitBeadRead()
    {
      ResultReporter.GetNewFileName();
      ResultReporter.StartNewWellReport();
      _chkRegionCount = false;
      BeadCount = 0;
      OnStartingToReadWell();
    }

    public void UpdateStateMachine()
    {
      _stateMach.Action();
    }

    /// <summary>
    /// Starts a sequence of commands to finalize well measurement. The sequence is in a form of state machine that takes several timer ticks
    /// </summary>
    public void StartStateMachine()
    {
      _stateMach.Start();
    }

    public void SetReadingParamsForWell(int index)
    {
      MainCommand("Set Property", code: 0xaa, parameter: (ushort)WellsInOrder[index].runSpeed);
      MainCommand("Set Property", code: 0xc2, parameter: (ushort)WellsInOrder[index].chanConfig);
      BeadsToCapture = WellsInOrder[index].termCnt;
      MinPerRegion = WellsInOrder[index].regTermCnt;
      TerminationType = WellsInOrder[index].termType;
      MakeNewWellResults();
    }

    private void MakeNewWellResults()
    {
      WellResults.Clear();
      if (RegionsToOutput != null && RegionsToOutput.Count != 0)
      {
        foreach (var region in MapCtroller.ActiveMap.regions)
        {
          if (RegionsToOutput.Contains(region.Number))
            WellResults.Add(new WellResults { regionNumber = (ushort)region.Number });
        }
      }
      if (Reg0stats)
        WellResults.Add(new WellResults { regionNumber = 0 });
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
        case "End Sampling":
          OnFinishedReadingWell();
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

    public void WellNext()
    {
      ReadingRow = PlateRow;
      ReadingCol = PlateCol;
    }

    public bool EndBeadRead()
    {
      if (_readingA)
        MainCommand("End Bead Read A");
      else
        MainCommand("End Bead Read B");
      CurrentWellIdx++;
      return CurrentWellIdx > WellsToRead;
    }

    internal void SetupRead()
    {
      SetReadingParamsForWell(CurrentWellIdx);
      if (_readingA)
      {
        if (CurrentWellIdx < WellsToRead)   //more than one to go
        {
          SetAspirateParamsForWell(CurrentWellIdx + 1);
          MainCommand("Read B Aspirate A");
        }
        else
          MainCommand("Read B");
      }
      else
      {
        if (CurrentWellIdx < WellsToRead)
        {
          SetAspirateParamsForWell(CurrentWellIdx + 1);
          MainCommand("Read A Aspirate B");
        }
        else
          MainCommand("Read A");
      }
    }

    public void SetAspirateParamsForWell(int idx)
    {
      MainCommand("Set Property", code: 0xad, parameter: (ushort)WellsInOrder[idx].rowIdx);
      MainCommand("Set Property", code: 0xae, parameter: (ushort)WellsInOrder[idx].colIdx);
      MainCommand("Set Property", code: 0xaf, parameter: (ushort)WellsInOrder[idx].sampVol);
      MainCommand("Set Property", code: 0xac, parameter: (ushort)WellsInOrder[idx].washVol);
      MainCommand("Set Property", code: 0xc4, parameter: (ushort)WellsInOrder[idx].agitateVol);
      PlateRow = (byte)WellsInOrder[idx].rowIdx;
      PlateCol = (byte)WellsInOrder[idx].colIdx;
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
        case 0: //min beads in each region
                //do statistical magic
          if (_chkRegionCount)  //a region made it, are there more that haven't
          {
            byte IsDone = 1;   //assume all region have enough beads
            foreach (WellResults region in WellResults)
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
        case 1: //total beads captured
          if (BeadCount >= BeadsToCapture)
          {
            StartStateMachine();
          }
          break;
        case 2: //end of sample 
          break;
      }
    }

    internal static void FillActiveWellResults(in BeadInfoStruct outBead)
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

    public void StartOperation(HashSet<int> regionsToOutput = null)
    {
      MainCommand("Set Property", code: 0xce, parameter: MapCtroller.ActiveMap.calParams.minmapssc);  //set ssc gates for this map
      MainCommand("Set Property", code: 0xcf, parameter: MapCtroller.ActiveMap.calParams.maxmapssc);
      BeadProcessor.ConstructClassificationMap(MapCtroller.ActiveMap);
      //read section of plate
      MainCommand("Get FProperty", code: 0x58);
      MainCommand("Get FProperty", code: 0x68);
      ResultReporter.StartNewPlateReport();
      MainCommand("Get FProperty", code: 0x20); //get high dnr property
      SetAspirateParamsForWell(0);  //setup for first read
      RegionsToOutput = regionsToOutput;
      SetReadingParamsForWell(0);
      MainCommand("Set Property", code: 0x19, parameter: 1); //bubble detect on
      MainCommand("Position Well Plate");   //move motors. next position is set in properties 0xad and 0xae
      MainCommand("Aspirate Syringe A"); //handles down and pickup sample
      WellNext();   //save well numbers for file name
      InitBeadRead();   //gets output file ready
      ResultReporter.ClearSummary();
      TotalBeads = 0;

      if (WellsToRead == 0)    //only one well in region
        MainCommand("Read A");
      else
      {
        SetAspirateParamsForWell(1);
        MainCommand("Read A Aspirate B");
      }
      CurrentWellIdx = 0;
      if (TerminationType != 1)    //set some limit for running to eos or if regions are wrong
        BeadsToCapture = 100000;
    }

    private void OnStartingToReadWell()
    {
      IsMeasurementGoing = true;
      StartingToReadWell?.Invoke(this, new ReadingWellEventArgs(ReadingRow, ReadingCol, ResultReporter.FullFileName));
    }

    private void OnFinishedReadingWell()
    {
      FinishedReadingWell?.Invoke(this, new ReadingWellEventArgs(ReadingRow, ReadingCol));
    }

    internal void OnFinishedMeasurement()
    {
      IsMeasurementGoing = false;
      MainCommand("Set Property", code: 0x19);  //bubble detect off
      ResultReporter.StartNewSummaryReport();
      if (Mode ==  OperationMode.Verification)
        Verificator.CalculateResults();
      FinishedMeasurement?.Invoke(this, EventArgs.Empty);
    }

    internal void OnNewStatsAvailable()
    {
      NewStatsAvailable?.Invoke(this, new StatsEventArgs(BeadProcessor.Stats, BeadProcessor.AvgBg));
    }

    #if DEBUG
    public void SetBoardVersion(int v)
    {
      BoardVersion = v;
    }
    #endif
  }
}