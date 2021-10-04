using System.Reflection;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows.Threading;
using System.Windows;
using Ei_Dimension.ViewModels;
using MicroCy;
using System.Threading.Tasks;

namespace Ei_Dimension
{
  public partial class App : Application
  {
    public static (PropertyInfo prop, object VM) NumpadShow { get; set; }
    public static (PropertyInfo prop, object VM, int index) SelectedTextBox { get; set; }
    public static MicroCyDevice Device { get; private set; }
    public static SplashScreen SplashScreen { get; private set; }
    private static DispatcherTimer _dispatcherTimer;
    private static bool _workOrderPending;
    private static bool _cancelKeyboardInjectionFlag;
    private static bool _histogramUpdateGoing;
    public App()
    {
      SplashScreen = Program.SplashScreen;
      _cancelKeyboardInjectionFlag = false;
      SetLanguage("en-US");
      Device = new MicroCyDevice(typeof(USBConnection), useStaticMaps: true);
      try
      {
        Device.ActiveMap = Device.MapList[Settings.Default.DefaultMap];
      }
      catch { }
      Device.SystemControl = Settings.Default.SystemControl;
      Device.Compensation = Settings.Default.Compensation;
      Device.SampleSize = Settings.Default.SampleSize;
      // RegCtr_SampSize.Text = Device.SampleSize.ToString(); //bead maps
      Device.Outfilename = Settings.Default.SaveFileName;
      Device.Everyevent = Settings.Default.Everyevent;
      Device.RMeans = Settings.Default.RMeans;
      Device.PltRept = Settings.Default.PlateReport;
      Device.TerminationType = Settings.Default.EndRead;
      Device.MinPerRegion = Settings.Default.MinPerRegion;
      Device.BeadsToCapture = Settings.Default.BeadsToCapture;
      if (!System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
        Device.MainCommand("Startup");
      Device.XAxisSel = Settings.Default.XAxisG;        //TODO: delete this, when refactor ReplyFromMC. only needed in legacy to build graphs
      Device.YAxisSel = Settings.Default.YAxisG;        //TODO: delete this, when refactor ReplyFromMC. only needed in legacy to build graphs
      Device.HdnrTrans = Settings.Default.HDnrTrans;
      Device.MainCommand("Set Property", code: 0x97, parameter: 1170);  //set current limit of aligner motors if leds are off
      Device.MainCommand("Get Property", code: 0xca);

      _dispatcherTimer = new DispatcherTimer();
      _dispatcherTimer.Tick += TimerTick;
      _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
      _dispatcherTimer.Start();
      _workOrderPending = false;
      _histogramUpdateGoing = false;
    }

    public static int GetActiveMapIndex()
    {
      return GetMapIndex(Device.ActiveMap.mapName);
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
      for(var i = 0; i < Device.MapList.Count; i++)
      {
        if (Device.MapList[i].mapName == mapName)
        {
          Device.ActiveMap = Device.MapList[i];
          Settings.Default.DefaultMap = i;
          Settings.Default.Save();
          break;
        }
      }
      var CaliVM = CalibrationViewModel.Instance;
      if (CaliVM != null)
      {
        CaliVM.CurrentMapName[0] = Device.ActiveMap.mapName;
        CaliVM.EventTriggerContents[1] = Device.ActiveMap.minmapssc.ToString();
        CaliVM.EventTriggerContents[2] = Device.ActiveMap.maxmapssc.ToString();
      }
      var ChannelsVM = ChannelsViewModel.Instance;
      if (ChannelsVM != null)
      {
        ChannelsVM.CurrentMapName[0] = Device.ActiveMap.mapName;
        ChannelsVM.Bias30Parameters[0] = Device.ActiveMap.calgssc.ToString();
        ChannelsVM.Bias30Parameters[1] = Device.ActiveMap.calrpmaj.ToString();
        ChannelsVM.Bias30Parameters[2] = Device.ActiveMap.calrpmin.ToString();
        ChannelsVM.Bias30Parameters[4] = Device.ActiveMap.calrssc.ToString();
        ChannelsVM.Bias30Parameters[5] = Device.ActiveMap.calcl1.ToString();
        ChannelsVM.Bias30Parameters[6] = Device.ActiveMap.calcl2.ToString();
        ChannelsVM.Bias30Parameters[7] = Device.ActiveMap.calvssc.ToString();
        if (Device.ActiveMap.dimension3)
        {
          ChannelsVM.Bias30Parameters[3] = Device.ActiveMap.calcl3.ToString();
          ChannelsVM.Bias30Parameters[8] = Device.ActiveMap.calcl0.ToString();
        }
      }
    }

    public static void SetSystemControl(byte num)
    {
      Device.SystemControl = num;
      Settings.Default.SystemControl = num;
      Settings.Default.Save();
    }

    public static void SetTerminationType(byte num)
    {
      Device.TerminationType = num;
      Settings.Default.EndRead = num;
      Settings.Default.Save();
    }

    public static void SetHeatmapX(byte num)
    {
      Device.XAxisSel = num;
      Settings.Default.XAxisG = num;
      Settings.Default.Save();
    }

    public static void SetHeatmapY(byte num)
    {
      Device.YAxisSel = num;
      Settings.Default.YAxisG = num;
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
        ComponentsVM.InputSelectorState.Clear();
        if (ComponentsVM.IInputSelectorState == 0)
        {
          ComponentsVM.InputSelectorState.Add(RM.GetString(nameof(Language.Resources.Components_To_Pickup),
            curCulture));
          ComponentsVM.InputSelectorState.Add(RM.GetString(nameof(Language.Resources.Components_To_Cuvet),
            curCulture));
        }
        else
        {
          ComponentsVM.InputSelectorState.Add(RM.GetString(nameof(Language.Resources.Components_To_Cuvet),
            curCulture));
          ComponentsVM.InputSelectorState.Add(RM.GetString(nameof(Language.Resources.Components_To_Pickup),
            curCulture));
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
      var ExperimentVM = ExperimentViewModel.Instance;
      if (ExperimentVM != null)
      {
        ExperimentVM.DialogFilter = RM.GetString(nameof(Language.Resources.Text_Files), curCulture) +
          "|*.txt|" + RM.GetString(nameof(Language.Resources.All_Files), curCulture) + "|*.*";
        ExperimentVM.DialogTitleSave = RM.GetString(nameof(Language.Resources.Experiment_Save_Template_Dialog_Title), curCulture);
        ExperimentVM.DialogTitleLoad = RM.GetString(nameof(Language.Resources.Experiment_Load_Template_Dialog_Title), curCulture);
      }
      var ChannelsVM = ChannelsViewModel.Instance;
      if (ChannelsVM != null)
      {
        ChannelsVM.SensitivityItems[0].Content = RM.GetString(nameof(Language.Resources.Channels_Sens_B), curCulture);
        ChannelsVM.SensitivityItems[1].Content = RM.GetString(nameof(Language.Resources.Channels_Sens_C), curCulture);
        int i = Device.ChannelBIsHiSensitivity ? 0 : 1;
        ChannelsVM.SelectedSensitivityContent = ChannelsVM.SensitivityItems[i].Content;
      }
      #endregion
    }

    public static void InjectToFocusedTextbox(string input, bool keyboardinput = false)
    {
      if (SelectedTextBox.prop != null && !_cancelKeyboardInjectionFlag)
      {
        string temp = "";
        if (keyboardinput)
        {
          temp = input;
        }
        else
        {
          _cancelKeyboardInjectionFlag = true;
          temp = ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index];
          if (input == "")
          {
            if (temp.Length > 0)
              ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = temp =
                temp.Remove(temp.Length - 1, 1);
          }
          else
            ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = temp += input;
          _cancelKeyboardInjectionFlag = false;
        }
        float fRes;
        int iRes;
        ushort usRes;
        byte bRes;
        switch (SelectedTextBox.prop.Name)
        {
          case "CompensationPercentageContent":
            if (float.TryParse(temp, out fRes))
            {
              Device.Compensation = fRes;
              Settings.Default.Compensation = fRes;
            }
            break;
          case "DNRContents":
            if (SelectedTextBox.index == 0)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.HDnrCoef = fRes;
                Device.MainCommand("Set FProperty", code: 0x20, fparameter: fRes);
              }
            }
            if (SelectedTextBox.index == 1)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.HdnrTrans = fRes;
                Settings.Default.HDnrTrans = fRes;
              }
            }
            break;
          case "EndRead":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MinPerRegion = iRes;
                Settings.Default.MinPerRegion = iRes;
              }
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.BeadsToCapture = iRes;
                Settings.Default.BeadsToCapture = iRes;
              }
            }
            break;
          case "Volumes":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0xaf, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0xac, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0xc4, parameter: (ushort)iRes);
              }
            }
            break;
          case "EventTriggerContents":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0xcd, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0xce, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0xcf, parameter: (ushort)iRes);
              }
            }
            break;
          case "ClassificationTargetsContents":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(temp, out iRes) && iRes > 0 && iRes < 30000)
              {
                Device.MainCommand("Set Property", code: 0x8b, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(temp, out iRes) && iRes > 0 && iRes < 30000)
              {
                //  chart2.Series["CLTARGET"].Points.RemoveAt(0);
                //  chart2.Series["CLTARGET"].Points.AddXY(jj, kk);
                Device.MainCommand("Set Property", code: 0x8c, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(temp, out iRes) && iRes > 0 && iRes < 30000)
              {
                //  chart2.Series["CLTARGET"].Points.RemoveAt(0);
                //  chart2.Series["CLTARGET"].Points.AddXY(jj, kk);
                Device.MainCommand("Set Property", code: 0x8d, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(temp, out iRes) && iRes > 0 && iRes < 30000)
              {
                Device.MainCommand("Set Property", code: 0x8e, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(temp, out iRes) && iRes > 0 && iRes < 30000)
              {
                Device.MainCommand("Set Property", code: 0x8f, parameter: (ushort)iRes);
              }
            }
            break;
          case "SheathSyringeParameters":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x30, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x31, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x32, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x33, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x34, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 5)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x35, parameter: (ushort)iRes);
              }
            }
            break;
          case "SamplesSyringeParameters":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x38, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x39, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x3a, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x3b, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x3c, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 5)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x3d, parameter: (ushort)iRes);
              }
            }
            break;
          case "SiPMTempCoeff":
            if (float.TryParse(temp, out fRes))
            {
              Device.MainCommand("Set FProperty", code: 0x02, fparameter: fRes);
            }
            break;
          case "Bias30Parameters":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x28, parameter: (ushort)iRes);
                Device.TempGreenSsc = iRes;
              }
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x29, parameter: (ushort)iRes);
                Device.TempRpMaj = iRes;
              }
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x2a, parameter: (ushort)iRes);
                Device.TempRpMin = iRes;
              }
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x2c, parameter: (ushort)iRes);
                Device.TempCl3 = iRes;
              }
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x2d, parameter: (ushort)iRes);
                Device.TempRedSsc = iRes;
              }
            }
            if (SelectedTextBox.index == 5)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x2e, parameter: (ushort)iRes);
                Device.TempCl1 = iRes;
              }
            }
            if (SelectedTextBox.index == 6)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x2f, parameter: (ushort)iRes);
                Device.TempCl2 = iRes;
              }
            }
            if (SelectedTextBox.index == 7)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x25, parameter: (ushort)iRes);
                Device.TempVioletSsc = iRes;
              }
            }
            if (SelectedTextBox.index == 8)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x26, parameter: (ushort)iRes);
                Device.TempCl0 = iRes;
              }
            }
            if (SelectedTextBox.index == 9)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x24, parameter: (ushort)iRes);
              }
            }
            break;
          case "ChannelsOffsetParameters":
            if (SelectedTextBox.index == 0)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0xa0, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 1)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0xa4, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0xa5, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0xa3, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0xa2, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 5)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0xa1, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 6)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x9f, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 7)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x9d, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 8)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x9c, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 9)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x9e, parameter: (ushort)iRes);
              }
            }
            break;
          case "ParametersX":
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x53, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x51, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x52, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 6)
            {
              if (ushort.TryParse(temp, out usRes))
              {
                Device.MainCommand("Set Property", code: 0x50, parameter: (ushort)usRes);
                Settings.Default.StepsPerRevX = usRes;
              }
            }
            if (SelectedTextBox.index == 7)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x90, parameter: (ushort)iRes);
              }
            }
            break;
          case "ParametersY":
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x63, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x61, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x62, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 6)
            {
              if (ushort.TryParse(temp, out usRes))
              {
                Device.MainCommand("Set Property", code: 0x60, parameter: (ushort)usRes);
                Settings.Default.StepsPerRevY = usRes;
              }
            }
            if (SelectedTextBox.index == 7)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x91, parameter: (ushort)iRes);
              }
            }
            break;
          case "ParametersZ":
            if (SelectedTextBox.index == 2)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x43, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 3)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x41, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 4)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x42, parameter: (ushort)iRes);
              }
            }
            if (SelectedTextBox.index == 6)
            {
              if (ushort.TryParse(temp, out usRes))
              {
                Device.MainCommand("Set Property", code: 0x40, parameter: (ushort)usRes);
                Settings.Default.StepsPerRevZ = usRes;
              }
            }
            if (SelectedTextBox.index == 7)
            {
              if (int.TryParse(temp, out iRes))
              {
                Device.MainCommand("Set Property", code: 0x92, parameter: (ushort)iRes);
              }
            }
            break;
          case "StepsParametersX":
            if (SelectedTextBox.index == 0)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.MainCommand("Set FProperty", code: 0x58, fparameter: fRes);
              }
            }
            if (SelectedTextBox.index == 1)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.MainCommand("Set FProperty", code: 0x5a, fparameter: fRes);
              }
            }
            if (SelectedTextBox.index == 2)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.MainCommand("Set FProperty", code: 0x5c, fparameter: fRes);
              }
            }
            if (SelectedTextBox.index == 3)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.MainCommand("Set FProperty", code: 0x5e, fparameter: fRes);
              }
            }
            if (SelectedTextBox.index == 4)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.MainCommand("Set FProperty", code: 0x56, fparameter: fRes);
              }
            }
            break;
          case "StepsParametersY":
            if (SelectedTextBox.index == 0)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.MainCommand("Set FProperty", code: 0x68, fparameter: fRes);
              }
            }
            if (SelectedTextBox.index == 1)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.MainCommand("Set FProperty", code: 0x6a, fparameter: fRes);
              }
            }
            if (SelectedTextBox.index == 2)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.MainCommand("Set FProperty", code: 0x6c, fparameter: fRes);
              }
            }
            if (SelectedTextBox.index == 3)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.MainCommand("Set FProperty", code: 0x6e, fparameter: fRes);
              }
            }
            if (SelectedTextBox.index == 4)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.MainCommand("Set FProperty", code: 0x66, fparameter: fRes);
              }
            }
            break;
          case "StepsParametersZ":
            if (SelectedTextBox.index == 0)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.MainCommand("Set FProperty", code: 0x48, fparameter: fRes);
              }
            }
            if (SelectedTextBox.index == 1)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.MainCommand("Set FProperty", code: 0x4a, fparameter: fRes);
              }
            }
            if (SelectedTextBox.index == 2)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.MainCommand("Set FProperty", code: 0x4c, fparameter: fRes);
              }
            }
            if (SelectedTextBox.index == 3)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.MainCommand("Set FProperty", code: 0x4e, fparameter: fRes);
              }
            }
            if (SelectedTextBox.index == 4)
            {
              if (float.TryParse(temp, out fRes))
              {
                Device.MainCommand("Set FProperty", code: 0x46, fparameter: fRes);
              }
            }
            break;
          case "IdexTextBoxInputs":
            if (SelectedTextBox.index == 0)
            {
              if (byte.TryParse(temp, out bRes))
              {
                Device.IdexPos = bRes;
              }
            }
            if (SelectedTextBox.index == 1)
            {
              if (ushort.TryParse(temp, out usRes))
              {
                Device.IdexSteps = usRes;
              }
            }
            break;
          case "BaseFileName":
            Device.Outfilename = temp;
            Settings.Default.SaveFileName = temp;
            break;
        }
        Settings.Default.Save();
      }
    }

    public static void ResetFocusedTextbox()
    {
      SelectedTextBox = (null, null, 0);
    }

    public static void HideNumpad()
    {
      NumpadShow.prop.SetValue(NumpadShow.VM, Visibility.Hidden);
    }

    public static void AcquisitionTemplateLoaded(Models.AcquisitionTemplate newTemplate)
    {
      try
      {
        var DashVM = DashboardViewModel.Instance;
        DashVM.SpeedItems[newTemplate.Speed].Click(1);
        DashVM.ClassiMapItems[GetMapIndex(newTemplate.Map)].Click(2);
        DashVM.ChConfigItems[newTemplate.ChConfig].Click(3);
        DashVM.OrderItems[newTemplate.Order].Click(4);
        DashVM.SysControlItems[newTemplate.SysControl].Click(5);
        DashVM.EndReadItems[newTemplate.EndRead].Click(6);
        DashVM.EndRead[0] = newTemplate.MinPerRegion.ToString();
        DashVM.FocusedBox(0);
        InjectToFocusedTextbox(newTemplate.MinPerRegion.ToString(), true);
        DashVM.EndRead[1] = newTemplate.TotalEvents.ToString();
        DashVM.FocusedBox(1);
        InjectToFocusedTextbox(newTemplate.TotalEvents.ToString(), true);
        DashVM.Volumes[0] = newTemplate.SampleVolume.ToString();
        DashVM.FocusedBox(2);
        InjectToFocusedTextbox(newTemplate.SampleVolume.ToString(), true);
        DashVM.Volumes[1] = newTemplate.WashVolume.ToString();
        DashVM.FocusedBox(3);
        InjectToFocusedTextbox(newTemplate.WashVolume.ToString(), true);
        DashVM.Volumes[2] = newTemplate.AgitateVolume.ToString();
        DashVM.FocusedBox(4);
        InjectToFocusedTextbox(newTemplate.AgitateVolume.ToString(), true);
      }
      catch { }
    }

    private static void TimerTick(object sender, EventArgs e)
    {
      TextBoxUpdater();
      HistogramHandler();
      UpdateEventCounter();
      UpdatePressureMonitor();
      WellStateHandler();
      WorkOrderHandler();
      //TODO: REmove: for map construction in another program
      /*
      if (Device.Newmap)
      {
        //  chart2.Series["REGIONS"].Enabled = true;
        Device.Newmap = false;
        for (var x = 0; x < 255; x++)
        {
          for (int y = 0; y < 255; y++)
          {
            if (Device._classificationMap[x, y] > 0) //TODO: remove
            {
              float expx = (float)Math.Exp(x / 24.526);
              float expy = (float)Math.Exp(y / 24.526);
              //  chart2.Series["REGIONS"].Points.AddXY(expx, expy);
            }
          }
        }
      }
      */
    }

    private static void TextBoxUpdater()
    {
      float g = 0;
      while (Device.Commands.Count != 0)
      {
        lock (Device.Commands)
        {
          var exe = Device.Commands.Dequeue();
          switch (exe.Code)
          {
            case 0x02:
              ChannelsViewModel.Instance.SiPMTempCoeff[0] = exe.FParameter.ToString();
              break;
            case 0x04:
              ComponentsViewModel.Instance.IdexTextBoxInputs[0] = exe.Parameter.ToString("X2");
              break;
            case 0x10:  //cuvet drain cb
              ComponentsViewModel.Instance.ValvesStates[2] = exe.Parameter == 1;
              break;
            case 0x11:  //Fan
              ComponentsViewModel.Instance.ValvesStates[3] = exe.Parameter == 1;
              break;
            case 0x12:  //sample A valve cb LEGACY, use 0x18 to switch with IDEX
              ComponentsViewModel.Instance.ValvesStates[0] = exe.Parameter == 1;
              break;
            case 0x13:  //sample b LEGACY
              ComponentsViewModel.Instance.ValvesStates[1] = exe.Parameter == 1;
              break;
            case 0x14:
              switch (exe.Parameter)
              {
                case 0:
                  ComponentsViewModel.Instance.GetPositionTextBoxInputs[0] = exe.FParameter.ToString();
                  break;
                case 1:
                  ComponentsViewModel.Instance.GetPositionTextBoxInputs[1] = exe.FParameter.ToString();
                  break;
                case 2:
                  ComponentsViewModel.Instance.GetPositionTextBoxInputs[2] = exe.FParameter.ToString();
                  break;
              }
              break;
            case 0x15:
              ComponentsViewModel.Instance.GetPositionToggleButtonStateBool[0] = exe.Parameter == 1;
              break;
            case 0x16:
              MotorsViewModel.Instance.PollStepActive[0] = exe.Parameter == 1;
              break;
            case 0x18:
              if (exe.Parameter == 1)
              {
                if (ComponentsViewModel.Instance.IInputSelectorState == 0)
                {
                  var temp = ComponentsViewModel.Instance.InputSelectorState[0];
                  ComponentsViewModel.Instance.InputSelectorState[0] = ComponentsViewModel.Instance.InputSelectorState[1];
                  ComponentsViewModel.Instance.InputSelectorState[1] = temp;
                  ComponentsViewModel.Instance.IInputSelectorState = 1;
                }
              }
              else
              {
                if (ComponentsViewModel.Instance.IInputSelectorState == 1)
                {
                  var temp = ComponentsViewModel.Instance.InputSelectorState[0];
                  ComponentsViewModel.Instance.InputSelectorState[0] = ComponentsViewModel.Instance.InputSelectorState[1];
                  ComponentsViewModel.Instance.InputSelectorState[1] = temp;
                  ComponentsViewModel.Instance.IInputSelectorState = 0;
                }
              }
              break;
            //case 0x1a:
            //  ProxOncb.Checked = (exe.Parameter == 1);
            //  break;
            case 0x1b:
              if (exe.Parameter == 0)
              {
                CalibrationViewModel.Instance.CalibrationSelectorState[1] = false;
                CalibrationViewModel.Instance.CalibrationSelectorState[2] = false;
                CalibrationViewModel.Instance.CalibrationSelectorState[0] = true;
                CalibrationViewModel.Instance.CalibrationParameter = 0;
              }
              break;
            case 0x20:
              CalibrationViewModel.Instance.DNRContents[0] = exe.FParameter.ToString();
              Device.HDnrCoef = exe.FParameter;
              break;
            case 0x22:  //pressure
              double dd = exe.FParameter;
              DashboardViewModel.Instance.PressureMon[0] = dd.ToString("f3");
              if (dd < DashboardViewModel.Instance.MinPressure)
                DashboardViewModel.Instance.MinPressure = dd;
              if (dd > DashboardViewModel.Instance.MaxPressure)
                DashboardViewModel.Instance.MaxPressure = dd;
              DashboardViewModel.Instance.PressureMon[1] = DashboardViewModel.Instance.MaxPressure.ToString("f3");
              DashboardViewModel.Instance.PressureMon[2] = DashboardViewModel.Instance.MinPressure.ToString("f3");
              break;
            case 0x24:
              ChannelsViewModel.Instance.Bias30Parameters[9] = exe.Parameter.ToString(); 
              break;
            case 0x25:
              ChannelsViewModel.Instance.Bias30Parameters[7] = exe.Parameter.ToString();
              break;
            case 0x26:
              ChannelsViewModel.Instance.Bias30Parameters[8] = exe.Parameter.ToString();
              break;
            case 0x28:
              ChannelsViewModel.Instance.Bias30Parameters[0] = exe.Parameter.ToString();
              break;
            case 0x29:
              ChannelsViewModel.Instance.Bias30Parameters[1] = exe.Parameter.ToString();
              break;
            case 0x2a:
              ChannelsViewModel.Instance.Bias30Parameters[2] = exe.Parameter.ToString();
              break;
            case 0x2c:
              ChannelsViewModel.Instance.Bias30Parameters[3] = exe.Parameter.ToString();
              break;
            case 0x2d:
              ChannelsViewModel.Instance.Bias30Parameters[4] = exe.Parameter.ToString();
              break;
            case 0x2e:
              ChannelsViewModel.Instance.Bias30Parameters[5] = exe.Parameter.ToString();
              break;
            case 0x2f:
              ChannelsViewModel.Instance.Bias30Parameters[6] = exe.Parameter.ToString();
              break;
            case 0x30:
              SyringeSpeedsViewModel.Instance.SheathSyringeParameters[0] = exe.Parameter.ToString();
              break;
            case 0x31:
              SyringeSpeedsViewModel.Instance.SheathSyringeParameters[1] = exe.Parameter.ToString();
              break;
            case 0x32:
              SyringeSpeedsViewModel.Instance.SheathSyringeParameters[2] = exe.Parameter.ToString();
              break;
            case 0x33:
              SyringeSpeedsViewModel.Instance.SheathSyringeParameters[3] = exe.Parameter.ToString();
              break;
            case 0x34:
              SyringeSpeedsViewModel.Instance.SheathSyringeParameters[4] = exe.Parameter.ToString();
              break;
            case 0x35:
              SyringeSpeedsViewModel.Instance.SheathSyringeParameters[5] = exe.Parameter.ToString();
              break;
            case 0x38:
              SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[0] = exe.Parameter.ToString();
              break;
            case 0x39:
              SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[1] = exe.Parameter.ToString();
              break;
            case 0x3a:
              SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[2] = exe.Parameter.ToString();
              break;
            case 0x3b:
              SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[3] = exe.Parameter.ToString();
              break;
            case 0x3c:
              SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[4] = exe.Parameter.ToString();
              break;
            case 0x3d:
              SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[5] = exe.Parameter.ToString();
              break;
            case 0x41:
              MotorsViewModel.Instance.ParametersZ[3] = exe.Parameter.ToString();
              break;
            case 0x42:
              MotorsViewModel.Instance.ParametersZ[4] = exe.Parameter.ToString();
              break;
            case 0x43:
              MotorsViewModel.Instance.ParametersZ[2] = exe.Parameter.ToString();
              break;
            case 0x44:
              MotorsViewModel.Instance.ParametersZ[5] = exe.FParameter.ToString();
              break;
            case 0x46:
              MotorsViewModel.Instance.StepsParametersZ[4] = exe.FParameter.ToString();
              break;
            case 0x48:
              MotorsViewModel.Instance.StepsParametersZ[0] = exe.FParameter.ToString();
              break;
            case 0x4a:
              MotorsViewModel.Instance.StepsParametersZ[1] = exe.FParameter.ToString();
              break;
            case 0x4c:
              MotorsViewModel.Instance.StepsParametersZ[2] = exe.FParameter.ToString();
              break;
            case 0x4e:
              MotorsViewModel.Instance.StepsParametersZ[3] = exe.FParameter.ToString();
              break;
            case 0x51:
              MotorsViewModel.Instance.ParametersX[3] = exe.Parameter.ToString();
              break;
            case 0x52:
              MotorsViewModel.Instance.ParametersX[4] = exe.Parameter.ToString();
              break;
            case 0x53:
              MotorsViewModel.Instance.ParametersX[2] = exe.Parameter.ToString();
              break;
            case 0x54:
              MotorsViewModel.Instance.ParametersX[5] = exe.FParameter.ToString();
              break;
            case 0x56:
              MotorsViewModel.Instance.StepsParametersX[4] = exe.FParameter.ToString();
              break;
            case 0x58:
              MotorsViewModel.Instance.StepsParametersX[0] = exe.FParameter.ToString();
              break;
            case 0x5a:
              MotorsViewModel.Instance.StepsParametersX[1] = exe.FParameter.ToString();
              break;
            case 0x5c:
              MotorsViewModel.Instance.StepsParametersX[2] = exe.FParameter.ToString();
              break;
            case 0x5e:
              MotorsViewModel.Instance.StepsParametersX[3] = exe.FParameter.ToString();
              break;
            case 0x61:
              MotorsViewModel.Instance.ParametersY[3] = exe.Parameter.ToString();
              break;
            case 0x62:
              MotorsViewModel.Instance.ParametersY[4] = exe.Parameter.ToString();
              break;
            case 0x63:
              MotorsViewModel.Instance.ParametersY[2] = exe.Parameter.ToString();
              break;
            case 0x64:
              MotorsViewModel.Instance.ParametersY[5] = exe.FParameter.ToString();
              break;
            case 0x66:
              MotorsViewModel.Instance.StepsParametersY[4] = exe.FParameter.ToString();
              break;
            case 0x68:
              MotorsViewModel.Instance.StepsParametersY[0] = exe.FParameter.ToString();
              break;
            case 0x6a:
              MotorsViewModel.Instance.StepsParametersY[1] = exe.FParameter.ToString();
              break;
            case 0x6c:
              MotorsViewModel.Instance.StepsParametersY[2] = exe.FParameter.ToString();
              break;
            case 0x6e:
              MotorsViewModel.Instance.StepsParametersY[3] = exe.FParameter.ToString();
              break;
            case 0x80:
              ChannelsViewModel.Instance.TempParameters[7] = (exe.Parameter / 10.0).ToString("N1");
              break;
            case 0x81:
              ChannelsViewModel.Instance.TempParameters[8] = (exe.Parameter / 10.0).ToString("N1");
              break;
            case 0x82:
              ChannelsViewModel.Instance.BackgroundParameters[7] = exe.Parameter.ToString();
              break;
            case 0x83:
              ChannelsViewModel.Instance.BackgroundParameters[8] = exe.Parameter.ToString();
              break;
            case 0x84:
              ChannelsViewModel.Instance.TempParameters[9] = (exe.Parameter / 10.0).ToString("N1");
              break;
            case 0x85:
              ChannelsViewModel.Instance.BackgroundParameters[9] = exe.Parameter.ToString();
              break;
            case 0x8b:
              CalibrationViewModel.Instance.ClassificationTargetsContents[0] = exe.Parameter.ToString();
              break;
            case 0x8c:
              CalibrationViewModel.Instance.ClassificationTargetsContents[1] = exe.Parameter.ToString();
              break;
            case 0x8d:
              CalibrationViewModel.Instance.ClassificationTargetsContents[2] = exe.Parameter.ToString();
              break;
            case 0x90:
              MotorsViewModel.Instance.ParametersX[7] = exe.Parameter.ToString();
              break;
            case 0x91:
              MotorsViewModel.Instance.ParametersY[7] = exe.Parameter.ToString();
              break;
            case 0x92:
              MotorsViewModel.Instance.ParametersZ[7] = exe.Parameter.ToString();
              break;
            case 0x93:
              ChannelsViewModel.Instance.TcompBiasParameters[9] = exe.Parameter.ToString();
              break;
            case 0x94:
              ChannelsViewModel.Instance.TcompBiasParameters[8] = exe.Parameter.ToString();
              break;
            case 0x95:
              ChannelsViewModel.Instance.TcompBiasParameters[7] = exe.Parameter.ToString();
              break;
            case 0x96:
              ChannelsViewModel.Instance.TcompBiasParameters[6] = exe.Parameter.ToString();
              break;
            case 0x98:
              ChannelsViewModel.Instance.TcompBiasParameters[4] = exe.Parameter.ToString();
              break;
            case 0x99:
              ChannelsViewModel.Instance.TcompBiasParameters[3] = exe.Parameter.ToString();
              break;
            case 0x9a:
              ChannelsViewModel.Instance.TcompBiasParameters[1] = exe.Parameter.ToString();
              break;
            case 0x9b:
              ChannelsViewModel.Instance.TcompBiasParameters[2] = exe.Parameter.ToString();
              break;
            case 0x9c:
              ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[8] = exe.Parameter.ToString();
              break;
            case 0x9d:
              ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[7] = exe.Parameter.ToString();
              break;
            case 0x9e:
              ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[9] = exe.Parameter.ToString();
              break;
            case 0x9f:
              ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[6] = exe.Parameter.ToString();
              break;
            case 0xa0:
              ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[0] = exe.Parameter.ToString();
              break;
            case 0xa1:
              ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[5] = exe.Parameter.ToString();
              break;
            case 0xa2:
              ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[4] = exe.Parameter.ToString();
              break;
            case 0xa3:
              ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[3] = exe.Parameter.ToString();
              break;
            case 0xa4:
              ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[1] = exe.Parameter.ToString();
              break;
            case 0xa5:
              ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[2] = exe.Parameter.ToString();
              break;
            case 0xa6:
              ChannelsViewModel.Instance.TcompBiasParameters[0] = exe.Parameter.ToString();
              break;
            case 0xa7:
              ChannelsViewModel.Instance.TcompBiasParameters[5] = exe.Parameter.ToString();
              break;
            case 0xa8:
              DashboardViewModel.Instance.OrderItems[exe.Parameter].ForAppUpdater(4);
              break;
            case 0xa9:
              DashboardViewModel.Instance.ClassiMapItems[exe.Parameter].ForAppUpdater(2);
              break;
            case 0xaa:  //read speed
              DashboardViewModel.Instance.SpeedItems[exe.Parameter].ForAppUpdater(1);
              break;
            case 0xac:
              DashboardViewModel.Instance.Volumes[1] = exe.Parameter.ToString();
              break;
            case 0xad:  //TODO: remove?
              if (exe.Parameter > 15)
                exe.Parameter = 0;
              MotorsViewModel.Instance.WellRowButtonItems[exe.Parameter].ForAppUpdater(1);
              Device.PlateRow = (byte)exe.Parameter;
              break;
            case 0xae:  //TODO: remove?
              if (exe.Parameter > 24)
                exe.Parameter = 0;
              MotorsViewModel.Instance.WellColumnButtonItems[exe.Parameter].ForAppUpdater(2);
              Device.PlateCol = (byte)exe.Parameter;  //TODO: it doesn't accout for 96well; can go overboard and crash
              break;
            case 0xaf:
              DashboardViewModel.Instance.Volumes[0] = exe.Parameter.ToString();
              break;
            case 0xb0:
              ChannelsViewModel.Instance.TempParameters[0] = (exe.Parameter / 10.0).ToString("N1");
              break;
            case 0xb1:
              ChannelsViewModel.Instance.TempParameters[1] = (exe.Parameter / 10.0).ToString("N1");
              break;
            case 0xb2:
              ChannelsViewModel.Instance.TempParameters[2] = (exe.Parameter / 10.0).ToString("N1");
              break;
            case 0xb3:
              ChannelsViewModel.Instance.TempParameters[3] = (exe.Parameter / 10.0).ToString("N1");
              break;
            case 0xb4:
              ChannelsViewModel.Instance.TempParameters[4] = (exe.Parameter / 10.0).ToString("N1");
              break;
            case 0xb5:
              ChannelsViewModel.Instance.TempParameters[5] = (exe.Parameter / 10.0).ToString("N1");
              break;
            case 0xb6:
              ChannelsViewModel.Instance.TempParameters[6] = (exe.Parameter / 10.0).ToString("N1");
              break;
            case 0xb8:
              ChannelsViewModel.Instance.BackgroundParameters[0] = exe.Parameter.ToString();
              break;
            case 0xb9:
              ChannelsViewModel.Instance.BackgroundParameters[1] = exe.Parameter.ToString();
              break;
            case 0xba:
              ChannelsViewModel.Instance.BackgroundParameters[2] = exe.Parameter.ToString();
              break;
            case 0xbb:
              ChannelsViewModel.Instance.BackgroundParameters[3] = exe.Parameter.ToString();
              break;
            case 0xbc:
              ChannelsViewModel.Instance.BackgroundParameters[4] = exe.Parameter.ToString();
              break;
            case 0xbd:
              ChannelsViewModel.Instance.BackgroundParameters[5] = exe.Parameter.ToString();
              break;
            case 0xbe:
              ChannelsViewModel.Instance.BackgroundParameters[6] = exe.Parameter.ToString();
              break;
            case 0xc0:
              ComponentsViewModel.Instance.LasersActive[0] = (exe.Parameter & 0x01) == 1;
              ComponentsViewModel.Instance.LasersActive[1] = (exe.Parameter & 0x02) == 2;
              ComponentsViewModel.Instance.LasersActive[2] = (exe.Parameter & 0x04) == 4;
              break;
            case 0xc4:
              DashboardViewModel.Instance.Volumes[2] = exe.FParameter.ToString();
              break;
            case 0xc7:
              g = (float)(exe.Parameter / 4096.0 / 0.040 * 3.3);
              ComponentsViewModel.Instance.LaserVioletPowerValue[0] = g.ToString("N1") + " mw";
              break;
            case 0xc8:
              g = (float)(exe.Parameter / 4096.0 / 0.04 * 3.3);
              ComponentsViewModel.Instance.LaserGreenPowerValue[0] = g.ToString("N1") + " mw";
              break;
            case 0xc9:
              g = (float)(exe.Parameter / 4096.0 / 0.04 * 3.3);
              ComponentsViewModel.Instance.LaserRedPowerValue[0] = g.ToString("N1") + " mw";
              break;
            case 0xca:  //TODO: remove?
              CalibrationViewModel.Instance.GatingItems[exe.Parameter].ForAppUpdater();
              break;
            case 0xcc:  //sync pending
              var list = DashboardViewModel.Instance.ActiveList;
              for (var i = 0; i < 16; i++)
              {
                if ((exe.Parameter & (1 << i)) != 0)
                {
                  if (!list.Contains(Device.SyncElements[i]))
                    list.Add(Device.SyncElements[i]);
                }
                else if (list.Contains(Device.SyncElements[i]))
                  list.Remove(Device.SyncElements[i]);
              }
              break;
            case 0xcd:
              CalibrationViewModel.Instance.EventTriggerContents[0] = exe.Parameter.ToString();
              break;
            case 0xce:
              CalibrationViewModel.Instance.EventTriggerContents[1] = exe.Parameter.ToString();
              break;
            case 0xcf:
              CalibrationViewModel.Instance.EventTriggerContents[2] = exe.Parameter.ToString();
              break;
            case 0xf1:
              if (exe.Command == 1)  //sheath empty
              {
                Device.MainCommand("Set Property", code: 0xcb, parameter: 0x1000);
                Device.MainCommand("Sheath"); //halt 
                Device.MainCommand("Set Property", code: 0xc1, parameter: 1);  //switch to recovery command buffer #1
                if (MessageBox.Show("Sheath Empty\nRefill and press OK", "Operator Alert", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                  Device.MainCommand("Sheath Empty Prime");
                  Device.MainCommand("Set Property", code: 0xcb); //clear sync token to allow recovery to run
                }
              }
              else if (exe.Command == 2) //pressure overload
              {
                if (MessageBox.Show("Pressure Overload\nCheck for waste line obstructions\nPower Off System",
                  "Operator Alert", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                  //                                Environment.Exit(0);
                }
              }
              break;
            case 0xf2:
              string ws;
              if (exe.Command > 0x63)
              {
                if (exe.Parameter == 0x501)  //sample syringe A
                {
                  ws = "Sample syringe A Error " + exe.Command.ToString();
                }
                else ws = "Sample syringe B Error " + exe.Command.ToString();

                if (MessageBox.Show(ws + "\nPower Off System", "Operator Alert", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                  Device.WellsToRead = Device.CurrentWellIdx;
                  Device.SaveBeadFile();
                  //                                Environment.Exit(0);
                }
              }
              break;
            //  case 0xf8:
            //    string tabnam = tabControl1.SelectedTab.Name;
            //    Device.InitSTab(tabnam);
            //    break;
            case 0xfd:
              Device.EndState = 1; //start the end of well state machine
              if (Device.WellsToRead > 0)
              {
                Device.ClearGraphingArrays();
              }
              break;
            case 0xfe:
              Device.EndState = 1;
              if (Device.WellsToRead > 0)
              {
                Device.ClearGraphingArrays();
              }
              break;
          }
        }
      }

      if (Device.NewStats)
      {
        Device.NewStats = false;
        for (var i = 0; i < 10; i++)
        {
          DataAnalysisViewModel.Instance.MfiItems[i] = Device.GStats[i].mfi.ToString();
          DataAnalysisViewModel.Instance.CvItems[i] = Device.GStats[i].cv.ToString();
        }
      }
    }

    private static void WellStateHandler()
    {
      // end of well state machine
      switch (Device.EndState)
      {
        case 1:
          {
            Device.SavBeadCount = Device.BeadCount;   //save for stats
            Device.SavingWellIdx = Device.CurrentWellIdx; //save the index of the currrent well for background file save
            Device.MainCommand("End Sampling");    //sends message to instrument to stop sampling
            Device.EndState++;
            Console.WriteLine(string.Format("{0} Reporting End Sampling", DateTime.Now.ToString()));
            break;
          }
        case 2:
          {
            _ = Task.Run(Device.SaveBeadFile);
            Device.EndState++;
            Console.WriteLine(string.Format("{0} Reporting Background File Save Init", DateTime.Now.ToString()));
            break;
          }
        case 3:
          {
            if (!DashboardViewModel.Instance.ActiveList.Contains("WASHING"))
            {
              Device.EndState++;  //wait here until alternate syringe is finished washing
              Console.WriteLine(string.Format("{0} Reporting Washing Complete", DateTime.Now.ToString()));
            }
            else
            {
              Device.MainCommand("Get Property", code: 0xcc);
              System.Threading.Thread.Sleep(10);  //this is kind of a loop. This is a polling delay
            }
            break;
          }
        case 4:
          {
            Device.WellNext();   //saves current well address for filename in state 5
            Device.EndState++;
            Console.WriteLine(string.Format("{0} Reporting Setting up next well", DateTime.Now.ToString()));
            //highling the current well on the plate on the screen
            /*
            well384dgv.ClearSelection();
            well96dgv.ClearSelection();
            if (ExperimentViewModel.Instance.CurrentTableSize > 1)   
            {
              if (ExperimentViewModel.Instance.CurrentTableSize == 384)
              {
                well384dgv.Rows[m_MicroCy.readingRow].Cells[m_MicroCy.readingCol].Selected = true;
                well384dgv.Refresh();
              }
              else
              {
                well96dgv.Rows[m_MicroCy.readingRow].Cells[m_MicroCy.readingCol].Selected = true;
                well96dgv.Refresh();
              }
            }
            */
            break;
          }
        case 5:
          {
            Device.MainCommand("FlushCmdQueue");
            Device.MainCommand("Set Property", code: 0xc3); //clear empty syringe token
            Device.MainCommand("Set Property", code: 0xcb); //clear sync token to allow next sequence to execute
            Device.EndBeadRead();
            Device.EndState = 0;
            if (Device.CurrentWellIdx == (Device.WellsToRead + 1)) //if only one more to go
            {
              //  woNumbertb.Text = "";
              //  pltNumbertb.Text = "";
              Device.MainCommand("Set Property", code: 0x19);  //bubble detect off
            }
            else
            {
              Device.ClearGraphingArrays();
            }
            Console.WriteLine(string.Format("{0} Reporting End of current well", DateTime.Now.ToString()));
            break;
          }
      }
    }

    private static void WorkOrderHandler()
    {
      //see if work order is available
      if (DashboardViewModel.Instance.SelectedSystemControlIndex != 0)
      {
        if (Device.IsNewWorkOrder())
        {
          //  woNumbertb.Text = Device.WorkOrderName;
          _workOrderPending = true;
        }
      }
      if (_workOrderPending == true)
      {
        if (DashboardViewModel.Instance.SelectedSystemControlIndex == 1)  //no barcode required so allow start
        {
          DashboardViewModel.Instance.StartButtonEnabled = true;
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
          //  if (woNumbertb.Text == pltNumbertb.Text)
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

    private static void HistogramHandler()
    {

      while (Device.ClData.Count != 0)
      {
        lock (Device.ClData)
        {
          CLQueue cldp = Device.ClData.Dequeue();
          //  chart2.Series["CL1CL2"].Points.AddXY(cldp.xyclx, cldp.xycly);
        }
      }

      if (!_histogramUpdateGoing)
      {
        var ResVM = ResultsViewModel.Instance;
        _histogramUpdateGoing = true;
        _ = Current.Dispatcher.BeginInvoke((Action)(() => {
          for(var i = 0; i < ResVM.ForwardSsc.Count; i++)
          {
            ResVM.ForwardSsc[i].Value = Device.ForwardSscData[i];
            ResVM.VioletSsc[i].Value = Device.VioletSscData[i];
            ResVM.RedSsc[i].Value = Device.RedSscData[i];
            ResVM.GreenSsc[i].Value = Device.GreenSscData[i];
            ResVM.Reporter[i].Value = Device.Rp1Data[i];
          }
          _histogramUpdateGoing = false;
        }));
      }
    }

    private static void UpdateEventCounter()
    {
      DashboardViewModel.Instance.EventCountField[0] = Device.BeadCount.ToString();
    }

    private static void UpdatePressureMonitor()
    {
      if (DashboardViewModel.Instance.PressureMonToggleButtonState)
        Device.MainCommand("Get FProperty", code: 0x22);
    }

  }
}