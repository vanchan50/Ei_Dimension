using System;

namespace MicroCy
{
  internal class ConnectionFactory
  {
    public static ISerial MakeNewConnection(Type t)
    {
      if (t == typeof(USBConnection))
        return new USBConnection();
      return new USBConnection();
    }
  }
}
