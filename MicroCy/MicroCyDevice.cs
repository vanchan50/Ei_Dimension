using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.IO;
using MadWizard.WinUSBNet;
using Newtonsoft.Json;
using IronBarCode;
//using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
  #region STRUCTS
  public unsafe struct BeadInfoStruct
  {
    public uint Header;
    public uint EventTime;

    public byte fsc_bg;
    public byte vssc_bg;
    public byte cl0_bg;
    public byte cl1_bg;
    public byte cl2_bg;
    public byte cl3_bg;
    public byte rssc_bg;
    public byte gssc_bg;

    public ushort greenB_bg;    //measured background used in Compensation 
    public ushort greenC_bg;
    public ushort greenB;   //ADC measurement of optical split on RP1
    public ushort greenC;

    public byte l_offset_rg;    //samples between green and red peak
    public byte l_offset_gv;    //samples between green and violet peak
    public ushort region;
    public float fsc;       //Forward Scatter

    public float violetssc; //Violet A
    public float cl0;       //Violet B

    public float redssc;    //Red D
    public float cl1;       //Red C

    public float cl2;       //Red B
    public float cl3;       //Red A

    public float greenssc;
    public float reporter;  //computed from Green B and Green C

    public override string ToString()   //setup for csv output
    {
      return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25}\r",
        Header, EventTime, fsc_bg, vssc_bg, cl0_bg, cl1_bg, cl2_bg, cl3_bg, rssc_bg, gssc_bg,  greenB_bg, greenC_bg, greenB, greenC,
        l_offset_rg, l_offset_gv, region, fsc, violetssc, cl0, redssc, cl1, cl2, cl3, greenssc, reporter);
    }
  }

  [Serializable]
  public class Wells
  {
    public byte rowIdx { get; set; }
    public byte colIdx { get; set; }
    public byte runSpeed { get; set; }
    public byte termType { get; set; }
    public byte chanConfig { get; set; }
    public short sampVol { get; set; }
    public short washVol { get; set; }
    public short agitateVol { get; set; }
    public int termCnt { get; set; }
    public int regTermCnt { get; set; }
    public CustomMap thisWellsMap = new CustomMap();
  }

  public unsafe struct CommandStruct
  {
    public byte Code;
    public byte Command;
    public ushort Parameter;
    public float FParameter;

    public override string ToString()
    {
      return string.Format("{0:X2},{1:X2},{2},{3:F1}", Code, Command, Parameter, FParameter);
    }
  }

  public unsafe struct CLQueue
  {
    public uint xyclx;
    public uint xycly;
    public uint xycl2;
    public uint xycl3;
    public uint hrp1;
    public uint gssc;
    public uint rssc;
    public uint vssc;
  }

  [Serializable]
  public struct OutResults
  {
    public string row;
    public int col;
    public ushort region;
    public int count;
    public float medfi;
    public float meanfi;
    public float cv;
    public override string ToString()
    {
      return string.Format("{0},{1},{2},{3},{4},{5:F3},{6:F1}\r",
        row, col, region, count, medfi, meanfi, cv);
    }
  }
  #endregion STRUCTS

  #region ENUMS
  public enum USBRequestType
  {
    Standard = 0,
    Class = 1,
    Vendor = 2,
    Reserved = 3
  }

  public enum USBRequestType_Direction
  {
    HostToDevice = 0,
    DeviceToHost = 1
  }

  public enum USBRequestType_Recipient
  {
    Device = 0,
    Interface = 1,
    Endpoint = 2,
    Other = 3
  }

  public enum USBRequest
  {
    GET_STATUS = 0,
    CLEAR_FEATURE = 1,
    SET_FEATURE = 3,
    SET_ADDRESS = 5,
    GET_DESCRIPTOR = 6,
    SET_DESCRIPTOR = 7,
    GET_CONFIGURATION = 8,
    SET_CONFIGURATION = 9,
    GET_INTERFACE = 10,
    SET_INTERFACE = 11,
    SYNCH_FRAME = 12
  }

  public enum SyncItems
  {
    SHEATH = 0,
    SAMPLE_A = 1,
    SAMPLE_B = 2,
    FLASH = 3,

    ALIGNER = 4,
    VALVES = 5,
    X_MOTOR = 6,
    Y_MOTOR = 7,

    Z_MOTOR = 8,
    PROXIMITY = 9,
    PRESSURE = 10
  }
  #endregion ENUMS

  [Serializable]
  public class BeadRegion
  {
    public ushort regionNumber { get; set; }
    public bool isActive { get; set; }
    public bool isvector { get; set; }          //vector type maps are computed instead of described by map array
    public byte bitmaptype { get; set; }
    public int centerhighorderidx { get; set; } //log index of mean value of intesity measured during map create
    public int centermidorderidx { get; set; }
    public int centerloworderidx { get; set; }
    public int meanhighorder { get; set; } //mean value of intesity measured during map create
    public int meanmidorder { get; set; }
    public int meanloworder { get; set; }
    public int meanrp1bg { get; set; }
  }

  [Serializable]
  public class CustomMap
    {
        public string mapName { get; set; }

        public bool dimension3 { get; set; }    //is it 3 dimensional
        public int highorderidx { get; set; }   //is 0 for cl0, 1 for cl1, etc
        public int midorderidx { get; set; }
        public int loworderidx { get; set; }
        public ushort minmapssc { get; set; }
        public ushort maxmapssc { get; set; }
        public int calcl0 { get; set; }
        public int calcl1 { get; set; }
        public int calcl2 { get; set; }
        public int calcl3 { get; set; }
        public int calrpmaj { get; set; }
        public int calrpmin { get; set; }
        public int calrssc { get; set; }
        public int calgssc { get; set; }
        public int calvssc { get; set; }
        public List<BeadRegion> mapRegions = new List<BeadRegion>();
    }

  [Serializable]
  public class WorkOrder
  {
    public Guid plateID { get; set; }
    public Guid beadMapId { get; set; }
    public short numberRows { get; set; }
    public short numberCols { get; set; }
    public short wellDepth { get; set; }
    public DateTime createDateTime { get; set; }        //date and time per ISO8601
    public DateTime scheduleDateTime { get; set; }
    public List<Wells> woWells = new List<Wells>();
  }

  [Serializable]
  public class PlateReport
  {
    public Guid plateID { get; set; }
    public Guid beadMapId { get; set; }
    public DateTime completedDateTime { get; set; }
    public List<WellReport> rpWells = new List<WellReport>();
  }
  [Serializable]
  public class WellReport
    {
        public UInt16 prow { get; set; }
        public UInt16 pcol { get; set; }
        public List<RegionReport> rpReg = new List<RegionReport>();
    }
  [Serializable]
  public class RegionReport
    {
        public UInt16 region { get; set; }
        public UInt32 count { get; set; }
        public float medfi { get; set; }
        public float meanfi { get; set; }
        public float coefVar { get; set; }
    }
  [Serializable]
  public class WellResults
    {
        public UInt16 regionNumber { get; set; }
        public List<float> RP1vals = new List<float>();
        public List<float> RP1bgnd = new List<float>();
    }

  public class Gstats
  {
    public double mfi;
    public double cv;
  }

  public class MicroCyDevice
  {
    public WorkOrder WorkOrder { get; private set; }
    public PlateReport PlateReport { get; set; }
    public CustomMap ActiveMap { get; set; }
    public Queue<CommandStruct> Commands { get; } = new Queue<CommandStruct>();
    public Queue<CLQueue> ClData { get; } = new Queue<CLQueue>();
    public List<Wells> WellsInOrder { get; } = new List<Wells>();
    public List<Wells> Wells { get; } = new List<Wells>();
    public List<CustomMap> MapList { get; private set; } = new List<CustomMap>();
    public List<Gstats> GStats { get; set; } = new List<Gstats>();
    public ushort[,] ClassificationMap { get;} = new ushort[300, 300];
    public bool[,] ActWell = new bool[16, 24];  //some property in legacy to mark selected wells
    public int[] SscData = new int[256];  //Probably not necessary. was part of chart1 in legacy Only set in ReplyFromMC()
    public int[] Rp1Data = new int[256];  //Probably not necessary. was part of chart3 in legacy Only set in ReplyFromMC()
    public int SavingWellIdx { get; set; }
    public float HDnrCoef { get; set; }
    public int TempCl0 { get; set; }
    public int TempCl1 { get; set; }
    public int TempCl2 { get; set; }
    public int TempCl3 { get; set; }
    public int TempRedSsc { get; set; }
    public int TempGreenSsc { get; set; }
    public int TempVioletSsc { get; set; }
    public int TemprpMaj { get; set; }
    public int TemprpMin { get; set; }
    public int WellsToRead { get; set; }

    public string Outdir { get; set; }  //  user selectable
    public string Outfilename { get; set; } //TODO: prob not necessary
     
    public byte PlateRow { get; set; }
    public byte PlateCol { get; set; }
    public byte PlateType { get; set; }
    public int BeadsToCapture { get; set; }
    public int BeadCount { get; private set; }
    public int SavBeadCount { get; set; }
    public int CurrentWellIdx { get; set; }
    public int SampleSize { get; set; }
    public int BeadCntMapCreate { get; set; }
    public float MapPeakX { get; set; }
    public float MapPeakY { get; set; }
    public float Compensation { get; set; }
    public float HdnrTrans { get; set; }
    public bool RowOrder { get; set; }
    public int ScatterGate { get; set; }
    public bool NewStats { get; set; }
    public float IdexDir { get; set; }
    public ushort IdexSteps { get; set; }
    public byte IdexPos { get; set; }
    public int MinPerRegion { get; set; }
    public byte TerminationType { get; set; }
    public bool SubtRegBg { get; set; }
    public byte ReadingRow { get; set; }
    public byte ReadingCol { get; set; }
    public byte SscSelected { get; set; }
    public byte XAxisSel { get; set; }
    public byte YAxisSel { get; set; }
    public bool IsTube { get; set; }
    public bool ReadActive { get; set; }
    public bool Everyevent { get; set; }
    public bool RMeans { get; set; }
    public bool CalStats { get; set; }
    public bool PltRept { get; set; }
    public byte EndState { get; set; }
    public byte CreateMapState { get; set; }
    public bool OnlyClassified { get; set; }
    public bool Reg0stats { get; set; }
    public bool Newmap { get; set; }
    public string[] SyncElements { get; } = { "SHEATH", "SAMPLE_A", "SAMPLE_B", "FLASH", "END_WELL", "VALVES", "X_MOTOR",
      "Y_MOTOR", "Z_MOTOR", "PROXIMITY", "PRESSURE", "WASHING", "FAULT", "ALIGN MOTOR", "MAIN VALVE", "SINGLE STEP" };
    public byte SystemControl { get; set; }
    public string WorkOrderName { get; private set; }
    public DirectoryInfo RootDirectory { get; private set; }
    private bool _chkRegCnt;
    private bool _instrumentConnected;
    private bool _readingA;
    private byte _actPriIdx;
    private byte _actSecIdx;
    private byte[,] _map = new byte[300, 300];    //map for finding peak of a single region // added ClearMap()
    private byte[] _usbInputBuffer = new byte[512];
    private float[,] _sfi = new float[5000, 10];
    private string _workOrderPath;
    private string _fullFileName; //TODO: probably not necessary. look at refactoring InitBeadRead()
    private string _mapsFileName;
    private StringBuilder _summaryout = new StringBuilder();
    private StringBuilder _dataout = new StringBuilder();
    private List<WellResults> _wellResults = new List<WellResults>();
    private readonly USBDevice MicroCyUSBDevice;
    private readonly Dictionary<string, CommandStruct> MainCmdTemplatesDict = new Dictionary<string, CommandStruct>()
    {
      { "Sheath", new CommandStruct{ Code=0xd0,Command=0x0,Parameter=0,FParameter=0} },
      { "SampleA", new CommandStruct{ Code=0xd1,Command=0x0,Parameter=0,FParameter=0} },
      { "SampleB", new CommandStruct{ Code=0xd2,Command=0x0,Parameter=0,FParameter=0} },
      { "RefreshDac", new CommandStruct{ Code=0xd3,Command=0x0,Parameter=0,FParameter=0} },
      { "SetNextWell", new CommandStruct{ Code=0xd4,Command=0x0,Parameter=0,FParameter=0} },
      { "SetBaseline", new CommandStruct{ Code=0xd5,Command=0x0,Parameter=0,FParameter=0} },
      { "SaveToFlash", new CommandStruct{ Code=0xd6,Command=0x0,Parameter=0,FParameter=0} },
      { "Idex", new CommandStruct{ Code=0xd7,Command=0x0,Parameter=0,FParameter=0} },
      { "InitOpVars", new CommandStruct{ Code=0xd8,Command=0x0,Parameter=0,FParameter=0} },
      { "FlushCmdQueue", new CommandStruct{ Code=0xd9,Command=0x0,Parameter=0,FParameter=0} },
      { "Start Sampling", new CommandStruct{ Code=0xda,Command=0x0,Parameter=0,FParameter=0} },
      { "End Sampling", new CommandStruct{ Code=0xdb,Command=0x0,Parameter=0,FParameter=0} },
      { "AlignMotor", new CommandStruct{ Code=0xdc,Command=0x0,Parameter=0,FParameter=0} },
      { "MotorX", new CommandStruct{ Code=0xdd,Command=0x0,Parameter=0,FParameter=0} },
      { "MotorY", new CommandStruct{ Code=0xde,Command=0x0,Parameter=0,FParameter=0} },
      { "MotorZ", new CommandStruct{ Code=0xdf,Command=0x0,Parameter=0,FParameter=0} },
      { "Startup", new CommandStruct { Code=0xe0,Command=0x0,Parameter=0,FParameter=0} },
      { "Prime", new CommandStruct { Code=0xe1,Command=0x0,Parameter=0,FParameter=0} },
      { "Sheath Empty Prime", new CommandStruct { Code=0xe2,Command=0x0,Parameter=0,FParameter=0} },
      { "Wash A", new CommandStruct { Code=0xe3,Command=0x0,Parameter=0,FParameter=0} },
      { "Wash B", new CommandStruct { Code=0xe4,Command=0x0,Parameter=0,FParameter=0} },
      { "Eject Plate", new CommandStruct { Code=0xe5,Command=0x0,Parameter=0,FParameter=0} },
      { "Load Plate", new CommandStruct { Code=0xe6,Command=0x0,Parameter=0,FParameter=0} },
      { "Position Well Plate", new CommandStruct { Code=0xe7,Command=0x0,Parameter=0,FParameter=0} },
      { "Aspirate Syringe A", new CommandStruct { Code=0xe8,Command=0x0,Parameter=0,FParameter=0} },
      { "Aspirate Syringe B", new CommandStruct { Code=0xe9,Command=0x0,Parameter=0,FParameter=0} },
      { "Read A", new CommandStruct { Code=0xea,Command=0x0,Parameter=0,FParameter=0} },
      { "Read B", new CommandStruct { Code=0xeb,Command=0x0,Parameter=0,FParameter=0} },
      { "Read A Aspirate B", new CommandStruct { Code=0xec,Command=0x0,Parameter=0,FParameter=0} },
      { "Read B Aspirate A", new CommandStruct { Code=0xed,Command=0x0,Parameter=0,FParameter=0} },
      { "End Bead Read A", new CommandStruct { Code=0xee,Command=0x0,Parameter=0,FParameter=0} },
      { "End Bead Read B", new CommandStruct { Code=0xef,Command=0x0,Parameter=0,FParameter=0} },
      { "Set Aspirate Volume", new CommandStruct { Code=0xaf,Command=0x0,Parameter=0,FParameter=0}},
      { "Set Property", new CommandStruct {Code=0, Command= 0x02, Parameter=0,FParameter=0  } },
      { "Get Property", new CommandStruct {Code=0, Command= 0x01, Parameter=0,FParameter=0  } },
      { "Set FProperty", new CommandStruct {Code=0, Command= 0x02, Parameter=0,FParameter=0  } },
      { "Get FProperty", new CommandStruct {Code=0, Command= 0x01, Parameter=0,FParameter=0  } }
    };
    private readonly List<byte> Readertab = new List<byte>() { 0xaa, 0xac, 0xaf, 0xa9, 0xab, 0xa8, 0xc2, 0xc4, 0xcd, 0xce, 0xcf };
    private readonly List<byte> Reportingtab = new List<byte>() { 0x10, 0x12, 0x13, 0x20, 0x22, 0x24, 0x25, 0x26, 0x28, 0x29, 0x2a, 0x2c, 0x2d,
      0x2e, 0xc0, 0xc1, 0xc2, 0xc8, 0xc9, 0x80, 0x82, 0x84, 0x86, 0x88, 0x8a };
    private readonly List<byte> Calibtab = new List<byte>() { 0x20, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c,
      0x3d, 0x3e, 0x3f, 0x87, 0x88, 0x89, 0x8a, 0x8b, 0x8c, 0x8d, 0x8e, 0x8f, 0xcd, 0xce, 0xcf };
    private readonly List<byte> Channeltab = new List<byte>() { 0x24, 0x25, 0x26, 0x28, 0x29, 0x2a, 0x2c, 0x2d, 0x2e, 0x2f,0x93,0x94,0x95,0x96,0x98,0x99,0x9a,0x9b, 0x9c, 0x9d, 0x9e, 0x9f, 0xa0, 0xa1, 0xa2, 0xa3, 0xa4, 0xa5 ,0xa6,0xa7,0x80,0x81,0x82,0x83,0x84,0x85
        ,0xb0,0xb1,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb8,0xb9,0xba,0xbb,0xbc,0xbd,0xbe, 0x02};
    private readonly List<byte> Motorstab = new List<byte>() { 0x41, 0x42, 0x43, 0x44, 0x48, 0x4a, 0x46, 0x4c, 0x4e, 0x51, 0x52, 0x53, 0x54,
      0x56, 0x58, 0x5a, 0x5c, 0x5e, 0x61, 0x62, 0x63, 0x64, 0x66, 0x68, 0x6a, 0x6c, 0x6e, 0xa8, 0x1c, 0x1d, 0x1e, 0x1a, 0x90, 0x91, 0x92, 0x16 };
    private readonly List<byte> Componentstab = new List<byte>() { 0x10, 0x11, 0x12, 0x13, 0x14, 0x16, 0x17, 0x18, 0xc0, 0xc7, 0xc8, 0xc9 };
    private readonly List<int[,]> Bitmaplist = new List<int[,]>
    {
        new int[9,2] { { 5, 6 },{3,1 }, { 4, 0 }, { 5, 1 }, { 6, 1 }, { 6, 1 }, { 5, 2 }, { 3, 0 },{ 0, 0 }, },
        new int[13,2] { { 6, 6 },{4,0 }, { 5, 0 }, { 6, 1 }, { 6, 1 }, { 6, 0 }, { 7, 1 }, { 7, 1 }, { 7, 2 }, { 5, 1 }, { 4, 1 }, { 4, 0 }, { 0, 0 }, },
        new int[14,2] { { 6, 5 },{5,1 }, { 5, 0 }, { 7, 0 }, { 8, 1 }, { 8, 0 }, { 9, 1 }, { 8, 1 }, { 7, 0 }, { 8, 1 }, { 8, 1 }, { 7, 2 }, { 5, 0 }, { 0, 0 }, },
        new int[29,2] { { 15, 6 },{8,0 }, { 8, 1 }, { 7, 0 }, { 7, 0 }, { 7, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 9, 0 }, { 9, 0 }, { 9, 1 }, { 8, 0 }, { 9, 0 }, { 9, 0 }, { 9, 1 },
            { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 1 }, { 6, 2 }, { 4, 0 }, { 0, 0 },  },
    };
    private const string InterfaceGuid = "F70242C7-FB25-443B-9E7E-A4260F373982"; // interface GUID, not device guid
    private const string Bheader = "Preamble,Time(1 us Tick),FSC bg,Viol SSC bg,CL0 bg,CL1 bg,CL2 bg,CL3 bg,Red SSC bg,Green SSC bg," +
            "Green Maj bg, Green Min bg,Green Major,Green Minor,Red-Grn Offset,Grn-Viol Offset,Region,Forward Scatter,Violet SSC,CL0," +
            "Red SSC,CL1,CL2,CL3,Green SSC,Reporter\r ";
    private const string Sheader = "Row,Col,Region,Bead Count,Median FI,Trimmed Mean FI,CV%\r";
    private const int BeadsToGraph = 2000;

    public MicroCyDevice()
    {
      USBDeviceInfo[] di = USBDevice.GetDevices(InterfaceGuid);   // Get all the MicroCy devices connected
      try
      {
        MicroCyUSBDevice = new USBDevice(di[0].DevicePath);     // just grab the first one for now, but should support multiples
        Console.WriteLine(string.Format("{0}:{1}", MicroCyUSBDevice.Descriptor.FullName, MicroCyUSBDevice.Descriptor.SerialNumber));
        _instrumentConnected = true;
      }
      catch { }
      SetSystemDirectories();
      LoadMaps();
      SetProperty(1, 1);    //set version as 1 to enable work order handling
      _actPriIdx = 1;  //cl1;
      _actSecIdx = 2;  //cl2;
      Reg0stats = false;
      CalStats = false;
      Newmap = false;
      MinPerRegion = 100;
      BeadsToCapture = 500;
      IsTube = false;
      InitReadPipe();
      TerminationType = 2;     //default termination is end of sample
      Outdir = RootDirectory.FullName;
    }



    #region Methods

    public void ClearMap()
    {
      Array.Clear(_map, 0, _map.Length);
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


    #region Main Commands
    public void SheathPump(byte cmd, ushort parameter)
    {
        string sCmd = "Sheath";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        cs.Command = cmd;
        cs.Parameter = parameter;
        RunCmd(sCmd, cs);
    }
    public void SampleAPump(byte cmd, ushort parameter)
    {
        string sCmd = "SampleA";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        cs.Command = cmd;
        cs.Parameter = parameter;
        RunCmd(sCmd, cs);
    }
    public void SampleBPump(byte cmd, ushort parameter)
    {
        string sCmd = "SampleB";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        cs.Command = cmd;
        cs.Parameter = parameter;
        RunCmd(sCmd, cs);
    }
    public void RefreshDac()
    {
        string sCmd = "RefreshDac";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);

    }
    public void SetNextWell()
    {
        string sCmd = "SetNextWell";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);
    }
    public void SetBaseline(byte cmd)
    {
        string sCmd = "SetBaseline";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        cs.Command = cmd;
        RunCmd(sCmd, cs);
    }
    public void SaveToFlash()
    {
        string sCmd = "SaveToFlash";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);
    }
    public void ReadFlash()
    {
        string sCmd = "Future1";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);
    }
    public void InitOpVars(byte cmd)
    {
        string sCmd = "InitOpVars";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        cs.Command = cmd;   //0 = normal, 1= use factory defaults
        RunCmd(sCmd, cs);
    }
    public void FlushQueue()
    {
        string sCmd = "FlushCmdQueue";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
       RunCmd(sCmd, cs);
    }
    public void StartSampling()
    {
        string sCmd = "Start Sampling";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);
    }
    public void EndSampling()
    {
        string sCmd = "End Sampling";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);
    }
    public void MotorAlign(byte cmd, int parameter)
    {
        string sCmd = "AlignMotor";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        cs.Command = cmd;
        cs.Parameter = 0;
        cs.FParameter = (float)parameter;
        RunCmd(sCmd, cs);
    }

    public void MotorX(byte cmd, int parameter)
    {
        string sCmd = "MotorX";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        cs.Command = cmd;
        cs.Parameter = 0;
        cs.FParameter = (float)parameter;
        RunCmd(sCmd, cs);
    }
    public void MotorY(byte cmd, int parameter)
    {
        string sCmd = "MotorY";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        cs.Command = cmd;
        cs.Parameter = 0;
        cs.FParameter = (float)parameter;
        RunCmd(sCmd, cs);
    }
    public void MotorZ(byte cmd, int parameter)
    {
        string sCmd = "MotorZ";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        cs.Command = cmd;
        cs.Parameter = 0;
        cs.FParameter = (float)parameter;
        RunCmd(sCmd, cs);
    }

    // COMPLEX COMMANDS***********************************************
   
    public void Startup()
    {
      string sCmd = "Startup";
      CommandStruct cs = MainCmdTemplatesDict[sCmd];
      RunCmd(sCmd, cs);
    }
    public void Prime()
    {
        string sCmd = "Prime";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);
        
    }
    public void MoveIdex()
    {
        string sCmd = "Idex";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        cs.Code = 0xd7;
        cs.Command = IdexPos;
        cs.Parameter = IdexSteps;
        cs.FParameter = IdexDir;   //currently checked is direction high on driver ic
        RunCmd(sCmd, cs);

    }
    public void SheathEmptyPrime()
    {
        string sCmd = "Sheath Empty Prime";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);
    }
    public void WashA()
    {
        string sCmd = "Wash A";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);
    }
    public void WashB()
    {
        string sCmd = "Wash B";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);
    }
    public void EjectPlate()
    {
        string sCmd = "Eject Plate";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);
    }
    public void LoadPlate()
    {
        string sCmd = "Load Plate";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);
    }
    public void Position()  //next position is set in properties 0xad and 0xae
    {
        string sCmd =  "Position Well Plate";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);
    }

    public void AspirateA()
    {
        string sCmd =  "Aspirate Syringe A" ;
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);
    }
    public void AspirateB()
    {
        string sCmd = "Aspirate Syringe B";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);
    }
    public void ReadA()
    {
        string sCmd = "Read A";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        _readingA = true;
        RunCmd(sCmd, cs);
    }
    public void ReadB()
    {
        string sCmd = "Read B";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        _readingA = false;
        RunCmd(sCmd, cs);
    }
    public void ReadA_AspirB()
    {
        string sCmd = "Read A Aspirate B";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        _readingA = true;
        RunCmd(sCmd, cs);
    }
    public void ReadB_AspirA()
    {
        string sCmd = "Read B Aspirate A";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        _readingA = false ;
       RunCmd(sCmd, cs);
    }
    public void EndReadA()
    {
        string sCmd = "End Bead Read A";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);
    }
    public void EndReadB()
    {
        string sCmd = "End Bead Read B";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        RunCmd(sCmd, cs);
    }
    public void SetProperty(byte code, ushort parameter)
    {
        string sCmd = "Set Property";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        cs.Code = code;
        cs.Parameter = parameter;
        RunCmd(sCmd, cs);
    }
    public void SetFProperty(byte code, float fparameter)
    {
        string sCmd = "Set FProperty";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        cs.Code = code;
        cs.FParameter = fparameter;
        RunCmd(sCmd, cs);
    }
    public void GetProperty(byte code)
    {
        string sCmd = "Get Property";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        cs.Code = code;
        RunCmd(sCmd, cs);
    }
    public void GetFProperty(byte code)
    {
        string sCmd = "Get FProperty";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        cs.Code = code;
        RunCmd(sCmd, cs);
    }
    public void GetFProperty(byte code, UInt16 parameter)
    {
        string sCmd = "Get FProperty";
        CommandStruct cs = MainCmdTemplatesDict[sCmd];
        cs.Parameter = parameter;
        cs.Code = code;
        RunCmd(sCmd, cs);
    }

    public void InitializePlate(int idx)
    {
        switch (idx)
        {
            case 0:
                {
                    break;
                }
            case 1:
                {
                    break;
                }
            case 2:
                {
                    break;
                }
        }
    }
    public void InitSTab(string tabname)
    {
        switch (tabname)
        {
            case "readertab":
                {
                    foreach (byte code in Readertab)
                    {
                        GetProperty(code);
                    }

                    break;
                }
            case "reportingtab":
                {
                    foreach (byte code in Reportingtab)
                    {
                        GetProperty(code);
                    }

                    break;
                }
            case "calibtab":
                {
                    foreach (byte code in Calibtab)
                    {
                        GetProperty(code);
                    }

                    break;
                }
            case "channeltab":
                {
                    foreach (byte code in Channeltab)
                    {
                        GetProperty(code);
                    }

                    break;
                }
            case "motorstab":
                {
                    foreach (byte code in Motorstab)
                    {
                        GetProperty(code);
                    }

                    break;
                }
            case "componentstab":
                {
                    foreach (byte code in Componentstab)
                    {
                        GetProperty(code);
                    }

                    break;
                }
        }

    }
    #endregion Main Commands

    public void ConstructMap(CustomMap mmap)
    {
      //build classification map from ActiveMap using bitfield types A-D
      int row, irow, col, jcol,begidx,begidy,endidx,endidy;
      float xwidth,ywidth,cl2;
      int[,] bitpoints = new int[32, 2];
      _actPriIdx = (byte)mmap.midorderidx; //what channel cl0 - cl3?
      _actSecIdx = (byte)mmap.loworderidx;
      SetProperty(0xce, mmap.minmapssc);  //set ssc gates for this map
      SetProperty(0xcf, mmap.maxmapssc);
      Array.Clear(ClassificationMap, 0, ClassificationMap.Length);
      foreach (BeadRegion mapRegions in mmap.mapRegions)
      {
        if (!mapRegions.isvector)       //this region shape is taken from Bitmaplist array
        {
          Array.Clear(bitpoints, 0, bitpoints.Length);  //copy bitmap of the type specified (A B C D)
          Array.Copy(Bitmaplist[mapRegions.bitmaptype], bitpoints, Bitmaplist[mapRegions.bitmaptype].Length);
          row = mapRegions.centermidorderidx - bitpoints[0, 0];  //first position is value to backup before etching bitmap
          col = mapRegions.centerloworderidx - bitpoints[0, 1];
          irow = 1;
          while (bitpoints[irow, 0] != 0)
          {
            for (jcol = col; jcol < (col + bitpoints[irow, 0]); jcol++)
            {
              //handle region overlap by making overlap 0
              if (ClassificationMap[row + irow - 1, jcol] == 0)
              {
                ClassificationMap[row + irow - 1, jcol] = mapRegions.regionNumber;
              }
              //TODO: else seems useless,since ClassificationMap is zeroed in Array.Clear();
              else
              {
                ClassificationMap[row + irow - 1, jcol] = 0;
              }
            }
            col += bitpoints[irow, 1];  //second position is right shift amount for next line in map
            irow++;
          }
        }
        else
        {
          //populate a computed region
          cl2 = (float)mapRegions.meanloworder;
          xwidth = (float) 0.33 * mapRegions.meanmidorder + 50;
          ywidth = (float)0.33 * mapRegions.meanloworder + 50;

          begidx = Val_2_Idx((int)(mapRegions.meanmidorder - (xwidth * 0.5)));
          endidx = Val_2_Idx((int) (mapRegions.meanmidorder + (xwidth * 0.5 )));
          float xincer = 0;
          begidy = Val_2_Idx((int)(mapRegions.meanloworder - (ywidth /2)));
          endidy = Val_2_Idx((int)(mapRegions.meanloworder + (ywidth/2 ) ));
          if (begidx < 3) begidx = 3;
          if (begidy < 3) begidy = 3;
          for (irow = begidx; irow <= endidx; irow++)
          {
            for (jcol = begidy+(int)xincer; jcol <= endidy+(int)xincer; jcol++)
            {
              if (ClassificationMap[irow, jcol] == 0)
                ClassificationMap[irow, jcol] = mapRegions.regionNumber;
              else
                //TODO: else seems useless,since ClassificationMap is zeroed in Array.Clear();
                ClassificationMap[irow, jcol] = 0;  //zero any overlaps
            }
          }
        }
      }
      Newmap = true;
    }

    private void InitBeadRead(byte rown, byte coln)
    {
      //open file
      //first create uninique filename
      ushort colnum = (ushort)coln;    //well names are relative to 1
      if (!IsTube)
        colnum++;  //use 0 for tubes and true column for plates
      char rowletter = (char)(0x41 + rown);
      if (!Directory.Exists(Outdir))
        Outdir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      for (var differ = 0; differ < 100; differ++)
      {
        _fullFileName = Outdir + "\\" + Outfilename + rowletter + colnum.ToString() + '_' + differ.ToString()+".csv";
        if (!File.Exists(_fullFileName))
          break;
      }
      _dataout.Clear();
      _dataout.Append(Bheader);
      _chkRegCnt = false;
      BeadCount = 0;
    }

    public void SaveBeadFile() //cancels the begin read from endpoint 2
    {
      //write file
      char[] alphabet = Enumerable.Range('A', 16).Select(x => (char)x).ToArray();
      string bgfullFileName = _fullFileName;  //save name in bg process cause it gets changed in endstate 5
      string bgsummaryFileName = PrepareSummaryFile();
      List<WellResults> bgwellresults = _wellResults.ToList();
      string bgdataout = _dataout.ToString();
      byte bgreadingrow = ReadingRow;
      byte bgreadingcol = ReadingCol;
      Console.WriteLine(string.Format("{0} Reporting Background results cloned for save", DateTime.Now.ToString()));
      if ((bgfullFileName != null) & (Everyevent))
      {
        File.WriteAllText(bgfullFileName, _dataout.ToString());
      }
      if (RMeans)
      {
        PlateReport.rpWells.Add(new WellReport { prow= WellsInOrder[SavingWellIdx].rowIdx, pcol= WellsInOrder[SavingWellIdx].colIdx });
        foreach (WellResults regionNumber in bgwellresults)
        {
          OutResults rout = new OutResults();
          if (SavingWellIdx > (WellsInOrder.Count - 1)) SavingWellIdx = WellsInOrder.Count - 1;
          rout.row = alphabet[WellsInOrder[SavingWellIdx].rowIdx].ToString();
          rout.col = WellsInOrder[SavingWellIdx].colIdx + 1;    //columns are 1 based
          rout.count = regionNumber.RP1vals.Count();
          rout.region = regionNumber.regionNumber;
          if (rout.count > 2)
          {
            rout.meanfi = regionNumber.RP1vals.Average();
          }
          if (rout.count >= 20)
          {
            regionNumber.RP1vals.Sort();
            float rpbg = regionNumber.RP1bgnd.Average() * 16;
            int quarter = rout.count / 4;
            regionNumber.RP1vals.RemoveRange(rout.count - quarter , quarter);
            regionNumber.RP1vals.RemoveRange(0, quarter);
            rout.meanfi = regionNumber.RP1vals.Average();
            double sumsq = regionNumber.RP1vals.Sum(dataout => Math.Pow(dataout - rout.meanfi, 2));
            double stddev = Math.Sqrt(sumsq / regionNumber.RP1vals.Count() - 1);
            rout.cv = (float)stddev / rout.meanfi*100;
            if (double.IsNaN(rout.cv)) rout.cv = 0;
            rout.medfi = (float) Math.Round(regionNumber.RP1vals[quarter] - rpbg);
            rout.meanfi -= rpbg;
          }
          _summaryout.Append(rout.ToString());
//          if (SubtRegBg == true) rout.meanfi -= ActiveMap.mapRegions[regionNumber.regionNumber].meanrp1bg;
      
          PlateReport.rpWells[SavingWellIdx].rpReg.Add(new RegionReport(){ region = regionNumber.regionNumber,
            count = (uint)rout.count, medfi = rout.medfi, meanfi = rout.meanfi, coefVar = rout.cv }); 
        }
        if ((SavingWellIdx == WellsToRead)& (_summaryout.Length>0) & (RMeans==true) )  //end of read session (plate, plate section or tube) write summary stat file
        {
          File.WriteAllText(bgsummaryFileName, _summaryout.ToString());
        }
        if ((SavingWellIdx == WellsToRead) & (_summaryout.Length > 0) & (PltRept == true))    //end of read and json results requested
        {

          string rfilename = SystemControl == 0 ? Outfilename : WorkOrder.plateID.ToString();
          string resultfilename = Path.Combine(RootDirectory.FullName,"Result", rfilename); 
          TextWriter jwriter = null;
          try
          {
            var jcontents = JsonConvert.SerializeObject(PlateReport);   
            jwriter = new StreamWriter(resultfilename+".json");
            jwriter.Write(jcontents);
            if (File.Exists(_workOrderPath)) File.Delete(_workOrderPath);   //result is posted, delete work order
          }
          finally
          {
            if (jwriter != null)
              jwriter.Close();
          }
        }
      }
      if ((CalStats) & (SavBeadCount > 2))
      {
        double sumit = 0;
        double sumsq = 0;
        double mean = 0;
        double stddev = 0;
        double min;
        double max;
        double[] robustcnt = new double[10];
        if (SavBeadCount > 5000)
          SavBeadCount = 5000;
        for (int finx = 0; finx < 10; finx++)
        {
          for (int beads = 0; beads < SavBeadCount; beads++)
          {
            sumit += _sfi[beads, finx];
          }
          robustcnt[finx] = SavBeadCount; //start with total bead count
          mean = sumit / SavBeadCount;
          //find high and low bounds
          min = mean * 0.5;
          max = mean * 2;
          sumit = 0;
          for (int beads = 0; beads < SavBeadCount; beads++)
          {
            if ((_sfi[beads, finx] > min) & (_sfi[beads, finx] < max))
            {
              sumit += _sfi[beads, finx];
            }
            else
            {
              _sfi[beads, finx] = 0;
              robustcnt[finx]--;
            }
          }
          mean = sumit / robustcnt[finx];
          for (int beads = 0; beads < SavBeadCount; beads++)
          {
            if (_sfi[beads, finx] == 0)
              continue;
            sumsq += Math.Pow(mean - _sfi[beads, finx], 2);
          }
          stddev = Math.Sqrt(sumsq / (robustcnt[finx] - 1));
          GStats[finx].mfi = mean;
          GStats[finx].cv = (stddev / mean) * 100;
          if (double.IsNaN(GStats[finx].cv))
            GStats[finx].cv = 0;
          sumit = 0;
          sumsq = 0;
        }
        NewStats = true;
      }
      Console.WriteLine(string.Format("{0} Reporting Background File Save Complete", DateTime.Now.ToString()));
    }

        public void SetReadingParamsForWell(int idx)
        {

            SetProperty(0xaa, (ushort)WellsInOrder[idx].runSpeed);
            SetProperty(0xc2, (ushort)WellsInOrder[idx].chanConfig);
            BeadsToCapture = WellsInOrder[idx].termCnt;
            MinPerRegion = WellsInOrder[idx].regTermCnt;
            TerminationType = WellsInOrder[idx].termType;
            //            ConstructMap(WellsInOrder[idx].thisWellsMap);
            ConstructMap(ActiveMap);
            _wellResults.Clear();
            foreach (BeadRegion mapRegions in WellsInOrder[idx].thisWellsMap.mapRegions)
            {
                bool isInMap = ActiveMap.mapRegions.Any(xx => mapRegions.regionNumber == xx.regionNumber);
                if (isInMap==false)
                    Console.WriteLine(string.Format("{0} No Map for Work Order region {1}", DateTime.Now.ToString(),mapRegions.regionNumber));
                if ((mapRegions.isActive) & (isInMap))
                {
                    WellResults actreg = new WellResults();
                    actreg.regionNumber = mapRegions.regionNumber;
                    _wellResults.Add(actreg);
                }
            }
            if (Reg0stats == true)
            {
                WellResults actreg = new WellResults();
                actreg.regionNumber = 0;
                _wellResults.Add(actreg);

            }
        }

    public void SetAspirateParamsForWell(int idx)
    {
      SetProperty(0xad, (ushort)WellsInOrder[idx].rowIdx);
      SetProperty(0xae, (ushort)WellsInOrder[idx].colIdx);
      SetProperty(0xaf, (ushort)WellsInOrder[idx].sampVol);
      SetProperty(0xac, (ushort)WellsInOrder[idx].washVol);
      SetProperty(0xc4, (ushort)WellsInOrder[idx].agitateVol);
      PlateRow = (byte)WellsInOrder[idx].rowIdx;
      PlateCol = (byte)WellsInOrder[idx].colIdx;
    }

    public void InitReadPipe()
    {
      if (_instrumentConnected)
        _ = MicroCyUSBDevice.Interfaces[0].Pipes[0x81].BeginRead(_usbInputBuffer, 0, _usbInputBuffer.Length, new AsyncCallback(ReplyFromMC), null);
    }

    private void ReplyFromMC(IAsyncResult result)
    {
      byte jj, cl1, cl2;
      int xx, yy, zz, savx, savy;
      byte [] mapx = new byte [70000];
      float[] cl = new float[4];
      float grp1;
      double sidesc, sidsel;
      float cl1comp,cl2comp;

      MicroCyUSBDevice.Interfaces[0].Pipes[0x81].EndRead(result);
      if ((_usbInputBuffer[0] == 0xbe) && (_usbInputBuffer[1] == 0xad))
      {
        for (jj = 0; jj < 8; jj++)
        {
          BeadInfoStruct outbead = new BeadInfoStruct();
          outbead = BeadArrayToStruct<BeadInfoStruct>(_usbInputBuffer, jj);
          if (outbead.Header != 0xadbeadbe)
            break;
          cl1comp = outbead.greenB * Compensation / 100;
          cl2comp = (float)((float) cl1comp * 0.26);
          cl[0] = outbead.cl0;
          cl[1] = outbead.cl1 -cl1comp;   //Compensation
          outbead.cl1 = cl[1];
          cl[2] = outbead.cl2 - cl2comp;
          outbead.cl2 = cl[2];
          cl[3] = outbead.cl3;
          xx = (byte)(Math.Log(cl[_actPriIdx]) * 24.526);
          yy = (byte)(Math.Log(cl[_actSecIdx]) * 24.526);
          if ((xx > 0) & (yy > 0))
          {
            outbead.region = ClassificationMap[xx,yy];    //each well can have a different  classification map
          }
          else
            outbead.region = 0;

          //handle HI dnr channel
          //outbead.reporter = outbead.greenC;
          if (outbead.greenC > HdnrTrans)
              outbead.reporter = outbead.greenB * HDnrCoef;
          else
            outbead.reporter = outbead.greenC ;
//          accumrpbg += outbead.greenC;
          // if pcreg exists in wellsresults, add rp1 value to list
          //                    if (_wellResults.Any(w => w.regionNumber == pcreg))

          //_wellResults is a list of region numbers that are active
          //each entry has a list of rp1 values from each bead in that reagion
          int index = _wellResults.FindIndex(w => w.regionNumber == outbead.region);
          if (index>=0)
          {
            _wellResults[index].RP1vals.Add(outbead.reporter);   //
            _wellResults[index].RP1bgnd.Add(outbead.greenC_bg);
            if (_wellResults[index].RP1vals.Count == MinPerRegion)
            {
              _chkRegCnt = true;  //see if assay is done via sufficient beads in each region
            }
          }


          if (outbead.region == 0)
          {
            if (OnlyClassified)
              continue;
          }
          
          if ((cl[_actSecIdx] > 0) & (cl[_actPriIdx] > 0)& (CreateMapState == 1))
          {
            _map[xx, yy]++;
//            if (map[xx, yy] == 250) CreateMapState = 2; //we have enough to end map build
            // TODO if calc peak box checked in add region activity, check if 10k beads have been read, if so set state to 3
            if(BeadCntMapCreate++ > SampleSize)
              CreateMapState = 3;
          }
          if (BeadCount < BeadsToGraph)
          {
            CLQueue newdp = new CLQueue();
            switch (XAxisSel)
            {
              case 0:
                newdp.xyclx = (UInt32)outbead.cl0;
                break;
              case 1:
                newdp.xyclx = (UInt32)outbead.cl1;
                break;
              case 2:
                newdp.xyclx = (UInt32)outbead.cl2;
                break;
              default:
                newdp.xyclx = (UInt32)outbead.cl3;
                break;
            }
            switch (YAxisSel)
            {
              case 0:
                newdp.xycly = (UInt32)outbead.cl0;
                break;
              case 1:
                newdp.xycly = (UInt32)outbead.cl1;
                break;
              case 2:
                newdp.xycly = (UInt32)outbead.cl2;
                break;
              default:
                newdp.xycly = (UInt32)outbead.cl3;
                break;
            }
            switch (SscSelected)
                        {
                          case 0:
                              {
                                sidsel = outbead.fsc;
                                break;
                              }
                          case 1:
                              {
                                sidsel = outbead.violetssc;
                                break;
                              }
                          case 2:
                              {
                                sidsel = outbead.greenssc;
                                break;
                              }
                          default:
                              {
                                sidsel = outbead.redssc;
                                break;
                              }
                        }
            sidesc = Math.Log(sidsel) * 24.526;
            SscData[(byte)sidesc]++;
            grp1 = outbead.reporter;
            if (grp1 > 32000) grp1 = 32000; //don't let graph overflow
            Rp1Data[(byte)(Math.Log(grp1) * 24.526)]++;
            try
            {
              if ((newdp.xyclx > 1) && (newdp.xycly > 1))
              {
                lock (ClData)
                { ClData.Enqueue(newdp); }    //save data for graphing 
              }
            }
            finally { }
          }
          if (outbead.region > 0)
          {
            if (0 == MinPerRegion)  //do the check whenever and region gets to min  //TODO: legacy had some useless check. Probably not necessary if block
            {
              _chkRegCnt = true;
            }
          }
          if(Everyevent)
            _dataout.Append(outbead.ToString());
          //accum stats for run as a whole, used during aligment and QC
          if ((CalStats) & (BeadCount < 5000))
          {
            _sfi[BeadCount,6] = outbead.cl3;
            _sfi[BeadCount,3] = outbead.redssc;
            _sfi[BeadCount,4] = outbead.cl1;
            _sfi[BeadCount,5] = outbead.cl2;
            _sfi[BeadCount,0] = outbead.greenssc;
            _sfi[BeadCount,1] = outbead.greenB;
            _sfi[BeadCount,2] = outbead.greenC;
            _sfi[BeadCount,7] = outbead.violetssc;
            _sfi[BeadCount,8] = outbead.cl0;
            _sfi[BeadCount,9] = outbead.fsc;
          }
          BeadCount++;
        }
        if (CreateMapState == 2)
        {
          zz = 0;
          Array.Clear(mapx, 0, 70000);
          for (xx = 0; xx <256; xx++) //linearize the array
          {
            for (yy = 0; yy <256; yy++)
            {
              cl1 = (byte)xx;
              cl2 = (byte)yy;
              mapx[zz++] = _map[cl1, cl2];
            }
          }
          string mapdir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); //TODO: probably not necessary
          File.WriteAllBytes(mapdir + "\\" + "mapcounter.bin", mapx);
          CreateMapState = 0;
        }
        if (CreateMapState == 3)
            {
                zz = 0; savx = 0;savy = 0;
                for (xx = 0; xx < 256; xx++) //find peak
                {
                    for (yy = 0; yy < 256; yy++)
                    {
                        if (_map[xx, yy] > zz)
                        {
                            zz = _map[xx, yy];
                            savx = xx;
                            savy = yy;
                        }
                    }
                }
                MapPeakX = (float)Math.Exp(savx / 24.526);
                MapPeakY = (float)Math.Exp(savy / 24.526);
//                avgrpbg = accumrpbg / BeadCount;
                CreateMapState = 0;
                
                //find region peak and fill in text boxes on new region tab
            }
        Array.Clear(_usbInputBuffer, 0, _usbInputBuffer.Length);
        switch (TerminationType)
        {
          case 0: //min beads in each region
            //do statistical magic
            if (_chkRegCnt)  //a region made it, are there more that haven't
            {
              EndState = 1;   //assume all region have enough beads
              foreach (WellResults regionNumber in _wellResults)
              {
                if (regionNumber.RP1vals.Count() < MinPerRegion)
                {
                  EndState = 0;   //not done yet
                  break;
                }
              }
              _chkRegCnt = false;
            }
            break;
          case 1: //total beads captured
            if ((BeadCount >= BeadsToCapture) & ReadActive)
            {
              EndState = 1;
              ReadActive = false;
            }
            break;
          case 2: //end of sample 
            break;
        }
      }
      else
      {
        CommandStruct newcmd;
        lock (Commands)
        {
          // move received command to queue
          newcmd = ByteArrayToStruct<CommandStruct>(_usbInputBuffer);
          Commands.Enqueue(newcmd);
        }
        if ((newcmd.Code >= 0xd0) & (newcmd.Code <= 0xdf))
        {
          Console.WriteLine(string.Format("{0} E-series script [{1}]", DateTime.Now.ToString(), newcmd.ToString()));
        }
        else if (newcmd.Code > 0)
        {
          Console.Out.WriteLine(string.Format("{0} Received [{1}]", DateTime.Now.ToString(), newcmd.ToString()));
        }
      }
      _ = MicroCyUSBDevice.Interfaces[0].Pipes[0x81].BeginRead(_usbInputBuffer, 0, _usbInputBuffer.Length, new AsyncCallback(ReplyFromMC), null);
    }

    /// <summary>
    /// Sends a command OUT to the USB device, then checks the IN pipe for a return value.
    /// </summary>
    /// <param name="sCmdName">A friendly name for the command.</param>
    /// <param name="cs">The CommandStruct object containing the command parameters.  This will get converted to an 8-byte array.</param>
    private void RunCmd (string sCmdName, CommandStruct cs)
    {
      if (_instrumentConnected)
      {
        byte[] buffer = StructToByteArray(cs);
        //  1: MOVE DUE TO INSTR                Console.WriteLine(string.Format("{0} Sending [{1}]: {2}", DateTime.Now.ToString(), sCmdName, cs.ToString()));
        MicroCyUSBDevice.Interfaces[0].OutPipe.Write(buffer);
      }
      Console.WriteLine(string.Format("{0} Sending [{1}]: {2}", DateTime.Now.ToString(), sCmdName, cs.ToString())); //  MARK1 END
    }

    /// <summary>
    /// Converts an object / struct to a byte array
    /// </summary>
    /// <param name="obj">The object to convert.</param>
    /// <returns>The converted object</returns>
    static byte[] StructToByteArray(object obj)
    {
      int len = Marshal.SizeOf(obj);
      byte[] arrRet = new byte[len];

      IntPtr ptr = Marshal.AllocHGlobal(len);
      Marshal.StructureToPtr(obj, ptr, true);
      Marshal.Copy(ptr, arrRet, 0, len);
      Marshal.FreeHGlobal(ptr);

      return arrRet;
    }

    static CommandStruct ByteArrayToStruct<CommandStruct>(byte[] inmsg)
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

    static BeadInfoStruct BeadArrayToStruct<BeadInfoStruct>(byte[] beadmsg, byte kk)
    {
      IntPtr ptr = Marshal.AllocHGlobal(64);
      try
      {
        Marshal.Copy(beadmsg, kk * 64, ptr, 64);
        return (BeadInfoStruct)Marshal.PtrToStructure(ptr, typeof(BeadInfoStruct));
      }
      finally
      {
        Marshal.FreeHGlobal(ptr);
      }
    }

    public void LoadMaps()
    {
      //read the MapList if available
      string testfilename;
      testfilename = Path.Combine(RootDirectory.FullName, "Config", "DimensionMaps.txt");
      if (File.Exists(testfilename))
          _mapsFileName = testfilename;
      else _mapsFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DimensionMaps.txt");
      //TODO: if check seems unnecessary
      if (File.Exists(_mapsFileName))
      {
        TextReader reader = null;
        try
        {
          reader = new StreamReader(_mapsFileName);
          var fileContents = reader.ReadToEnd();
          MapList = JsonConvert.DeserializeObject<List<CustomMap>>(fileContents);
        }
        finally
        {
          if (reader != null) reader.Close();
        }
      }
      _mapsFileName = testfilename;    //make sure maps is written into new dir struct on first save
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
      if (WorkOrder.numberCols == 12)
        PlateType = 0;
      else
        PlateType = 1;
      // send well depth once that is worked out
      return true;
    }

    //Refactored

    private string PrepareSummaryFile() //  DEBUGINFO: in Legacy was int BtnRead_Click
    {
      string summaryFileName = "";
      for (var i = 0; i < 1000; i++)
      {
        summaryFileName = Outdir + "\\" + "Results__" + Outfilename + '_' + i.ToString() + ".csv";
        if (!File.Exists(summaryFileName))
          break;
      }
      _ = _summaryout.Clear();
      _ = _summaryout.Append(Sheader);
      return summaryFileName;
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
      MapList[idx].calrpmin = TemprpMin;
      MapList[idx].calrpmaj = TemprpMaj;
      MapList[idx].calrssc = TempRedSsc;
      MapList[idx].calgssc = TempGreenSsc;
      MapList[idx].calvssc = TempVioletSsc;
      MapList[idx].calcl0 = TempCl0;
      MapList[idx].calcl1 = TempCl1;
      MapList[idx].calcl2 = TempCl2;
      MapList[idx].calcl3 = TempCl3;
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
        EndReadA();         //sends EE to instrument
      else
        EndReadB();    //send EF to instrument
      WellReset();
    }

    private void WellReset()
    {
      CurrentWellIdx++;
      if (CurrentWellIdx <= WellsToRead)  //are there more to go
      {
        SetReadingParamsForWell(CurrentWellIdx);
        if (_readingA)
        {
          if (CurrentWellIdx < WellsToRead)   //more than one to go
          {
            SetAspirateParamsForWell(CurrentWellIdx + 1);
            ReadB_AspirA();
          }
          else if (CurrentWellIdx == WellsToRead)
          {
            ReadB();
          }
        }
        else
        {
          if (CurrentWellIdx < WellsToRead)
          {
            SetAspirateParamsForWell(CurrentWellIdx + 1);
            ReadA_AspirB();
          }
          else if (CurrentWellIdx == WellsToRead)
          {
            //handle end of plate things
            ReadA();
          }
        }
        InitBeadRead(ReadingRow, ReadingCol);   //gets output file redy
      }
      else
      {
        //do end of run things
        ReadActive = false; //  DEBUGINFO: was not in Legacy
      }
    }

    private static int Val_2_Idx(int val)
    {
      int retidx;
      retidx = (int)(Math.Log(val) * 24.526);
      return retidx;
    }
    #endregion Methods
  }
}
