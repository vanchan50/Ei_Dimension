using System.Collections.Generic;

namespace DIOS.Core
{
  public static class CommandLists
  {
    public static readonly Dictionary<string, CommandStruct> MainCmdTemplatesDict = new Dictionary<string, CommandStruct>()
    {
      { "Sheath",               new CommandStruct{ Code=0xd0, Command=0x00, Parameter=0, FParameter=0} },
      { "SampleA",              new CommandStruct{ Code=0xd1, Command=0x00, Parameter=0, FParameter=0} },
      { "SampleB",              new CommandStruct{ Code=0xd2, Command=0x00, Parameter=0, FParameter=0} },
      { "Idex",                 new CommandStruct{ Code=0xd7, Command=0x00, Parameter=0, FParameter=0} },
      { "AlignMotor",           new CommandStruct{ Code=0xdc, Command=0x00, Parameter=0, FParameter=0} },
      { "MotorX",               new CommandStruct{ Code=0xdd, Command=0x00, Parameter=0, FParameter=0} },
      { "MotorY",               new CommandStruct{ Code=0xde, Command=0x00, Parameter=0, FParameter=0} },
      { "MotorZ",               new CommandStruct{ Code=0xdf, Command=0x00, Parameter=0, FParameter=0} },
      { "Sheath Empty Prime",   new CommandStruct{ Code=0xe2, Command=0x00, Parameter=0, FParameter=0} },
      { "Set Property",         new CommandStruct{ Code=0x00, Command=0x02, Parameter=0, FParameter=0} },
      { "Get Property",         new CommandStruct{ Code=0x00, Command=0x01, Parameter=0, FParameter=0} },
      { "Set Temporary",        new CommandStruct{ Code=0x00, Command=0x03, Parameter=0, FParameter=0} }
    };
  }
}