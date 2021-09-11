using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using Ei_Dimension.ViewModels;
using MicroCy;

namespace Ei_Dimension
{
  public partial class App : Application
  {
    public static (PropertyInfo prop, object VM) NumpadShow { get; set; }
    public static (PropertyInfo prop, object VM, int index) SelectedTextBox { get; set; }
    public static MicroCyDevice Device { get; private set; }
    public App()
    {
      SetLanguage("en-US");

      Device = new MicroCyDevice();
      try
      {
        Device.ActiveMap = Device.MapList[Settings.Default.DefaultMap];
      }
      catch { }
      Device.SystemControl = Settings.Default.SystemControl;
      Device.Compensation = Settings.Default.Compensation;
      // reading VM add slist.DataSource = active_items;    //System monitor control
      Device.SampleSize = Settings.Default.SampleSize;
      // RegCtr_SampSize.Text = Device.SampleSize.ToString(); //bead maps
      Device.Everyevent = Settings.Default.Everyevent;
      //  everyEventcb.Checked = m_MicroCy.everyevent; //Data out
      Device.RMeans = Settings.Default.RMeans;
      // rmeanscb.Checked = m_MicroCy.RMeans; //Data out
      Device.PltRept = Settings.Default.PltRept;
      // plateResultscb.Checked = m_MicroCy.PltRept;  //Data out
      Device.TerminationType = Settings.Default.EndRead;
      Device.SubtRegBg = Settings.Default.SubtRegBg;
      Device.MinPerRegion = Settings.Default.MinPerRegion;
      Device.BeadsToCapture = Settings.Default.BeadsToCapture;
      if (!System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
        Device.MainCommand("Startup");
      Device.SscSelected = Settings.Default.SscHistSel; //TODO: delete this, when refactor ReplyFromMC. only needed in legacy to build graphs
      Device.XAxisSel = Settings.Default.XAxisG;        //TODO: delete this, when refactor ReplyFromMC. only needed in legacy to build graphs
      Device.YAxisSel = Settings.Default.YAxisG;        //TODO: delete this, when refactor ReplyFromMC. only needed in legacy to build graphs
      Device.HdnrTrans = Settings.Default.HDnrTrans;
      Device.MainCommand("Set Property", code: 0x97, parameter: 1170);  //set current limit of aligner motors if leds are off
      Device.MainCommand("Get Property", code: 0xca);
    }

    public static int GetActiveMapIndex()
    {
      int i = 0;
      for (; i < Device.MapList.Count; i++)
      {
        if (Device.MapList[i].mapName == Device.ActiveMap.mapName)
          break;
      }
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
      var ExpVM = ExperimentViewModel.Instance;
      if (ExpVM != null)
      {
        ExpVM.SpeedItems[0].Content = RM.GetString(nameof(Language.Resources.Dropdown_Normal), curCulture);
        ExpVM.SpeedItems[1].Content = RM.GetString(nameof(Language.Resources.Dropdown_Hi_Speed), curCulture);
        ExpVM.SpeedItems[2].Content = RM.GetString(nameof(Language.Resources.Dropdown_Hi_Sens), curCulture);

        ExpVM.ChConfigItems[0].Content = RM.GetString(nameof(Language.Resources.Dropdown_Standard), curCulture);
        ExpVM.ChConfigItems[1].Content = RM.GetString(nameof(Language.Resources.Dropdown_Cells), curCulture);
        ExpVM.ChConfigItems[2].Content = RM.GetString(nameof(Language.Resources.Dropdown_FM3D), curCulture);
      }
      #endregion
    }

    public static void InjectToFocusedTextbox(string input)
    {
      if (SelectedTextBox.prop != null)
      {
        var temp = ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index];
        if (input == "")
        {
          if(temp.Length > 0)
            ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = temp =
              temp.Remove(temp.Length - 1, 1);
        }
        else
          ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = temp += input;
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
                Device.TemprpMaj = iRes;
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
              if (int.TryParse(temp, out iRes))
              {

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
              if (int.TryParse(temp, out iRes))
              {

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
              if (int.TryParse(temp, out iRes))
              {

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
  }
}