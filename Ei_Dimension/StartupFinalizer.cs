﻿using Ei_Dimension.ViewModels;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Ei_Dimension.Controllers;
using Ei_Dimension.Graphing.HeatMap;
using MadWizard.WinUSBNet;
using DIOS.Core.HardwareIntercom;
using DIOS.Application;
using DIOS.Core;
using System.IO;

namespace Ei_Dimension
{
  internal static class StartupFinalizer
  {
    public static bool SettingsWiped;
    private static bool _done;
    private static USBNotifier usbnotif;
    /// <summary>
    /// Finish loading the UI. Should be called only once, after all the views have been loaded.
    /// Constructs the UI update timer
    /// </summary>
    public static async void Run()
    {
      if (_done)
        throw new Exception("StartupFinalizer can only be called once");

      SetupDevice();
      if (!System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
        App.DiosApp.Device.StartSelfTest();

      if (Program.SpecializedVer == CompanyID.China)
      {
        MainWindow.Instance.CompanyLogo.Source = new BitmapImage(new Uri(@"/Ei_Dimension;component/Icons/Emission_LogoCh.png", UriKind.Relative));
        MainWindow.Instance.CompanyLogo.Margin = new Thickness(1730, 1030, 5, 0);
        MainWindow.Instance.InstrumentLogo.Source = new BitmapImage(new Uri(@"/Ei_Dimension;component/Icons/dimension flow analyzer logoCh.png", UriKind.Relative));
        Views.ResultsView.Instance.InstrumentLogo.Source = new BitmapImage(new Uri(@"/Ei_Dimension;component/Icons/dimension flow analyzer logoCh.png", UriKind.Relative));
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

      IntPtr windowHandle = new System.Windows.Interop.WindowInteropHelper(App.Current.MainWindow).Handle;
      
      usbnotif = new USBNotifier(windowHandle, Guid.ParseExact("F70242C7-FB25-443B-9E7E-A4260F373982", "D"));
      usbnotif.Arrival += Usbnotif_Arrival;
      usbnotif.Removal += Usbnotif_Removal;

      HideChannels();
        #if DEBUG
        Notification.ShowError("Motor out of position messages are suppressed in StartupFinalizer");
        #endif

      var selfTestResult = await App.DiosApp.Device.GetSelfTestResultAsync();

        App.Logger.Log("Motor out of position messages are suppressed");
        selfTestResult.MotorX = null;
        selfTestResult.MotorY = null;
        selfTestResult.MotorZ = null;

      string selfTestErrorMessage = SelfTestErrorDecoder.Decode(selfTestResult);
      if (selfTestErrorMessage != null)
        Notification.ShowError(selfTestErrorMessage);
      
      App.Logger.Log($"Detected Board Rev v{App.DiosApp.Device.BoardVersion}");
      if(App.DiosApp.Device.FirmwareVersion != null)
        MainViewModel.AppVersion += App.DiosApp.Device.FirmwareVersion;
      App.Logger.Log(MainViewModel.AppVersion);
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

      App.DiosApp.Control = (SystemControl)Settings.Default.SystemControl;
      App.DiosApp.TerminationType = (Termination)Settings.Default.EndRead;
      App.DiosApp.MinPerRegion = Settings.Default.MinPerRegion;
      App.DiosApp.TotalBeadsToCapture = Settings.Default.BeadsToCapture;
      App.DiosApp.Device.MaxPressure = Settings.Default.MaxPressure;
      App.DiosApp.Device.ReporterScaling = Settings.Default.ReporterScaling;
      var hiSensChannel = Settings.Default.SensitivityChannelB ? HiSensitivityChannel.GreenB : HiSensitivityChannel.GreenC;
      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.HiSensitivityChannel, hiSensChannel);
      var map = App.DiosApp.MapController.ActiveMap;
      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.Attenuation, map.calParams.att);
      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.DNRTransition, map.calParams.DNRTrans);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SampleSyringeType);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SampleSyringeSize);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SheathFlushVolume);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.TraySteps);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.IsWellEdgeAgitateActive);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.DistanceToWellEdge);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.WellEdgeDeltaHeight);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FlushCycles);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.IsFlowCellInverted);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopAVolume);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopBVolume);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopAToPickupNeedle);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopBToPickupNeedle);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopAToFlowcellBase);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.LoopBToFlowcellBase);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.FlowCellNeedleVolume);
      App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FluidicPathLength, FluidicPathLength.PickupNeedleVolume);
      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.WashRepeatsAmount, 1); //1 is the default. same as in the box in dashboard
      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.AgitateRepeatsAmount, 1); //1 is the default. same as in the box in dashboard
      //App.DiosApp.Device.MainCommand("Set Property", code: 0x97, parameter: 1170);  //set current limit of aligner motors if leds are off //0x97 no longer used
    }

    private static void Usbnotif_Removal(object sender, USBEvent e)
    {
      App.DiosApp.Device.DisconnectedUSB();
    }

    private static void Usbnotif_Arrival(object sender, USBEvent e)
    {
      App.DiosApp.Device.ReconnectUSB();
    }

    private static void WipedSettingsMessage()
    {
      Notification.Show("User data was Corrupted.\nDefault Values Restored.\nPlease check the Instrument Settings");
    }

    private static void HideChannels()
    {
      ChannelOffsetViewModel.Instance.OldBoardOffsetsVisible = Visibility.Hidden;
      Views.ChannelOffsetView.Instance.SlidersSP.Visibility = Visibility.Visible;
      //Views.ChannelOffsetView.Instance.BaselineSP.Width = 180;
      Views.ChannelOffsetView.Instance.AvgBgSP.Width = 180;
    }
  }
}