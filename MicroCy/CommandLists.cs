using System.Collections.Generic;

namespace DIOS.Core
{
  public static class CommandLists
  {
    public static readonly Dictionary<string, CommandStruct> MainCmdTemplatesDict = new Dictionary<string, CommandStruct>()
    {
      { "Sync",                 new CommandStruct{ Code=0xfa, Command=0x00, Parameter=0, FParameter=0} },
      { "Sheath",               new CommandStruct{ Code=0xd0, Command=0x00, Parameter=0, FParameter=0} },
      { "SampleA",              new CommandStruct{ Code=0xd1, Command=0x00, Parameter=0, FParameter=0} },
      { "SampleB",              new CommandStruct{ Code=0xd2, Command=0x00, Parameter=0, FParameter=0} },
      { "RefreshDac",           new CommandStruct{ Code=0xd3, Command=0x00, Parameter=0, FParameter=0} },
      { "SetNextWell",          new CommandStruct{ Code=0xd4, Command=0x00, Parameter=0, FParameter=0} },
      { "SetBaseline",          new CommandStruct{ Code=0xd5, Command=0x00, Parameter=0, FParameter=0} },
      { "SaveToFlash",          new CommandStruct{ Code=0xd6, Command=0x00, Parameter=0, FParameter=0} },
      { "Idex",                 new CommandStruct{ Code=0xd7, Command=0x00, Parameter=0, FParameter=0} },
      { "InitOpVars",           new CommandStruct{ Code=0xd8, Command=0x00, Parameter=0, FParameter=0} },
      { "FlushCmdQueue",        new CommandStruct{ Code=0xd9, Command=0x00, Parameter=0, FParameter=0} },
      { "Start Sampling",       new CommandStruct{ Code=0xda, Command=0x00, Parameter=0, FParameter=0} },
      { "End Sampling",         new CommandStruct{ Code=0xdb, Command=0x00, Parameter=0, FParameter=0} },
      { "AlignMotor",           new CommandStruct{ Code=0xdc, Command=0x00, Parameter=0, FParameter=0} },
      { "MotorX",               new CommandStruct{ Code=0xdd, Command=0x00, Parameter=0, FParameter=0} },
      { "MotorY",               new CommandStruct{ Code=0xde, Command=0x00, Parameter=0, FParameter=0} },
      { "MotorZ",               new CommandStruct{ Code=0xdf, Command=0x00, Parameter=0, FParameter=0} },
      { "Startup",              new CommandStruct{ Code=0xe0, Command=0x00, Parameter=0, FParameter=0} },
      { "Prime",                new CommandStruct{ Code=0xe1, Command=0x00, Parameter=0, FParameter=0} },
      { "Sheath Empty Prime",   new CommandStruct{ Code=0xe2, Command=0x00, Parameter=0, FParameter=0} },
      { "Wash A",               new CommandStruct{ Code=0xe3, Command=0x00, Parameter=0, FParameter=0} },
      { "Wash B",               new CommandStruct{ Code=0xe4, Command=0x00, Parameter=0, FParameter=0} },
      { "Eject Plate",          new CommandStruct{ Code=0xe5, Command=0x00, Parameter=0, FParameter=0} },
      { "Load Plate",           new CommandStruct{ Code=0xe6, Command=0x00, Parameter=0, FParameter=0} },
      { "Position Well Plate",  new CommandStruct{ Code=0xe7, Command=0x00, Parameter=0, FParameter=0} },
      { "Aspirate Syringe A",   new CommandStruct{ Code=0xe8, Command=0x00, Parameter=0, FParameter=0} },
      { "Aspirate Syringe B",   new CommandStruct{ Code=0xe9, Command=0x00, Parameter=0, FParameter=0} },
      { "Read A",               new CommandStruct{ Code=0xea, Command=0x00, Parameter=0, FParameter=0} },
      { "Read B",               new CommandStruct{ Code=0xeb, Command=0x00, Parameter=0, FParameter=0} },
      { "Read A Aspirate B",    new CommandStruct{ Code=0xec, Command=0x00, Parameter=0, FParameter=0} },
      { "Read B Aspirate A",    new CommandStruct{ Code=0xed, Command=0x00, Parameter=0, FParameter=0} },
      { "End Bead Read A",      new CommandStruct{ Code=0xee, Command=0x00, Parameter=0, FParameter=0} },
      { "End Bead Read B",      new CommandStruct{ Code=0xef, Command=0x00, Parameter=0, FParameter=0} },
      { "Set Property",         new CommandStruct{ Code=0x00, Command=0x02, Parameter=0, FParameter=0} },
      { "Get Property",         new CommandStruct{ Code=0x00, Command=0x01, Parameter=0, FParameter=0} },
      { "Set FProperty",        new CommandStruct{ Code=0x00, Command=0x02, Parameter=0, FParameter=0} },
      { "Set Temporary",        new CommandStruct{ Code=0x00, Command=0x03, Parameter=0, FParameter=0} }
    };
  }
}