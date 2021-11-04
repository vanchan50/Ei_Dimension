using System.Collections.Generic;

namespace MicroCy
{
  internal static class CommandLists
  {
    public static readonly Dictionary<string, CommandStruct> MainCmdTemplatesDict = new Dictionary<string, CommandStruct>()
    {
      { "Sheath",               new CommandStruct{ Code=0xd0, Command=0x0, Parameter=0, FParameter=0} },
      { "SampleA",              new CommandStruct{ Code=0xd1, Command=0x0, Parameter=0, FParameter=0} },
      { "SampleB",              new CommandStruct{ Code=0xd2, Command=0x0, Parameter=0, FParameter=0} },
      { "RefreshDac",           new CommandStruct{ Code=0xd3, Command=0x0, Parameter=0, FParameter=0} },
      { "SetNextWell",          new CommandStruct{ Code=0xd4, Command=0x0, Parameter=0, FParameter=0} },
      { "SetBaseline",          new CommandStruct{ Code=0xd5, Command=0x0, Parameter=0, FParameter=0} },
      { "SaveToFlash",          new CommandStruct{ Code=0xd6, Command=0x0, Parameter=0, FParameter=0} },
      { "Idex",                 new CommandStruct{ Code=0xd7, Command=0x0, Parameter=0, FParameter=0} },
      { "InitOpVars",           new CommandStruct{ Code=0xd8, Command=0x0, Parameter=0, FParameter=0} },
      { "FlushCmdQueue",        new CommandStruct{ Code=0xd9, Command=0x0, Parameter=0, FParameter=0} },
      { "Start Sampling",       new CommandStruct{ Code=0xda, Command=0x0, Parameter=0, FParameter=0} },
      { "End Sampling",         new CommandStruct{ Code=0xdb, Command=0x0, Parameter=0, FParameter=0} },
      { "AlignMotor",           new CommandStruct{ Code=0xdc, Command=0x0, Parameter=0, FParameter=0} },
      { "MotorX",               new CommandStruct{ Code=0xdd, Command=0x0, Parameter=0, FParameter=0} },
      { "MotorY",               new CommandStruct{ Code=0xde, Command=0x0, Parameter=0, FParameter=0} },
      { "MotorZ",               new CommandStruct{ Code=0xdf, Command=0x0, Parameter=0, FParameter=0} },
      { "Startup",              new CommandStruct{ Code=0xe0, Command=0x0, Parameter=0, FParameter=0} },
      { "Prime",                new CommandStruct{ Code=0xe1, Command=0x0, Parameter=0, FParameter=0} },
      { "Sheath Empty Prime",   new CommandStruct{ Code=0xe2, Command=0x0, Parameter=0, FParameter=0} },
      { "Wash A",               new CommandStruct{ Code=0xe3, Command=0x0, Parameter=0, FParameter=0} },
      { "Wash B",               new CommandStruct{ Code=0xe4, Command=0x0, Parameter=0, FParameter=0} },
      { "Eject Plate",          new CommandStruct{ Code=0xe5, Command=0x0, Parameter=0, FParameter=0} },
      { "Load Plate",           new CommandStruct{ Code=0xe6, Command=0x0, Parameter=0, FParameter=0} },
      { "Position Well Plate",  new CommandStruct{ Code=0xe7, Command=0x0, Parameter=0, FParameter=0} },
      { "Aspirate Syringe A",   new CommandStruct{ Code=0xe8, Command=0x0, Parameter=0, FParameter=0} },
      { "Aspirate Syringe B",   new CommandStruct{ Code=0xe9, Command=0x0, Parameter=0, FParameter=0} },
      { "Read A",               new CommandStruct{ Code=0xea, Command=0x0, Parameter=0, FParameter=0} },
      { "Read B",               new CommandStruct{ Code=0xeb, Command=0x0, Parameter=0, FParameter=0} },
      { "Read A Aspirate B",    new CommandStruct{ Code=0xec, Command=0x0, Parameter=0, FParameter=0} },
      { "Read B Aspirate A",    new CommandStruct{ Code=0xed, Command=0x0, Parameter=0, FParameter=0} },
      { "End Bead Read A",      new CommandStruct{ Code=0xee, Command=0x0, Parameter=0, FParameter=0} },
      { "End Bead Read B",      new CommandStruct{ Code=0xef, Command=0x0, Parameter=0, FParameter=0} },
      { "Set Aspirate Volume",  new CommandStruct{ Code=0xaf, Command=0x0, Parameter=0, FParameter=0} },
      { "Set Property",         new CommandStruct{ Code=0x0, Command=0x02, Parameter=0, FParameter=0} },
      { "Get Property",         new CommandStruct{ Code=0x0, Command=0x01, Parameter=0, FParameter=0} },
      { "Set FProperty",        new CommandStruct{ Code=0x0, Command=0x02, Parameter=0, FParameter=0} },
      { "Get FProperty",        new CommandStruct{ Code=0x0, Command=0x01, Parameter=0, FParameter=0} }
    };
    public static readonly List<byte> Readertab = new List<byte>() { 0xaa, 0xac, 0xaf, 0xa9, 0xab, 0xa8, 0xc2, 0xc4, 0xcd, 0xce, 0xcf };
    public static readonly List<byte> Reportingtab = new List<byte>() { 0x10, 0x12, 0x13, 0x20, 0x22, 0x24, 0x25, 0x26, 0x28, 0x29, 0x2a, 0x2c, 0x2d,
      0x2e, 0xc0, 0xc1, 0xc2, 0xc8, 0xc9, 0x80, 0x82, 0x84, 0x86, 0x88, 0x8a };
    public static readonly List<byte> Calibtab = new List<byte>() { 0x20, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c,
      0x3d, 0x3e, 0x3f, 0x87, 0x88, 0x89, 0x8a, 0x8b, 0x8c, 0x8d, 0x8e, 0x8f, 0xcd, 0xce, 0xcf, 0xbf };
    public static readonly List<byte> Channeltab = new List<byte>() { 0x24, 0x25, 0x26, 0x28, 0x29, 0x2a, 0x2c, 0x2d, 0x2e, 0x2f,0x93,0x94,0x95,0x96,0x98,0x99,0x9a,0x9b, 0x9c, 0x9d, 0x9e, 0x9f, 0xa0, 0xa1, 0xa2, 0xa3, 0xa4, 0xa5 ,0xa6,0xa7,0x80,0x81,0x82,0x83,0x84,0x85
        ,0xb0,0xb1,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb8,0xb9,0xba,0xbb,0xbc,0xbd,0xbe, 0x02};
    public static readonly List<byte> Motorstab = new List<byte>() { 0x41, 0x42, 0x43, 0x44, 0x48, 0x4a, 0x46, 0x4c, 0x4e, 0x51, 0x52, 0x53, 0x54,
      0x56, 0x58, 0x5a, 0x5c, 0x5e, 0x61, 0x62, 0x63, 0x64, 0x66, 0x68, 0x6a, 0x6c, 0x6e, 0xa8, 0x1c, 0x1d, 0x1e, 0x1a, 0x90, 0x91, 0x92, 0x16 };
    public static readonly List<byte> Componentstab = new List<byte>() { 0x10, 0x11, 0x12, 0x13, 0x14, 0x16, 0x17, 0x18, 0xc0, 0xc7, 0xc8, 0xc9 };
    public static readonly List<int[,]> Bitmaplist = new List<int[,]>
    {
        new int[9,2]  { { 5, 6 }, {3,1 }, { 4, 0 }, { 5, 1 }, { 6, 1 }, { 6, 1 }, { 5, 2 }, { 3, 0 }, { 0, 0 } },
        new int[13,2] { { 6, 6 }, {4,0 }, { 5, 0 }, { 6, 1 }, { 6, 1 }, { 6, 0 }, { 7, 1 }, { 7, 1 }, { 7, 2 }, { 5, 1 }, { 4, 1 }, { 4, 0 }, { 0, 0 } },
        new int[14,2] { { 6, 5 }, {5,1 }, { 5, 0 }, { 7, 0 }, { 8, 1 }, { 8, 0 }, { 9, 1 }, { 8, 1 }, { 7, 0 }, { 8, 1 }, { 8, 1 }, { 7, 2 }, { 5, 0 }, { 0, 0 } },
        new int[29,2] { { 15, 6 },{8,0 }, { 8, 1 }, { 7, 0 }, { 7, 0 }, { 7, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 9, 0 }, { 9, 0 }, { 9, 1 }, { 8, 0 }, { 9, 0 }, { 9, 0 }, { 9, 1 },
            { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 0 }, { 8, 1 }, { 6, 2 }, { 4, 0 }, { 0, 0 }  }
    };
  }
}
