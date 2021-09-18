using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCy
{
  public class ConnectionFactory
  {
    public static ISerial MakeNewConnection(Type t)
    {
      //if (t == typeof(USBConnection))
      return new USBConnection();
    }
  }
}
