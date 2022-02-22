using System;

namespace DIOS.Core
{
  internal static class ConnectionFactory
  {
    public static ISerial MakeNewConnection(Type t)
    {
      if (t == typeof(USBConnection))
        return new USBConnection();
      return new USBConnection();
    }
  }
}
