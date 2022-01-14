using Ei_Dimension.Models;
using Ei_Dimension.ViewModels;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Ei_Dimension
{
  internal static class StartupFinalizer
  {
    public static bool SettingsWiped;
    private static bool _done;
    /// <summary>
    /// Finish loading the UI. Should be called only once, after all the views have been loaded.
    /// Constructs the UI update timer
    /// </summary>
    public static void Run()
    {
      if (_done)
        throw new Exception("StartupFinalizer can only be called once");

      if (Program.SpecializedVer == CompanyID.China)
      {
        MainWindow.Instance.CompanyLogo.Source = new BitmapImage(new Uri(@"/Ei_Dimension;component/Icons/Emission_LogoCh.png", UriKind.Relative));
        MainWindow.Instance.CompanyLogo.Margin = new Thickness(1730, 1030, 5, 0);
        MainWindow.Instance.InstrumentLogo.Source = new BitmapImage(new Uri(@"/Ei_Dimension;component/Icons/dimension flow analyzer logoCh.png", UriKind.Relative));
        Views.ResultsView.Instance.InstrumentLogo.Source = new BitmapImage(new Uri(@"/Ei_Dimension;component/Icons/dimension flow analyzer logoCh.png", UriKind.Relative));
      }

      App.MapRegions = MapRegions.Create(
        Views.SelRegionsView.Instance.RegionsBorder,
        Views.SelRegionsView.Instance.RegionsNamesBorder,
        Views.ResultsView.Instance.Table,
        Views.DashboardView.Instance.DbActiveRegionNo,
        Views.DashboardView.Instance.DbActiveRegionName,
        Views.VerificationView.Instance.VerificationNums,
        Views.VerificationView.Instance.VerificationReporterValues);
      ResultsViewModel.Instance.PlatePictogram.SetGrid(Views.ResultsView.Instance.DrawingPlate);
      ResultsViewModel.Instance.PlatePictogram.SetWarningGrid(Views.ResultsView.Instance.WarningGrid);
      Views.CalibrationView.Instance.clmap.DataContext = DashboardViewModel.Instance;
      Views.VerificationView.Instance.clmap.DataContext = DashboardViewModel.Instance;
      Views.ChannelsView.Instance.clmap.DataContext = DashboardViewModel.Instance;
      //load map parameters
      if (Settings.Default.LastTemplate != "None")
      {
        TemplateSelectViewModel.Instance.SelectedItem = Settings.Default.LastTemplate;
        TemplateSelectViewModel.Instance.LoadTemplate();
      }
      else
      {
        DashboardViewModel.Instance.ClassiMapItems[Settings.Default.DefaultMap].Click(2);
      }

      ResultsViewModel.Instance.FillWorldMaps();
      App.SetLanguage(MaintenanceViewModel.Instance.LanguageItems[Settings.Default.Language].Locale);
      Views.ExperimentView.Instance.DbButton.IsChecked = true;
      App.Device.MainCommand("Get Property", code: 0x01);
      //3D plot TRS transforms
      var matrix = Views.ResultsView.Instance.AnalysisPlot.ContentTransform.Value;
      matrix.Rotate(new System.Windows.Media.Media3D.Quaternion(new System.Windows.Media.Media3D.Vector3D(0, 1, 0), 90));
      matrix.Rotate(new System.Windows.Media.Media3D.Quaternion(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), 40));
      matrix.Rotate(new System.Windows.Media.Media3D.Quaternion(new System.Windows.Media.Media3D.Vector3D(0, 1, 0), -15));
      matrix.Translate(new System.Windows.Media.Media3D.Vector3D(-100, 100, 0));
      ((System.Windows.Media.Media3D.MatrixTransform3D)Views.ResultsView.Instance.AnalysisPlot.ContentTransform).Matrix = matrix;

      App.CheckAvailableWorkOrders();
      Program.SplashScreen.Close(TimeSpan.FromMilliseconds(1000));
      
      UITimer.Start();
      _done = true;

      if(SettingsWiped)
        WipedSettingsMessage();
    }

    private static void WipedSettingsMessage()
    {
      Notification.Show("User data was Corrupted.\nDefault Values Restored.\nPlease check the Instrument Settings");
    }
  }
}
