using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ei_Dimension
{
  public class Program
  {
    public static SplashScreen SplashScreen { get; } = new SplashScreen(@"/Icons/Splash.png");
    [STAThread]
    public static void Main(string[] args)
    {
    //  SplashScreen.Show(false, true);
      var app = new App();
      app.InitializeComponent();
      _ = app.Run();
    }
  }
}
