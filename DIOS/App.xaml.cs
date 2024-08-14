using System.Reflection;
using System;
using System.Windows;
using System.IO;
using System.Configuration;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Extensions.Hosting;
using Ei_Dimension.ViewModels;
using Ei_Dimension.Cache;
using Ei_Dimension.Controllers;
using DIOS.Core;
using DIOS.Core.HardwareIntercom;
using DIOS.Application;
using DIOS.Application.Domain;
using DIOS.Application.FileIO;
using Ei_Dimension.Models;

namespace Ei_Dimension;

public partial class App : Application
{
  public static IHost AppHost { get; }
  public static DIOSApp DiosApp { get; private set; }
  public static (PropertyInfo prop, object VM) NumpadShow { get; set; }
  public static (PropertyInfo prop, object VM) KeyboardShow { get; set; }
  public static ResultsCache Cache { get; } = new ();
  public static MapRegionsController MapRegions { get; set; }
  /// <summary>doubles the variable in device, but for UI usage</summary>
  public static bool ChannelRedirectionEnabled { get; private set; } = false;
  public static event EventHandler PostMeasurementAction;
  public static ILogger Logger { get; private set; }
  public static WorkOrder CurrentWorkOrder { get; set; }
  public static bool _nextWellWarning = false;
  public static readonly HashSet<char> InvalidChars = new();

  private static bool _workOrderPending = false;
  private static readonly IncomingUpdateHandler IncomingUpdateHandler = new ();


  static App()
  {
    InvalidChars.UnionWith(Path.GetInvalidFileNameChars());
    CorruptSettingsChecker();
  }

  public App()
  {
    var drives = DriveInfo.GetDrives();
    var appFolder = Path.Combine($"{drives[0].Name}", "Emissioninc", Environment.MachineName);
    Logger = new Logger(appFolder);
    DiosApp = new(appFolder, Logger);
    Logger.Log($"Application version: {DiosApp.BUILD}");

    StatisticsExtension.TailDiscardPercentage = Settings.Default.StatisticsTailDiscardPercentage;

    DiosApp.MapController.OnAppLoaded(Settings.Default.DefaultMap);
    DiosApp.Device.StartingToReadWell += StartingToReadWellEventHandler;
    DiosApp.Device.FinishedReadingWell += FinishedReadingWellEventHandler;
    DiosApp.Device.FinishedMeasurement += FinishedMeasurementEventHandler;
    DiosApp.MapController.ChangedActiveMap += MapChangedEventHandler;
    DiosApp.Device.ParameterUpdate += IncomingUpdateHandler.ParameterUpdateEventHandler;
    DiosApp.WorkOrderController.NewWorkOrder += OnNewWorkOrder;

    App.Current.Dispatcher.UnhandledException += DispatcherExceptionHandler;
  }

  private static void CorruptSettingsChecker()
  {
    try
    {
      Settings.Default.Reload();
      var value = Settings.Default.DefaultMap;
    }
    catch (ConfigurationErrorsException ex)
    {
      string filename = ((ConfigurationErrorsException)ex.InnerException).Filename;
      File.Delete(filename);
      StartupFinalizer.SettingsWiped = true;
      MessageBox.Show("Settings file is Corrupted.\n\nPlease Restart the Application.");
      Environment.Exit(1);
    }
  }

  public static void RunInUIThreadSync(Action method)
  {
    App.Current.Dispatcher.Invoke(method);
  }

  public static void RunInUIThreadAsync(Delegate method)
  {
    App.Current.Dispatcher.BeginInvoke(method);
  }

  public static void SetActiveMap(string mapName)
  {
    var index = DiosApp.MapController.GetMapIndexByName(mapName);
    if (index < 0)
      return;
    var map = DiosApp.MapController.GetMapByIndex(index);
    if (map is null)
      return;
    DiosApp.MapController.SetMap(map);
    Settings.Default.DefaultMap = index;
    Settings.Default.Save();
  }

  public static void SetSystemControl(byte num)
  {
    DiosApp.Control = (SystemControl)num;
    Settings.Default.SystemControl = num;
    Settings.Default.Save();
    if (DiosApp.Control == SystemControl.WorkOrder)
    {
      ExperimentViewModel.Instance.WellSelectVisible = Visibility.Hidden;
      MainButtonsViewModel.Instance.EnableStartButton(false);
      MainButtonsViewModel.Instance.ShowScanButton();
    }
    else
    {
      ExperimentViewModel.Instance.WellSelectVisible = Visibility.Visible;
      MainButtonsViewModel.Instance.EnableStartButton(true);
      MainButtonsViewModel.Instance.HideScanButton();
    }
  }

  public static void SetTerminationType(byte num)
  {
    DiosApp.Terminator.TerminationType = (Termination)num;
    DashboardViewModel.Instance.EndReadVisibilitySwitch();
    Settings.Default.EndRead = num;
    Settings.Default.Save();
  }

  public static void SetChannelConfig(ChannelConfiguration chConfig)
  {
    var OEMMode = chConfig is ChannelConfiguration.OEMA
                                or ChannelConfiguration.OEMPMT;

    DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelConfiguration, chConfig);
    SetOEMMode(OEMMode);
  }

  public static void Export(DevExpress.Xpf.Charts.ChartControlBase chart, in int dpi)
  {
    var options = new DevExpress.XtraPrinting.ImageExportOptions
    {
      TextRenderingMode = DevExpress.XtraPrinting.TextRenderingMode.SingleBitPerPixelGridFit,
      Resolution = dpi,
      Format = new System.Drawing.Imaging.ImageFormat(System.Drawing.Imaging.ImageFormat.Png.Guid)
    };
    try
    {
      var path = DiosApp.Publisher.Outdir + @"\SavedImages";
      if (DiosApp.Publisher.OutputDirectoryExists(path))
      {
        chart.ExportToImage($"{path}\\{DiosApp.Publisher.Date}.png", options);
      }
    }
    catch
    {
      Current.Dispatcher.Invoke(() =>
        Notification.Show("Save failed"));
    }
  }

  public static void HideNumpad()
  {
    UserInputHandler.InputSanityCheck();
    NumpadShow.prop.SetValue(NumpadShow.VM, Visibility.Hidden);
  }

  public static void HideKeyboard()
  {
    UserInputHandler.InputSanityCheck();
    KeyboardShow.prop.SetValue(KeyboardShow.VM, Visibility.Hidden);
  }

  public static void UnfocusUIElement()
  {
    if (Ei_Dimension.MainWindow.Instance == null)
      return;
    System.Windows.Input.FocusManager.SetFocusedElement(System.Windows.Input.FocusManager.GetFocusScope(Ei_Dimension.MainWindow.Instance), null);
    System.Windows.Input.Keyboard.ClearFocus();
  }

  public static void SetOEMMode(bool On)
  {
    DiosApp._beadProcessor._channelRedirectionEnabled = On;
    ChannelRedirectionEnabled = On;
    LanguageSwap.TranslateChannelsOffsetVM();//swaps between red <-> green
    LanguageSwap.TranslateChannelsVM();
    LanguageSwap.TranslateResultsVM();
    LanguageSwap.TranslateStatisticsTableVM();
    DiosApp.Publisher.IsOEMModeActive = On;
    //ResultsViewModel.Instance.SwapXYNamesToOEM(On);
  }

  /*
  private static void WorkOrderHandler()
  {
    //see if work order is available
    if (DashboardViewModel.Instance.SelectedSystemControlIndex != 0)
    {
      if (Device.IsNewWorkOrder())
      {
        DashboardViewModel.Instance.WorkOrder[0] = Device.WorkOrderName;
        _workOrderPending = true;
      }
    }
    if (_workOrderPending)
    {
      if (DashboardViewModel.Instance.SelectedSystemControlIndex == 1)  //no barcode required so allow start
      {
        MainButtonsViewModel.Instance.StartButtonEnabled = true;
        _workOrderPending = false;
      }
      else if (DashboardViewModel.Instance.SelectedSystemControlIndex == 2)    //barcode required
      {
        //if (videogoing & (video != null))
        //{
        //  BarcodeResult plateGUID = BarcodeReader.QuicklyReadOneBarcode(video, BarcodeEncoding.PDF417, true);
        //  if (plateGUID != null)
        //    pltNumbertb.Text = plateGUID.Value;
        //}
        //else
        //  DashboardViewModel.Instance.ValidateBCodeButtonEnabled = true;
        //  if (DashboardViewModel.Instance.WorkOrder[0] == pltNumbertb.Text)
        //  {
        //    DashboardViewModel.Instance.StartButtonEnabled = true;
        //    _workOrderPending = false;
        //    Device.MainCommand("Set Property", code: 0x17); //leds off
        //    DashboardViewModel.Instance.ValidateBCodeButtonEnabled = false;
        //  }
        //  else
        //  {
        //    //  pltNumbertb.BackColor = Color.Red;
        //  }
      }
    }
  }
  */

  private void StartingToReadWellEventHandler(object sender, ReadingWellEventArgs e)
  {
    ActiveRegionsStatsController.Instance.IsMeasurementGoing = true;
    DiosApp.Terminator.TotalBeadsToCapture = (int)e.Well.BeadsToCapture;
    DiosApp.Terminator.MinPerRegion = (int)e.Well.MinPerRegion;
    DiosApp.Terminator.TerminationTime = (int)e.Well.TerminationTimer;
    DiosApp.Terminator.TerminationType = e.Well.TerminationType;

    Logger.Log($"Starting to read well {e.Well.CoordinatesString()} with Params:\n\t\t\tTermination: {DiosApp.Terminator.TerminationType}\n\t\t\tMinPerRegion: {DiosApp.Terminator.MinPerRegion}\n\t\t\tBeadsToCapture: {DiosApp.Terminator.TotalBeadsToCapture}\n\t\t\tTerminationTimer: {DiosApp.Terminator.TerminationTime}");
    Logger.Log(e.Well.PrintActiveRegions());

    var wellFilePath = DiosApp.Publisher.BeadEventFile.GenerateNewFileName(e.Well);
    DiosApp.Results.StartNewWell(e.Well);
    DiosApp.ResultsProc.NewWellStarting();
    MultiTube.GetModifiedWellIndexes(e.Well, out var row, out var col);

    //ResultsViewModel.Instance.PlatePictogramIsCovered = Visibility.Visible; //TODO: temporary solution

    PlatePictogramViewModel.Instance.PlatePictogram.CurrentlyReadCell = (row, col);
    PlatePictogramViewModel.Instance.PlatePictogram.ChangeState(row, col, WellType.NowReading, GetWarningState(), wellFilePath);

    App.Current.Dispatcher.Invoke(() =>
    {
      PlatePictogramViewModel.Instance.CornerButtonClick(Models.DrawingPlate.CalculateCorner(row, col));
      ResultsViewModel.Instance.ClearGraphs();
    });
    ActiveRegionsStatsController.Instance.ResetCurrentActiveRegionsDisplayedStats();

    DiosApp.ResultsProc.StartBeadProcessing();//call after IsMeasurementGoing == true
  }

  private static Models.WellWarningState GetWarningState()
  {
    if (_nextWellWarning)
    {
      _nextWellWarning = false;
      return Models.WellWarningState.YellowWarning;
    }
    return Models.WellWarningState.OK;
  }

  public void FinishedReadingWellEventHandler(object sender, ReadingWellEventArgs e)
  {
    ActiveRegionsStatsController.Instance.IsMeasurementGoing = false;
    Logger.Log($"Finished Reading well {e.Well.CoordinatesString()}");
    DiosApp.Results.FreezeWellResults();

    Task.Run(() =>
    {
      var wellStats = DiosApp.Results.MakeWellStats();
      _ = App.Current.Dispatcher.BeginInvoke(
        ActiveRegionsStatsController.Instance.FinalUpdateRegionsProcedure, wellStats);
      DiosApp.SaveWellFiles(wellStats);
    });

    var type = DiosApp.GetWellStateForPictogram();

    MultiTube.GetModifiedWellIndexes(e.Well, out var row, out var col, proceed: true);

    //Cache.Store(row, col);
    //ResultsViewModel.Instance.PlatePictogramIsCovered = Visibility.Hidden; //TODO: temporary solution

    PlatePictogramViewModel.Instance.PlatePictogram.CurrentlyReadCell = (-1, -1);
    PlatePictogramViewModel.Instance.PlatePictogram.ChangeState(row, col, type);

    DiosApp.Publisher.PlateStatusFile.Overwrite(PlatePictogramViewModel.Instance.PlatePictogram.GetSerializedPlate());


    var allBeadsSpan = DiosApp.Results.GetAllBeadEventsAsSpan();
    var histogramPeaks = HistogramBinner.BinData(allBeadsSpan);


    var stats = DiosApp.Results.CurrentWellResults.GetChannelStats();
    var averageBackgrounds = DiosApp.Results.CurrentWellResults.GetBackgroundChannelsAverages();
    _ = Current.Dispatcher.BeginInvoke(() =>
    {
      StatisticsTableViewModel.Instance.DecodeCalibrationStats(stats, histogramPeaks, current: true);
      ChannelOffsetViewModel.Instance.DecodeBackgroundStats(averageBackgrounds);
    });

    _ = Current.Dispatcher.BeginInvoke(() =>
    {
      if (CalibrationViewModel.Instance.DoPostCalibrationRun)//only runs after a successful calibration.
      {
        //hopefully this doesn't trigger before the calibration succesful message. TODO: a proper synchronization
        CalibrationViewModel.Instance.DoPostCalibrationRun = false;
        PostMeasurementAction -= CalibrationViewModel.Instance.CalibrationSuccessPostRun;
        var report = CalibrationViewModel.Instance.FormNewCalibrationReport(true, stats);
        Task.Run(() =>
        {
          DiosApp.Publisher.CalibrationFile.CreateAndWrite(report);
        });
        CalibrationViewModel.Instance.CalibrationSuccess();
      }
    });
  }

  public void FinishedMeasurementEventHandler()
  {
    if (DiosApp.Device.Mode != OperationMode.Normal)
      DiosApp.Normalization.Restore();

    DiosApp.Results.PlateReport.completedDateTime = DateTime.Now;
    var plateReportJson = DiosApp.Results.PlateReport.JSONify();
    if (DiosApp.Control == SystemControl.Manual &&
          DiosApp.Device.Mode == OperationMode.Normal)
    {
      DiosApp.Publisher.PlateReportFile.CreateAndWrite(plateReportJson);
    }
    else if (DiosApp.Control == SystemControl.WorkOrder)
    {
      DiosApp.Publisher.PlateReportFile.CreateAndWrite(plateReportJson, CurrentWorkOrder.PlateID);
    }
    
    PlatePictogramViewModel.Instance.PlatePictogram.CurrentlyReadCell = (-1, -1);
    switch (DiosApp.Device.Mode)
    {
      case OperationMode.Normal:
        var legacyReport = DiosApp.Results.PlateReport.LegacyReport(DiosApp.Publisher.IncludeReg0InPlateSummary);
        OutputLegacyReport(legacyReport);

        if (DiosApp.RunPlateContinuously)
        {
          App.Current.Dispatcher.BeginInvoke(async () =>
          {
            await DiosApp.Device.EjectPlate();
            await Task.Delay(5000);
            await DiosApp.Device.LoadPlate();
            await MainButtonsViewModel.Instance.StartButtonClick();
          });
        }
        break;
      case OperationMode.Calibration:
        CalibrationViewModel.Instance.CalibrationFailCheck();
        break;
      case OperationMode.Verification:
        var report = VerificationViewModel.Instance.FormNewVerificationReport(DiosApp.Verificator);
        Task.Run(() =>
        {
          DiosApp.Publisher.VerificationFile.CreateAndWrite(report);
        });

        if (report.Status)
        {
          _ = Current.Dispatcher.BeginInvoke(VerificationViewModel.VerificationSuccess);
        }
        else
        {
          Current.Dispatcher.Invoke(() => Notification.ShowError(Language.Resources.Validation_Fail));
        }
        Current.Dispatcher.Invoke(DashboardViewModel.Instance.ValModeToggle);
        break;
    }

    if (DiosApp.Control == SystemControl.WorkOrder)
    {
      App.Current.Dispatcher.Invoke(() =>
      {
        DashboardViewModel.Instance.WorkOrderID[0] = "";
        CurrentWorkOrder = null;
        MainButtonsViewModel.Instance.EnableStartButton(false);
        MainButtonsViewModel.Instance.ShowScanButton();
      });
    }
    else
    {
      App.Current.Dispatcher.Invoke(() =>
      {
        MainButtonsViewModel.Instance.EnableStartButton(true);
      });
    }
    PostMeasurementAction?.Invoke(this, EventArgs.Empty);
  }

  public void MapChangedEventHandler(object sender, MapModel map)
  {
    DiosApp.SetMap(map);
    _ = Current.Dispatcher.BeginInvoke(() =>
    {
      if (ResultsViewModel.Instance != null)
        ResultsViewModel.Instance.FillWorldMaps();

      if (CalibrationViewModel.Instance != null)
        CalibrationViewModel.Instance.OnMapChanged(map);

      if (ChannelsViewModel.Instance != null)
        ChannelsViewModel.Instance.OnMapChanged(map);

      if (DashboardViewModel.Instance != null)
        DashboardViewModel.Instance.OnMapChanged(map);

      if (NormalizationViewModel.Instance != null)
        NormalizationViewModel.Instance.OnMapChanged(map);

      if (ResultsViewModel.Instance.AnalysisMap != null)
        ResultsViewModel.Instance.AnalysisMap.OnMapChanged(map);

      if (VerificationViewModel.Instance != null)
        VerificationViewModel.Instance.OnMapChanged(map);


    });
  }

  public static void OutputLegacyReport(string legacyReport)
  {
    var wholeReport = new StringBuilder()
      .AppendLine("Program,\"DIOS\"")
      .AppendLine($"Build,\"{DiosApp.BUILD}\",Firmware,\"{DiosApp.Device.FirmwareVersion}\"")
      .AppendLine($"Date,\"{DiosApp.Publisher.LegacyReportDate}\"\n")
      .AppendLine($"Instrument,\"{Environment.MachineName}\"")
      .AppendLine($"Session,\"{DiosApp.Publisher.Outfilename}\"\n\n\n\n\n\n\n")
      .AppendLine($"Samples,\"{WellsSelectViewModel.Instance.CurrentTableSize}\"\n")
      .Append(legacyReport)
      .ToString();

    if (DiosApp.Control == SystemControl.Manual)
    {
      DiosApp.Publisher.LegacyReportFile.CreateAndWrite(wholeReport);
    }
    else
    {
      DiosApp.Publisher.LegacyReportFile.CreateAndWrite(wholeReport, CurrentWorkOrder.PlateID);
    }
  }

  protected override void OnStartup(StartupEventArgs e)
  {
    DevExpress.Xpf.Core.ApplicationThemeHelper.ApplicationThemeName = DevExpress.Xpf.Core.Theme.Office2019ColorfulName;
    base.OnStartup(e);
  }

  public static void InitSTab(string tabname)
  {
    //Removing this can lead to unforseen crucial bugs in instrument operation. If so - do with extra care
    //one example is a check in CommandLists.Readertab for changed plate parameter,which could happen in manual well selection in motors tab
    Action actionList;
    switch (tabname)
    {
      case "readertab":
        actionList = () =>
        {
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.Volume, VolumeType.Wash);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.Volume, VolumeType.Sample);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.Volume, VolumeType.Agitate);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.PlateType); // plate type needed here?really? same question for the rest, actually
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.WellReadingSpeed);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.WellReadingOrder);
        };
        break;
      case "reportingtab":
        actionList = () =>
        {
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ValveCuvetDrain); //this all makes no sense. just check the commands. probably
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.WashPump);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ValveFan1);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.Pressure);
        };
        break;
      case "calibtab":
        actionList = () =>
        {
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SyringeSpeedSheath, SyringeSpeed.Normal);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SyringeSpeedSheath, SyringeSpeed.HiSpeed);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SyringeSpeedSheath, SyringeSpeed.HiSensitivity);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SyringeSpeedSheath, SyringeSpeed.Flush);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SyringeSpeedSheath, SyringeSpeed.Pickup);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SyringeSpeedSheath, SyringeSpeed.MaxSpeed);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SyringeSpeedSample, SyringeSpeed.Normal);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SyringeSpeedSample, SyringeSpeed.HiSpeed);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SyringeSpeedSample, SyringeSpeed.HiSensitivity);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SyringeSpeedSample, SyringeSpeed.Flush);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SyringeSpeedSample, SyringeSpeed.Pickup);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SyringeSpeedSample, SyringeSpeed.MaxSpeed);
        };
        break;
      case "channeltab":
        actionList = () =>
        {
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.GreenA);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.GreenB);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.GreenC);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.RedA);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.RedB);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.RedC);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.RedD);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.VioletA);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.VioletB);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.ForwardScatter);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelCompensationBias, Channel.GreenA);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelCompensationBias, Channel.GreenB);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelCompensationBias, Channel.GreenC);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelCompensationBias, Channel.RedA);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelCompensationBias, Channel.RedB);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelCompensationBias, Channel.RedC);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelCompensationBias, Channel.RedD);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelCompensationBias, Channel.VioletA);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelCompensationBias, Channel.VioletB);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelCompensationBias, Channel.ForwardScatter);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelTemperature, Channel.GreenA);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelTemperature, Channel.GreenB);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelTemperature, Channel.GreenC);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelTemperature, Channel.RedA);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelTemperature, Channel.RedB);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelTemperature, Channel.RedC);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelTemperature, Channel.RedD);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelTemperature, Channel.VioletA);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelTemperature, Channel.VioletB);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelTemperature, Channel.ForwardScatter);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelOffset, Channel.GreenA);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelOffset, Channel.GreenB);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelOffset, Channel.GreenC);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelOffset, Channel.RedA);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelOffset, Channel.RedB);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelOffset, Channel.RedC);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelOffset, Channel.RedD);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelOffset, Channel.VioletA);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelOffset, Channel.VioletB);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.CalibrationMargin);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SiPMTempCoeff);
        };
        break;
      case "motorstab":
        actionList = () =>
        {
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorX, MotorParameterType.Slope);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorX, MotorParameterType.StartSpeed);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorX, MotorParameterType.RunSpeed);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorX, MotorParameterType.CurrentStep);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorY, MotorParameterType.Slope);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorY, MotorParameterType.StartSpeed);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorY, MotorParameterType.RunSpeed);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorY, MotorParameterType.CurrentStep);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorZ, MotorParameterType.Slope);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorZ, MotorParameterType.StartSpeed);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorZ, MotorParameterType.RunSpeed);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorZ, MotorParameterType.CurrentStep);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorZ, MotorParameterType.CurrentLimit);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorStepsX, MotorStepsX.Tube);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorStepsX, MotorStepsX.Plate96Column1);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorStepsX, MotorStepsX.Plate96Column12);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorStepsX, MotorStepsX.Plate384Column1);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorStepsX, MotorStepsX.Plate384Column24);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorStepsY, MotorStepsY.Tube);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorStepsY, MotorStepsY.Plate96RowA);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorStepsY, MotorStepsY.Plate96RowH);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorStepsY, MotorStepsY.Plate384RowA);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorStepsY, MotorStepsY.Plate384RowP);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorStepsZ, MotorStepsZ.Tube);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorStepsZ, MotorStepsZ.A1);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorStepsZ, MotorStepsZ.A12);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorStepsZ, MotorStepsZ.H1);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorStepsZ, MotorStepsZ.H12);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.PollStepActivity);
        };
        break;
      case "componentstab":
        actionList = () =>
        {
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ValveCuvetDrain);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.WashPump);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ValveFan1);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.PollStepActivity);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.IsInputSelectorAtPickup);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.Pressure);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.IsLaserActive);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.LaserPower, LaserType.Violet);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.LaserPower, LaserType.Green);
          DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.LaserPower, LaserType.Red);
        };
        break;
      default:
        return;
    }

    actionList.Invoke();
  }

  public void OnNewWorkOrder(object sender, WorkOrderEventArgs e)
  {
    _workOrderPending = true;
    DashboardViewModel.Instance.WorkOrderID[0] = e.FileName;  //check for already existing one 
    // if WO already selected -> allow start. else the WO checking action should perform the same check
    if (DiosApp.Control == SystemControl.WorkOrder)  //no barcode required so allow start
    {
      MainButtonsViewModel.Instance.EnableStartButton(true);
      _workOrderPending = false;  //questionable logic. can check on non empty textbox
    }
  }

  private void DispatcherExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs args)
  {
    Logger.Log("[PROBLEM] Dispatcher exception");
    Logger.Log($"[PROBLEM] Source: {args.Exception.Source}");
    Logger.Log($"[PROBLEM] Message: {args.Exception.Message}");
    if (args.Exception.TargetSite is not null)
    {
      Logger.Log($"[PROBLEM] From: {args.Exception.TargetSite.Name}");
    }
    Logger.Log($"[PROBLEM] trace: {args.Exception.StackTrace}");
  }
}