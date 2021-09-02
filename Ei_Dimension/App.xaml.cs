using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using Ei_Dimension.ViewModels;

namespace Ei_Dimension
{
  public partial class App : Application
  {
    public static (System.Reflection.PropertyInfo prop, object VM) NumpadShow { get; set; }
    public static (System.Reflection.PropertyInfo prop, object VM, int index) SelectedTextBox { get; set; }
    public static MicroCy.MicroCyDevice Device { get; private set; }
    public App()
    {
      SetLanguage("en-US");

      Device = new MicroCy.MicroCyDevice();
      //reading VM add SetControl(m_MicroCy.SystemControl);
      Device.SystemControl = Settings.Default.SystemControl;
      Device.Outdir = Settings.Default.DirPath; //TODO: probably not necessary
      Device.SubtRegBg = Settings.Default.SubtRegBg;
      // reading VM add .SelectedIndex = Properties.Settings.Default.defaultMap;
      // calibration VM add Properties.Settings.Default.Compensation.ToString();
      // reading VM add slist.DataSource = active_items;
      Device.SampleSize = Settings.Default.sampleSize;
      // RegCtr_SampSize.Text = Device.SampleSize.ToString();
      Device.Everyevent = Settings.Default.Everyevent;
      Device.RMeans = Settings.Default.RMeans;
      // rmeanscb.Checked = m_MicroCy.RMeans;
      Device.PltRept = Settings.Default.PltRept;
      // plateResultscb.Checked = m_MicroCy.PltRept;
      // m_MicroCy.InitSTab("readertab");


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
        ComponentsVM.InputSelectorState.Add(RM.GetString(nameof(Language.Resources.Components_To_Pickup),
          curCulture));
        ComponentsVM.InputSelectorState.Add(RM.GetString(nameof(Language.Resources.Components_To_Cuvet),
          curCulture));
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
            ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = temp.Remove(temp.Length - 1, 1);
          return;
        }
        ((ObservableCollection<string>)SelectedTextBox.prop.GetValue(SelectedTextBox.VM))[SelectedTextBox.index] = temp + input;
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
