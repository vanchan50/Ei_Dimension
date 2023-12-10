using Ei_Dimension.ViewModels;
using System;
using Ei_Dimension.Controllers;
using Ei_Dimension.Graphing.HeatMap;
using DIOS.Core.HardwareIntercom;
using DIOS.Core;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using DIOS.Application.SerialIO;

namespace Ei_Dimension;

internal static class StartupFinalizer
{
  public static bool SettingsWiped;
  private static bool _done;
  private static readonly BitmapImage ChLogo = new(new Uri(@"/Ei_Dimension;component/Icons/Emission_LogoCh.png", UriKind.Relative));
  private static readonly BitmapImage ChInstrumentLogo = new(new Uri(@"/Ei_Dimension;component/Icons/dimension flow analyzer logoCh.png", UriKind.Relative));
  /// <summary>
  /// Finish loading the UI. Should be called only once, after all the views have been loaded.
  /// Constructs the UI update timer
  /// </summary>
  public static void Run()
  {
    if (_done)
      throw new Exception("StartupFinalizer can only be called once");
    _done = true;

    SetupDevice();
    if (!System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
      App.DiosApp.Device.Hardware.SendCommand(DeviceCommandType.Startup);
    Settings.Default.SanityCheckEnabled = false;
    Settings.Default.Save();
    if (!Settings.Default.SanityCheckEnabled)
    {
      UserInputHandler._disableSanityCheck = true;
      App.Logger.Log("Sanity check disabled");
    }

    if (Program.SpecializedVer == CompanyID.China)
    {
      OverrideLogosWithChinese();
    }

    App.MapRegions = new MapRegionsController(
      Views.SelRegionsView.Instance.RegionsBorder,
      Views.SelRegionsView.Instance.RegionsNamesBorder,
      Views.ResultsView.Instance.Table,
      Views.DashboardView.Instance.DbActiveRegionNo,
      Views.DashboardView.Instance.DbActiveRegionName,
      Views.VerificationView.Instance.VerificationNums,
      Views.VerificationView.Instance.VerificationReporterValues,
      Views.NormalizationView.Instance.NormalizationNums,
      Views.NormalizationView.Instance.NormalizationMFIValues);

    HeatMapAPI.API.SetupChart(Views.ResultsView.Instance);

    ActiveRegionsStatsController.Instance.DisplayCurrentBeadStats();
    PlatePictogramViewModel.Instance.PlatePictogram.SetGrid(Views.PlatePictogramView.Instance.DrawingPlate);
    PlatePictogramViewModel.Instance.PlatePictogram.SetWarningGrid(Views.PlatePictogramView.Instance.WarningGrid);
    Views.CalibrationView.Instance.clmap.DataContext = DashboardViewModel.Instance;
    Views.VerificationView.Instance.clmap.DataContext = DashboardViewModel.Instance;
    Views.ChannelsView.Instance.clmap.DataContext = DashboardViewModel.Instance;
    Views.NormalizationView.Instance.clmap.DataContext = DashboardViewModel.Instance;
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

    LanguageSwap.SetLanguage(MaintenanceViewModel.Instance.LanguageItems[Settings.Default.Language].Locale);
    Views.ExperimentView.Instance.DbButton.IsChecked = true;
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.CalibrationMargin);

    App.DiosApp.WorkOrderController.OnAppLoaded();
    Program.SplashScreen.Close(TimeSpan.FromMilliseconds(1000));
      
    UITimer.Start();

    if(SettingsWiped)
      WipedSettingsMessage();

    IntPtr windowHandle = new System.Windows.Interop.WindowInteropHelper(App.Current.MainWindow).Handle;
    USBWatchdog.Setup(windowHandle, App.DiosApp.ReconnectUSB, App.DiosApp.DisconnectedUSB);

    //App.Logger.Log($"Detected Board Rev v{App.DiosApp.Device.BoardVersion}");
    App.Logger.Log(MainViewModel.Instance.AppVersion);
#if DEBUG
    App.Logger.Log("DEBUG MODE");
#endif
  }

  private static void SetupDevice()
  {
    if (App.DiosApp.Device == null)
      throw new Exception("Device not initialized");

    App.DiosApp.Device.Init();

    if (Directory.Exists(Settings.Default.LastOutFolder))
      App.DiosApp.Publisher.Outdir = Settings.Default.LastOutFolder;
    else
    {
      Settings.Default.LastOutFolder = App.DiosApp.Publisher.Outdir;
      Settings.Default.Save();
    }
    App.DiosApp.Publisher.Outfilename = Settings.Default.SaveFileName;
    App.DiosApp.Publisher.IsPlateReportPublishingActive = Settings.Default.PlateReport;
    App.DiosApp.Publisher.IsBeadEventPublishingActive = Settings.Default.Everyevent;
    App.DiosApp.Publisher.IsResultsPublishingActive = Settings.Default.RMeans;
    App.DiosApp.Publisher.IsLegacyPlateReportPublishingActive = Settings.Default.LegacyPlateReport;
    App.DiosApp.Publisher.IsOnlyClassifiedBeadsPublishingActive = Settings.Default.OnlyClassifed;
    DashboardViewModel.Instance.SysControlItems[Settings.Default.SystemControl].Click(5);
    DashboardViewModel.Instance.EndReadItems[Settings.Default.EndRead].Click(6);
    App.DiosApp.Terminator.MinPerRegion = Settings.Default.MinPerRegion;
    App.DiosApp.Terminator.TotalBeadsToCapture = Settings.Default.BeadsToCapture;
    App.DiosApp.Terminator.TerminationTime = Settings.Default.TerminationTimer;
    App.DiosApp.Device.ReporterScaling = Settings.Default.ReporterScaling;
    var hiSensChannel = Settings.Default.SensitivityChannelB ? HiSensitivityChannel.GreenB : HiSensitivityChannel.GreenC;
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.HiSensitivityChannel, hiSensChannel);
    var map = App.DiosApp.MapController.ActiveMap;
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.Attenuation, map.calParams.att);
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.DNRTransition, map.calParams.DNRTrans);
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.UseWashStation, Settings.Default.UseWashStation);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelConfiguration);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SampleSyringeType);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SampleSyringeSize);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SheathFlushVolume);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.TraySteps);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.IsWellEdgeAgitateActive);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.DistanceToWellEdge);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.WellEdgeDeltaHeight);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FlushCycles);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.WashStationXCenterCoordinate);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.GreenAVoltage);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.WashStationDepth);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.PressureWarningLevel);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorX, MotorParameterType.EncoderSteps);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorY, MotorParameterType.EncoderSteps);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorZ, MotorParameterType.EncoderSteps);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopAVolume);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopBVolume);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopAToPickupNeedle);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopBToPickupNeedle);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopAToFlowcellBase);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopBToFlowcellBase);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.FlowCellNeedleVolume);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.SpacerSlug);
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.Volume, VolumeType.Sample, 0);
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.Volume, VolumeType.Wash, 0);
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.Volume, VolumeType.ProbeWash, 0);
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.Volume, VolumeType.Agitate, 0);
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.WashRepeatsAmount, 1); //1 is the default. same as in the box in dashboard
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ProbewashRepeatsAmount, 0); //0 is the default. same as in the box in dashboard
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.AgitateRepeatsAmount, 1); //1 is the default. same as in the box in dashboard
    //App.DiosApp.Device.MainCommand("Set Property", code: 0x97, parameter: 1170);  //set current limit of aligner motors if leds are off //0x97 no longer used

  }

  private static void WipedSettingsMessage()
  {
    Notification.Show("User data was Corrupted.\nDefault Values Restored.\nPlease check the Instrument Settings");
  }

  private static void OverrideLogosWithChinese()
  {
    MainWindow.Instance.CompanyLogo.Source = ChLogo;
    MainWindow.Instance.CompanyLogo.Margin = new Thickness(1730, 1030, 5, 0);
    MainWindow.Instance.InstrumentLogo.Source = ChInstrumentLogo;
    Views.ResultsView.Instance.InstrumentLogo.Source = ChInstrumentLogo;
  }
}