using System.Reflection;
using System;
using System.Windows;
using Ei_Dimension.ViewModels;
using MicroCy;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Configuration;

namespace Ei_Dimension
{
  public partial class App : Application
  {
    public static (PropertyInfo prop, object VM) NumpadShow { get; set; }
    public static (PropertyInfo prop, object VM) KeyboardShow { get; set; }
    public static MicroCyDevice Device { get; private set; }
    public static Models.MapRegions MapRegions { get; set; }  //Performs operations on injected views
    public static bool _nextWellWarning;

    private static bool _workOrderPending;

    public App()
    {
      CorruptSettingsChecker();
      Device = new MicroCyDevice();
      SetLogOutput();
      SetupDevice();
      Device.StartingToReadWell += StartingToReadWellEventHandler;
      Device.FinishedReadingWell += FinishedReadingWellEventHandler;
      Device.FinishedMeasurement += FinishedMeasurementEventHandler;
      Device.NewStatsAvailable += NewStatsAvailableEventHandler;
      ResultReporter.Outfilename = Settings.Default.SaveFileName;
      _workOrderPending = false;
      _nextWellWarning = false;
      var watcher = new FileSystemWatcher($"{MicroCyDevice.RootDirectory.FullName}\\WorkOrder");
      watcher.NotifyFilter = NotifyFilters.FileName;
      watcher.Filter = "*.txt";
      watcher.EnableRaisingEvents = true;
      watcher.Created += OnNewWorkOrder;
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
      }
    }

    private static void SetupDevice()
    {
      if (Device == null)
        throw new Exception("Device not initialized");

      if (Settings.Default.DefaultMap > Device.MapList.Count - 1)
      {
        try
        {
          Device.ActiveMap = Device.MapList[0];
        }
        catch
        {
          throw new Exception($"Could not find Maps in {MicroCyDevice.RootDirectory.FullName + @"\Config" } folder");
        }
      }
      else
      {
        Device.ActiveMap = Device.MapList[Settings.Default.DefaultMap];
      }
      MicroCyDevice.SystemControl = Settings.Default.SystemControl;
      MicroCyDevice.Everyevent = Settings.Default.Everyevent;
      MicroCyDevice.RMeans = Settings.Default.RMeans;
      MicroCyDevice.PlateReportActive = Settings.Default.PlateReport;
      MicroCyDevice.TerminationType = Settings.Default.EndRead;
      MicroCyDevice.MinPerRegion = Settings.Default.MinPerRegion;
      MicroCyDevice.BeadsToCapture = Settings.Default.BeadsToCapture;
      MicroCyDevice.OnlyClassified = Settings.Default.OnlyClassifed;
      MicroCyDevice.ChannelBIsHiSensitivity = Settings.Default.SensitivityChannelB;
      Device.MainCommand("Set Property", code: 0xbf, parameter: (ushort)Device.ActiveMap.calParams.att);
      if (!System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
        Device.MainCommand("Startup");
      MicroCy.InstrumentParameters.Calibration.HdnrTrans = Device.ActiveMap.calParams.DNRTrans;
      MicroCy.InstrumentParameters.Calibration.Compensation = Device.ActiveMap.calParams.compensation;
      Device.MainCommand("Set Property", code: 0x97, parameter: 1170);  //set current limit of aligner motors if leds are off
      Device.MainCommand("Get Property", code: 0xca);
    }

    public static int GetMapIndex(string MapName)
    {
      int i = 0;
      for (; i < Device.MapList.Count; i++)
      {
        if (Device.MapList[i].mapName == MapName)
          break;
      }
      if (i == Device.MapList.Count)
        i = -1;
      return i;
    }

    public static void SetActiveMap(string mapName)
    {
      for (var i = 0; i < Device.MapList.Count; i++)
      {
        if (Device.MapList[i].mapName == mapName)
        {
          Device.ActiveMap = Device.MapList[i];
          Settings.Default.DefaultMap = i;
          Settings.Default.Save();
          Device.MainCommand("Set Property", code: 0xbf, parameter: (ushort)Device.ActiveMap.calParams.att);
          break;
        }
      }
      ResultsViewModel.Instance.FillWorldMaps();

      var CaliVM = CalibrationViewModel.Instance;
      if (CaliVM != null)
      {
        CaliVM.EventTriggerContents[1] = Device.ActiveMap.calParams.minmapssc.ToString();
        Device.MainCommand("Set Property", code: 0xce, parameter: (ushort)Device.ActiveMap.calParams.minmapssc);
        CaliVM.EventTriggerContents[2] = Device.ActiveMap.calParams.maxmapssc.ToString();
        Device.MainCommand("Set Property", code: 0xcf, parameter: (ushort)Device.ActiveMap.calParams.maxmapssc);
        CaliVM.AttenuationBox[0] = Device.ActiveMap.calParams.att.ToString();
        Device.MainCommand("Set Property", code: 0xbf, parameter: (ushort)Device.ActiveMap.calParams.att);


        CaliVM.EventTriggerContents[0] = Device.ActiveMap.calParams.height.ToString();
        Device.MainCommand("Set Property", code: 0xcd, parameter: Device.ActiveMap.calParams.height);
        CaliVM.CompensationPercentageContent[0] = Device.ActiveMap.calParams.compensation.ToString();
        MicroCy.InstrumentParameters.Calibration.Compensation = Device.ActiveMap.calParams.compensation;
        CaliVM.DNRContents[0] = Device.ActiveMap.calParams.DNRCoef.ToString();
        MicroCy.InstrumentParameters.Calibration.HDnrCoef = Device.ActiveMap.calParams.DNRCoef;
        Device.MainCommand("Set FProperty", code: 0x20, fparameter: Device.ActiveMap.calParams.DNRCoef);
        CaliVM.DNRContents[1] = Device.ActiveMap.calParams.DNRTrans.ToString();
        MicroCy.InstrumentParameters.Calibration.HdnrTrans = Device.ActiveMap.calParams.DNRTrans;
        CaliVM.ClassificationTargetsContents[0] = Device.ActiveMap.calParams.CL0.ToString();
        Device.MainCommand("Set Property", code: 0x8b, parameter: (ushort)Device.ActiveMap.calParams.CL0);
        CaliVM.ClassificationTargetsContents[1] = Device.ActiveMap.calParams.CL1.ToString();
        Device.MainCommand("Set Property", code: 0x8c, parameter: (ushort)Device.ActiveMap.calParams.CL1);
        CaliVM.ClassificationTargetsContents[2] = Device.ActiveMap.calParams.CL2.ToString();
        Device.MainCommand("Set Property", code: 0x8d, parameter: (ushort)Device.ActiveMap.calParams.CL2);
        CaliVM.ClassificationTargetsContents[3] = Device.ActiveMap.calParams.CL3.ToString();
        Device.MainCommand("Set Property", code: 0x8e, parameter: (ushort)Device.ActiveMap.calParams.CL3);
        CaliVM.ClassificationTargetsContents[4] = Device.ActiveMap.calParams.RP1.ToString();
        Device.MainCommand("Set Property", code: 0x8f, parameter: (ushort)Device.ActiveMap.calParams.RP1);
        CaliVM.GatingItems[Device.ActiveMap.calParams.gate].Click();
      }

      var ChannelsVM = ChannelsViewModel.Instance;
      if (ChannelsVM != null)
      {
        ChannelsVM.Bias30Parameters[0] = Device.ActiveMap.calgssc.ToString();
        Device.MainCommand("Set Property", code: 0x28, parameter: (ushort)Device.ActiveMap.calgssc);
        ChannelsVM.Bias30Parameters[1] = Device.ActiveMap.calrpmaj.ToString();
        Device.MainCommand("Set Property", code: 0x29, parameter: (ushort)Device.ActiveMap.calrpmaj);
        ChannelsVM.Bias30Parameters[2] = Device.ActiveMap.calrpmin.ToString();
        Device.MainCommand("Set Property", code: 0x2a, parameter: (ushort)Device.ActiveMap.calrpmin);
        ChannelsVM.Bias30Parameters[3] = Device.ActiveMap.calcl3.ToString();
        Device.MainCommand("Set Property", code: 0x2c, parameter: (ushort)Device.ActiveMap.calcl3);
        ChannelsVM.Bias30Parameters[4] = Device.ActiveMap.calrssc.ToString();
        Device.MainCommand("Set Property", code: 0x2d, parameter: (ushort)Device.ActiveMap.calrssc);
        ChannelsVM.Bias30Parameters[5] = Device.ActiveMap.calcl1.ToString();
        Device.MainCommand("Set Property", code: 0x2e, parameter: (ushort)Device.ActiveMap.calcl1);
        ChannelsVM.Bias30Parameters[6] = Device.ActiveMap.calcl2.ToString();
        Device.MainCommand("Set Property", code: 0x2f, parameter: (ushort)Device.ActiveMap.calcl2);
        ChannelsVM.Bias30Parameters[7] = Device.ActiveMap.calvssc.ToString();
        Device.MainCommand("Set Property", code: 0x25, parameter: (ushort)Device.ActiveMap.calvssc);
        ChannelsVM.Bias30Parameters[8] = Device.ActiveMap.calcl0.ToString();
        Device.MainCommand("Set Property", code: 0x26, parameter: (ushort)Device.ActiveMap.calcl0);
        ChannelsVM.Bias30Parameters[9] = Device.ActiveMap.calfsc.ToString();
        Device.MainCommand("Set Property", code: 0x24, parameter: (ushort)Device.ActiveMap.calfsc);
      }

      var DashVM = DashboardViewModel.Instance;
      if (DashVM != null)
      {
        if (Device.ActiveMap.validation)
        {
          DashVM.CalValModeEnabled = true;
          DashVM.CaliDateBox[0] = Device.ActiveMap.caltime;
          DashVM.ValidDateBox[0] = Device.ActiveMap.valtime;
        }
        else
        {
          DashVM.CalValModeEnabled = false;
          DashVM.CaliDateBox[0] = null;
          DashVM.ValidDateBox[0] = null;
        }
      }

      bool Warning = false;
      if (Device.ActiveMap.validation)
      {
        var valDate = DateTime.Parse(Device.ActiveMap.valtime, new System.Globalization.CultureInfo("en-GB"));
        switch (Settings.Default.VerificationWarningIndex)
        {
          case 0:
            Warning = valDate.AddDays(1) < DateTime.Today;
            break;
          case 1:
            Warning = valDate.AddDays(7) < DateTime.Today;
            break;
          case 2:
            Warning = valDate.AddMonths(1) < DateTime.Today;
            break;
          case 3:
            Warning = valDate.AddMonths(3) < DateTime.Today;
            break;
          case 4:
            Warning = valDate.AddYears(1) < DateTime.Today;
            break;
        }
      }
      DashVM.VerificationWarningVisible = Warning? Visibility.Visible : Visibility.Hidden;

    }

    public static void LockMapSelection()
    {
      Views.DashboardView.Instance.MapSelectr.IsEnabled = false;
      Views.CalibrationView.Instance.MapSelectr.IsEnabled = false;
      Views.VerificationView.Instance.MapSelectr.IsEnabled = false;
      Views.ChannelsView.Instance.MapSelectr.IsEnabled = false;
    }

    public static void UnlockMapSelection()
    {
      Views.DashboardView.Instance.MapSelectr.IsEnabled = true;
      Views.CalibrationView.Instance.MapSelectr.IsEnabled = true;
      Views.VerificationView.Instance.MapSelectr.IsEnabled = true;
      Views.ChannelsView.Instance.MapSelectr.IsEnabled = true;
    }

    public static void SetLanguage(string locale)
    {
      if (string.IsNullOrEmpty(locale))
        locale = "en-US";
      var exCulture = Language.TranslationSource.Instance.CurrentCulture;
      var curCulture = Language.TranslationSource.Instance.CurrentCulture = new System.Globalization.CultureInfo(locale);
      var RM = Language.Resources.ResourceManager;

      #region Translation hack
      var ComponentsVM = ComponentsViewModel.Instance;
      if (ComponentsVM != null)
      {
        if (ComponentsVM.IInputSelectorState == 0)
        {
          ComponentsVM.InputSelectorState[0] = RM.GetString(nameof(Language.Resources.Components_To_Pickup),
            curCulture);
          ComponentsVM.InputSelectorState[1] = RM.GetString(nameof(Language.Resources.Components_To_Cuvet),
            curCulture);
        }
        else
        {
          ComponentsVM.InputSelectorState[0] = RM.GetString(nameof(Language.Resources.Components_To_Cuvet),
            curCulture);
          ComponentsVM.InputSelectorState[1] = RM.GetString(nameof(Language.Resources.Components_To_Pickup),
            curCulture);
        }

        ComponentsVM.SyringeControlItems[0].Content = RM.GetString(nameof(Language.Resources.Dropdown_Halt), curCulture);
        ComponentsVM.SyringeControlItems[1].Content = RM.GetString(nameof(Language.Resources.Dropdown_Move_Absolute), curCulture);
        ComponentsVM.SyringeControlItems[2].Content = RM.GetString(nameof(Language.Resources.Dropdown_Pickup), curCulture);
        ComponentsVM.SyringeControlItems[3].Content = RM.GetString(nameof(Language.Resources.Dropdown_Pre_inject), curCulture);
        ComponentsVM.SyringeControlItems[4].Content = RM.GetString(nameof(Language.Resources.Dropdown_Speed), curCulture);
        ComponentsVM.SyringeControlItems[5].Content = RM.GetString(nameof(Language.Resources.Dropdown_Initialize), curCulture);
        ComponentsVM.SyringeControlItems[6].Content = RM.GetString(nameof(Language.Resources.Dropdown_Boot), curCulture);
        ComponentsVM.SyringeControlItems[7].Content = RM.GetString(nameof(Language.Resources.Dropdown_Valve_Left), curCulture);
        ComponentsVM.SyringeControlItems[8].Content = RM.GetString(nameof(Language.Resources.Dropdown_Valve_Right), curCulture);
        ComponentsVM.SyringeControlItems[9].Content = RM.GetString(nameof(Language.Resources.Dropdown_Micro_step), curCulture);
        ComponentsVM.SyringeControlItems[10].Content = RM.GetString(nameof(Language.Resources.Dropdown_Speed_Preset), curCulture);
        ComponentsVM.SyringeControlItems[11].Content = RM.GetString(nameof(Language.Resources.Dropdown_Pos), curCulture);
        ComponentsVM.SelectedSheathContent = ComponentsVM.SyringeControlItems[0].Content;
        ComponentsVM.SelectedSampleAContent = ComponentsVM.SyringeControlItems[0].Content;
        ComponentsVM.SelectedSampleBContent = ComponentsVM.SyringeControlItems[0].Content;

        ComponentsVM.GetPositionToggleButtonState = ComponentsVM.GetPositionToggleButtonState ==
          RM.GetString(nameof(Language.Resources.OFF), exCulture) ? RM.GetString(nameof(Language.Resources.OFF), curCulture)
          : RM.GetString(nameof(Language.Resources.ON), curCulture);
      }
      var CaliVM = CalibrationViewModel.Instance;
      if (CaliVM != null)
      {
        CaliVM.GatingItems[0].Content = RM.GetString(nameof(Language.Resources.Dropdown_None), curCulture);
        CaliVM.GatingItems[1].Content = RM.GetString(nameof(Language.Resources.Dropdown_Green_SSC), curCulture);
        CaliVM.GatingItems[2].Content = RM.GetString(nameof(Language.Resources.Dropdown_Red_SSC), curCulture);
        CaliVM.GatingItems[3].Content = RM.GetString(nameof(Language.Resources.Dropdown_Green_Red_SSC), curCulture);
        CaliVM.GatingItems[4].Content = RM.GetString(nameof(Language.Resources.Dropdown_Rp_bg), curCulture);
        CaliVM.GatingItems[5].Content = RM.GetString(nameof(Language.Resources.Dropdown_Green_Rp_bg), curCulture);
        CaliVM.GatingItems[6].Content = RM.GetString(nameof(Language.Resources.Dropdown_Red_Rp_bg), curCulture);
        CaliVM.GatingItems[7].Content = RM.GetString(nameof(Language.Resources.Dropdown_Green_Red_Rp_bg), curCulture);
        CaliVM.SelectedGatingContent = CaliVM.GatingItems[0].Content;
      }
      var DashVM = DashboardViewModel.Instance;
      if (DashVM != null)
      {
        DashVM.SpeedItems[0].Content = RM.GetString(nameof(Language.Resources.Dropdown_Normal), curCulture);
        DashVM.SpeedItems[1].Content = RM.GetString(nameof(Language.Resources.Dropdown_Hi_Speed), curCulture);
        DashVM.SpeedItems[2].Content = RM.GetString(nameof(Language.Resources.Dropdown_Hi_Sens), curCulture);
        DashVM.SelectedSpeedContent = DashVM.SpeedItems[0].Content;

        DashVM.ChConfigItems[0].Content = RM.GetString(nameof(Language.Resources.Dropdown_Standard), curCulture);
        DashVM.ChConfigItems[1].Content = RM.GetString(nameof(Language.Resources.Dropdown_Cells), curCulture);
        DashVM.ChConfigItems[2].Content = RM.GetString(nameof(Language.Resources.Dropdown_FM3D), curCulture);
        DashVM.SelectedChConfigContent = DashVM.ChConfigItems[0].Content;

        DashVM.OrderItems[0].Content = RM.GetString(nameof(Language.Resources.Column), curCulture);
        DashVM.OrderItems[1].Content = RM.GetString(nameof(Language.Resources.Row), curCulture);
        DashVM.SelectedOrderContent = DashVM.OrderItems[0].Content;

        DashVM.SysControlItems[0].Content = RM.GetString(nameof(Language.Resources.Experiment_Manual), curCulture);
        DashVM.SysControlItems[1].Content = RM.GetString(nameof(Language.Resources.Experiment_Work_Order), curCulture);
        //DashVM.SysControlItems[2].Content = RM.GetString(nameof(Language.Resources.Experiment_Work_Order_Plus_Bcode), curCulture);
        DashVM.SelectedSysControlContent = DashVM.SysControlItems[0].Content;

        DashVM.EndReadItems[0].Content = RM.GetString(nameof(Language.Resources.Experiment_Min_Per_Reg), curCulture);
        DashVM.EndReadItems[1].Content = RM.GetString(nameof(Language.Resources.Experiment_Total_Events), curCulture);
        DashVM.EndReadItems[2].Content = RM.GetString(nameof(Language.Resources.Experiment_End_of_Sample), curCulture);
        DashVM.SelectedEndReadContent = DashVM.EndReadItems[0].Content;
      }
      var ChannelsVM = ChannelOffsetViewModel.Instance;
      if (ChannelsVM != null)
      {
        ChannelsVM.SensitivityItems[0].Content = RM.GetString(nameof(Language.Resources.Channels_Sens_B), curCulture);
        ChannelsVM.SensitivityItems[1].Content = RM.GetString(nameof(Language.Resources.Channels_Sens_C), curCulture);
        int i = MicroCyDevice.ChannelBIsHiSensitivity ? 0 : 1;
        ChannelsVM.SelectedSensitivityContent = ChannelsVM.SensitivityItems[i].Content;
      }
      var VerVM = VerificationViewModel.Instance;
      if (VerVM != null)
      {
        VerVM.VerificationWarningItems[0].Content = RM.GetString(nameof(Language.Resources.Daily), curCulture);
        VerVM.VerificationWarningItems[1].Content = RM.GetString(nameof(Language.Resources.Weekly), curCulture);
        VerVM.VerificationWarningItems[2].Content = RM.GetString(nameof(Language.Resources.Monthly), curCulture);
        VerVM.VerificationWarningItems[3].Content = RM.GetString(nameof(Language.Resources.Quarterly), curCulture);
        VerVM.VerificationWarningItems[4].Content = RM.GetString(nameof(Language.Resources.Yearly), curCulture);
        VerVM.SelectedVerificationWarningContent = VerVM.VerificationWarningItems[Settings.Default.VerificationWarningIndex].Content;
      }
      var ResVM = ResultsViewModel.Instance;
      if (ResVM != null)
      {
        if (Settings.Default.SensitivityChannelB)
        {
          ResVM.HiSensitivityChannelName[0] = RM.GetString(nameof(Language.Resources.DataAn_Green_Min),
            curCulture);
          ResVM.HiSensitivityChannelName[1] = RM.GetString(nameof(Language.Resources.DataAn_Green_Maj),
            curCulture);
        }
        else
        {
          ResVM.HiSensitivityChannelName[0] = RM.GetString(nameof(Language.Resources.DataAn_Green_Maj),
            curCulture);
          ResVM.HiSensitivityChannelName[1] = RM.GetString(nameof(Language.Resources.DataAn_Green_Min),
            curCulture);
        }
      }
      #endregion
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
    
    private static void StartingToReadWellEventHandler(object sender, ReadingWellEventArgs e)
    {
      MultiTube.GetModifiedWellIndexes(e, out var row, out var col);

      ResultsViewModel.Instance.PlatePictogram.CurrentlyReadCell = (row, col);
      ResultsViewModel.Instance.PlatePictogram.ChangeState(row, col, Models.WellType.NowReading, GetWarningState(), FilePath: e.FilePath);
      
      App.Current.Dispatcher.Invoke(() =>
      {
        ResultsViewModel.Instance.CornerButtonClick(Models.DrawingPlate.CalculateCorner(row, col));
        ResultsViewModel.Instance.ClearGraphs();
      });
      MapRegions.ResetCurrentActiveRegionsDisplayedStats();
      #if DEBUG
      Device.MainCommand("Set FProperty", code: 0x06);
      #endif
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

    public static void FinishedReadingWellEventHandler(object sender, ReadingWellEventArgs e)
    {
      var type = GetWellStateForPictogram();
      
      MultiTube.GetModifiedWellIndexes(e, out var row, out var col);
      MultiTube.Proceed();

      ResultsViewModel.Instance.PlatePictogram.ChangeState(row, col, type);
      SavePlateState();
      if (DashboardViewModel.Instance.SelectedSystemControlIndex == 1)
      {
        App.Current.Dispatcher.Invoke(() => DashboardViewModel.Instance.WorkOrder[0] = ""); //actually questionable if not in workorder operation
      }
      #if DEBUG
      Device.MainCommand("Get FProperty", code: 0x06);
      #endif
    }

    private static Models.WellType GetWellStateForPictogram()
    {
      var type = Models.WellType.Success;
      if (MicroCyDevice.Mode == OperationMode.Normal)
      {
        if (MicroCyDevice.TerminationType == 0 && MicroCyDevice.WellResults.Count > 0)
        {
          foreach (var wr in MicroCyDevice.WellResults)
          {
            if (wr.RP1vals.Count < MicroCyDevice.MinPerRegion * 0.75)
            {
              type = Models.WellType.Fail;
              break;
            }
            if (wr.RP1vals.Count < MicroCyDevice.MinPerRegion)
            {
              type = Models.WellType.LightFail;
              break;
            }
          }
        }
      }
      return type;
    }

    public static void FinishedMeasurementEventHandler(object sender, EventArgs e)
    {
      MainButtonsViewModel.Instance.StartButtonEnabled = true;
      ResultsViewModel.Instance.PlatePictogram.CurrentlyReadCell = (-1, -1);
      switch (MicroCyDevice.Mode)
      {
        case OperationMode.Normal:
          break;
        case OperationMode.Calibration:
          if (++CalibrationViewModel.Instance.CalFailsInARow >= 3 && CalibrationViewModel.Instance.CalJustFailed)
          {
            Notification.ShowLocalized(nameof(Language.Resources.Calibration_Fail), System.Windows.Media.Brushes.Red);
            DashboardViewModel.Instance.CalModeToggle();
          }
          else if (CalibrationViewModel.Instance.CalJustFailed)
            Notification.ShowLocalized(nameof(Language.Resources.Calibration_in_Progress), System.Windows.Media.Brushes.Green);
          break;
        case OperationMode.Verification:
          Validator.CalculateResults();
          if (VerificationViewModel.AnalyzeVerificationResults())
          {
            _ = Current.Dispatcher.BeginInvoke((Action)VerificationViewModel.VerificationSuccess);
          }
          else
            Notification.ShowLocalized(nameof(Language.Resources.Validation_Fail), System.Windows.Media.Brushes.Red);
          break;
      }
    }

    public static void NewStatsAvailableEventHandler(object sender, StatsEventArgs e)
    {
      _ = Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        for (var i = 0; i < 10; i++)
        {
          ResultsViewModel.Instance.CurrentMfiItems[i] = e.GStats[i].mfi.ToString($"{0:0.0}");
          ResultsViewModel.Instance.CurrentCvItems[i] = e.GStats[i].cv.ToString($"{0:0.00}");
          ChannelOffsetViewModel.Instance.AverageBg[i] = e.AvgBg[i].ToString($"{0:0.00}");
        }
      }));
    }

    public static void SetLogOutput()
    {
      ResultReporter.OutDirCheck();
      if (!Directory.Exists(ResultReporter.Outdir + "\\SystemLogs"))
        Directory.CreateDirectory(ResultReporter.Outdir + "\\SystemLogs");
      string logPath = Path.Combine(Path.Combine(@"C:\Emissioninc", Environment.MachineName), "SystemLogs", "EventLog");
      string logFilePath = logPath + ".txt";
    
      string backFilePath = logPath + ".bak";
      if (File.Exists(logFilePath))
      {
        File.Delete(backFilePath);
        File.Move(logFilePath, backFilePath);
      }
      var fs = new FileStream(logFilePath, FileMode.Create);
      var logWriter = new StreamWriter(fs);
      logWriter.AutoFlush = true;
      Console.SetOut(logWriter);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      DevExpress.Xpf.Core.ApplicationThemeHelper.ApplicationThemeName = DevExpress.Xpf.Core.Theme.Office2019ColorfulName;
      base.OnStartup(e);
    }

    public static void SavePlateState()
    {
      //overwrite the whole thing
      try
      {
        string contents = ResultsViewModel.Instance.PlatePictogram.GetSerializedPlate();
        File.WriteAllText($"{MicroCyDevice.RootDirectory.FullName}\\Status\\StatusFile.json", contents);
      }
      catch(Exception e)
      {
        Notification.Show($"Problem with status file save, Please report this issue to the Manufacturer {e.Message}");
      }
    }

    public void OnNewWorkOrder(object sender, FileSystemEventArgs e)
    {
      var name = Path.GetFileNameWithoutExtension(e.Name);
      ResultReporter.WorkOrderPath = e.FullPath;
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
      string[] fileEntries = Directory.GetFiles($"{MicroCyDevice.RootDirectory.FullName}\\WorkOrder", "*.txt");
      if (fileEntries.Length == 0)
        return;
      var name = Path.GetFileNameWithoutExtension(fileEntries[0]);
      ResultReporter.WorkOrderPath = fileEntries[0];
      int i = 1;
      while (!ParseWorkOrder())
      {
        if (i < fileEntries.Length)
        {
          ResultReporter.WorkOrderPath = fileEntries[i];
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
        using (TextReader reader = new StreamReader(ResultReporter.WorkOrderPath))
        {
          var contents = reader.ReadToEnd();
          MicroCyDevice.WorkOrder = Newtonsoft.Json.JsonConvert.DeserializeObject<WorkOrder>(contents);
        }
      }
      catch
      {
        return false;
      }
      return true;
    }
  }
}