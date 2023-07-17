using System;
using System.Windows;

namespace Ei_Dimension
{
  public class Program
  {
    internal static CompanyID SpecializedVer = CompanyID.US;
    public static SplashScreen SplashScreen { get; private set; }
    private static App _app = new App();
    [STAThread]
    public static void Main(string[] args)
    {
      if (SpecializedVer == CompanyID.US)
        SplashScreen = new SplashScreen(@"/Icons/Splash.png");
      else if (SpecializedVer == CompanyID.China)
        SplashScreen = new SplashScreen(@"/Icons/SplashCh.png");
#if !DEBUG
      SplashScreen.Show(false, true);
#endif
      try
      {
        _app.InitializeComponent();
        _ = _app.Run();
      }
      catch (Exception e)
      {
        App.Logger.Log(e.Message);
        App.Logger.Log(e.StackTrace);
      }
    }
  }
}