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
        public UInt32 Header;
        public UInt32 EventTime;

        public byte fsc_bg;
        public byte vssc_bg;
        public byte cl0_bg;
        public byte cl1_bg;
        public byte cl2_bg;
        public byte cl3_bg;
        public byte rssc_bg;
        public byte gssc_bg;

        public UInt16 greenB_bg;    //measured background used in compensation 
        public UInt16 greenC_bg;
        public UInt16 greenB;   //ADC measurement of optical split on RP1
        public UInt16 greenC;

        public byte l_offset_rg;    //samples between green and red peak
        public byte l_offset_gv;    //samples between green and violet peak
        public UInt16 region;
        public float fsc;       //Forward Scatter

        public float violetssc; //Violet A
        public float cl0;       //Violet B

        public float redssc;    //Red D
        public float cl1;       //Red C

        public float cl2;       //Red B
        public float cl3;       //Red A

        public float greenssc;
        public float reporter;  //computed from Green B and Green C

        public override String ToString()   //setup for csv output
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25}\r"
                            , Header, EventTime, fsc_bg, vssc_bg, cl0_bg, cl1_bg, cl2_bg, cl3_bg
                            , rssc_bg, gssc_bg,  greenB_bg, greenC_bg, greenB, greenC
                             , l_offset_rg, l_offset_gv, region, fsc, violetssc, cl0, redssc, cl1, cl2, cl3, greenssc, reporter);
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
        public Int16 sampVol { get; set; }
        public Int16 washVol { get; set; }
        public Int16 agitateVol { get; set; }
        public int termCnt { get; set; }
        public int regTermCnt { get; set; }
        public CustomMap thisWellsMap = new CustomMap();
    }
    public unsafe struct CommandStruct
    {
        public byte Code;  // uInt8
        public byte Command;  // uInt8
        public UInt16 Parameter;
        public float FParameter;

        public override String ToString()
        {
            return string.Format("{0:X2},{1:X2},{2},{3:F1}", Code, Command, Parameter, FParameter);
        }
    }
    public unsafe struct CLQueue
    {
        public UInt32 xyclx;
        public UInt32 xycly;
        public UInt32 xycl2;
        public UInt32 xycl3;
        public UInt32 hrp1;
        public UInt32 gssc;
        public UInt32 rssc;
        public UInt32 vssc;
    }
    public class BRegion
    {
        public bool isActive { get; set; }
        public string rnum { get; set; }
    }
    #endregion STRUCTS
  
  #region ENUMS
    //public enum ReadSpeeds
    //{
    //    Normal=1,
    //    HighSpeed=2,
    //    HighSensitivity=3
    //}

    //public enum PlateTypes
    //{
    //    Plate96Well=1,
    //    Plate384Well=2
    //}
    //public enum syr_cmds
    //{
    //    Halt=0,
    //    Move_Abosolute=1,
    //    Pickup=2,
    //    Dispense=3,
    //    Set_Speed=4,
    //    Initialize=5,
    //    Boot=6,
    //    Valve_Left=7,
    //    Valve_Right=8
    //}
    //public enum Syringes
    //{
    //    A=1,
    //    B=2
    //}

    public enum USBRequestType
    {
        Standard = 0,
        Class = 1,
        Vendor = 2,
        Reserved = 3
    }
    public enum USBRequestType_Direction
    {
        HostToDevice=0,
        DeviceToHost=1
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
    //public enum Tab0Props
    //{

    //}
    #endregion ENUMS
  //public class BuildRows
  //{
  //    private int r_Index;
  //    private string r_value;
  
  //    public BuildRows(int index, string value)
  //    {
  //        r_Index = index;
  //        r_value = value;
  //    }
  //    public int Index
  //    {
  //       get { return r_Index; }     
  //    }
  //    public string Value
  //    {
  //        get { return r_value; }     
  //    }
  //}
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
  public class BeadRecord
    {
        public UInt32 Header { get; set; }
        public UInt32 EventTime { get; set; }

        public byte fsc_bg { get; set; }
        public byte vssc_bg { get; set; }
        public byte cl0_bg { get; set; }
        public byte cl1_bg { get; set; }
        public byte cl2_bg { get; set; }
        public byte cl3_bg { get; set; }
        public byte rssc_bg { get; set; }
        public byte gssc_bg { get; set; }

        public UInt16 greenB_bg { get; set; }    //measured background used in compensation 
        public UInt16 greenC_bg { get; set; }
        public UInt16 greenB { get; set; }   //ADC measurement of optical split on RP1
        public UInt16 greenC { get; set; }

        public byte l_offset_rg { get; set; }    //samples between green and red peak
        public byte l_offset_gv { get; set; }    //samples between green and violet peak
        public UInt16 region { get; set; }
        public float fsc { get; set; }       //Forward Scatter

        public float violetssc { get; set; } //Violet A
        public float cl0 { get; set; }      //Violet B

        public float redssc { get; set; }    //Red D
        public float cl1 { get; set; }      //Red C

        public float cl2 { get; set; }     //Red B
        public float cl3 { get; set; }      //Red A

        public float greenssc { get; set; }
        public float reporter { get; set; }  //computed from Green B and Green C

        public override String ToString()   //setup for csv output
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25:N0}\r"
                            , Header, EventTime, fsc_bg, vssc_bg, cl0_bg, cl1_bg, cl2_bg, cl3_bg
                            , rssc_bg, gssc_bg, greenB_bg, greenC_bg, greenB, greenC
                             , l_offset_rg, l_offset_gv, region, fsc, violetssc, cl0, redssc, cl1, cl2, cl3, greenssc, reporter);
        }

    }
  [Serializable]
  public class WorkOrder
    {
        public Guid plateID { get; set; }
        public Guid beadMapId { get; set; }
        public Int16 numberRows { get; set; }
        public Int16 numberCols { get; set; }
        public Int16 wellDepth { get; set; }
        public DateTime createDateTime { get; set; }        //date and time per ISO8601
        public DateTime scheduleDateTime { get; set; }
        public List<Wells> woWells = new List<Wells>();
    }//    [Serializable]
    //public class WorkOrderWells
    //{
    //    public UInt16 rowIdx { get; set; }
    //    public UInt16 colIdx { get; set; }
    //    public Int16 runSpeed { get; set; }
    //    public Int16 termType { get; set; }
    //    public Int16 chanConfig { get; set; }
    //    public Int16 sampVol { get; set; }
    //    public Int16 washVol { get; set; }
    //    public Int16 agitateVol { get; set; }
    //    public Int16 termCnt { get; set; }
    //    public Int16 regTermCnt { get; set; }
    //    public CustomMap thisWellsMap;

    //    public List<UInt16> regionsUsed = new List<UInt16>(12) { 6, 11, 12, 13, 17, 18, 24, 25, 32, 33, 41, 45 };
    //}
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
  public class ParamStats
    {
        public float[] sfi { get; set; }
    }
  public class Gstats
    {
        public double mfi { get; set; }
        public double cv { get; set; }
    }
  [Serializable]
  public class OutResults
    {
        public string row { get; set; }
        public int col { get; set; }
        public ushort region { get; set; }
        public int count { get; set; }
        public float medfi { get; set; }
        public float meanfi { get; set; }
        public float cv { get; set; }
        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4},{5:F3},{6:F1}\r"
                ,row,col,region,count,medfi,meanfi,cv);
        }
    }
  public class MicroCyDevice
    {
        // interface GUID, not device guid
        // [HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\USB\VID_072A&PID_0011\5&112bc8d3&0&9\Device Parameters]
        // "DeviceInterfaceGUID" = "{F70242C7-FB25-443B-9E7E-A4260F373982}"
    const string INTERFACE_GUID = "F70242C7-FB25-443B-9E7E-A4260F373982";
        //const string INTERFACE_GUID = "F72FE0D4-CBCB-407d-8814-9ED673D0DD6B";
    const string bheader = "Preamble,Time(1 us Tick),FSC bg,Viol SSC bg,CL0 bg,CL1 bg,CL2 bg,CL3 bg,Red SSC bg,Green SSC bg," +
            "Green Maj bg, Green Min bg,Green Major,Green Minor,Red-Grn Offset,Grn-Viol Offset,Region,Forward Scatter,Violet SSC,CL0," +
            "Red SSC,CL1,CL2,CL3,Green SSC,Reporter\r ";
    const string sheader = "Row,Col,Region,Bead Count,Median FI,Trimmed Mean FI,CV%\r";
    public string[] sync_elements = { "SHEATH", "SAMPLE_A", "SAMPLE_B", "FLASH", "END_WELL", "VALVES", "X_MOTOR", "Y_MOTOR", "Z_MOTOR", "PROXIMITY", "PRESSURE","WASHING","FAULT","ALIGN MOTOR","MAIN VALVE","SINGLE STEP"};
    public List <string> active_sync = new List<string>();
    public Queue<CommandStruct> commands = new Queue<CommandStruct>();
    public WorkOrder workOrder = new WorkOrder();
    public PlateReport plateReport = new PlateReport();
    public Queue<CLQueue> cldata = new Queue<CLQueue>();
    public List<Wells> wellsinorder = new List<Wells>();
    public List<Wells> wells = new List<Wells>();
    public List<CustomMap> maplist = new List<CustomMap>();
    public CustomMap activemap = new CustomMap();
    public List<WellResults> wellresults = new List<WellResults>();
    public List<Gstats> gstats = new List<Gstats>();
    public List<int[,]> bitmaplist = new List<int[,]>
    {
        new int[9,2] { { 5, 6 },{3,1 }, { 4, 0 }, { 5, 1 }, { 6, 1 }, { 6, 1 }, { 5, 2 }, { 3, 0 },{ 0, 0 }, },
        new int[13,2] { { 6, 6 },{4,0 }, { 5, 0 }, { 6, 1 }, { 6, 1 }, { 6, 0 }, { 7, 1 }, { 7, 1 }, { 7, 2 }, { 5, 1 }, { 4, 1 }, { 4, 0 }, { 0, 0 }, },
        new int[14,2] { { 6, 5 },{5,1 }, { 5, 0 }, { 7, 0 }, { 8, 1 }, { 8, 0 }, { 9, 1 }, { 8, 1 }, { 7, 0 }, { 8, 1 }, { 8, 1 }, { 7, 2 }, { 5, 0 }, { 0, 0 }, },
        new int[29,2] { { 15, 6 },{8,0 }, { 8, 1 }, { 7, 0 }, { 7, 0 }, { 7, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 9, 0 }, { 9, 0 }, { 9, 1 }, { 8, 0 }, { 9, 0 }, { 9, 0 }, { 9, 1 },
            { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 1 }, { 6, 2 }, { 4, 0 }, { 0, 0 },  },
    };
    string mapsFileName;

    public const int BEADSTOGRAPH = 2000;
    public byte[,] map = new byte[300, 300];    //map for finding peak of a single region
    public ushort[,] classmap = new ushort[300, 300];
    public int[] regioncnt = new int[500];
    public static  ushort[] values = new ushort[256];
    public ushort[] Values { get => values; set => values = value; }
    float[] fvalues = new float[256];
    float[] rp1sum = new float[300];
    float[,] sfi = new float[5000, 10];
    List<byte> readertab = new List<byte>() { 0xaa, 0xac, 0xaf, 0xa9, 0xab, 0xa8, 0xc2, 0xc4, 0xcd, 0xce, 0xcf };
    List<byte> reportingtab = new List<byte>() { 0x10, 0x12, 0x13, 0x20, 0x22, 0x24, 0x25, 0x26, 0x28, 0x29, 0x2a, 0x2c, 0x2d, 0x2e, 0xc0, 0xc1, 0xc2, 0xc8, 0xc9, 0x80, 0x82, 0x84, 0x86, 0x88, 0x8a };
    List<byte> calibtab = new List<byte>() { 0x20, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f, 0x87, 0x88, 0x89, 0x8a, 0x8b, 0x8c, 0x8d, 0x8e, 0x8f, 0xcd, 0xce, 0xcf };
    List<byte> channeltab = new List<byte>() { 0x24, 0x25, 0x26, 0x28, 0x29, 0x2a, 0x2c, 0x2d, 0x2e, 0x2f,0x93,0x94,0x95,0x96,0x98,0x99,0x9a,0x9b, 0x9c, 0x9d, 0x9e, 0x9f, 0xa0, 0xa1, 0xa2, 0xa3, 0xa4, 0xa5 ,0xa6,0xa7,0x80,0x81,0x82,0x83,0x84,0x85
        ,0xb0,0xb1,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb8,0xb9,0xba,0xbb,0xbc,0xbd,0xbe, 0x02};
    List<byte> motorstab = new List<byte>() { 0x41, 0x42, 0x43, 0x44, 0x48, 0x4a, 0x46,0x4c,0x4e, 0x51, 0x52, 0x53, 0x54, 0x56, 0x58, 0x5a, 0x5c, 0x5e, 0x61, 0x62, 0x63, 0x64, 0x66, 0x68, 0x6a, 0x6c, 0x6e, 0xa8, 0x1c, 0x1d, 0x1e, 0x1a, 0x90, 0x91, 0x92, 0x16 };
    List<byte> componentstab = new List<byte>() { 0x10, 0x11, 0x12, 0x13, 0x14, 0x16, 0x17, 0x18, 0xc0, 0xc7, 0xc8, 0xc9 };
    public List<BRegion> bregions = new List<BRegion>();
    public string[] syncelements = new string[11];
    public bool[,] actwell = new bool[16, 24];
    byte[] usbINbuf = new byte[512];
    public int[] sscdata = new int[256];
    public int[] rp1data = new int[256];
    public byte[,] chartarray2 = new byte [256, 256];
    public int savingwellidx { get; set; }
    public float hdnrcoef { get; set; }
    public int tempcl0 { get; set; }
    public int tempcl1 { get; set; }
    public int tempcl2 { get; set; }
    public int tempcl3 { get; set; }
    public int temprssc { get; set; }
    public int tempgssc { get; set; }
    public int tempvssc { get; set; }
    public int temprpmaj { get; set; }
    public int temprpmin { get; set; }

    public int wells_to_read { get; set; }
    public Int16 sampvol { get; set; }
    public Int16 washvol { get; set; }
    public Int16 agitatevol { get; set; }
    public string Outdir { get; set; }
    public string LISInputdir { get; set; }
    public string mapdir { get; set; }
    public string Outfilename { get; set; }
    public byte plateRow { get; set; }
    public byte plateCol { get; set; }
    public byte plateType { get; set; }
    public byte chanconfig { get; set; }
    public string fullFileName { get; set; }
    public string summaryFileName { get; set; }
    public int beadsToCapture { get; set; }
    public int beadCount { get; set; }
    public int savbeadCount { get; set; }
    public int currentwellidx { get; set; }
    public int sampleSize { get; set; }
    public byte act3didx { get; set; }
    public byte actpriidx { get; set; }
    public byte actsecidx { get; set; }
    public float reg3dpeak { get; set; }
    public float regpripeak { get; set; }
    public float regsecpeak { get; set; }
    public int beadcnt_map_create { get; set; }
    public float mappeakx { get; set; }
    public float mappeaky { get; set; }
    public float compensation { get; set; }
    public float pemolcoef { get; set; }
    public float hdnrtrans { get; set; }
//        public float accumrpbg { get; set; }
//        public float avgrpbg { get; set; }
        //public float xmotleftpos { get; set; }
        //public float ymotbackpos { get; set; }
        //public int xmotctr { get; set; }
        //public int ymotctr { get; set; }
    public bool roworder { get; set; }
    public int scatterGate { get; set; }

    public bool newstats { get; set; }
    public float idex_dir { get; set; }
    public ushort idex_steps { get; set; }
    public byte idex_pos { get; set; }

    public int minperregion { get; set; }
    public ushort sync_status { get; set; }
    public byte termtype { get; set; }
    public object cmdobj { get; set; }
    public object beadobj { get; set; }
    public bool subtregbg { get; set; }
    public bool readingA { get; set; }
    public byte readingRow { get; set; }
    public byte readingCol { get; set; }
    public byte sscselected { get; set; }
    public byte xaxissel { get; set; }
    public byte yaxissel { get; set; }
    public bool isTube { get; set; }
    public bool readActive { get; set; }
    public bool chk_regcnt { get; set; }
    public bool everyevent { get; set; }
    public bool rmeans { get; set; }
    public bool calstats { get; set; }
    public bool pltrept { get; set; }
    public byte endState { get; set; }
    public byte createmapstate { get; set; }
    public bool only_classified { get; set; }
    public bool reg0stats { get; set; }
    public bool newmap { get; set; }
    public bool instrument_connected { get; set; }
    public byte systemControl { get; set; }
    public string woname { get; set; }
    public string wopath { get; set; }
    public DirectoryInfo RootDirectory { get; set; }
    public StringBuilder dataout = new StringBuilder();
    public StringBuilder summaryout = new StringBuilder();
    CommandStruct newcmd = new CommandStruct();
    CLQueue newdp = new CLQueue();
    Dictionary<string, CommandStruct> m_MainCmdTemplates = new Dictionary<string, CommandStruct>()
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

    #region Constructor / Destructor

    public MicroCyDevice()
    {
      instrument_connected = true;

      USBDeviceInfo[] di = USBDevice.GetDevices(INTERFACE_GUID);   // Get all the MicroCy devices connected
      try
      {
        MicroCyUSBDevice = new USBDevice(di[0].DevicePath);     // just grab the first one for now, but should support multiples
        Console.WriteLine(string.Format("{0}:{1}", MicroCyUSBDevice.Descriptor.FullName, MicroCyUSBDevice.Descriptor.SerialNumber));
      }
      catch { instrument_connected = false; }
      syncelements = Enum.GetNames(typeof(SyncItems));

      LoadMaps();
      CreateSystemDirectories();
      SetProperty(1, 1);    //set version as 1 to enable work order handling
      act3didx = 0;   //cl0
      actpriidx = 1;  //cl1;
      actsecidx = 2;  //cl2;
      beadobj = 88;
      reg0stats = false;
      calstats = false;
      newmap = false;
      minperregion = 100;
      beadsToCapture = 500;
      isTube = false;
      InitReadPipe();
      termtype = 2;     //default termination is end of sample
    }

    #endregion Constructor / Destructor

    #region Methods
        /// <summary>
        /// USBDevice object for the MicroCy device
        /// </summary>
        public USBDevice MicroCyUSBDevice
        {
            get;
            private set;
        }
//        public Queue<CommandStruct> Commands { get => Commands1; set => Commands1 = value; }
//        public Queue<CommandStruct> Commands1 { get => commands; set => commands = value; }
        public StringBuilder Dataout { get => dataout; set => dataout = value; }
        public char[] alphabet { get; private set; }
    
        #endregion Properties

    #region Methods

    #region Compound Commands
        //public void RunPlate (PlateTypes plateType)
        //{
        //}

        #endregion

    private void CreateSystemDirectories()
    {
      RootDirectory = new DirectoryInfo(Path.Combine(@"C:\Emissioninc", Environment.MachineName));
      List<string> subDirectories = new List<string>(7) { "Config", "WorkOrder", "Archive", "Result", "Summary", "Raw", "SystemLogs" };
      foreach(var d in subDirectories)
      {
        RootDirectory.CreateSubdirectory(d);
      }
    }

    #region Main Commands
        public void SheathPump(byte cmd, ushort parameter)
        {
            string sCmd = "Sheath";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            cs.Command = cmd;
            cs.Parameter = parameter;
            RunCmd(sCmd, cs);
        }
        public void SampleAPump(byte cmd, ushort parameter)
        {
            string sCmd = "SampleA";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            cs.Command = cmd;
            cs.Parameter = parameter;
            RunCmd(sCmd, cs);
        }
        public void SampleBPump(byte cmd, ushort parameter)
        {
            string sCmd = "SampleB";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            cs.Command = cmd;
            cs.Parameter = parameter;
            RunCmd(sCmd, cs);
        }
        public void RefreshDac()
        {
            string sCmd = "RefreshDac";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);

        }
        public void SetNextWell()
        {
            string sCmd = "SetNextWell";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);

        }
       public void SetBaseline(byte cmd)
        {
            
            string sCmd = "SetBaseline";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            cs.Command = cmd;
            RunCmd(sCmd, cs);
        }
        public void SaveToFlash()
        {

            string sCmd = "SaveToFlash";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);
        }
        public void ReadFlash()
        {

            string sCmd = "Future1";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);
        }
        public void InitOpVars(byte cmd)
        {
            string sCmd = "InitOpVars";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            cs.Command = cmd;   //0 = normal, 1= use factory defaults
            RunCmd(sCmd, cs);
        }
        public void FlushQueue()
        {
            string sCmd = "FlushCmdQueue";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
           RunCmd(sCmd, cs);
        }
        public void StartSampling()
        {
            string sCmd = "Start Sampling";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);
        }
        public void EndSampling()
        {
            string sCmd = "End Sampling";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);
        }
        public void MotorAlign(byte cmd, int parameter)
        {
            string sCmd = "AlignMotor";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            cs.Command = cmd;
            cs.Parameter = 0;
            cs.FParameter = (float)parameter;
            RunCmd(sCmd, cs);
        }

        public void MotorX(byte cmd, int parameter)
        {
            string sCmd = "MotorX";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            cs.Command = cmd;
            cs.Parameter = 0;
            cs.FParameter = (float)parameter;
            RunCmd(sCmd, cs);
        }
        public void MotorY(byte cmd, int parameter)
        {
            string sCmd = "MotorY";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            cs.Command = cmd;
            cs.Parameter = 0;
            cs.FParameter = (float)parameter;
            RunCmd(sCmd, cs);
        }
        public void MotorZ(byte cmd, int parameter)
        {
            string sCmd = "MotorZ";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            cs.Command = cmd;
            cs.Parameter = 0;
            cs.FParameter = (float)parameter;
            RunCmd(sCmd, cs);
        }

       /// COMPLEX COMMANDS***********************************************
   
        public void Startup()
        {

            string sCmd = "Startup";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);
        }
       public void Prime()
        {
            string sCmd = "Prime";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);
            
        }
        public void MoveIdex()
        {
            string sCmd = "Idex";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            cs.Code = 0xd7;
            cs.Command = idex_pos;
            cs.Parameter = idex_steps;
            cs.FParameter = idex_dir;   //currently checked is direction high on driver ic
            RunCmd(sCmd, cs);

        }
        public void SheathEmptyPrime()
        {
            string sCmd = "Sheath Empty Prime";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);
        }
        public void WashA()
        {
            string sCmd = "Wash A";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);
        }
        public void WashB()
        {
            string sCmd = "Wash B";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);
        }
        public void EjectPlate()
        {
            string sCmd = "Eject Plate";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);
        }
        public void LoadPlate()
        {
            string sCmd = "Load Plate";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);
        }
        public void Position()  //next position is set in properties 0xad and 0xae
        {
            string sCmd =  "Position Well Plate";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);
        }

        public void AspirateA()
        {
            string sCmd =  "Aspirate Syringe A" ;
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);
        }
        public void AspirateB()
        {
            string sCmd = "Aspirate Syringe B";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);
        }
        public void ReadA()
        {
            string sCmd = "Read A";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            readingA = true;
            RunCmd(sCmd, cs);
        }
        public void ReadB()
        {
            string sCmd = "Read B";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            readingA = false;
            RunCmd(sCmd, cs);
        }
        public void ReadA_AspirB()
        {
            string sCmd = "Read A Aspirate B";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            readingA = true;
            RunCmd(sCmd, cs);
        }
        public void ReadB_AspirA()
        {
            string sCmd = "Read B Aspirate A";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            readingA = false ;
           RunCmd(sCmd, cs);
        }
       public void EndReadA()
        {
            string sCmd = "End Bead Read A";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);
        }
        public void EndReadB()
        {
            string sCmd = "End Bead Read B";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            RunCmd(sCmd, cs);
        }
        public void SetProperty(byte code, ushort parameter)
        {
            string sCmd = "Set Property";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            cs.Code = code;
            cs.Parameter = parameter;
            RunCmd(sCmd, cs);
        }
        public void SetFProperty(byte code, float fparameter)
        {
            string sCmd = "Set FProperty";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            cs.Code = code;
            cs.FParameter = fparameter;
            RunCmd(sCmd, cs);
        }
        public void GetProperty(byte code)
        {
            string sCmd = "Get Property";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            cs.Code = code;
            RunCmd(sCmd, cs);
        }
        public void GetFProperty(byte code)
        {
            string sCmd = "Get FProperty";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            cs.Code = code;
            RunCmd(sCmd, cs);
        }
        public void GetFProperty(byte code, UInt16 parameter)
        {
            string sCmd = "Get FProperty";
            CommandStruct cs = m_MainCmdTemplates[sCmd];
            cs.Parameter = parameter;
            cs.Code = code;
            RunCmd(sCmd, cs);
        }
        public void WellNext()
        {
            readingRow=plateRow;
            readingCol = plateCol;
        }
        public void EndBeadRead()
        {
            if (readingA)
            {
                EndReadA();         //sends EE to instrument
            }
            else EndReadB();    //send EF to instrument
            currentwellidx++;
            if (currentwellidx <= wells_to_read)  //are there more to go
            {
                SetReadingParamsForWell(currentwellidx);
                if (readingA)
                {
                    if (currentwellidx < wells_to_read)   //more than one to go
                    {
                        SetAspirateParamsForWell(currentwellidx + 1);
                        ReadB_AspirA();
                    }
                    else if (currentwellidx == wells_to_read)
                    {
                        ReadB();
                    }
                }
                else
                {
                    if (currentwellidx < wells_to_read)
                    {
                        SetAspirateParamsForWell(currentwellidx + 1);
                        ReadA_AspirB();
                    }
                    else if (currentwellidx == wells_to_read)
                    {
                        //handle end of plate things
                        ReadA();
                    }
                }
                InitBeadRead(readingRow, readingCol);   //gets output file redy
                //if (roworder == true)
                //{
                //    if (wellsinorder[currentwellidx - 1].rowIdx != (byte)wellsinorder[currentwellidx].rowIdx)
                //    {
                //        SetFProperty(0x54, (xmotctr + xmotleftpos - 1));
                //        xmotctr = 0;
                //    }
                //    else
                //    {
                //        xmotctr++;
                //    }
                //}

            }
        }
        public void WellReset()
        { 
            currentwellidx++;
            if (currentwellidx <= wells_to_read)  //are there more to go
            {
                SetReadingParamsForWell(currentwellidx);
                if (readingA)
                {
                    if (currentwellidx < wells_to_read)   //more than one to go
                    {
                        SetAspirateParamsForWell(currentwellidx + 1);
                        ReadB_AspirA();
                    }
                    else if (currentwellidx == wells_to_read)
                    {
                        ReadB();
                    }
                }
                else
                {
                    if (currentwellidx < wells_to_read)
                    {
                        SetAspirateParamsForWell(currentwellidx + 1);
                        ReadA_AspirB();
                    }
                    else if (currentwellidx == wells_to_read)
                    {
                        //handle end of plate things
                        ReadA();
                    }
                }
                InitBeadRead(readingRow, readingCol);   //gets output file redy
                //if (roworder == true)
                //{
                //    if (wellsinorder[currentwellidx - 1].rowIdx != (byte)wellsinorder[currentwellidx].rowIdx)
                //    {
                //        SetFProperty(0x54, (xmotctr + xmotleftpos - 1));
                //        xmotctr = 0;
                //    }
                //    else
                //    {
                //        xmotctr++;
                //    }
                //}
                //else
                //{
                //    if (wellsinorder[currentwellidx - 1].colIdx != (byte)wellsinorder[currentwellidx].colIdx)
                //    {
                //        SetFProperty(0x64, (ymotctr + ymotbackpos - 1));
                //        ymotctr = 0;
                //    }
                //    else ymotctr++;
                //}

            }
            else
            {
                //do end of run things
                readActive = false;

            }
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
                        foreach (byte code in readertab)
                        {
                            GetProperty(code);
                        }

                        break;
                    }
                case "reportingtab":
                    {
                        foreach (byte code in reportingtab)
                        {
                            GetProperty(code);
                        }

                        break;
                    }
                case "calibtab":
                    {
                        foreach (byte code in calibtab)
                        {
                            GetProperty(code);
                        }

                        break;
                    }
                case "channeltab":
                    {
                        foreach (byte code in channeltab)
                        {
                            GetProperty(code);
                        }

                        break;
                    }
                case "motorstab":
                    {
                        foreach (byte code in motorstab)
                        {
                            GetProperty(code);
                        }

                        break;
                    }
                case "componentstab":
                    {
                        foreach (byte code in componentstab)
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
      //build classification map from activemap using bitfield types A-D
      int row, irow, col, jcol,begidx,begidy,endidx,endidy;
      float xwidth,ywidth,cl2;
      int[,] bitpoints = new int[32, 2];
      actpriidx = (byte)mmap.midorderidx; //what channel cl0 - cl3?
      actsecidx = (byte)mmap.loworderidx;
      SetProperty(0xce, mmap.minmapssc);  //set ssc gates for this map
      SetProperty(0xcf, mmap.maxmapssc);
      Array.Clear(classmap, 0, classmap.Length);
      foreach (BeadRegion mapRegions in mmap.mapRegions)
            {
                if (mapRegions.isvector == false)       //this region shape is taken from bitmaplist array
                {
                    Array.Clear(bitpoints, 0, bitpoints.Length);  //copy bitmap of the type specified (A B C D)
                    Array.Copy(bitmaplist[mapRegions.bitmaptype], bitpoints, bitmaplist[mapRegions.bitmaptype].Length);
                    row = mapRegions.centermidorderidx - bitpoints[0, 0];  //first position is value to backup before etching bitmap
                    col = mapRegions.centerloworderidx - bitpoints[0, 1];
                    irow = 1;
                    while (bitpoints[irow, 0] != 0)
                    {
                        for (jcol = col; jcol < (col + bitpoints[irow, 0]); jcol++)
                        {
                            //handle region overlap by making overlap 0
                            if (classmap[row + irow - 1, jcol] == 0)
                            {
                                classmap[row + irow - 1, jcol] = mapRegions.regionNumber;
                            }
                            else
                            {
                                classmap[row + irow - 1, jcol] = 0;
                            }
                        }
                        col += bitpoints[irow, 1];  //second position is right shift amount for next line in map
                        irow++;
                    }
                }
                else
                {
                    //populate a computed region
                    cl2 = (float )mapRegions.meanloworder;
                    xwidth = (float) 0.33 * mapRegions.meanmidorder + 50;
                    //                    ywidth = (float)((float) 0.0004 * cl2 * cl2 + 0.01 * cl2 + 60);
                    ywidth = (float)0.33 * mapRegions.meanloworder + 50;

                    //begidx = Val_2_Idx((int)(mapRegions.meanmidorder - (xwidth / 2)));
                    //endidx = Val_2_Idx((int) (mapRegions.meanmidorder + (xwidth / 2)));
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
                            if (classmap[irow, jcol] == 0) classmap[irow, jcol] = mapRegions.regionNumber;
                            else classmap[irow, jcol] = 0;  //zero any overlaps
                        }
//                        xincer+= (xwidth/ywidth);
                    }

                }
            }
      newmap = true;
    }

    public void InitBeadRead(byte rown,byte coln)
    {
      char rowletter;
      int differ;
      ushort colnum;

      //open file
      //first create uninique filename
      colnum = (ushort)coln;    //well names are relative to 1
      if (!isTube)
        colnum++;  //use 0 for tubes and true column for plates
      rowletter = (char)(0x41 + rown);
      if (!Directory.Exists(Outdir))
        Outdir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      for (differ = 0; differ < 100; differ++)
      {
        fullFileName = Outdir+"\\"+Outfilename + rowletter + colnum.ToString() + '_' + differ.ToString()+".csv";
        if (!System.IO.File.Exists(fullFileName))
          break;
      }
      dataout.Clear();
      dataout.Append(bheader);
      chk_regcnt = false;
      Array.Clear(regioncnt,0,101);
      Array.Clear(rp1sum, 0, 101);
      beadCount = 0;
    }

    public void PrepareSummaryFile()
    {
      for (var i = 0; i < 1000; i++)
      {
        summaryFileName = Outdir + "\\" + "Results__" + Outfilename  + '_' + i.ToString() + ".csv";
        if (!File.Exists(summaryFileName))
          break;
      }
      summaryout.Clear();
      summaryout.Append(sheader);

    }

    public void SaveBeadFile() //cancels the begin read from endpoint 2
    {
      //this.MicroCyUSBDevice.Interfaces[0].Pipes[0x82].EndRead(result);
      //write file
      alphabet = Enumerable.Range('A', 16).Select(x => (char)x).ToArray();
      int quarter;
      byte bgreadingrow, bgreadingcol;
      string bgfullFileName, bgsummaryFileName;
      string bgdataout, rfilename;
      bgfullFileName = fullFileName;  //save name in bg process cause it gets changed in endstate 5
      bgsummaryFileName = summaryFileName;
      List<WellResults> bgwellresults = wellresults.ToList();
      bgdataout = dataout.ToString();
      bgreadingrow = readingRow;
      bgreadingcol = readingCol;
      Console.WriteLine(string.Format("{0} Reporting Background results cloned for save", DateTime.Now.ToString()));
      if ((bgfullFileName != null) & (everyevent==true))
      {
        File.WriteAllText(bgfullFileName, dataout.ToString());
      }
      if (rmeans)
      {
        plateReport.rpWells.Add(new WellReport { prow= wellsinorder[savingwellidx].rowIdx, pcol= wellsinorder[savingwellidx].colIdx });
        foreach (WellResults regionNumber in bgwellresults)
        {
          OutResults rout = new OutResults();
          if (savingwellidx > (wellsinorder.Count - 1)) savingwellidx = wellsinorder.Count - 1;
          rout.row = alphabet[wellsinorder[savingwellidx].rowIdx].ToString();
          rout.col = wellsinorder[savingwellidx].colIdx + 1;    //columns are 1 based
          rout.count = regionNumber.RP1vals.Count();
          rout.region = regionNumber.regionNumber;
          if (rout.count > 2)
          {
            rout.meanfi = regionNumber.RP1vals.Average();
          }
          if (rout.count >= 20)
          {
            regionNumber.RP1vals.Sort();
            float rpbg = regionNumber.RP1bgnd.Average()*16;
            quarter = rout.count / 4;
            regionNumber.RP1vals.RemoveRange(rout.count - quarter , quarter);
            regionNumber.RP1vals.RemoveRange(0, quarter);
            rout.meanfi = regionNumber.RP1vals.Average();
            double sumsq = regionNumber.RP1vals.Sum(dataout => Math.Pow(dataout - rout.meanfi, 2));
            double stddev = Math.Sqrt(sumsq / regionNumber.RP1vals.Count() - 1);
            rout.cv = (float)stddev / rout.meanfi*100;
            if (double.IsNaN(rout.cv)) rout.cv = 0;
            rout.medfi = (float) Math.Round(regionNumber.RP1vals[quarter]-rpbg);
            rout.meanfi -= rpbg;
          }
          summaryout.Append(rout.ToString());
//          if (subtregbg == true) rout.meanfi -= activemap.mapRegions[regionNumber.regionNumber].meanrp1bg;

          plateReport.rpWells[savingwellidx].rpReg.Add(new RegionReport(){ region = regionNumber.regionNumber,
            count = (uint)rout.count, medfi = rout.medfi, meanfi = rout.meanfi, coefVar = rout.cv }); 
        }
        if ((savingwellidx == wells_to_read)& (summaryout.Length>0) & (rmeans==true) )  //end of read session (plate, plate section or tube) write summary stat file
        {
          File.WriteAllText(bgsummaryFileName, summaryout.ToString());
        }
        if ((savingwellidx == wells_to_read) & (summaryout.Length > 0) & (pltrept == true))    //end of read and json results requested
        {
          rfilename = systemControl == 0 ? Outfilename : workOrder.plateID.ToString();
          string resultfilename = Path.Combine(@"C:\Emissioninc", Environment.MachineName,"Result", rfilename);
          TextWriter jwriter = null;
          try
          {
            var jcontents = JsonConvert.SerializeObject(plateReport);   
            jwriter = new StreamWriter(resultfilename+".json");
            jwriter.Write(jcontents);
            if (File.Exists(wopath)) File.Delete(wopath);   //result is posted, delete work order
          }
          finally
          {
            if (jwriter != null)
              jwriter.Close();
          }
        }
      }
      if ((calstats) & (savbeadCount > 2))
      {
        double sumit = 0;
        double sumsq = 0;
        double mean = 0;
        double stddev = 0;
        double min;
        double max;
        double[] robustcnt = new double[10];
        if (savbeadCount > 5000)
          savbeadCount = 5000;
        for (int finx = 0; finx < 10; finx++)
        {
          for (int beads = 0; beads < savbeadCount; beads++)
          {
              sumit += sfi[beads, finx];
          }
          robustcnt[finx] = savbeadCount; //start with total bead count
          mean = sumit / savbeadCount;
          //find high and low bounds
          min = mean * 0.5;
          max = mean * 2;
          sumit = 0;
          for (int beads = 0; beads < savbeadCount; beads++)
          {
            if ((sfi[beads, finx] > min) & (sfi[beads, finx] < max))
            {
              sumit += sfi[beads, finx];
            }
            else
            {
              sfi[beads, finx] = 0;
              robustcnt[finx]--;
            }
          }
          mean = sumit / robustcnt[finx];
          for (int beads = 0; beads < savbeadCount; beads++)
          {
            if (sfi[beads, finx] == 0)
              continue;
            sumsq += Math.Pow(mean - sfi[beads, finx], 2);
          }
          stddev = Math.Sqrt(sumsq / (robustcnt[finx] - 1));
          gstats[finx].mfi = mean;
          gstats[finx].cv = (stddev / mean) * 100;
          if (double.IsNaN(gstats[finx].cv))
            gstats[finx].cv = 0;
          sumit = 0;
          sumsq = 0;
        }
        newstats = true;
      }
      Console.WriteLine(string.Format("{0} Reporting Background File Save Complete", DateTime.Now.ToString()));
    }

        public void SetReadingParamsForWell(int idx)
        {

            SetProperty(0xaa, (ushort)wellsinorder[idx].runSpeed);
            SetProperty(0xc2, (ushort)wellsinorder[idx].chanConfig);
            beadsToCapture = wellsinorder[idx].termCnt;
            minperregion = wellsinorder[idx].regTermCnt;
            termtype = wellsinorder[idx].termType;
            //            ConstructMap(wellsinorder[idx].thisWellsMap);
            ConstructMap(activemap);
            wellresults.Clear();
            foreach (BeadRegion mapRegions in wellsinorder[idx].thisWellsMap.mapRegions)
            {
                bool isInMap = activemap.mapRegions.Any(xx => mapRegions.regionNumber == xx.regionNumber);
                if (isInMap==false)
                    Console.WriteLine(string.Format("{0} No Map for Work Order region {1}", DateTime.Now.ToString(),mapRegions.regionNumber));
                if ((mapRegions.isActive) & (isInMap))
                {
                    WellResults actreg = new WellResults();
                    actreg.regionNumber = mapRegions.regionNumber;
                    wellresults.Add(actreg);
                }
            }
            if (reg0stats == true)
            {
                WellResults actreg = new WellResults();
                actreg.regionNumber = 0;
                wellresults.Add(actreg);

            }
        }
        public void SetAspirateParamsForWell(int idx)
        {
            SetProperty(0xad, (ushort)wellsinorder[idx].rowIdx);
            SetProperty(0xae, (ushort)wellsinorder[idx].colIdx);
            SetProperty(0xaf, (ushort)wellsinorder[idx].sampVol);
            SetProperty(0xac, (ushort)wellsinorder[idx].washVol);
            SetProperty(0xc4, (ushort)wellsinorder[idx].agitateVol);
            plateRow = (byte)wellsinorder[idx].rowIdx;
            plateCol = (byte)wellsinorder[idx].colIdx;

        }
        //        private void ReplyFromBeadReader(IAsyncResult result)
        //        {
        //            byte jj;
        //            bool isDone;
        ////            string aa;
        //            this.MicroCyUSBDevice.Interfaces[0].Pipes[0x82].EndRead(result);
        //            for (jj = 0; jj < 16; jj++)
        //            {
        //                outbead = BeadArrayToStruct<BeadInfoStruct>(beadbuffer,jj);
        //                if (outbead.Header != 0xadbeadbe) break;
        //                dataout.Append(outbead.ToString());
        //                beadCount++;
        //            }
        //            Array.Clear(beadbuffer, 0, 512);
        //            isDone = false;
        //            switch (termtype)
        //            {
        //                case 0: //min beads in each region
        //                    {
        //                        //do statistical magic
        //                        break;
        //                    }
        //                case 1: //total beads captured
        //                    {
        //                        if (beadCount >= beadsToCapture) isDone = true;
        //                        break;
        //                    }
        //                case 2: //end of sample 
        //                    {
        //                        if (sync_status == 0) isDone = true;
        //                        break;
        //                    }
        //            }

        //            if (isDone)
        //            {
        //                SaveBeadFile();
        //                SetProperty(0xcb, 0); //clear sync token to allow next sequence to execute
        //                EndBeadRead();
        //            }
        //            else
        //            {
        //                this.MicroCyUSBDevice.Interfaces[0].Pipes[0x82].BeginRead(beadbuffer, 0, 512, new AsyncCallback(ReplyFromBeadReader), beadobj);
        //            }
        //        }
    public void InitReadPipe()
    {
      if (instrument_connected)
      {
        MicroCyUSBDevice.Interfaces[0].Pipes[0x81].BeginRead(usbINbuf, 0, 512, new AsyncCallback(ReplyFromMC), cmdobj);
      }
    }

    private void ReplyFromMC(IAsyncResult result)
    {
      byte jj,cl1,cl2;
      int xx,yy,zz,savx,savy;
      byte [] mapx = new byte [70000];
      float[] cl = new float[4];
      float grp1;
      double sidesc, sidsel;
      float cl1comp,cl2comp;

      MicroCyUSBDevice.Interfaces[0].Pipes[0x81].EndRead(result);
      if ((usbINbuf[0] == 0xbe) && (usbINbuf[1] == 0xad))
      {
        for (jj = 0; jj < 8; jj++)
        {
          BeadInfoStruct outbead = new BeadInfoStruct();
          outbead = BeadArrayToStruct<BeadInfoStruct>(usbINbuf, jj);
          if (outbead.Header != 0xadbeadbe)
            break;
          cl1comp = outbead.greenB * compensation / 100;
          cl2comp = (float)((float) cl1comp * 0.26);
          cl[0] = outbead.cl0;
          cl[1] = outbead.cl1 -cl1comp;   //compensation
          outbead.cl1 = cl[1];
          cl[2] = outbead.cl2 - cl2comp;
          outbead.cl2 = cl[2];
          cl[3] = outbead.cl3;
          xx = (byte)(Math.Log(cl[actpriidx]) * 24.526);
          yy = (byte)(Math.Log(cl[actsecidx]) * 24.526);
          if ((xx > 0) & (yy > 0))
          {
            outbead.region = classmap[xx,yy];    //each well can have a different  classification map
          }
          else
            outbead.region = 0;

          //handle HI dnr channel
          //outbead.reporter = outbead.greenC;
          if (outbead.greenC > hdnrtrans)
              outbead.reporter = outbead.greenB * hdnrcoef;
          else
            outbead.reporter = outbead.greenC ;
//          accumrpbg += outbead.greenC;
          // if pcreg exists in wellsresults, add rp1 value to list
          //                    if (wellresults.Any(w => w.regionNumber == pcreg))

          //wellresults is a list of region numbers that are active
          //each entry has a list of rp1 values from each bead in that reagion
          int index = wellresults.FindIndex(w => w.regionNumber == outbead.region);
          if (index>=0)
          {
            wellresults[index].RP1vals.Add(outbead.reporter);   //
            wellresults[index].RP1bgnd.Add(outbead.greenC_bg);
            if (wellresults[index].RP1vals.Count == minperregion)
            {
              chk_regcnt = true;  //see if assay is done via sufficient beads in each region
            }
          }


          if (outbead.region == 0)
          {
            if (only_classified)
              continue;
          }
          
          if ((cl[actsecidx] > 0) & (cl[actpriidx] > 0)& (createmapstate == 1))
          {
            map[xx, yy]++;
//            if (map[xx, yy] == 250) createmapstate = 2; //we have enough to end map build
            // TODO if calc peak box checked in add region activity, check if 10k beads have been read, if so set state to 3
            if(beadcnt_map_create++ > sampleSize)
              createmapstate = 3;
          }
          if (beadCount < BEADSTOGRAPH)
          {
            switch (xaxissel)
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
            switch (yaxissel)
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
            switch (sscselected)
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
            sidesc = (Math.Log(sidsel) * 24.526);
            sscdata[(byte)sidesc]++;
            grp1 = outbead.reporter;
            if (grp1 > 32000) grp1 = 32000; //don't let graph overflow
            rp1data[(byte)(Math.Log(grp1) * 24.526)]++;
            try
            {
              if ((newdp.xyclx > 1) && (newdp.xycly > 1))
              {
                lock (cldata)
                { cldata.Enqueue(newdp); }    //save data for graphing 
              }
            }
            finally { }
          }
          if (outbead.region > 0)
          {
            if (regioncnt[outbead.region] == minperregion)  //do the check whenever and region gets to min
            {
              chk_regcnt = true;
            }
          }
          if(everyevent) dataout.Append(outbead.ToString());
          //accum stats for run as a whole, used during aligment and QC
          if ((calstats) & (beadCount < 5000))
          {
            sfi[beadCount,6] = outbead.cl3;
            sfi[beadCount,3] = outbead.redssc;
            sfi[beadCount,4] = outbead.cl1;
            sfi[beadCount,5] = outbead.cl2;
            sfi[beadCount,0] = outbead.greenssc;
            sfi[beadCount,1] = outbead.greenB;
            sfi[beadCount,2] = outbead.greenC;
            sfi[beadCount,7] = outbead.violetssc;
            sfi[beadCount,8] = outbead.cl0;
            sfi[beadCount,9] = outbead.fsc;
          }
          beadCount++;
        }
        if (createmapstate == 2)
        {
          zz = 0;
          Array.Clear(mapx, 0, 70000);
          for (xx = 0; xx <256; xx++) //linearize the array
          {
            for (yy = 0; yy <256; yy++)
            {
              cl1 = (byte)xx;
              cl2 = (byte)yy;
              mapx[zz++] = map[cl1, cl2];
            }
          }
          mapdir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
          File.WriteAllBytes(mapdir + "\\" + "mapcounter.bin", mapx);
          createmapstate = 0;
        }
        if (createmapstate == 3)
            {
                zz = 0; savx = 0;savy = 0;
                for (xx = 0; xx < 256; xx++) //find peak
                {
                    for (yy = 0; yy < 256; yy++)
                    {
                        if (map[xx, yy] > zz)
                        {
                            zz = map[xx, yy];
                            savx = xx;
                            savy = yy;
                        }
                    }
                }
                mappeakx = (float)Math.Exp(savx / 24.526);
                mappeaky = (float)Math.Exp(savy / 24.526);
//                avgrpbg = accumrpbg / beadCount;
                createmapstate = 0;
                
                //find region peak and fill in text boxes on new region tab
            }
        Array.Clear(usbINbuf, 0, 512);
        switch (termtype)
        {
          case 0: //min beads in each region
            //do statistical magic
            if (chk_regcnt)  //a region made it, are there more that haven't
            {
              endState = 1;   //assume all region have enough beads
              foreach (WellResults regionNumber in wellresults)
              {
                if (regionNumber.RP1vals.Count() < minperregion)
                {
                  endState = 0;   //not done yet
                  break;
                }
              }
              chk_regcnt = false;
            }
            break;
          case 1: //total beads captured
            if ((beadCount >= beadsToCapture) & readActive)
            {
              endState = 1;
              readActive = false;
            }
            break;
          case 2: //end of sample 
            break;
        }
      }
      else
      {
        lock (commands)
        {
          // move received command to queue
          newcmd = ByteArrayToStruct<CommandStruct>(usbINbuf);
          Values[newcmd.Code] = newcmd.Parameter;
          fvalues[newcmd.Code] = newcmd.FParameter;
          commands.Enqueue(newcmd);
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
      MicroCyUSBDevice.Interfaces[0].Pipes[0x81].BeginRead(usbINbuf, 0, 512, new AsyncCallback(ReplyFromMC), cmdobj);
    }

    /// <summary>
    /// Sends a command OUT to the USB device, then checks the IN pipe for a return value.
    /// </summary>
    /// <param name="sCmdName">A friendly name for the command.</param>
    /// <param name="cs">The CommandStruct object containing the command parameters.  This will get converted to an 8-byte array.</param>
    private void RunCmd (string sCmdName, CommandStruct cs)
    {
      if (instrument_connected)
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
      //read the maplist if available
      string testfilename;
      //  mapsFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DimensionMaps.txt");
      testfilename = Path.Combine(@"C:\Emissioninc", Environment.MachineName, "Config","DimensionMaps.txt");
      if (File.Exists(testfilename))
          mapsFileName = testfilename;
      else mapsFileName= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DimensionMaps.txt");

      if (File.Exists(mapsFileName))
      {
        TextReader reader = null;
        try
        {
          reader = new StreamReader(mapsFileName);
          var fileContents = reader.ReadToEnd();
          maplist = JsonConvert.DeserializeObject<List<CustomMap>>(fileContents);
        }
        finally
        {
          if (reader != null) reader.Close();
        }
      }
      mapsFileName = testfilename;    //make sure maps is written into new dir struct on first save
    }

    public bool IsNewWorkOrder()
    {
      string chkpath = Path.Combine(@"C:\Emissioninc", Environment.MachineName, "WorkOrder");
      string[] fileEntries = Directory.GetFiles(chkpath, "*.txt");
      if (fileEntries.Length == 0)
        return false;
      woname = Path.GetFileNameWithoutExtension(fileEntries[0]);
      wopath = fileEntries[0];
      TextReader reader = null;
      try
      {
        reader = new StreamReader(wopath);
        var fileContents = reader.ReadToEnd();
        workOrder = JsonConvert.DeserializeObject<WorkOrder>(fileContents);
      }
      finally
      {
        if (reader != null)
          reader.Close();
        //File.Delete(wopath);
      }
      //set plate type
      if (workOrder.numberCols == 12)
        plateType = 0;
      else
        plateType = 1;
      // send well depth once that is worked out
      return true;
    }

        public void SaveMaps()
        {
 //           string jFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DimensionWorkOrder.txt");
            string jFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DimensionPlateResults.txt");
            TextWriter writer = null;
            TextWriter jwriter = null;
            try
            {
                var contents = JsonConvert.SerializeObject(maplist);
                writer = new StreamWriter(mapsFileName);
                writer.Write(contents);
                var jcontents = JsonConvert.SerializeObject(plateReport);   ///test code to write json for schema creation
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
            maplist[idx].calgssc = tempgssc;
            maplist[idx].calrpmaj = temprpmaj;
            maplist[idx].calrpmin = temprpmin;
            maplist[idx].calcl3 = tempcl3;
            maplist[idx].calrssc = temprssc;
            maplist[idx].calcl1 = tempcl1;
            maplist[idx].calcl2 = tempcl2;
            maplist[idx].calvssc = tempvssc;
            maplist[idx].calcl0 = tempcl0;
            SaveMaps();
        }
        public void InitBitMaps()
        {

        }
        public int Idx_2_Val(int idx)
        {
            int retval;
            retval = (int)(Math.Exp(idx )/ 24.526);
            return retval;
        }
        public int Val_2_Idx(int val)
        {
            int retidx;
            retidx = (int)(Math.Log(val) * 24.526);
            return retidx;
        }
    

   #endregion Methods
  }
}
