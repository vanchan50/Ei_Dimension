using System;
using System.Windows;

namespace Ei_Dimension
{
  public class Program
  {
    internal static CompanyID SpecializedVer = CompanyID.US;
    public const string BUILD = "1.4.0.7";
    public static SplashScreen SplashScreen { get; private set; }
    [STAThread]
    public static void Main(string[] args)
    {
      if (SpecializedVer == CompanyID.US)
        SplashScreen = new SplashScreen(@"/Icons/Splash.png");
      else if (SpecializedVer == CompanyID.China)
        SplashScreen = new SplashScreen(@"/Icons/SplashCh.png");
#if !DEBUG
      SplashScreen.Show(false, true);
      var app = new App();
      app.InitializeComponent();
      _ = app.Run();
#endif
#if DEBUG
      try
      {
        var app = new App();
        app.InitializeComponent();
        _ = app.Run();
      }
      catch (Exception e)
      {
        Console.WriteLine(e.StackTrace);
      }
#endif
    }
  }
}