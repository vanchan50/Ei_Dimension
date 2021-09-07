using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ei_Dimension
{
  public class Program
  {
    [STAThread]
    public static void Main(string[] args)
    {
      var app = new App();
      app.InitializeComponent();
      _ = app.Run();
    }
  }
}
