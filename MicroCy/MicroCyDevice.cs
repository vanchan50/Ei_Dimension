using System;
using System.Collections.Generic;
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
    public static WorkOrder WorkOrder { get;set; }
    public CustomMap ActiveMap { get; set; }
    public static ConcurrentQueue<CommandStruct> Commands { get; } = new ConcurrentQueue<CommandStruct>();
    public static ConcurrentQueue<BeadInfoStruct> DataOut { get; } = new ConcurrentQueue<BeadInfoStruct>();
    public static List<Wells> WellsInOrder { get; set; } = new List<Wells>();
    public List<CustomMap> MapList { get; } = new List<CustomMap>();
    public static List<WellResults> WellResults { get; } = new List<WellResults>();
    public event EventHandler<ReadingWellEventArgs> StartingToReadWell;
    public event EventHandler<ReadingWellEventArgs> FinishedReadingWell;
    public event EventHandler FinishedMeasurement;
    public event EventHandler<GstatsEventArgs> NewStatsAvailable;
    public static OperationMode Mode { get; set; }
    public int BoardVersion { get; set; }
    public static int WellsToRead { get; set; }
    public static int BeadsToCapture { get; set; }
    public static int BeadCount { get; internal set; }
    public static int TotalBeads { get; internal set; }
    public int CurrentWellIdx { get; set; }
    public int ScatterGate { get; set; }
    public static int MinPerRegion { get; set; }
    public static bool IsMeasurementGoing { get; private set; }
    public static bool ReadActive { get; set; }
    public static bool Everyevent { get; set; }
    public static bool RMeans { get; set; }
    public static bool PltRept { get; set; }
    public static bool OnlyClassified { get; set; }
    public bool Reg0stats { get; set; }
    public static bool ChannelBIsHiSensitivity { get; set; }
    public byte PlateRow { get; set; }
    public byte PlateCol { get; set; }
    public static byte TerminationType { get; set; }
    public static byte ReadingRow { get; set; }
    public static byte ReadingCol { get; set; }
    public static byte EndState { get; set; }
    public static byte SystemControl { get; set; }
    public static DirectoryInfo RootDirectory { get; private set; }
    private static bool _chkRegionCount;
    private static bool _readingA;
    private static DataController _dataController;

    public MicroCyDevice(Type connectionType)
    {
      _dataController = new DataController(connectionType);
      MainCommand("Sync");
      TotalBeads = 0;
      Mode = OperationMode.Normal;
      SetSystemDirectories();
      MoveMaps();
      LoadMaps();
      Reg0stats = false;
      //_serialConnection.BeginRead(ReplyFromMC);   //default termination is end of sample
      EndState = 0;
      ReadActive = false;
      IsMeasurementGoing = false;
    }

    public void InitBeadRead()
    {
      ResultReporter.OutDirCheck();
      if (!Directory.Exists($"{ResultReporter.Outdir}\\AcquisitionData"))
        Directory.CreateDirectory($"{ResultReporter.Outdir}\\AcquisitionData");
      ResultReporter.GetNewFileName();
      ResultReporter.StartNewWellReport();
      _chkRegionCount = false;
      BeadCount = 0;
      OnStartingToReadWell();
    }

    public void GStatsFiller()
    {
      if (BeadProcessor.SavBeadCount > 2)
      {
        BeadProcessor.FillGStats();
        OnNewStatsAvailable();
      }
    }

    public void SetReadingParamsForWell(int index, HashSet<int> regionsToOutput = null)
    {
      MainCommand("Set Property", code: 0xaa, parameter: (ushort)WellsInOrder[index].runSpeed);
      MainCommand("Set Property", code: 0xc2, parameter: (ushort)WellsInOrder[index].chanConfig);
      BeadsToCapture = WellsInOrder[index].termCnt;
      MinPerRegion = WellsInOrder[index].regTermCnt;
      TerminationType = WellsInOrder[index].termType;
      WellResults.Clear();
      foreach (var region in ActiveMap.regions)
      {
        if(regionsToOutput != null && regionsToOutput.Contains(region.Number))
          WellResults.Add(new WellResults { regionNumber = (ushort)region.Number });
      }
      if (Reg0stats)
        WellResults.Add(new WellResults { regionNumber = 0 });
    }

    public void LoadMaps()
    {
      string path = Path.Combine(RootDirectory.FullName, "Config");
      var files = Directory.GetFiles(path, "*.dmap");
      foreach(var mp in files)
      {
        using (TextReader reader = new StreamReader(mp))
        {
          var fileContents = reader.ReadToEnd();
          try
          {
            MapList.Add(JsonConvert.DeserializeObject<CustomMap>(fileContents));
          }
          catch { }
        }
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
      lock (DataController.UsbOutCV)
      {
        Monitor.Pulse(DataController.UsbOutCV);
      }

      //RunCmd(command, cs);
    }

    public void StopWellMeasurement()
    {
      BeadProcessor.SavBeadCount = BeadCount;   //save for stats
      ResultReporter.SavingWellIdx = CurrentWellIdx; //save the index of the currrent well for background file save
      MainCommand("End Sampling");    //sends message to instrument to stop sampling
      Console.WriteLine($"{DateTime.Now.ToString()} Reporting End Sampling");
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

    public void SaveCalVals(MapCalParameters param)
    {
      var idx = MapList.FindIndex(x => x.mapName == ActiveMap.mapName);
      var map = MapList[idx];
      if(param.TempRpMin >= 0)
        map.calrpmin = param.TempRpMin;
      if (param.TempRpMaj >= 0)
        map.calrpmaj = param.TempRpMaj;
      if (param.TempRedSsc >= 0)
        map.calrssc = param.TempRedSsc;
      if (param.TempGreenSsc >= 0)
        map.calgssc = param.TempGreenSsc;
      if (param.TempVioletSsc >= 0)
        map.calvssc = param.TempVioletSsc;
      if (param.TempCl0 >= 0)
        map.calcl0 = param.TempCl0;
      if (param.TempCl1 >= 0)
        map.calcl1 = param.TempCl1;
      if (param.TempCl2 >= 0)
        map.calcl2 = param.TempCl2;
      if (param.TempCl3 >= 0)
        map.calcl3 = param.TempCl3;
      if (param.TempFsc >= 0)
        map.calfsc = param.TempFsc;
      if (param.Compensation >= 0)
        map.calParams.compensation = param.Compensation;
      if (param.Gating >= 0)
        map.calParams.gate = (ushort)param.Gating;
      if (param.Height >= 0)
        map.calParams.height = (ushort)param.Height;
      if (param.DNRCoef >= 0)
        map.calParams.DNRCoef = param.DNRCoef;
      if (param.DNRTrans >= 0)
        map.calParams.DNRTrans = param.DNRTrans;
      if (param.MinSSC >= 0)
        map.calParams.minmapssc = (ushort)param.MinSSC;
      if (param.MaxSSC >= 0)
        map.calParams.maxmapssc = (ushort)param.MaxSSC;
      if (param.Attenuation >= 0)
        map.calParams.att = param.Attenuation;
      if (param.CL0 >= 0)
        map.calParams.CL0 = param.CL0;
      if (param.CL1 >= 0)
        map.calParams.CL1 = param.CL1;
      if (param.CL2 >= 0)
        map.calParams.CL2 = param.CL2;
      if (param.CL3 >= 0)
        map.calParams.CL3 = param.CL3;
      if (param.RP1 >= 0)
        map.calParams.RP1 = param.RP1;
      if (param.Caldate != null)
        map.caltime = param.Caldate;
      if (param.Valdate != null)
        map.valtime = param.Valdate;

      MapList[idx] = map;
      ActiveMap = MapList[idx];

      var contents = JsonConvert.SerializeObject(map);
      using (var stream = new StreamWriter(RootDirectory.FullName + @"/Config/" + map.mapName + @".dmap"))
      {
        stream.Write(contents);
      }
    }

    public void WellNext()
    {
      ReadingRow = PlateRow;
      ReadingCol = PlateCol;
    }

    public void EndBeadRead(HashSet<int> regionsToOutput = null)
    {
      if (_readingA)
        MainCommand("End Bead Read A");
      else
        MainCommand("End Bead Read B");
      CurrentWellIdx++;
      if (CurrentWellIdx <= WellsToRead)  //are there more to go
      {
        SetReadingParamsForWell(CurrentWellIdx, regionsToOutput);
        if (_readingA)
        {
          if (CurrentWellIdx < WellsToRead)   //more than one to go
          {
            SetAspirateParamsForWell(CurrentWellIdx + 1);
            MainCommand("Read B Aspirate A");
          }
          else
            //handle end of plate things
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
            //handle end of plate things
            MainCommand("Read A");
        }
        InitBeadRead();   //gets output file redy
      }
      else
        OnFinishedMeasurement();
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
      foreach (var d in subDirectories)
      {
        RootDirectory.CreateSubdirectory(d);
      }
      Directory.CreateDirectory(RootDirectory.FullName + @"\Result" + @"\Summary");
      Directory.CreateDirectory(RootDirectory.FullName + @"\Result" + @"\Detail");
    }

    private void MoveMaps()
    {
      string path = $"{Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)}\\Maps";
      string[] files = null;
      try
      {
        files = Directory.GetFiles(path, "*.dmap");
      }
      catch { return; }

      foreach (var mp in files)
      {
        string name = mp.Substring(mp.LastIndexOf("\\") + 1);
        string destination = $"{RootDirectory.FullName}\\Config\\{name}";
        if (!File.Exists(destination))
        {
          File.Copy(mp, destination);
        }
        else
        {
          var badDate = new DateTime(2021, 12, 1);  // File.GetCreationTime(mp);
          var date = File.GetCreationTime(destination);
          date = date.Date;
          if(date < badDate)
          {
            File.Delete(destination);
            File.Copy(mp, destination);
            File.SetCreationTime(destination, DateTime.Now);
          }
        }
      }
    }

    internal static void TerminationReadyCheck()
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
            EndState = IsDone;
            _chkRegionCount = false;
          }
          break;
        case 1: //total beads captured
          if ((BeadCount >= BeadsToCapture) && ReadActive)
          {
            EndState = 1;
            ReadActive = false;
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
      MainCommand("Set Property", code: 0xce, parameter: ActiveMap.calParams.minmapssc);  //set ssc gates for this map
      MainCommand("Set Property", code: 0xcf, parameter: ActiveMap.calParams.maxmapssc);
      BeadProcessor.ConstructClassificationMap(ActiveMap);
      //read section of plate
      MainCommand("Get FProperty", code: 0x58);
      MainCommand("Get FProperty", code: 0x68);
      ResultReporter.StartNewPlateReport();
      MainCommand("Get FProperty", code: 0x20); //get high dnr property
      ReadActive = true;
      SetAspirateParamsForWell(0);  //setup for first read
      SetReadingParamsForWell(0, regionsToOutput);
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

    private void OnFinishedMeasurement()
    {
      IsMeasurementGoing = false;
      ReadActive = false;
      ResultReporter.StartNewSummaryReport();
      FinishedMeasurement?.Invoke(this, EventArgs.Empty);
    }

    private void OnNewStatsAvailable()
    {
      NewStatsAvailable?.Invoke(this, new GstatsEventArgs(BeadProcessor.Stats));
    }
  }
}