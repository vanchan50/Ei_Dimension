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
    public List<WellResults> WellResults { get; } = new List<WellResults>();
    public event EventHandler<ReadingWellEventArgs> StartingToReadWell;
    public event EventHandler<ReadingWellEventArgs> FinishedReadingWell;
    public event EventHandler FinishedMeasurement;
    public event EventHandler<GstatsEventArgs> NewStatsAvailable;
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
    public bool ReadActive { get; set; }
    public bool Everyevent { get; set; }
    public bool RMeans { get; set; }
    public bool PltRept { get; set; }
    public bool OnlyClassified { get; set; }
    public bool Reg0stats { get; set; }
    public bool ChannelBIsHiSensitivity { get; set; }
    public byte PlateRow { get; set; }
    public byte PlateCol { get; set; }
    public byte TerminationType { get; set; }
    public byte ReadingRow { get; set; }
    public byte ReadingCol { get; set; }
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
    private float[,] _sfi = new float[5000, 10];
    private float _greenMin;
    private float _greenMaj;
    private string _workOrderPath;
    private string _fullFileName; //TODO: probably not necessary. look at refactoring InitBeadRead()
    private StringBuilder _summaryout = new StringBuilder();
    private StringBuilder _dataout = new StringBuilder();
    private List<Gstats> _gStats = new List<Gstats>(10);
    private readonly ISerial _serialConnection;
    private const string Bheader = "Preamble,Time(1 us Tick),FSC bg,Viol SSC bg,CL0 bg,CL1 bg,CL2 bg,CL3 bg,Red SSC bg,Green SSC bg," +
            "Green Maj bg, Green Min bg,Green Major,Green Minor,Red-Grn Offset,Grn-Viol Offset,Region,Forward Scatter,Violet SSC,CL0," +
            "Red SSC,CL1,CL2,CL3,Green SSC,Reporter\r ";
    private const string Sheader = "Row,Col,Region,Bead Count,Median FI,Trimmed Mean FI,CV%\r";
    private static double[] _classificationBins;
    private int[,] _classificationMap;

    public MicroCyDevice(Type connectionType)
    {
      _classificationBins = GenerateLogSpace(1, 50000, 256);
      _serialConnection = ConnectionFactory.MakeNewConnection(connectionType);
      SetSystemDirectories();
      LoadMaps();
      MainCommand("Set Property", code: 1, parameter: 1);    //set version as 1 to enable work order handling
      Reg0stats = false;
      _serialConnection.BeginRead(ReplyFromMC);   //default termination is end of sample
      Outdir = RootDirectory.FullName;
      EndState = 0;
      ReadActive = false;
      Outfilename = "ResultFile";
      IsMeasurementGoing = false;
    }

    public void ConstructClassificationMap(CustomMap Cmap)
    {
      MainCommand("Set Property", code: 0xce, parameter: Cmap.minmapssc);  //set ssc gates for this map
      MainCommand("Set Property", code: 0xcf, parameter: Cmap.maxmapssc);

      _actPrimaryIndex = (byte)Cmap.midorderidx; //what channel cl0 - cl3?
      _actSecondaryIndex = (byte)Cmap.loworderidx;

      _classificationMap = new int[256, 256];
      foreach(var point in Cmap.classificationMap)
      {
        _classificationMap[point.x, point.y] = point.r;
      }
    }

    public void InitBeadRead(byte rown, byte coln)
    {
      //open file
      //first create uninique filename

      //if(!isTube)
      coln++;  //use 0 for tubes and true column for plates
      char rowletter = (char)(0x41 + rown);
      if (!Directory.Exists(Outdir + "\\AcquisitionData"))
        Directory.CreateDirectory(Outdir + "\\AcquisitionData");
      for (var differ = 0; differ < int.MaxValue; differ++)
      {
        _fullFileName = Outdir + "\\AcquisitionData\\" + Outfilename + rowletter + coln.ToString() + '_' + differ.ToString() + ".csv";
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
      if (SavBeadCount > 2)
      {
        SavBeadCount = SavBeadCount > 5000 ? 5000 : SavBeadCount;
        _gStats.Clear();
        FillGStats();
        OnNewStatsAvailable();
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
      // regions should have been added in ascending order
      foreach (var point in ActiveMap.classificationMap)
      {
        if(!WellResults.Any(x => x.regionNumber == point.r ))
        {
          WellResults.Add(new WellResults { regionNumber = (ushort)point.r });
        }
      }
      if (Reg0stats)
        WellResults.Add(new WellResults { regionNumber = 0 });
    }

    private void ReplyFromMC(IAsyncResult result)
    {
      _serialConnection.EndRead(result);
      
      if ((_serialConnection.InputBuffer[0] == 0xbe) && (_serialConnection.InputBuffer[1] == 0xad))
      {
        if (IsMeasurementGoing) //  this condition avoids the necessity of cleaning up leftover data in the system USB interface. That could happen after operation abortion and program restart
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

    public bool IsNewWorkOrder()
    {
      string chkpath = Path.Combine(RootDirectory.FullName, "WorkOrder");
      string[] fileEntries = Directory.GetFiles(chkpath, "*.txt");
      if (fileEntries.Length == 0)
        return false;
      WorkOrderName = Path.GetFileNameWithoutExtension(fileEntries[0]);
      _workOrderPath = fileEntries[0];
      try
      {
        using (TextReader reader = new StreamReader(_workOrderPath))
        {
          var contents = reader.ReadToEnd();
          WorkOrder = JsonConvert.DeserializeObject<WorkOrder>(contents);
        }
      }
      catch { }
      // send well depth once that is worked out
      return true;
    }

    //Refactored

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
      //Removing this can lead to unforseen crucial bugs in instrument operation. If so - do with extra care
      //one example is a check in CommandLists.Readertab for changed plate parameter,which could happen in manual well selection in motors tab
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

    public void SaveCalVals(BiasAt30Temp? temps = null, ushort minSSC = 0, ushort maxSSC = 0)
    {
      var idx = MapList.FindIndex(x => x.mapName == ActiveMap.mapName);
      var map = MapList[idx];
      if (temps != null)
      {
        map.calrpmin = temps.Value.TempRpMin;
        map.calrpmaj = temps.Value.TempRpMaj;
        map.calrssc = temps.Value.TempRedSsc;
        map.calgssc = temps.Value.TempGreenSsc;
        map.calvssc = temps.Value.TempVioletSsc;
        map.calcl0 = temps.Value.TempCl0;
        map.calcl1 = temps.Value.TempCl1;
        map.calcl2 = temps.Value.TempCl2;
        map.calcl3 = temps.Value.TempCl3;
        map.calfsc = temps.Value.TempFsc;
      }
      else
      {
        if (minSSC != 0 && maxSSC != 0)
        {
          map.minmapssc = minSSC;
          map.maxmapssc = maxSSC;
        }
      }
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
    /// <summary>
    /// Sends a command OUT to the USB device, then checks the IN pipe for a return value.
    /// </summary>
    /// <param name="sCmdName">A friendly name for the command.</param>
    /// <param name="cs">The CommandStruct object containing the command parameters.  This will get converted to an 8-byte array.</param>
    private void RunCmd(string sCmdName, CommandStruct cs)
    {
      if (_serialConnection.IsActive)
        _serialConnection.Write(StructToByteArray(cs));
      Console.WriteLine(string.Format("{0} Sending [{1}]: {2}", DateTime.Now.ToString(), sCmdName, cs.ToString())); //  MARK1 END
    }

    private string GetSummaryFileName()
    {
      string summaryFileName = "";
      if (!Directory.Exists(Outdir + "\\Result\\Summary"))
        Directory.CreateDirectory(Outdir + "\\Result\\Summary");
      for (var i = 0; i < int.MaxValue; i++)
      {
        summaryFileName = Outdir + "\\Result\\Summary\\" + "Summary_" + Outfilename + '_' + i.ToString() + ".csv";
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

    private void FillCalibrationStatsRow(in BeadInfoStruct outbead)
    {
      if (BeadCount < 5000)
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
      rout.count = regionNumber.RP1vals.Count;
      rout.region = regionNumber.regionNumber;
      List<float> rp1temp = new List<float>(regionNumber.RP1vals);
      if (rout.count > 2)
        rout.meanfi = rp1temp.Average();
      if (rout.count >= 20)
      {
        rp1temp.Sort();
        float rpbg = regionNumber.RP1bgnd.Average() * 16;
        int quarter = rout.count / 4;
        rp1temp.RemoveRange(rout.count - quarter, quarter);
        rp1temp.RemoveRange(0, quarter);
        rout.meanfi = rp1temp.Average();
        double sumsq = rp1temp.Sum(dataout => Math.Pow(dataout - rout.meanfi, 2));
        double stddev = Math.Sqrt(sumsq / rp1temp.Count() - 1);
        rout.cv = (float)stddev / rout.meanfi * 100;
        if (double.IsNaN(rout.cv)) rout.cv = 0;
        rout.medfi = (float)Math.Round(rp1temp[quarter] - rpbg);
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
      outbead.region = (ushort)ClassifyBeadToRegion(cl);
      //handle HI dnr channel
      outbead.reporter = _greenMin > Calibration.HdnrTrans ? _greenMaj * Calibration.HDnrCoef : _greenMin;
    }

    private int ClassifyBeadToRegion(float[] cl)
    {
      int x = 0;
      int y = 0;
      x = Array.BinarySearch(_classificationBins, cl[_actPrimaryIndex]);
      if (x < 0)
        x = ~x;
      y = Array.BinarySearch(_classificationBins, cl[_actSecondaryIndex]);
      if (y < 0)
        y = ~y;
      x = x < 255 ? x : 255;
      y = y < 255 ? y : 255;
      return _classificationMap[x, y];
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
        if (!_chkRegionCount)
          _chkRegionCount = WellResults[index].RP1vals.Count == MinPerRegion;  //see if assay is done via sufficient beads in each region
      }
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
        if (!Directory.Exists($"{Outdir}\\Result"))
          _ = Directory.CreateDirectory($"{Outdir}\\Result");
        using (TextWriter jwriter = new StreamWriter($"{Outdir}\\Result\\" + rfilename + ".json"))
        {
          var jcontents = JsonConvert.SerializeObject(PlateReport);
          jwriter.Write(jcontents);
          if (File.Exists(_workOrderPath))
            File.Delete(_workOrderPath);   //result is posted, delete work order
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
        _gStats.Add(new Gstats
        {
          mfi = mean,
          cv = gcv
        });
      }
    }

    private void OnStartingToReadWell()
    {
      IsMeasurementGoing = true;
      StartingToReadWell?.Invoke(this, new ReadingWellEventArgs(ReadingRow, ReadingCol, _fullFileName));
    }

    private void OnFinishedReadingWell()
    {
      FinishedReadingWell?.Invoke(this, new ReadingWellEventArgs(ReadingRow, ReadingCol));
    }

    private void OnFinishedMeasurement()
    {
      IsMeasurementGoing = false;
      FinishedMeasurement?.Invoke(this, EventArgs.Empty);
    }

    private void OnNewStatsAvailable()
    {
      NewStatsAvailable?.Invoke(this, new GstatsEventArgs(_gStats));
    }

    private static double[] GenerateLogSpace(int min, int max, int logBins, bool baseE = false)
    {
      double logarithmicBase = 10;
      double logMin = Math.Log10(min);
      double logMax = Math.Log10(max);
      if (baseE)
      {
        logarithmicBase = Math.E;
        logMin = Math.Log(min);
        logMax = Math.Log(max);
      }
      double delta = (logMax - logMin) / logBins;
      double accDelta = delta;
      double[] Result = new double[logBins];
      for (int i = 1; i <= logBins; ++i)
      {
        Result[i - 1] = Math.Pow(logarithmicBase, logMin + accDelta);
        accDelta += delta;
      }
      return Result;
    }
  }
}