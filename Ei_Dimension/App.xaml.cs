using System.Reflection;
using System;
using System.Windows;
using Ei_Dimension.ViewModels;
using DIOS.Core;
using System.IO;
using System.Configuration;
using System.Text;
using DIOS.Core.HardwareIntercom;
using Ei_Dimension.Cache;
using Ei_Dimension.Controllers;
using DIOS.Application;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ei_Dimension.Graphing;

namespace Ei_Dimension
{
  public partial class App : Application
  {
    public static DIOSApp DiosApp { get; } = new DIOSApp();
    public static (PropertyInfo prop, object VM) NumpadShow { get; set; }
    public static (PropertyInfo prop, object VM) KeyboardShow { get; set; }
    public static ResultsCache Cache { get; } = new ResultsCache();
    public static MapRegionsController MapRegions { get; set; }
    /// <summary>doubles the variable in device, but for UI usage</summary>
    public static bool ChannelRedirectionEnabled { get; private set; } = false;

    public static ILogger Logger => DiosApp.Logger;

    public static bool _nextWellWarning;
    public static readonly HashSet<char> InvalidChars = new HashSet<char>();

    private static bool _workOrderPending;
    private static FileSystemWatcher _workOrderWatcher;
    private static readonly TextBoxHandler _textBoxHandler = new TextBoxHandler();

    static App()
    {
      InvalidChars.UnionWith(Path.GetInvalidFileNameChars());
      CorruptSettingsChecker();
    }

    public App(Device device = null)
    {
      InitApp(device);
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

    public static int GetMapIndex(string mapName)
    {
      var mapList = DiosApp.MapController.MapList;
      int i = 0;
      for (; i < mapList.Count; i++)
      {
        if (mapList[i].mapName == mapName)
          break;
      }
      if (i == mapList.Count)
        i = -1;
      return i;
    }

    public static void SetActiveMap(string mapName)
    {
      for (var i = 0; i < DiosApp.MapController.MapList.Count; i++)
      {
        if (DiosApp.MapController.MapList[i].mapName == mapName)
        {
          DiosApp.MapController.SetMap(DiosApp.MapController.MapList[i]);
          Settings.Default.DefaultMap = i;
          Settings.Default.Save();
          break;
        }
      }
    }

    public static void SetSystemControl(byte num)
    {
      DiosApp.Control = (SystemControl)num;
      Settings.Default.SystemControl = num;
      Settings.Default.Save();
    }

    public static void SetTerminationType(byte num)
    {
      DiosApp.TerminationType = (Termination)num;
      Settings.Default.EndRead = num;
      Settings.Default.Save();
    }

    public static void SetChannelConfig(ChannelConfiguration chConfig)
    {
      DiosApp.Device.Hardware.SetParameter(DeviceParameterType.ChannelConfiguration, chConfig);
      ChannelRedirectionEnabled = chConfig == ChannelConfiguration.OEMA ||
                                    chConfig == ChannelConfiguration.OEMPMT;
      LanguageSwap.TranslateChannelsOffsetVM();//swaps between red <-> green
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
        DiosApp.Publisher.OutDirCheck();
        if (!Directory.Exists(DiosApp.Publisher.Outdir + "\\SavedImages"))
          Directory.CreateDirectory(DiosApp.Publisher.Outdir + "\\SavedImages");
        chart.ExportToImage(DiosApp.Publisher.Outdir + @"\SavedImages\" + DiosApp.Publisher.Date + ".png", options);
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
      DiosApp.TotalBeadsToCapture = e.Well.BeadsToCapture;
      DiosApp.MinPerRegion = e.Well.MinPerRegion;
      //DiosApp.TerminationType = e.Well.TermType;

      Logger.Log($"Starting to read well {e.Well.CoordinatesString()} with Params:\nTermination: {(int)DiosApp.TerminationType}\nMinPerRegion: {DiosApp.MinPerRegion}\nBeadsToCapture: {DiosApp.TotalBeadsToCapture}");

      var wellFilePath = DiosApp.Publisher.BeadEventFile.MakeNewFileName(e.Well);
      DiosApp.Results.StartNewWell(e.Well);
      MultiTube.GetModifiedWellIndexes(e.Well, out var row, out var col);

      //ResultsViewModel.Instance.PlatePictogramIsCovered = Visibility.Visible; //TODO: temporary solution

      PlatePictogramViewModel.Instance.PlatePictogram.CurrentlyReadCell = (row, col);
      PlatePictogramViewModel.Instance.PlatePictogram.ChangeState(row, col, Models.WellType.NowReading, GetWarningState(), wellFilePath);
      
      App.Current.Dispatcher.Invoke(() =>
      {
        PlatePictogramViewModel.Instance.CornerButtonClick(Models.DrawingPlate.CalculateCorner(row, col));
        ResultsViewModel.Instance.ClearGraphs();
      });
      ActiveRegionsStatsController.Instance.ResetCurrentActiveRegionsDisplayedStats();
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
      Logger.Log($"Finished Reading well {e.Well.CoordinatesString()}");
      SaveWellFiles();

      var type = GetWellStateForPictogram();
      
      MultiTube.GetModifiedWellIndexes(e.Well, out var row, out var col, proceed:true);

      //Cache.Store(row, col);
      //ResultsViewModel.Instance.PlatePictogramIsCovered = Visibility.Hidden; //TODO: temporary solution

      PlatePictogramViewModel.Instance.PlatePictogram.CurrentlyReadCell = (-1, -1);
      PlatePictogramViewModel.Instance.PlatePictogram.ChangeState(row, col, type);

      DiosApp.Publisher.PlateStatusFile.Overwrite(PlatePictogramViewModel.Instance.PlatePictogram.GetSerializedPlate());
      if (DiosApp.Control == SystemControl.WorkOrder)
      {
        App.Current.Dispatcher.Invoke(() => DashboardViewModel.Instance.WorkOrder[0] = ""); //actually questionable if not in workorder operation
      }

      var stats = DiosApp.Results.WellResults.GetStats();
      var averageBackgrounds = DiosApp.Results.WellResults.GetBackgroundAverages();
      _ = Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        ResultsViewModel.Instance.DecodeCalibrationStats(stats, current: true);
        ChannelOffsetViewModel.Instance.DecodeBackgroundStats(averageBackgrounds);
      }));
    }

    private static void SaveWellFiles()
    {
      _ = Task.Run(() =>
      {
        DiosApp.Results.MakeWellStats();  //TODO: need to check if the well is finished reading before call
        DiosApp.Publisher.ResultsFile.AppendAndWrite(DiosApp.Results.PublishWellStats()); //makewellstats should be awaited only for this method
        DiosApp.Publisher.BeadEventFile.CreateAndWrite(DiosApp.Results.PublishBeadEvents());
        DiosApp.Publisher.DoSomethingWithWorkOrder();
        DiosApp.Logger.Log($"{DateTime.Now.ToString()} Background File Save Complete");
      });
    }

    private static Models.WellType GetWellStateForPictogram()
    {
      var type = Models.WellType.Success;

      if (DiosApp.Device.Mode != OperationMode.Normal
          || DiosApp.TerminationType != Termination.MinPerRegion)
        return type;

      var lacking = DiosApp.Results.MinPerRegionAchieved(DiosApp.MinPerRegion);
      //not achieved
      if (lacking < 0)
      {
        //if lacking more then 25% of minperregion beads 
        if (-lacking > DiosApp.MinPerRegion * 0.25)
        {
          type = Models.WellType.Fail;
        }
        else
        {
          type = Models.WellType.LightFail;
        }
      }
      return type;
    }

    public void FinishedMeasurementEventHandler(object sender, EventArgs e)
    {
      if (DiosApp.Device.Mode == OperationMode.Verification)
        DiosApp.Verificator.CalculateResults(DiosApp.MapController.ActiveMap);

      DiosApp.Results.PlateReport.completedDateTime = DateTime.Now;
      DiosApp.Results.EndOfOperationReset();
      var plateReportJson = DiosApp.Results.PlateReport.JSONify();
      if (DiosApp.Control == SystemControl.Manual)
      {
        DiosApp.Publisher.PlateReportFile.CreateAndWrite(plateReportJson);
      }
      else
      {
        DiosApp.Publisher.PlateReportFile.CreateAndWrite(plateReportJson, DiosApp.WorkOrder.plateID.ToString());
      }

      MainButtonsViewModel.Instance.StartButtonEnabled = true;
      PlatePictogramViewModel.Instance.PlatePictogram.CurrentlyReadCell = (-1, -1);
      switch (DiosApp.Device.Mode)
      {
        case OperationMode.Normal:
          var header = MapRegionsController.GetLegacyReportHeader(DiosApp.Publisher.IncludeReg0InPlateSummary);
          var legacyReport = DiosApp.Results.PlateReport.LegacyReport(header, DiosApp.Publisher.IncludeReg0InPlateSummary);
          OutputLegacyReport(legacyReport);

          if (DiosApp.RunPlateContinuously)
          {
            App.Current.Dispatcher.BeginInvoke((Action)(async() =>
            {
              DiosApp.Device.EjectPlate();
              await System.Threading.Tasks.Task.Delay(5000);
              DiosApp.Device.LoadPlate();
              MainButtonsViewModel.Instance.StartButtonClick();
            }));
          }
          break;
        case OperationMode.Calibration:
          CalibrationViewModel.Instance.CalibrationFailCheck();
          break;
        case OperationMode.Verification:
          if (VerificationViewModel.Instance.AnalyzeVerificationResults(out var errorMsg))
          {
            _ = Current.Dispatcher.BeginInvoke((Action)VerificationViewModel.VerificationSuccess);
          }
          else
          {
            Current.Dispatcher.Invoke(()=>Notification.ShowError(errorMsg, 26));
          }
          DiosApp.Verificator.PublishReport();
          Current.Dispatcher.Invoke(DashboardViewModel.Instance.ValModeToggle);
          //Notification.ShowLocalizedError(nameof(Language.Resources.Validation_Fail));
          break;
      }
    }

    public void MapChangedEventHandler(object sender, MapModel map)
    {
      DiosApp.Device.SetMap(map);
      _ = Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        ResultsViewModel.Instance.FillWorldMaps();

        if (CalibrationViewModel.Instance != null)
          CalibrationViewModel.Instance.OnMapChanged(map);

        if (ComponentsViewModel.Instance != null)
          ComponentsViewModel.Instance.OnMapChanged(map);

        if (ChannelsViewModel.Instance != null)
          ChannelsViewModel.Instance.OnMapChanged(map);

        if (DashboardViewModel.Instance != null)
          DashboardViewModel.Instance.OnMapChanged(map);

        if (NormalizationViewModel.Instance != null)
          NormalizationViewModel.Instance.OnMapChanged(map);

        if (ResultsViewModel.Instance.AnalysisMap != null)
          ResultsViewModel.Instance.AnalysisMap.OnMapChanged(map);
      }));
    }

    public static void OutputLegacyReport(string legacyReport)
    {
      var bldr = new StringBuilder();
      bldr.AppendLine("Program,\"DIOS\"");
      bldr.AppendLine($"Build,\"{DiosApp.BUILD}\",Firmware,\"{DiosApp.Device.FirmwareVersion}\"");
      bldr.AppendLine($"Date,\"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"))}\"\n");

      bldr.AppendLine($"Instrument,\"{Environment.MachineName}\"");
      bldr.AppendLine($"Session,\"{DiosApp.Publisher.Outfilename}\"\n\n\n\n\n\n\n");

      bldr.AppendLine($"Samples,\"{WellsSelectViewModel.Instance.CurrentTableSize}\"\n");
      bldr.Append(legacyReport);
      var wholeReport = bldr.ToString();

      if (DiosApp.Control == SystemControl.Manual)
      {
        DiosApp.Publisher.LegacyReportFile.CreateAndWrite(wholeReport);
      }
      else
      {
        DiosApp.Publisher.LegacyReportFile.CreateAndWrite(wholeReport, DiosApp.WorkOrder.plateID.ToString());
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
      Action actionList = null;
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
            DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelConfiguration); //never used in the ui or anywhere
          };
          break;
        case "reportingtab":
          actionList = () =>
          {
            DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ValveCuvetDrain); //this all makes no sense. just check the commands. probably
            DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ValveFan2);
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
            DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelOffset, Channel.ForwardScatter);
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
            DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorX, MotorParameterType.CurrentLimit);
            DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorY, MotorParameterType.Slope);
            DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorY, MotorParameterType.StartSpeed);
            DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorY, MotorParameterType.RunSpeed);
            DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorY, MotorParameterType.CurrentStep);
            DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.MotorY, MotorParameterType.CurrentLimit);
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
            DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ValveFan2);
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

      if (actionList != null)
        actionList.Invoke();
    }

    public void OnNewWorkOrder(object sender, FileSystemEventArgs e)
    {
      var name = Path.GetFileNameWithoutExtension(e.Name);
      DiosApp.Publisher.WorkOrderPath = e.FullPath;
      if (!ParseWorkOrder())
        return;
      
      _workOrderPending = true;
      DashboardViewModel.Instance.WorkOrder[0] = name;  //check for already existing one 
      // if WO already selected -> allow start. else the WO checking action should perform the same check
      if (DashboardViewModel.Instance.SelectedSystemControlIndex == 1)  //no barcode required so allow start
      {
        MainButtonsViewModel.Instance.StartButtonEnabled = true;
        _workOrderPending = false;  //questionable logic. can check on non empty textbox
      }
    }

    public static void CheckAvailableWorkOrders()
    {
      string[] fileEntries = Directory.GetFiles($"{DiosApp.RootDirectory.FullName}\\WorkOrder", "*.txt");
      if (fileEntries.Length == 0)
        return;
      var name = Path.GetFileNameWithoutExtension(fileEntries[0]);
      DiosApp.Publisher.WorkOrderPath = fileEntries[0];
      int i = 1;
      while (!ParseWorkOrder())
      {
        if (i < fileEntries.Length)
        {
          DiosApp.Publisher.WorkOrderPath = fileEntries[i];
          name = Path.GetFileNameWithoutExtension(fileEntries[i]);
          i++;
        }
        else
          return;
      }

      DashboardViewModel.Instance.WorkOrder[0] = name;  //should be first succesfully parsed
      _workOrderPending = true;
      // if WO already selected -> allow start. else the WO checking action should perform the same check
      if (DashboardViewModel.Instance.SelectedSystemControlIndex == 1)  //no barcode required so allow start
      {
        MainButtonsViewModel.Instance.StartButtonEnabled = true;
        _workOrderPending = false;
      }
    }

    private static bool ParseWorkOrder()
    {
      try
      {
        using (TextReader reader = new StreamReader(DiosApp.Publisher.WorkOrderPath))
        {
          var contents = reader.ReadToEnd();
          DiosApp.WorkOrder = Newtonsoft.Json.JsonConvert.DeserializeObject<WorkOrder>(contents);
        }
      }
      catch
      {
        return false;
      }
      return true;
    }

    private void InitApp(Device device)
    {
      StatisticsExtension.TailDiscardPercentage = Settings.Default.StatisticsTailDiscardPercentage;
      
      DiosApp.MapController.OnAppLoaded(Settings.Default.DefaultMap);
      DiosApp.Device.StartingToReadWell += StartingToReadWellEventHandler;
      DiosApp.Device.FinishedReadingWell += FinishedReadingWellEventHandler;
      DiosApp.Device.FinishedMeasurement += FinishedMeasurementEventHandler;
      DiosApp.MapController.ChangedActiveMap += MapChangedEventHandler;
      DiosApp.Device.ParameterUpdate += _textBoxHandler.ParameterUpdateEventHandler;
      _workOrderPending = false;
      _nextWellWarning = false;
      _workOrderWatcher = new FileSystemWatcher($"{DiosApp.RootDirectory.FullName}\\WorkOrder");
      _workOrderWatcher.NotifyFilter = NotifyFilters.FileName;
      _workOrderWatcher.Filter = "*.txt";
      _workOrderWatcher.EnableRaisingEvents = true;
      _workOrderWatcher.Created += OnNewWorkOrder;
    }

    public static void SetupDevice()
    {
      if (DiosApp.Device == null)
        throw new Exception("Device not initialized");

      DiosApp.Device.Init();

      if (Directory.Exists(Settings.Default.LastOutFolder))
        DiosApp.Publisher.Outdir = Settings.Default.LastOutFolder;
      else
      {
        Settings.Default.LastOutFolder = DiosApp.Publisher.Outdir;
        Settings.Default.Save();
      }
      DiosApp.Publisher.Outfilename = Settings.Default.SaveFileName;
      DiosApp.Publisher.IsPlateReportPublishingActive = Settings.Default.PlateReport;
      DiosApp.Publisher.IsBeadEventPublishingActive = Settings.Default.Everyevent;
      DiosApp.Publisher.IsResultsPublishingActive = Settings.Default.RMeans;
      DiosApp.Publisher.IsLegacyPlateReportPublishingActive = Settings.Default.LegacyPlateReport;
      DiosApp.Publisher.IsOnlyClassifiedBeadsPublishingActive = Settings.Default.OnlyClassifed;

      DiosApp.Control = (SystemControl)Settings.Default.SystemControl;
      DiosApp.TerminationType = (Termination)Settings.Default.EndRead;
      DiosApp.MinPerRegion = Settings.Default.MinPerRegion;
      DiosApp.TotalBeadsToCapture = Settings.Default.BeadsToCapture;
      DiosApp.Device.MaxPressure = Settings.Default.MaxPressure;
      DiosApp.Device.ReporterScaling = Settings.Default.ReporterScaling;
      var hiSensChannel = Settings.Default.SensitivityChannelB ? HiSensitivityChannel.GreenB : HiSensitivityChannel.GreenC;
      DiosApp.Device.Hardware.SetParameter(DeviceParameterType.HiSensitivityChannel, hiSensChannel);
      DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.Attenuation, DiosApp.MapController.ActiveMap.calParams.att);
      DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.DNRTransition, DiosApp.MapController.ActiveMap.calParams.DNRTrans);
      DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SampleSyringeType);
      DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SampleSyringeSize);
      DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SheathFlushVolume);
      DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.TraySteps);
      DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.IsWellEdgeAgitateActive);
      DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.DistanceToWellEdge);
      DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.WellEdgeDeltaHeight);
      DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.FlushCycles);
      DiosApp.Device.Hardware.SetParameter(DeviceParameterType.WashRepeatsAmount, 1); //1 is the default. same as in the box in dashboard
      DiosApp.Device.Hardware.SetParameter(DeviceParameterType.AgitateRepeatsAmount, 1); //1 is the default. same as in the box in dashboard
      //DiosApp.Device.MainCommand("Set Property", code: 0x97, parameter: 1170);  //set current limit of aligner motors if leds are off //0x97 no longer used
    }
  }
}