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
    public static int SpecializedVer = 1;
    public static SplashScreen SplashScreen { get; private set; }
    [STAThread]
    public static void Main(string[] args)
    {
      if (SpecializedVer == 0)
        SplashScreen = new SplashScreen(@"/Icons/Splash.png");
      else
        SplashScreen = new SplashScreen(@"/Icons/SplashCh.png");
      SplashScreen.Show(false, true);
      var app = new App();
      app.InitializeComponent();
      _ = app.Run();
    }
  }
}
