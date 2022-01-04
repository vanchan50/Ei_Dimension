using System.Reflection;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows.Threading;
using System.Windows;
using Ei_Dimension.ViewModels;
using MicroCy;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace Ei_Dimension
{
  public partial class App : Application
  {
    public static (PropertyInfo prop, object VM) NumpadShow { get; set; }
    public static (PropertyInfo prop, object VM) KeyboardShow { get; set; }
    public static (PropertyInfo prop, object VM, int index, TextBox tb) SelectedTextBox
    {
      get { return _selectedTextBox; }
      set {
        if ((value.prop != null) && ((value.prop != _selectedTextBox.prop) || (value.index != _selectedTextBox.index)))
        {
          InputSanityCheck();
        }
        _selectedTextBox = value;
        if (value.prop != null)
        {
          _tempOldString = ((ObservableCollection<string>)_selectedTextBox.prop.GetValue(_selectedTextBox.VM))[_selectedTextBox.index];
          value.tb.Background = (System.Windows.Media.Brush)App.Current.Resources["MenuButtonBackgroundActive"];
        }
        else
          _tempOldString = null;
      }
    }
    public static MicroCyDevice Device { get; private set; }
    public static Models.MapRegions MapRegions { get; set; }  //Performs operations on injected views
    public static bool _nextWellWarning;

    private static (PropertyInfo prop, object VM, int index, TextBox tb) _selectedTextBox;
    private static string _tempOldString;
    private static string _tempNewString;
    private static DispatcherTimer _dispatcherTimer;
    private static bool _workOrderPending;
    private static bool _cancelKeyboardInjectionFlag;
    private static bool _isStartup;
    public App()
    {
      _cancelKeyboardInjectionFlag = false;
      Device = new MicroCyDevice(typeof(USBConnection));
      SetLogOutput();
      if (Settings.Default.DefaultMap > Device.MapList.Count - 1)
      {
        try
        {
          Device.ActiveMap = Device.MapList[0];
        }
        catch
        {
          MessageBox.Show($"Could not find Maps in {MicroCyDevice.RootDirectory.FullName + @"\Config"} folder");
          throw new Exception($"Could not find Maps in {MicroCyDevice.RootDirectory.FullName + @"\Config" } folder");
        }
      }
      else
      {
        Device.ActiveMap = Device.MapList[Settings.Default.DefaultMap];
      }
      MicroCyDevice.SystemControl = Settings.Default.SystemControl;
      ResultReporter.Outfilename = Settings.Default.SaveFileName;
      MicroCyDevice.Everyevent = Settings.Default.Everyevent;
      MicroCyDevice.RMeans = Settings.Default.RMeans;
      MicroCyDevice.PltRept = Settings.Default.PlateReport;
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
      Device.StartingToReadWell += StartingToReadWellEventHandler;
      Device.FinishedReadingWell += FinishedReadingWellEventHandler;
      Device.FinishedMeasurement += FinishedMeasurementEventHandler;
      Device.NewStatsAvailable += NewStatsAvailableEventHandler;
      _dispatcherTimer = new DispatcherTimer();
      _dispatcherTimer.Tick += TimerTick;
      _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
      _dispatcherTimer.Start();
      _workOrderPending = false;
      _isStartup = true;
      _nextWellWarning = false;
      var watcher = new FileSystemWatcher($"{MicroCyDevice.RootDirectory.FullName}\\WorkOrder");
      watcher.NotifyFilter = NotifyFilters.FileName;
      watcher.Filter = "*.txt";
      watcher.EnableRaisingEvents = true;
      watcher.Created += OnNewWorkOrder;
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

    public static void SetSystemControl(byte num)
    {
      MicroCyDevice.SystemControl = num;
      Settings.Default.SystemControl = num;
      Settings.Default.Save();
    }

    public static void SetTerminationType(byte num)
    {
      MicroCyDevice.TerminationType = num;
      Settings.Default.EndRead = num;
      Settings.Default.Save();
    }

    public static void SetSensitivityChannel(byte num)
    {
      MicroCyDevice.ChannelBIsHiSensitivity = num == 0;
      Settings.Default.SensitivityChannelB = num == 0;
      Settings.Default.Save();
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
      #endregion
    }

    public static void InjectToFocusedTextbox(string input, bool keyboardinput = false)
    {
      if (SelectedTextBox.prop != null && !_cancelKeyboardInjectionFlag)
      {
        if (keyboardinput)
        {
          _tempNewString = input;
        }
        else
        {
          _cancelKeyboardInjectionFlag = true;
          _tempNewString = ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index];
          if (input == "")
          {
            if (_tempNewString.Length > 0)
              ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = _tempNewString = _tempNewString.Remove(_tempNewString.Length - 1, 1);
          }
          else
            ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = _tempNewString += input;
          _cancelKeyboardInjectionFlag = false;
        }
      }
    }

    public static void InputSanityCheck()
    {
      if (SelectedTextBox.prop != null && _tempNewString != null)
      {
        float fRes;
        int iRes;
        ushort usRes;
        byte bRes;
        bool failed = false;
        string ErrorMessage = null;
        switch (SelectedTextBox.prop.Name)
        {
          case "CompensationPercentageContent":
            if (float.TryParse(_tempNewString, out fRes))
            {
              if (fRes >= 0 && fRes <= 10)
              {
                MicroCy.InstrumentParameters.Calibration.Compensation = fRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "Compensation is out of range [0-10]";
            break;
          case "DNRContents":
            if (SelectedTextBox.index == 0)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 1 && fRes <= 300)
                {
                  MicroCy.InstrumentParameters.Calibration.HDnrCoef = fRes;
                  Device.MainCommand("Set FProperty", code: 0x20, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "High DNR Coefficient is out of range [1-300]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 1 && fRes <= 30000)
                {
                  MicroCy.InstrumentParameters.Calibration.HdnrTrans = fRes;
                  break;
                }
              }
              failed = true;
              ErrorMessage = "High DNR Transition is out of range [1-30000]";
            }
            break;
          case "EndRead":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1)
                {
                  MicroCyDevice.MinPerRegion = iRes;
                  Settings.Default.MinPerRegion = iRes;
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Minimum amount of beads should be a positive number";
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1)
                {
                  MicroCyDevice.BeadsToCapture = iRes;
                  Settings.Default.BeadsToCapture = iRes;
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Total amount of bead events should be a positive number";
            }
            break;
          case "Volumes":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 10 && iRes <= 100)
                {
                  Device.MainCommand("Set Property", code: 0xaf, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Sample Volume is out of range [10-100]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1 && iRes <= 100)
                {
                  Device.MainCommand("Set Property", code: 0xac, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Wash Volume is out of range [1-100]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1 && iRes <= 500)
                {
                  Device.MainCommand("Set Property", code: 0xc4, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Agitate Volume is out of range [1-500]";
            }
            break;
          case "EventTriggerContents":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1 && iRes <= 2000)
                {
                  Device.MainCommand("Set Property", code: 0xcd, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Event height is out of range [1-2000]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 20000)
                {
                  Device.MainCommand("Set Property", code: 0xce, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Min SSC is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 30000)
                {
                  Device.MainCommand("Set Property", code: 0xcf, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Max SSC is out of range [0-30000]";
            }
            break;
          case "ClassificationTargetsContents":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 30000)
                {
                  Device.MainCommand("Set Property", code: 0x8b, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "CL0 Classification Target is out of range [0-30000]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 30000)
                {
                  Device.MainCommand("Set Property", code: 0x8c, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "CL1 Classification Target is out of range [0-30000]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 30000)
                {
                  Device.MainCommand("Set Property", code: 0x8d, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "CL2 Classification Target is out of range [0-30000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 30000)
                {
                  Device.MainCommand("Set Property", code: 0x8e, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "CL3 Classification Target is out of range [0-30000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 30000)
                {
                  Device.MainCommand("Set Property", code: 0x8f, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "RP1 Classification Target is out of range [0-30000]";
            }
            break;
          case "AttenuationBox":
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes >= 0 && iRes <= 100)
              {
                Device.MainCommand("Set Property", code: 0xbf, parameter: (ushort)iRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "Attenuation is out of range [0-100]";
            break;
          case "SheathSyringeParameters":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 1000)
                {
                  Device.MainCommand("Set Property", code: 0x30, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Normal Sheath is out of range [1-1000]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 1000)
                {
                  Device.MainCommand("Set Property", code: 0x31, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Hi Speed Sheath is out of range [1-1000]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 1000)
                {
                  Device.MainCommand("Set Property", code: 0x32, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Hi Sens Sheath is out of range [1-1000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 8000)
                {
                  Device.MainCommand("Set Property", code: 0x33, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Flush Sheath is out of range [1-8000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 8000)
                {
                  Device.MainCommand("Set Property", code: 0x34, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Pickup Sheath is out of range [1-8000]";
            }
            if (SelectedTextBox.index == 5)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 8000)
                {
                  Device.MainCommand("Set Property", code: 0x35, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Max Speed is out of range [1-8000]";
            }
            break;
          case "SamplesSyringeParameters":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 1000)
                {
                  Device.MainCommand("Set Property", code: 0x38, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Normal Samples is out of range [1-1000]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 1000)
                {
                  Device.MainCommand("Set Property", code: 0x39, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Hi Speed Samples is out of range [1-1000]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 1000)
                {
                  Device.MainCommand("Set Property", code: 0x3a, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Hi Sens Samples is out of range [1-1000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 8000)
                {
                  Device.MainCommand("Set Property", code: 0x3b, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Flush Samples is out of range [1-8000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 8000)
                {
                  Device.MainCommand("Set Property", code: 0x3c, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Pickup Samples is out of range [1-8000]";
            }
            if (SelectedTextBox.index == 5)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 8000)
                {
                  Device.MainCommand("Set Property", code: 0x3d, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Max Speed Samples is out of range [1-8000]";
            }
            break;
          case "SiPMTempCoeff":
            if (float.TryParse(_tempNewString, out fRes))
            {
              if (fRes >= -10.0000000001 && fRes <= 10.00000000000001)
              {
                Device.MainCommand("Set FProperty", code: 0x02, fparameter: fRes);
                break;
              }
            }
            failed = true;
            ErrorMessage = "SiPM Temp erature Coefficient is out of range [-10.0 - 10.0]";
            break;
          case "Bias30Parameters":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 3500) || (Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  Device.MainCommand("Set Property", code: 0x28, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Green A (SSC) is out of range {range}";
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 3500) || (Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  Device.MainCommand("Set Property", code: 0x29, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Green B (PE) is out of range {range}";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 3500) || (Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  Device.MainCommand("Set Property", code: 0x2a, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Green C PE is out of range {range}";
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 3500) || (Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  Device.MainCommand("Set Property", code: 0x2c, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Red A (CL3) is out of range {range}";
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 3500) || (Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  Device.MainCommand("Set Property", code: 0x2d, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Red B (SSC) is out of range {range}";
            }
            if (SelectedTextBox.index == 5)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 3500) || (Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  Device.MainCommand("Set Property", code: 0x2e, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Red C (CL1) is out of range {range}";
            }
            if (SelectedTextBox.index == 6)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 3500) || (Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  Device.MainCommand("Set Property", code: 0x2f, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Red D (CL2) is out of range {range}";
            }
            if (SelectedTextBox.index == 7)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 3500) || (Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  Device.MainCommand("Set Property", code: 0x25, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Violet A (SSC) is out of range {range}";
            }
            if (SelectedTextBox.index == 8)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 3500) || (Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  Device.MainCommand("Set Property", code: 0x26, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Violet B (CL0) is out of range {range}";
            }
            if (SelectedTextBox.index == 9)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 3500) || (Device.BoardVersion >= 1 && iRes <= 10000)))
                {
                  Device.MainCommand("Set Property", code: 0x24, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-10000]" : "[0-3500]";
              ErrorMessage = $"Forward Scatter is out of range {range}";
            }
            break;
          case "ChannelsOffsetParameters":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 4095) || (Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  Device.MainCommand("Set Property", code: 0xa0, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Green A (SSC) is out of range {range}";
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 4095) || (Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  Device.MainCommand("Set Property", code: 0xa4, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Green B (PE) is out of range {range}";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 4095) || (Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  Device.MainCommand("Set Property", code: 0xa5, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Green C PE is out of range {range}";
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 4095) || (Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  Device.MainCommand("Set Property", code: 0xa3, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Red A (CL3) is out of range {range}";
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 4095) || (Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  Device.MainCommand("Set Property", code: 0xa2, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Red B (SSC) is out of range {range}";
            }
            if (SelectedTextBox.index == 5)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 4095) || (Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  Device.MainCommand("Set Property", code: 0xa1, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Red C (CL1) is out of range {range}";
            }
            if (SelectedTextBox.index == 6)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 4095) || (Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  Device.MainCommand("Set Property", code: 0x9f, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Red D (CL2) is out of range {range}";
            }
            if (SelectedTextBox.index == 7)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 4095) || (Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  Device.MainCommand("Set Property", code: 0x9d, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Violet A (SSC) is out of range {range}";
            }
            if (SelectedTextBox.index == 8)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 4095) || (Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  Device.MainCommand("Set Property", code: 0x9c, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Violet B (CL0) is out of range {range}";
            }
            if (SelectedTextBox.index == 9)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 4095) || (Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  Device.MainCommand("Set Property", code: 0x9e, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Forward Scatter is out of range {range}";
            }
            break;
          case "ParametersX":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 65535)
                {
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X Steps is out of range [0-65535]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 3000)
                {
                  Device.MainCommand("Set Property", code: 0x53, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X Slope is out of range [1000-3000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 5000)
                {
                  Device.MainCommand("Set Property", code: 0x51, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X Start Speed is out of range [1000-5000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 10000)
                {
                  Device.MainCommand("Set Property", code: 0x52, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X Run Speed is out of range [1000-10000]";
            }
            if (SelectedTextBox.index == 6)
            {
              if (ushort.TryParse(_tempNewString, out usRes))
              {
                if (usRes >= 200 && usRes <= 2000)
                {
                  Device.MainCommand("Set Property", code: 0x50, parameter: (ushort)usRes);
                  Settings.Default.StepsPerRevX = usRes;
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X Encoder Steps is out of range [200-2000]";
            }
            if (SelectedTextBox.index == 7)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 4095) || (Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  Device.MainCommand("Set Property", code: 0x90, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"X Current Limit is out of range {range}";
            }
            break;
          case "ParametersY":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 65535)
                {
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Steps is out of range [0-65535]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 3000)
                {
                  Device.MainCommand("Set Property", code: 0x63, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Slope is out of range [1000-3000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 5000)
                {
                  Device.MainCommand("Set Property", code: 0x61, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Start Speed is out of range [1000-5000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 10000)
                {
                  Device.MainCommand("Set Property", code: 0x62, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Run Speed is out of range [1000-10000]";
            }
            if (SelectedTextBox.index == 6)
            {
              if (ushort.TryParse(_tempNewString, out usRes))
              {
                if (usRes >= 200 && usRes <= 2000)
                {
                  Device.MainCommand("Set Property", code: 0x60, parameter: (ushort)usRes);
                  Settings.Default.StepsPerRevY = usRes;
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Encoder Steps is out of range [200-2000]";
            }
            if (SelectedTextBox.index == 7)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 4095) || (Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  Device.MainCommand("Set Property", code: 0x91, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Y Current Limit is out of range {range}";
            }
            break;
          case "ParametersZ":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && iRes <= 65535)
                {
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z Steps is out of range [0-65535]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 3000)
                {
                  Device.MainCommand("Set Property", code: 0x43, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z Slope is out of range [1000-3000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 5000)
                {
                  Device.MainCommand("Set Property", code: 0x41, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z Start Speed is out of range [1000-5000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 1000 && iRes <= 10000)
                {
                  Device.MainCommand("Set Property", code: 0x42, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z Run Speed is out of range [1000-10000]";
            }
            if (SelectedTextBox.index == 6)
            {
              if (ushort.TryParse(_tempNewString, out usRes))
              {
                if (usRes >= 200 && usRes <= 2000)
                {
                  Device.MainCommand("Set Property", code: 0x40, parameter: (ushort)usRes);
                  Settings.Default.StepsPerRevZ = usRes;
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z Encoder Steps is out of range [200-2000]";
            }
            if (SelectedTextBox.index == 7)
            {
              if (int.TryParse(_tempNewString, out iRes))
              {
                if (iRes >= 0 && ((Device.BoardVersion == 0 && iRes <= 4095) || (Device.BoardVersion >= 1 && iRes <= 65535)))
                {
                  Device.MainCommand("Set Property", code: 0x92, parameter: (ushort)iRes);
                  break;
                }
              }
              failed = true;
              string range = Device.BoardVersion >= 1 ? "[0-65535]" : "[0-4095]";
              ErrorMessage = $"Y Current Limit is out of range {range}";
            }
            break;
          case "StepsParametersX":
            if (SelectedTextBox.index == 0)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  Device.MainCommand("Set FProperty", code: 0x58, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X 96W C1 is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  Device.MainCommand("Set FProperty", code: 0x5a, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X 96W C12 is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  Device.MainCommand("Set FProperty", code: 0x5c, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X 384W C1 is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  Device.MainCommand("Set FProperty", code: 0x5e, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X 384W C24 is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  Device.MainCommand("Set FProperty", code: 0x56, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "X Tube is out of range [0-20000]";
            }
            break;
          case "StepsParametersY":
            if (SelectedTextBox.index == 0)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  Device.MainCommand("Set FProperty", code: 0x68, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Row A is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  Device.MainCommand("Set FProperty", code: 0x6a, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Row H is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  Device.MainCommand("Set FProperty", code: 0x6c, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Row A is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  Device.MainCommand("Set FProperty", code: 0x6e, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Row P is out of range [0-20000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 20000.0000000001)
                {
                  Device.MainCommand("Set FProperty", code: 0x66, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Y Tube is out of range [0-20000]";
            }
            break;
          case "StepsParametersZ":
            if (SelectedTextBox.index == 0)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 1000.0000000001)
                {
                  Device.MainCommand("Set FProperty", code: 0x48, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z A1 is out of range [0-1000]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 1000.0000000001)
                {
                  Device.MainCommand("Set FProperty", code: 0x4a, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z A12 is out of range [0-1000]";
            }
            if (SelectedTextBox.index == 2)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 1000.0000000001)
                {
                  Device.MainCommand("Set FProperty", code: 0x4c, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z H1 is out of range [0-1000]";
            }
            if (SelectedTextBox.index == 3)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 1000.0000000001)
                {
                  Device.MainCommand("Set FProperty", code: 0x4e, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z H12 is out of range [0-1000]";
            }
            if (SelectedTextBox.index == 4)
            {
              if (float.TryParse(_tempNewString, out fRes))
              {
                if (fRes >= 0 && fRes <= 1000.0000000001)
                {
                  Device.MainCommand("Set FProperty", code: 0x46, fparameter: fRes);
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Z Tube is out of range [0-1000]";
            }
            break;
          case "IdexTextBoxInputs":
            if (SelectedTextBox.index == 0)
            {
              if (byte.TryParse(_tempNewString, out bRes))
              {
                if (bRes >= 0 && bRes <= 255)
                {
                  MicroCy.InstrumentParameters.Idex.Pos = bRes;
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Idex Position is out of range [0-255]";
            }
            if (SelectedTextBox.index == 1)
            {
              if (ushort.TryParse(_tempNewString, out usRes))
              {
                if (usRes >= 0 && usRes <= 65535)
                {
                  MicroCy.InstrumentParameters.Idex.Steps = usRes;
                  break;
                }
              }
              failed = true;
              ErrorMessage = "Idex Max Steps is out of range [0-65535]";
            }
            break;
          case "BaseFileName":
            ResultReporter.Outfilename = _tempNewString;
            Settings.Default.SaveFileName = _tempNewString;
            break;
          case "MaxPressureBox":
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes >= 5 && iRes <= 40)
              {
                Settings.Default.MaxPressure = iRes;
                break;
              }
            }
            failed = true;
            ErrorMessage = "Max Pressure is out of range [5-40]";
            break;
          case "TemplateSaveName":
            TemplateSelectViewModel.Instance.TemplateSaveName[0] = _tempNewString;
            break;
          case "SanitizeSecondsContent":
            if (int.TryParse(_tempNewString, out iRes))
            {
              if (iRes >= 1 && iRes <= 100)
              {
                break;
              }
            }
            failed = true;
            ErrorMessage = "UVC Sanitize Seconds is out of range [1-100]";
            break;
        }
        if(VerificationViewModel.Instance.isActivePage)
        {
          if (int.TryParse(_tempNewString, out iRes))
          {
            if (iRes < 0 || iRes > 100000)
            {
              failed = true;
              ErrorMessage = "Reporter value is out of range [0-100000]";
            }
          }
          else
          {
            failed = true;
            ErrorMessage = "Reporter value is out of range [0-100000]";
          }
        }
        Settings.Default.Save();
        if (failed)
        {
          ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = _tempOldString;
          //ShowNotification(ErrorMessage);
          SelectedTextBox.tb.Background = System.Windows.Media.Brushes.Red;
        }
        else
        {
          if (_tempNewString.TrimStart('0') != "")
            ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = _tempNewString.TrimStart('0');
          else
            ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = "0";
          SelectedTextBox.tb.Background = (System.Windows.Media.Brush)App.Current.Resources["AppBackground"];
        }
        _tempNewString = null;
        _tempOldString = null;
      }
      else if (SelectedTextBox.prop != null && _tempNewString == null)
      {
        SelectedTextBox.tb.Background = (System.Windows.Media.Brush)App.Current.Resources["AppBackground"];
      }
      SelectedTextBox = (null, null, 0, null);
    }

    public static void HideNumpad()
    {
      InputSanityCheck();
      SelectedTextBox = (null, null, 0, null);
      NumpadShow.prop.SetValue(NumpadShow.VM, Visibility.Hidden);
    }

    public static void HideKeyboard()
    {
      InputSanityCheck();
      SelectedTextBox = (null, null, 0, null);
      KeyboardShow.prop.SetValue(KeyboardShow.VM, Visibility.Hidden);
    }

    private static void TimerTick(object sender, EventArgs e)
    {
      if (_isStartup) //TODO: can be a Task launched from ctor, that polls if all instances are != null
        OnAppLoaded();

      TextBoxHandler.Update();
      if (MicroCyDevice.IsMeasurementGoing)
      {
        GraphsHandler.Update();
        ActiveRegionsStatsHandler.Update();
        TextBoxHandler.UpdateEventCounter();
        WellStateHandler();
      }
      ServiceMenuEnabler.Update();
    }

    private static void WellStateHandler()
    {
      // end of well state machine
      switch (MicroCyDevice.EndState)
      {
        case 0:
          break;
        case 1:
          Device.StopWellMeasurement();
          MicroCyDevice.EndState++;
          break;
        case 2:
          var tempres = new List<WellResults>(MicroCyDevice.WellResults.Count);
          for(var i = 0; i < MicroCyDevice.WellResults.Count; i++)
          {
            var r = new WellResults();
            r.RP1vals = new List<float>(MicroCyDevice.WellResults[i].RP1vals);
            r.RP1bgnd = new List<float>(MicroCyDevice.WellResults[i].RP1bgnd);
            r.regionNumber = MicroCyDevice.WellResults[i].regionNumber;
            tempres.Add(r);
          }
          _ = Task.Run(() => ResultReporter.SaveBeadFile(tempres));
          Device.GStatsFiller();
          MicroCyDevice.EndState++;
          Console.WriteLine($"{DateTime.Now.ToString()} Reporting Background File Save Init");
          break;
        case 3:
          if (!MainButtonsViewModel.Instance.ActiveList.Contains("WASHING"))
          {
            MicroCyDevice.EndState++;  //wait here until alternate syringe is finished washing
            Console.WriteLine($"{DateTime.Now.ToString()} Reporting Washing Complete");
          }
          else
            Device.MainCommand("Get Property", code: 0xcc);
          break;
        case 4:
          Device.WellNext();  //saves current well address for filename in state 5
          MicroCyDevice.EndState++;
          Console.WriteLine($"{DateTime.Now.ToString()} Reporting Setting up next well");
          break;
        case 5:
          Device.MainCommand("FlushCmdQueue");
          Device.MainCommand("Set Property", code: 0xc3); //clear empty syringe token
          Device.MainCommand("Set Property", code: 0xcb); //clear sync token to allow next sequence to execute
          if (MicroCyDevice.Mode == OperationMode.Normal)
          {
            Device.EndBeadRead(MapRegions.ActiveRegionNums);
          }
          else
          {
            Device.EndBeadRead();
          }
          MicroCyDevice.EndState = 0;
          Task.Run(()=>
          {
            GC.Collect();
            GC.WaitForPendingFinalizers();
          });
          if (Device.CurrentWellIdx == (MicroCyDevice.WellsToRead + 1)) //if only one more to go
          {
            DashboardViewModel.Instance.WorkOrder[0] = "";
            Device.MainCommand("Set Property", code: 0x19);  //bubble detect off
          }
          Console.WriteLine($"{DateTime.Now.ToString()} Reporting End of current well");
          break;
      }
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
      var warning = Models.WellWarningState.OK;
      if (_nextWellWarning)
      {
        _nextWellWarning = false;
        warning = Models.WellWarningState.YellowWarning;
      }
      
      MultiTube.GetModifiedWellIndexes(e, out var row, out var col);

      ResultsViewModel.Instance.PlatePictogram.CurrentlyReadCell = (row, col);
      ResultsViewModel.Instance.PlatePictogram.ChangeState(row, col, Models.WellType.NowReading, warning, FilePath: e.FilePath);
      ResultsViewModel.Instance.CornerButtonClick(Models.DrawingPlate.CalculateCorner(row, col));
      ResultsViewModel.Instance.ClearGraphs();
      MapRegions.ResetCurrentActiveRegionsDisplayedStats();
#if DEBUG
      Device.MainCommand("Set FProperty", code: 0x06);
#endif
    }

    public static void FinishedReadingWellEventHandler(object sender, ReadingWellEventArgs e)
    {
      var type = GetWellStateForPictogram();
      
      MultiTube.GetModifiedWellIndexes(e, out var row, out var col);
      MultiTube.Proceed();

      ResultsViewModel.Instance.PlatePictogram.ChangeState(row, col, type);
      SavePlateState();
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
            ShowLocalizedNotification(nameof(Language.Resources.Calibration_Fail), System.Windows.Media.Brushes.Red);
            DashboardViewModel.Instance.CalModeToggle();
          }
          else if (CalibrationViewModel.Instance.CalJustFailed)
            ShowLocalizedNotification(nameof(Language.Resources.Calibration_in_Progress), System.Windows.Media.Brushes.Green);
          break;
        case OperationMode.Verification:
          Validator.CalculateResults();
          if (VerificationViewModel.AnalyzeVerificationResults())
          {
            _ = Current.Dispatcher.BeginInvoke((Action)VerificationViewModel.VerificationSuccess);
          }
          else
            ShowLocalizedNotification(nameof(Language.Resources.Validation_Fail), System.Windows.Media.Brushes.Red);
          break;
      }
    }

    public static void NewStatsAvailableEventHandler(object sender, GstatsEventArgs e)
    {
      _ = Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        for (var i = 0; i < 10; i++)
        {
          ResultsViewModel.Instance.MfiItems[i] = e.GStats[i].mfi.ToString($"{0:0.0}");
          ResultsViewModel.Instance.CvItems[i] = e.GStats[i].cv.ToString($"{0:0.00}");
        }
      }));
    }

    public static void SetLogOutput()
    {
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

    private static void OnAppLoaded()
    {
      MapRegions = Models.MapRegions.Create(
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
      if (Settings.Default.LastTemplate != "None")
      {
        TemplateSelectViewModel.Instance.SelectedItem = Settings.Default.LastTemplate;
        TemplateSelectViewModel.Instance.LoadTemplate();
      }
      ResultsViewModel.Instance.FillWorldMaps();
      SetLanguage(MaintenanceViewModel.Instance.LanguageItems[Settings.Default.Language].Locale);
      Program.SplashScreen.Close(TimeSpan.FromMilliseconds(1000));
      Views.ExperimentView.Instance.DbButton.IsChecked = true;
      Device.MainCommand("Get Property", code: 0x01);

      var matrix = Views.ResultsView.Instance.AnalysisPlot.ContentTransform.Value;
      matrix.Rotate(new System.Windows.Media.Media3D.Quaternion(new System.Windows.Media.Media3D.Vector3D(0, 1, 0), 90));
      matrix.Rotate(new System.Windows.Media.Media3D.Quaternion(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), 40));
      matrix.Rotate(new System.Windows.Media.Media3D.Quaternion(new System.Windows.Media.Media3D.Vector3D(0, 1, 0), -15));
      matrix.Translate(new System.Windows.Media.Media3D.Vector3D(-100, 100, 0));
      ((System.Windows.Media.Media3D.MatrixTransform3D)Views.ResultsView.Instance.AnalysisPlot.ContentTransform).Matrix = matrix;
      CheckAvailableWorkOrders();
      _isStartup = false;
    }

    public static void ShowNotification(string text, System.Windows.Media.Brush background = null)
    {
      NotificationViewModel.Instance.Text[0] = text;
      if (background != null)
        NotificationViewModel.Instance.Background = background;
      NotificationViewModel.Instance.NotificationVisible = Visibility.Visible;
    }

    public static void ShowNotification(string text, Action action1, string actionButton1Text, System.Windows.Media.Brush background = null )
    {
      NotificationViewModel.Instance.Action1 = action1;
      NotificationViewModel.Instance.ActionButtonText[0] = actionButton1Text;
      NotificationViewModel.Instance.ButtonVisible[0] = Visibility.Visible;
      NotificationViewModel.Instance.ButtonVisible[2] = Visibility.Hidden;
      ShowNotification(text, background);
    }

    public static void ShowNotification(string text, Action action1, string actionButton1Text, Action action2, string actionButton2Text, System.Windows.Media.Brush background = null)
    {
      NotificationViewModel.Instance.Action2 = action2;
      NotificationViewModel.Instance.ActionButtonText[1] = actionButton2Text;
      NotificationViewModel.Instance.ButtonVisible[1] = Visibility.Visible;
      ShowNotification(text, action1, actionButton1Text, background);
    }

    public static void ShowLocalizedNotification(string nameofLocalizationString, System.Windows.Media.Brush background = null)
    {
      ShowNotification(Language.Resources.ResourceManager.GetString(nameofLocalizationString,
          Language.TranslationSource.Instance.CurrentCulture), background);
    }

    public static void ShowLocalizedNotification(string nameofLocalizationString, Action action1, string nameofActionButton1Text, System.Windows.Media.Brush background = null)
    {
      NotificationViewModel.Instance.Action1 = action1;
      NotificationViewModel.Instance.ActionButtonText[0] = Language.Resources.ResourceManager.GetString(nameofActionButton1Text,
          Language.TranslationSource.Instance.CurrentCulture);
      NotificationViewModel.Instance.ButtonVisible[0] = Visibility.Visible;
      NotificationViewModel.Instance.ButtonVisible[2] = Visibility.Hidden;
      ShowLocalizedNotification(nameofLocalizationString, background);
    }

    public static void ShowLocalizedNotification(string nameofLocalizationString, Action action1, string nameofActionButton1Text, Action action2, string nameofActionButton2Text, System.Windows.Media.Brush background = null)
    {
      NotificationViewModel.Instance.Action2 = action2;
      NotificationViewModel.Instance.ActionButtonText[1] = Language.Resources.ResourceManager.GetString(nameofActionButton2Text,
          Language.TranslationSource.Instance.CurrentCulture);
      NotificationViewModel.Instance.ButtonVisible[1] = Visibility.Visible;
      ShowLocalizedNotification(nameofLocalizationString, action1, nameofActionButton1Text, background);
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
        ShowNotification($"Problem with status file save, Please report this issue to the Manufacturer {e.Message}");
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