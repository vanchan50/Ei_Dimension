using System.Collections.Generic;

namespace DIOS.Core
{
  public static class CommandLists
  {
    public static readonly Dictionary<string, CommandStruct> MainCmdTemplatesDict = new Dictionary<string, CommandStruct>()
    {
      { "Set Property",         new CommandStruct{ Code=0x00, Command=0x02, Parameter=0, FParameter=0} },
      { "Get Property",         new CommandStruct{ Code=0x00, Command=0x01, Parameter=0, FParameter=0} },
    };
  }
}