using Ei_Dimension.ViewModels;
using DIOS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ei_Dimension
{
  public static class LanguageSwap
  {
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
          ComponentsVM.InputSelectorState[0] = RM.GetString(nameof(Language.Resources.Components_To_Cuvet),
            curCulture);
          ComponentsVM.InputSelectorState[1] = RM.GetString(nameof(Language.Resources.Components_To_Pickup),
            curCulture);
        }
        else
        {
          ComponentsVM.InputSelectorState[0] = RM.GetString(nameof(Language.Resources.Components_To_Pickup),
            curCulture);
          ComponentsVM.InputSelectorState[1] = RM.GetString(nameof(Language.Resources.Components_To_Cuvet),
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
        ComponentsVM.SelectedSheathContent = ComponentsVM.SyringeControlItems[ComponentsVM.SyringeControlStates[0]].Content;
        ComponentsVM.SelectedSampleAContent = ComponentsVM.SyringeControlItems[ComponentsVM.SyringeControlStates[1]].Content;
        ComponentsVM.SelectedSampleBContent = ComponentsVM.SyringeControlItems[ComponentsVM.SyringeControlStates[2]].Content;

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
        CaliVM.SelectedGatingContent = CaliVM.GatingItems[CaliVM.SelectedGatingIndex].Content;
      }
      var DashVM = DashboardViewModel.Instance;
      if (DashVM != null)
      {
        DashVM.SpeedItems[0].Content = RM.GetString(nameof(Language.Resources.Dropdown_Normal), curCulture);
        DashVM.SpeedItems[1].Content = RM.GetString(nameof(Language.Resources.Dropdown_Hi_Speed), curCulture);
        DashVM.SpeedItems[2].Content = RM.GetString(nameof(Language.Resources.Dropdown_Hi_Sens), curCulture);
        DashVM.SelectedSpeedContent = DashVM.SpeedItems[DashVM.SelectedSpeedIndex].Content;

        DashVM.ChConfigItems[0].Content = RM.GetString(nameof(Language.Resources.Dropdown_Standard), curCulture);
        DashVM.ChConfigItems[1].Content = RM.GetString(nameof(Language.Resources.Dropdown_Cells), curCulture);
        DashVM.ChConfigItems[2].Content = RM.GetString(nameof(Language.Resources.Dropdown_FM3D), curCulture);
        DashVM.SelectedChConfigContent = DashVM.ChConfigItems[DashVM.SelectedChConfigIndex].Content;

        DashVM.OrderItems[0].Content = RM.GetString(nameof(Language.Resources.Column), curCulture);
        DashVM.OrderItems[1].Content = RM.GetString(nameof(Language.Resources.Row), curCulture);
        DashVM.SelectedOrderContent = DashVM.OrderItems[DashVM.SelectedOrderIndex].Content;

        DashVM.SysControlItems[0].Content = RM.GetString(nameof(Language.Resources.Experiment_Manual), curCulture);
        DashVM.SysControlItems[1].Content = RM.GetString(nameof(Language.Resources.Experiment_Work_Order), curCulture);
        //DashVM.SysControlItems[2].Content = RM.GetString(nameof(Language.Resources.Experiment_Work_Order_Plus_Bcode), curCulture);
        DashVM.SelectedSysControlContent = DashVM.SysControlItems[DashVM.SelectedSystemControlIndex].Content;

        DashVM.EndReadItems[0].Content = RM.GetString(nameof(Language.Resources.Experiment_Min_Per_Reg), curCulture);
        DashVM.EndReadItems[1].Content = RM.GetString(nameof(Language.Resources.Experiment_Total_Events), curCulture);
        DashVM.EndReadItems[2].Content = RM.GetString(nameof(Language.Resources.Experiment_End_of_Sample), curCulture);
        DashVM.SelectedEndReadContent = DashVM.EndReadItems[DashVM.SelectedEndReadIndex].Content;
      }
      var ChannelsVM = ChannelOffsetViewModel.Instance;
      if (ChannelsVM != null)
      {
        ChannelsVM.SensitivityItems[0].Content = RM.GetString(nameof(Language.Resources.Channels_Sens_B), curCulture);
        ChannelsVM.SensitivityItems[1].Content = RM.GetString(nameof(Language.Resources.Channels_Sens_C), curCulture);
        ChannelsVM.SelectedSensitivityContent = ChannelsVM.SensitivityItems[(int)App.Device.SensitivityChannel].Content;
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
      var TemplVM = TemplateSelectViewModel.Instance;
      if (TemplVM != null)
      {
        if (TemplVM.TemplateSaveName[0] == RM.GetString(nameof(Language.Resources.DefaultTemplateName), exCulture))
          TemplVM.TemplateSaveName[0] = RM.GetString(nameof(Language.Resources.DefaultTemplateName), curCulture);
      }
      var ExpVM = ExperimentViewModel.Instance;
      if (ExpVM != null)
      {
        if (ExpVM.CurrentTemplateName == RM.GetString(nameof(Language.Resources.TemplateName_None), exCulture))
          ExpVM.CurrentTemplateName = RM.GetString(nameof(Language.Resources.TemplateName_None), curCulture);
      }
      var ResVM = ResultsViewModel.Instance;
      if (ResVM != null)
      {
        if (ResVM.PlexButtonString == RM.GetString(nameof(Language.Resources.Experiment_Active_Regions), exCulture))
        ResVM.PlexButtonString = RM.GetString(nameof(Language.Resources.Experiment_Active_Regions), curCulture);
        else if (ResVM.PlexButtonString == RM.GetString(nameof(Language.Resources.Experiment_Stats), exCulture))
          ResVM.PlexButtonString = RM.GetString(nameof(Language.Resources.Experiment_Stats), curCulture);
      }
      #endregion
    }
  }
}
