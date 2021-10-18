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

/*
 * Most commands on the host side parallel the Properties and Methods document fo QB-1000
 * The only complex action is reading a region of wells defined by the READ SECTION parameters and button
 * They define an rectangular area of the plate that may include the entire plate. The complexity is
 * furthered by the fact that a read can be terminated in many different ways:
 * 1. Manually with the END SECTION READ button
 * 2. Manually with the END READ button which just ends the current well read and goes on the the next well
 * 3. OUT OF SHEATH condition, the sheath syringe is a position 0
 * 4. OUT OF SAMPLE contition, the sample syringe (either A or B) is at postion 0
 * 5. Require number of beads read
 * 6. Required number of beads read in each region.
 * 7. Instrument fault (bubbles, plunger overload, clog, low laser power, etc)
 * 
 * When the instrument detects one of these end condtions: QB_cmd_proc.c / SyringeEmpty()
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
    public WorkOrder WorkOrder { get; private set; }
    public PlateReport PlateReport { get; set; }
    public CustomMap ActiveMap { get; set; }
    public Queue<CommandStruct> Commands { get; } = new Queue<CommandStruct>();
    public ConcurrentQueue<BeadInfoStruct> DataOut { get; } = new ConcurrentQueue<BeadInfoStruct>();
    public List<Wells> WellsInOrder { get; set; } = new List<Wells>();
    public List<CustomMap> MapList { get; private set; } = new List<CustomMap>();
    public List<Gstats> GStats { get; } = new List<Gstats>(10);
    public List<WellResults> WellResults { get; } = new List<WellResults>();
    public event EventHandler<ReadingWellEventArgs> StartingToReadWell;
    public event EventHandler<ReadingWellEventArgs> FinishedReadingWell;
    public event EventHandler FinishedMeasurement;
    public int SavingWellIdx { get; set; }
    public int WellsToRead { get; set; }
    public int BeadsToCapture { get; set; }
    public int BeadCount { get; private set; }
    public int SavBeadCount { get; set; }
    public int CurrentWellIdx { get; set; }
    public int SampleSize { get; set; }
    public int BeadCntMapCreate { get; set; }
    public int ScatterGate { get; set; }
    public int MinPerRegion { get; set; }
    public bool IsMeasurementGoing { get; private set; }
    public bool NewStats { get; set; }
    public bool IsTube { get; set; }
    public bool ReadActive { get; set; }
    public bool Everyevent { get; set; }
    public bool RMeans { get; set; }
    public bool CalStats { get; set; }
    public bool PltRept { get; set; }
    public bool OnlyClassified { get; set; }
    public bool Reg0stats { get; set; }
    public bool ChannelBIsHiSensitivity { get; set; }
    public byte PlateRow { get; set; }
    public byte PlateCol { get; set; }
    public byte PlateType { get; set; }
    public byte TerminationType { get; set; }
    public byte ReadingRow { get; set; }
    public byte ReadingCol { get; set; }
    public byte XAxisSel { get; set; }    //TODO: delete this
    public byte YAxisSel { get; set; }    //TODO: delete this
    public byte EndState { get; set; }
    public byte SystemControl { get; set; }

    public string Outdir { get; set; }  //  user selectable
    public string Outfilename { get; set; }

    public string[] SyncElements { get; } = { "SHEATH", "SAMPLE_A", "SAMPLE_B", "FLASH", "END_WELL", "VALVES", "X_MOTOR",
      "Y_MOTOR", "Z_MOTOR", "PROXIMITY", "PRESSURE", "WASHING", "FAULT", "ALIGN MOTOR", "MAIN VALVE", "SINGLE STEP" };
    public string WorkOrderName { get; private set; }
    public DirectoryInfo RootDirectory { get; private set; }
    private bool _chkRegionCount;
    private bool _readingA;
    private byte _actPrimaryIndex;
    private byte _actSecondaryIndex;
    private byte[,] _map = new byte[300, 300];    //map for finding peak of a single region // added ClearMap()
    private byte[,] _classificationMap { get; } = new byte[300, 300]; //TODO: was ushort
    private float[,] _sfi = new float[5000, 10];
    private float _greenMin;
    private float _greenMaj;
    private string _workOrderPath;
    private string _fullFileName; //TODO: probably not necessary. look at refactoring InitBeadRead()
    private string _mapsFileName;
    private StringBuilder _summaryout = new StringBuilder();
    private StringBuilder _dataout = new StringBuilder();
    private readonly ISerial _serialConnection;
    private const string Bheader = "Preamble,Time(1 us Tick),FSC bg,Viol SSC bg,CL0 bg,CL1 bg,CL2 bg,CL3 bg,Red SSC bg,Green SSC bg," +
            "Green Maj bg, Green Min bg,Green Major,Green Minor,Red-Grn Offset,Grn-Viol Offset,Region,Forward Scatter,Violet SSC,CL0," +
            "Red SSC,CL1,CL2,CL3,Green SSC,Reporter\r ";
    private const string Sheader = "Row,Col,Region,Bead Count,Median FI,Trimmed Mean FI,CV%\r";
    private readonly bool _useStaticMaps;

    public MicroCyDevice(Type connectionType, bool useStaticMaps)
    {
      _serialConnection = ConnectionFactory.MakeNewConnection(connectionType);
      SetSystemDirectories();
      LoadMaps();
      MainCommand("Set Property", code: 1, parameter: 1);    //set version as 1 to enable work order handling
      _actPrimaryIndex = 1;  //cl1;
      _actSecondaryIndex = 2;  //cl2;
      Reg0stats = false;
      CalStats = false;
      IsTube = false;
      _serialConnection.BeginRead(ReplyFromMC);   //default termination is end of sample
      Outdir = RootDirectory.FullName;
      EndState = 0;
      ReadActive = false;
      Outfilename = "ResultFile";
      IsMeasurementGoing = false;
      _useStaticMaps = useStaticMaps;
      if (_useStaticMaps)
        ConstructClassificationMap(null);
    }

    public void ConstructClassificationMap(CustomMap mmap)
    {
      //TODO: NEEDS a FIX after decided
      if (_useStaticMaps)
      {
        using (var BinReader = new BinaryReader(File.OpenRead(@"../../StaticMaps/StaticMap.bin")))
        {
          for (var i = 0; i < 256; i++)
          {
            for (var j = 0; j < 256; j++)
            {
              //for some reason in .bin file the map is drawn upwards-right
              _classificationMap[j, i] = BinReader.ReadByte();
            }
          }
        }

        //using (var Reader = new StreamReader(@"../../StaticMaps/QBlogo.h"))
        //{
        //  string data = Reader.ReadToEnd();
        //  int index = data.IndexOf("0x");
        //  for (var i = 0; i < 256; i++)
        //  {
        //    for (var j = 0; j < 256; j++)
        //    {
        //      //for some reason in .bin file the map is drawn upwards-right
        //      _classificationMap[j, i] = byte.Parse(data.Substring(index + 2, 2), System.Globalization.NumberStyles.HexNumber);
        //      index = data.IndexOf("0x", index + 2);
        //    }
        //  }
        //}
        return;
      }

      //build classification map from ActiveMap using bitfield types A-D
      int[,] bitpoints = new int[32, 2];
      _actPrimaryIndex = (byte)mmap.midorderidx; //what channel cl0 - cl3?
      _actSecondaryIndex = (byte)mmap.loworderidx;
      MainCommand("Set Property", code: 0xce, parameter: mmap.minmapssc);  //set ssc gates for this map
      MainCommand("Set Property", code: 0xcf, parameter: mmap.maxmapssc);
      Array.Clear(_classificationMap, 0, _classificationMap.Length);
      foreach (BeadRegion mapRegions in mmap.mapRegions)
      {
        if (!mapRegions.isvector)       //this region shape is taken from Bitmaplist array
        {
          Array.Clear(bitpoints, 0, bitpoints.Length);  //copy bitmap of the type specified (A B C D)
          Array.Copy(CommandLists.Bitmaplist[mapRegions.bitmaptype], bitpoints, CommandLists.Bitmaplist[mapRegions.bitmaptype].Length);
          int rowBase = mapRegions.centermidorderidx - bitpoints[0, 0];  //first position is value to backup before etching bitmap
          int col = mapRegions.centerloworderidx - bitpoints[0, 1];
          int irow = 1; // 56 31 40 51 61 61 52 30 00
          while (bitpoints[irow, 0] != 0)
          {
            for (int jcol = col; jcol < (col + bitpoints[irow, 0]); jcol++)
            {
              //handle region overlap by making overlap 0
              _classificationMap[rowBase + irow - 1, jcol] = _classificationMap[rowBase + irow - 1, jcol] == 0 ? (byte)mapRegions.regionNumber : (byte)0;
            }
            col += bitpoints[irow, 1];  //second position is right shift amount for next line in map
            irow++;
          }
        }
        else
        {
          //populate a computed region
          float xwidth = (float)0.33 * mapRegions.meanmidorder + 50;
          float ywidth = (float)0.33 * mapRegions.meanloworder + 50;
          int begidx = Val_2_Idx((int)(mapRegions.meanmidorder - (xwidth * 0.5)));
          int endidx = Val_2_Idx((int)(mapRegions.meanmidorder + (xwidth * 0.5)));
          float xincer = 0;
          int begidy = Val_2_Idx((int)(mapRegions.meanloworder - (ywidth / 2)));
          int endidy = Val_2_Idx((int)(mapRegions.meanloworder + (ywidth / 2)));
          if (begidx < 3)
            begidx = 3;
          if (begidy < 3)
            begidy = 3;
          for (int row = begidx; row <= endidx; row++)
          {
            for (int col = begidy + (int)xincer; col <= endidy + (int)xincer; col++)
            {
              //zero any overlaps
              _classificationMap[row, col] = _classificationMap[row, col] == 0 ? (byte)mapRegions.regionNumber : (byte)0;
            }
          }
        }
      }
    }

    public void InitBeadRead(byte rown, byte coln)
    {
      //open file
      //first create uninique filename
      ushort colnum = (ushort)coln;    //well names are relative to 1
      if (!IsTube)
        coln++;  //use 0 for tubes and true column for plates
      char rowletter = (char)(0x41 + rown);
      if (!Directory.Exists(Outdir))
        Outdir = RootDirectory.FullName;
      for (var differ = 0; differ < int.MaxValue; differ++)
      {
        _fullFileName = Outdir + "\\AcquisitionData\\" + Outfilename + rowletter + coln.ToString() + '_' + differ.ToString()+".csv";
        if (!File.Exists(_fullFileName))
          break;
      }
      _ = _dataout.Clear();
      _ = _dataout.Append(Bheader);
      _chkRegionCount = false;
      BeadCount = 0;
      OnStartingToReadWell();
    }

    public void SaveBeadFile() //cancels the begin read from endpoint 2
    {
      //write file
      Console.WriteLine(string.Format("{0} Reporting Background results cloned for save", DateTime.Now.ToString()));
      if ((_fullFileName != null) && Everyevent)
        File.WriteAllText(_fullFileName, _dataout.ToString());
      if (RMeans)
      {
        ClearSummary();
        if (PlateReport != null && WellsInOrder.Count != 0)
          PlateReport.rpWells.Add(new WellReport {
            prow = WellsInOrder[SavingWellIdx].rowIdx,
            pcol = WellsInOrder[SavingWellIdx].colIdx
          });
        char[] alphabet = Enumerable.Range('A', 16).Select(x => (char)x).ToArray();
        for (var i = 0; i < WellResults.Count; i++)
        {
          WellResults regionNumber = WellResults[i];
          SavingWellIdx = SavingWellIdx > WellsInOrder.Count - 1 ? WellsInOrder.Count - 1 : SavingWellIdx;
          OutResults rout = FillOutResults(regionNumber, in alphabet);
          _ = _summaryout.Append(rout.ToString());
          AddToPlateReport(in rout);
        }
        OutputSummaryFiles();
      }
      if (CalStats && (SavBeadCount > 2))
      {
        SavBeadCount = SavBeadCount > 5000 ? 5000 : SavBeadCount;
        GStats.Clear();
        FillGStats();
        NewStats = true;
      }
      Console.WriteLine(string.Format("{0} Reporting Background File Save Complete", DateTime.Now.ToString()));
    }

    public void SetReadingParamsForWell(int index)
    {
      MainCommand("Set Property", code: 0xaa, parameter: (ushort)WellsInOrder[index].runSpeed);
      MainCommand("Set Property", code: 0xc2, parameter: (ushort)WellsInOrder[index].chanConfig);
      BeadsToCapture = WellsInOrder[index].termCnt;
      MinPerRegion = WellsInOrder[index].regTermCnt;
      TerminationType = WellsInOrder[index].termType;
      ConstructClassificationMap(ActiveMap);
      WellResults.Clear();
      foreach (BeadRegion mapRegions in WellsInOrder[index].thisWellsMap.mapRegions)
      {
        bool isInMap = ActiveMap.mapRegions.Any(x => mapRegions.regionNumber == x.regionNumber);
        if (!isInMap)
          Console.WriteLine(string.Format("{0} No Map for Work Order region {1}", DateTime.Now.ToString(),mapRegions.regionNumber));
        if (mapRegions.isActive && isInMap)
          WellResults.Add(new WellResults { regionNumber = mapRegions.regionNumber });
      }
      if (Reg0stats)
        WellResults.Add(new WellResults { regionNumber = 0 });
    }

    private void ReplyFromMC(IAsyncResult result)
    {
      _serialConnection.EndRead(result);
      if ((_serialConnection.InputBuffer[0] == 0xbe) && (_serialConnection.InputBuffer[1] == 0xad))
      {
        for (byte i = 0; i < 8; i++)
        {
          BeadInfoStruct outbead;
          if (!GetBeadFromBuffer(_serialConnection.InputBuffer, i, out outbead))
            break;
          CalculateBeadParams(ref outbead);

          FillActiveWellResults(in outbead);
          if (outbead.region == 0 && OnlyClassified)
            continue;
          DataOut.Enqueue(outbead);
          if (Everyevent)
            _ = _dataout.Append(outbead.ToString());
          //accum stats for run as a whole, used during aligment and QC
          FillCalibrationStatsRow(in outbead);
          BeadCount++;
        }
        Array.Clear(_serialConnection.InputBuffer, 0, _serialConnection.InputBuffer.Length);
        TerminationReadyCheck();
      }
      else
        GetCommandFromBuffer();

      _serialConnection.BeginRead(ReplyFromMC);
    }

    public void LoadMaps()
    {
      //read the MapList if available
      string testfilename = Path.Combine(RootDirectory.FullName, "Config", "DimensionMaps.txt");
      if (File.Exists(testfilename))
      {
        _mapsFileName = testfilename;
        try
        {
          using (TextReader reader = new StreamReader(_mapsFileName))
          {
            var fileContents = reader.ReadToEnd();
            MapList = JsonConvert.DeserializeObject<List<CustomMap>>(fileContents);
          }
        }
        catch { }
      }
      else
        throw new Exception("Error: DimensionMaps does not exist");
    }

    public bool IsNewWorkOrder()
    {
      string chkpath = Path.Combine(RootDirectory.FullName, "WorkOrder");
      string[] fileEntries = Directory.GetFiles(chkpath, "*.txt");
      if (fileEntries.Length == 0)
        return false;
      WorkOrderName = Path.GetFileNameWithoutExtension(fileEntries[0]);
      _workOrderPath = fileEntries[0];
      TextReader reader = null;
      try
      {
        reader = new StreamReader(_workOrderPath);
        var fileContents = reader.ReadToEnd();
        WorkOrder = JsonConvert.DeserializeObject<WorkOrder>(fileContents);
      }
      finally
      {
        if (reader != null)
          reader.Close();
      }
      //set plate type
      PlateType = WorkOrder.numberCols == 12 ? (byte)0 : (byte)1;
      // send well depth once that is worked out
      return true;
    }

    //Refactored

    public void ClearMap()
    {
      Array.Clear(_map, 0, _map.Length);
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
      RunCmd(command, cs);
    }

    public void InitSTab(string tabname)
    {
      List<byte> list = CommandLists.Readertab;
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
      }
      foreach (byte Code in list)
      {
        MainCommand("Get Property", code: Code);
      }
    }

    public void SaveMaps()
    {
      string jFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DimensionPlateResults.txt");
      TextWriter writer = null;
      TextWriter jwriter = null;
      try
      {
        var contents = JsonConvert.SerializeObject(MapList);
        writer = new StreamWriter(_mapsFileName);
        writer.Write(contents);
        var jcontents = JsonConvert.SerializeObject(PlateReport);   ///test code to write json for schema creation
        jwriter = new StreamWriter(jFileName);
        jwriter.Write(jcontents);
      }
      finally
      {
        if (writer != null)
          writer.Close();
        if (jwriter != null)
          jwriter.Close();
      }
    }

    public void SaveCalVals(int idx)
    {
      var map = MapList[idx];
      map.calrpmin = BiasAt30Temp.TempRpMin;
      map.calrpmaj = BiasAt30Temp.TempRpMaj;
      map.calrssc = BiasAt30Temp.TempRedSsc;
      map.calgssc = BiasAt30Temp.TempGreenSsc;
      map.calvssc = BiasAt30Temp.TempVioletSsc;
      map.calcl0 = BiasAt30Temp.TempCl0;
      map.calcl1 = BiasAt30Temp.TempCl1;
      map.calcl2 = BiasAt30Temp.TempCl2;
      map.calcl3 = BiasAt30Temp.TempCl3;
      MapList[idx] = map;
      SaveMaps();
    }

    public void WellNext()
    {
      ReadingRow = PlateRow;
      ReadingCol = PlateCol;
    }

    public void EndBeadRead()
    {
      if (_readingA)
        MainCommand("End Bead Read A");         //sends EE to instrument
      else
        MainCommand("End Bead Read B");    //send EF to instrument
      CurrentWellIdx++;
      if (CurrentWellIdx <= WellsToRead)  //are there more to go
      {
        SetReadingParamsForWell(CurrentWellIdx);
        if (_readingA)
        {
          if (CurrentWellIdx < WellsToRead)   //more than one to go
          {
            SetAspirateParamsForWell(CurrentWellIdx + 1);
            MainCommand("Read B Aspirate A");
          }
          else if (CurrentWellIdx == WellsToRead)
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
          else if (CurrentWellIdx == WellsToRead)
            //handle end of plate things
            MainCommand("Read A");
        }
        InitBeadRead(ReadingRow, ReadingCol);   //gets output file redy
      }
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
      List<string> subDirectories = new List<string>(7) { "Config", "WorkOrder", "Archive", "Result", "Status", "AcquisitionData", "SystemLogs" };
      foreach (var d in subDirectories)
      {
        RootDirectory.CreateSubdirectory(d);
      }
    }
    /// <summary>
    /// Sends a command OUT to the USB device, then checks the IN pipe for a return value.
    /// </summary>
    /// <param name="sCmdName">A friendly name for the command.</param>
    /// <param name="cs">The CommandStruct object containing the command parameters.  This will get converted to an 8-byte array.</param>
    private void RunCmd(string sCmdName, CommandStruct cs)
    {
      if (_serialConnection.IsActive)
      {
        byte[] buffer = StructToByteArray(cs);
        _serialConnection.Write(buffer);
      }
      Console.WriteLine(string.Format("{0} Sending [{1}]: {2}", DateTime.Now.ToString(), sCmdName, cs.ToString())); //  MARK1 END
    }

    private string GetSummaryFileName()
    {
      string summaryFileName = "";
      for (var i = 0; i < int.MaxValue; i++)
      {
        summaryFileName = Outdir + "\\Summary\\" + "Summary_" + Outfilename + '_' + i.ToString() + ".csv";
        if (!File.Exists(summaryFileName))
          break;
      }
      return summaryFileName;
    }

    private static byte[] StructToByteArray(object obj)
    {
      int len = Marshal.SizeOf(obj);
      byte[] arrRet = new byte[len];

      IntPtr ptr = Marshal.AllocHGlobal(len);
      Marshal.StructureToPtr(obj, ptr, true);
      Marshal.Copy(ptr, arrRet, 0, len);
      Marshal.FreeHGlobal(ptr);

      return arrRet;
    }

    private static CommandStruct ByteArrayToStruct<CommandStruct>(byte[] inmsg)
    {
      IntPtr ptr = Marshal.AllocHGlobal(8);
      try
      {
        Marshal.Copy(inmsg, 0, ptr, 8);
        return (CommandStruct)Marshal.PtrToStructure(ptr, typeof(CommandStruct));
      }
      finally
      {
        Marshal.FreeHGlobal(ptr);
      }
    }

    private static BeadInfoStruct BeadArrayToStruct<BeadInfoStruct>(byte[] beadmsg, byte shift)
    {
      IntPtr ptr = Marshal.AllocHGlobal(64);
      try
      {
        Marshal.Copy(beadmsg, shift * 64, ptr, 64);
        return (BeadInfoStruct)Marshal.PtrToStructure(ptr, typeof(BeadInfoStruct));
      }
      finally
      {
        Marshal.FreeHGlobal(ptr);
      }
    }

    private static int Val_2_Idx(int val)
    {
      int retidx;
      retidx = (int)(Math.Log(val) * 24.526);
      return retidx;
    }

    private void GetCommandFromBuffer()
    {
      CommandStruct newcmd;
      lock (Commands)
      {
        // move received command to queue
        newcmd = ByteArrayToStruct<CommandStruct>(_serialConnection.InputBuffer);
        Commands.Enqueue(newcmd);
      }
      if ((newcmd.Code >= 0xd0) && (newcmd.Code <= 0xdf))
      {
        Console.WriteLine(string.Format("{0} E-series script [{1}]", DateTime.Now.ToString(), newcmd.ToString()));
      }
      else if (newcmd.Code > 0)
      {
        Console.Out.WriteLine(string.Format("{0} Received [{1}]", DateTime.Now.ToString(), newcmd.ToString()));
      }
    }

    private void TerminationReadyCheck()
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
              if (region.RP1vals.Count() < MinPerRegion)
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

    private void FillCalibrationStatsRow(in BeadInfoStruct outbead)
    {
      if (CalStats && (BeadCount < 5000))
      {
        _sfi[BeadCount, 0] = outbead.greenssc;
        _sfi[BeadCount, 1] = outbead.greenB;
        _sfi[BeadCount, 2] = outbead.greenC;
        _sfi[BeadCount, 3] = outbead.redssc;
        _sfi[BeadCount, 4] = outbead.cl1;
        _sfi[BeadCount, 5] = outbead.cl2;
        _sfi[BeadCount, 6] = outbead.cl3;
        _sfi[BeadCount, 7] = outbead.violetssc;
        _sfi[BeadCount, 8] = outbead.cl0;
        _sfi[BeadCount, 9] = outbead.fsc;
      }
    }

    private OutResults FillOutResults(WellResults regionNumber, in char[] alphabet)
    {
      OutResults rout = new OutResults();
      rout.row = alphabet[WellsInOrder[SavingWellIdx].rowIdx].ToString();
      rout.col = WellsInOrder[SavingWellIdx].colIdx + 1;    //columns are 1 based
      rout.count = regionNumber.RP1vals.Count();
      rout.region = regionNumber.regionNumber;
      if (rout.count > 2)
        rout.meanfi = regionNumber.RP1vals.Average();
      if (rout.count >= 20)
      {
        regionNumber.RP1vals.Sort();
        float rpbg = regionNumber.RP1bgnd.Average() * 16;
        int quarter = rout.count / 4;
        regionNumber.RP1vals.RemoveRange(rout.count - quarter, quarter);
        regionNumber.RP1vals.RemoveRange(0, quarter);
        rout.meanfi = regionNumber.RP1vals.Average();
        double sumsq = regionNumber.RP1vals.Sum(dataout => Math.Pow(dataout - rout.meanfi, 2));
        double stddev = Math.Sqrt(sumsq / regionNumber.RP1vals.Count() - 1);
        rout.cv = (float)stddev / rout.meanfi * 100;
        if (double.IsNaN(rout.cv)) rout.cv = 0;
        rout.medfi = (float)Math.Round(regionNumber.RP1vals[quarter] - rpbg);
        rout.meanfi -= rpbg;
      }
      return rout;
    }

    private bool GetBeadFromBuffer(byte[] buffer,byte shift, out BeadInfoStruct outbead)
    {
      outbead = BeadArrayToStruct<BeadInfoStruct>(buffer, shift);
      return outbead.Header == 0xadbeadbe ? true : false;
    }

    private void CalculateBeadParams(ref BeadInfoStruct outbead)
    {
      //greenMaj is the hi dyn range channel, greenMin is the high sensitivity channel(depends on filter placement)
      if (ChannelBIsHiSensitivity)
      {
        _greenMaj = outbead.greenC;
        _greenMin = outbead.greenB;
      }
      else
      {
        _greenMaj = outbead.greenB;
        _greenMin = outbead.greenC;
      }
      float[] cl = MakeClArr(in outbead);
      //each well can have a different  classification map
      outbead.cl1 = cl[1];
      outbead.cl2 = cl[2];
      outbead.region = ClassifyBeadToRegion(cl);
      //handle HI dnr channel
      outbead.reporter = _greenMaj > Calibration.HdnrTrans ? _greenMaj * Calibration.HDnrCoef : _greenMin;
    }

    private ushort ClassifyBeadToRegion(float[] cl)
    {
      int x = (byte)(Math.Log(cl[_actPrimaryIndex]) * 24.526);
      int y = (byte)(Math.Log(cl[_actSecondaryIndex]) * 24.526);
      return (x > 0) && (y > 0) ? _classificationMap[x, y] : (ushort)0;
    }

    private float[] MakeClArr(in BeadInfoStruct outbead)
    {
      float cl1comp = _greenMaj * Calibration.Compensation / 100;
      float cl2comp = cl1comp * 0.26f;
      return new float[]{
            outbead.cl0,
            outbead.cl1 - cl1comp,  //Compensation
            outbead.cl2 - cl2comp,  //Compensation
            outbead.cl3
      };
    }

    private void FillActiveWellResults(in BeadInfoStruct outbead)
    {
      //WellResults is a list of region numbers that are active
      //each entry has a list of rp1 values from each bead in that reagion
      ushort region = outbead.region;
      int index = WellResults.FindIndex(w => w.regionNumber == region);
      if (index >= 0)
      {
        WellResults[index].RP1vals.Add(outbead.reporter);
        WellResults[index].RP1bgnd.Add(outbead.greenC_bg);
        _chkRegionCount = WellResults[index].RP1vals.Count == MinPerRegion;  //see if assay is done via sufficient beads in each region
      }
    }

    public byte[,] GetStaticMap()
    {
      return _classificationMap;
    }

    private void AddToPlateReport(in OutResults outRes)
    {
      PlateReport.rpWells[SavingWellIdx].rpReg.Add(new RegionReport
      {
        region = outRes.region,
        count = (uint)outRes.count,
        medfi = outRes.medfi,
        meanfi = outRes.meanfi,
        coefVar = outRes.cv
      });
    }

    public void ClearSummary()
    {
      _ = _summaryout.Clear();
      _ = _summaryout.Append(Sheader);
    }

    private void OutputSummaryFiles()
    {
      if ((SavingWellIdx == WellsToRead) && (_summaryout.Length > 0))  //end of read session (plate, plate section or tube) write summary stat file
        File.WriteAllText(GetSummaryFileName(), _summaryout.ToString());
      OutputPlateReport();
    }

    private void OutputPlateReport()
    {
      if ((SavingWellIdx == WellsToRead) && (_summaryout.Length > 0) && PltRept)    //end of read and json results requested
      {
        string rfilename = SystemControl == 0 ? Outfilename : WorkOrder.plateID.ToString();
        string resultfilename = Path.Combine(RootDirectory.FullName, "Result", rfilename);
        TextWriter jwriter = null;
        try
        {
          var jcontents = JsonConvert.SerializeObject(PlateReport);
          jwriter = new StreamWriter(resultfilename + ".json");
          jwriter.Write(jcontents);
          if (File.Exists(_workOrderPath))
            File.Delete(_workOrderPath);   //result is posted, delete work order
        }
        finally
        {
          if (jwriter != null)
            jwriter.Close();
        }
      }
    }

    private void FillGStats()
    {
      for (int finx = 0; finx < 10; finx++)
      {
        double sumit = 0;
        for (int beads = 0; beads < SavBeadCount; beads++)
        {
          sumit += _sfi[beads, finx];
        }
        double robustcnt = SavBeadCount; //start with total bead count
        double mean = sumit / SavBeadCount;
        //find high and low bounds
        double min = mean * 0.5;
        double max = mean * 2;
        sumit = 0;
        for (int beads = 0; beads < SavBeadCount; beads++)
        {
          if ((_sfi[beads, finx] > min) && (_sfi[beads, finx] < max))
            sumit += _sfi[beads, finx];
          else
          {
            _sfi[beads, finx] = 0;
            robustcnt--;
          }
        }
        mean = sumit / robustcnt;
        double sumsq = 0;
        for (int beads = 0; beads < SavBeadCount; beads++)
        {
          if (_sfi[beads, finx] == 0)
            continue;
          sumsq += Math.Pow(mean - _sfi[beads, finx], 2);
        }
        double stddev = Math.Sqrt(sumsq / (robustcnt - 1));

        double gcv = (stddev / mean) * 100;
        if (double.IsNaN(gcv))
          gcv = 0;
        GStats.Add(new Gstats
        {
          mfi = mean,
          cv = gcv
        });
      }
    }

    private void OnStartingToReadWell() //protected virtual method
    {
      IsMeasurementGoing = true;
      StartingToReadWell?.Invoke(this, new ReadingWellEventArgs(ReadingRow, ReadingCol, _fullFileName));
    }

    private void OnFinishedReadingWell() //protected virtual method
    {
      FinishedReadingWell?.Invoke(this, new ReadingWellEventArgs(ReadingRow, ReadingCol));
    }

    private void OnFinishedMeasurement() //protected virtual method
    {
      IsMeasurementGoing = false;
      FinishedMeasurement?.Invoke(this, EventArgs.Empty);
    }
  }
}