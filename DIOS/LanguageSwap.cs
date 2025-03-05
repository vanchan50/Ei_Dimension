using System.Resources;
using System.Globalization;
using Ei_Dimension.ViewModels;

namespace Ei_Dimension;

public static class LanguageSwap
{
  private static readonly ResourceManager _rm = Language.Resources.ResourceManager;
  private static CultureInfo _curCulture;
  private static CultureInfo _exCulture;
  /// <summary>
  /// 
  /// </summary>
  /// <param name="locale"></param>
  /// <exception cref="CultureNotFoundException"></exception>
  public static void SetLanguage(string locale = "en-US")
  {
    _exCulture = Language.TranslationSource.Instance.CurrentCulture;
    _curCulture = Language.TranslationSource.Instance.CurrentCulture = new CultureInfo(locale);

    #region Translation hack
      TranslateComponentsVM();
      TranslateCalibrationVM();
      TranslateDashboardVM();
      TranslateChannelsVM();
      TranslateVerificationVM();
      TranslateTemplateSelectVM();
      TranslateExperimentVM();
      TranslateResultsVM();
      TranslateStatisticsTableVM();
      #endregion
  }

  public static void TranslateComponentsVM()
  {
    var ComponentsVM = ComponentsViewModel.Instance;
    if (ComponentsVM != null)
    {
      if (ComponentsVM.IInputSelectorState == 0)
      {
        ComponentsVM.InputSelectorState[0] = _rm.GetString(nameof(Language.Resources.Components_To_Cuvet),
          _curCulture);
        ComponentsVM.InputSelectorState[1] = _rm.GetString(nameof(Language.Resources.Components_To_Pickup),
          _curCulture);
      }
      else
      {
        ComponentsVM.InputSelectorState[0] = _rm.GetString(nameof(Language.Resources.Components_To_Pickup),
          _curCulture);
        ComponentsVM.InputSelectorState[1] = _rm.GetString(nameof(Language.Resources.Components_To_Cuvet),
          _curCulture);
      }


      if (Settings.Default.PressureUnitsPSI)
      {
        ComponentsVM.PressureUnit[0] = _rm.GetString(nameof(Language.Resources.Pressure_Units_PSI),
          _curCulture);
      }
      else
      {
        ComponentsVM.PressureUnit[0] = _rm.GetString(nameof(Language.Resources.Pressure_Units_kPa),
          _curCulture);
      }


      ComponentsVM.ChConfigItems[0].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Standard), _curCulture);
      ComponentsVM.ChConfigItems[1].Content = _rm.GetString(nameof(Language.Resources.Dropdown_StandardSpectraplex), _curCulture);
      ComponentsVM.ChConfigItems[2].Content = _rm.GetString(nameof(Language.Resources.Dropdown_FM3D), _curCulture);
      ComponentsVM.ChConfigItems[3].Content = _rm.GetString(nameof(Language.Resources.Dropdown_StandardPlusFSC), _curCulture);
      //intentionally skip index 4
      ComponentsVM.SelectedChConfigContent = ComponentsVM.ChConfigItems[(int)ComponentsVM.SelectedChConfigIndex].Content;

      ComponentsVM.SyringeControlItems[0].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Halt), _curCulture);
      ComponentsVM.SyringeControlItems[1].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Move_Absolute), _curCulture);
      ComponentsVM.SyringeControlItems[2].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Pickup), _curCulture);
      ComponentsVM.SyringeControlItems[3].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Pre_inject), _curCulture);
      ComponentsVM.SyringeControlItems[4].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Speed), _curCulture);
      ComponentsVM.SyringeControlItems[5].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Initialize), _curCulture);
      ComponentsVM.SyringeControlItems[6].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Boot), _curCulture);
      ComponentsVM.SyringeControlItems[7].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Valve_Left), _curCulture);
      ComponentsVM.SyringeControlItems[8].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Valve_Right), _curCulture);
      ComponentsVM.SyringeControlItems[9].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Micro_step), _curCulture);
      ComponentsVM.SyringeControlItems[10].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Speed_Preset), _curCulture);
      ComponentsVM.SyringeControlItems[11].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Pos), _curCulture);
      ComponentsVM.SelectedSheathContent = ComponentsVM.SyringeControlItems[ComponentsVM.SyringeControlStates[0]].Content;
      ComponentsVM.SelectedSampleAContent = ComponentsVM.SyringeControlItems[ComponentsVM.SyringeControlStates[1]].Content;
      ComponentsVM.SelectedSampleBContent = ComponentsVM.SyringeControlItems[ComponentsVM.SyringeControlStates[2]].Content;

      ComponentsVM.GetPositionToggleButtonState = ComponentsVM.GetPositionToggleButtonState ==
        _rm.GetString(nameof(Language.Resources.OFF), _exCulture) ? _rm.GetString(nameof(Language.Resources.OFF), _curCulture)
        : _rm.GetString(nameof(Language.Resources.ON), _curCulture);
    }
  }

  public static void TranslateCalibrationVM()
  {
    var CaliVM = CalibrationViewModel.Instance;
    if (CaliVM != null)
    {
      CaliVM.GatingItems[0].Content = _rm.GetString(nameof(Language.Resources.Dropdown_None), _curCulture);
      CaliVM.GatingItems[1].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Green_SSC), _curCulture);
      CaliVM.GatingItems[2].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Red_SSC), _curCulture);
      CaliVM.GatingItems[3].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Green_Red_SSC), _curCulture);
      CaliVM.GatingItems[4].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Rp_bg), _curCulture);
      CaliVM.GatingItems[5].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Green_Rp_bg), _curCulture);
      CaliVM.GatingItems[6].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Red_Rp_bg), _curCulture);
      CaliVM.GatingItems[7].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Green_Red_Rp_bg), _curCulture);
      CaliVM.SelectedGatingContent = CaliVM.GatingItems[CaliVM.SelectedGatingIndex].Content;

      CaliVM.ClClassificatorItems[0].Content = _rm.GetString(nameof(Language.Resources.ChannelOffsets_Red_A), _curCulture);
      CaliVM.ClClassificatorItems[1].Content = _rm.GetString(nameof(Language.Resources.ChannelOffsets_Red_B), _curCulture);
      CaliVM.ClClassificatorItems[2].Content = _rm.GetString(nameof(Language.Resources.ChannelOffsets_Red_C), _curCulture);
      CaliVM.ClClassificatorItems[3].Content = _rm.GetString(nameof(Language.Resources.ChannelOffsets_Red_D), _curCulture);
      CaliVM.ClClassificatorItems[4].Content = _rm.GetString(nameof(Language.Resources.ChannelOffsets_Green_A), _curCulture);
      CaliVM.ClClassificatorItems[5].Content = _rm.GetString(nameof(Language.Resources.ChannelOffsets_Green_B), _curCulture);
      CaliVM.ClClassificatorItems[6].Content = _rm.GetString(nameof(Language.Resources.ChannelOffsets_Green_C), _curCulture);
      CaliVM.ClClassificatorItems[7].Content = _rm.GetString(nameof(Language.Resources.Channels_Green_D), _curCulture);
      CaliVM.SelectedCl1ClassificatorContent = CaliVM.ClClassificatorItems[CaliVM.SelectedCl1ClassificatorIndex].Content;
      CaliVM.SelectedCl2ClassificatorContent = CaliVM.ClClassificatorItems[CaliVM.SelectedCl2ClassificatorIndex].Content;
      CaliVM.SelectedSensitivityContent = CaliVM.ClClassificatorItems[CaliVM.SelectedSensitivityIndex].Content;
      CaliVM.SelectedExChannelContent = CaliVM.ClClassificatorItems[CaliVM.SelectedExChannelIndex].Content;

      CaliVM.ReporterItems[0].Content = _rm.GetString(nameof(Language.Resources.ChannelOffsets_Red_A), _curCulture);
      CaliVM.ReporterItems[1].Content = _rm.GetString(nameof(Language.Resources.ChannelOffsets_Red_B), _curCulture);
      CaliVM.ReporterItems[2].Content = _rm.GetString(nameof(Language.Resources.ChannelOffsets_Red_C), _curCulture);
      CaliVM.ReporterItems[3].Content = _rm.GetString(nameof(Language.Resources.ChannelOffsets_Red_D), _curCulture);
      CaliVM.ReporterItems[4].Content = _rm.GetString(nameof(Language.Resources.ChannelOffsets_Green_A), _curCulture);
      CaliVM.ReporterItems[5].Content = _rm.GetString(nameof(Language.Resources.ChannelOffsets_Green_B), _curCulture);
      CaliVM.ReporterItems[6].Content = _rm.GetString(nameof(Language.Resources.ChannelOffsets_Green_C), _curCulture);
      CaliVM.ReporterItems[7].Content = _rm.GetString(nameof(Language.Resources.Channels_Green_D), _curCulture);
      CaliVM.ReporterItems[8].Content = _rm.GetString(nameof(Language.Resources.Channels_None), _curCulture);
      CaliVM.SelectedReporterContent[0] = CaliVM.ReporterItems[CaliVM.SelectedReporterIndex[0]].Content;
      CaliVM.SelectedReporterContent[1] = CaliVM.ReporterItems[CaliVM.SelectedReporterIndex[1]].Content;
      CaliVM.SelectedReporterContent[2] = CaliVM.ReporterItems[CaliVM.SelectedReporterIndex[2]].Content;
      CaliVM.SelectedReporterContent[3] = CaliVM.ReporterItems[CaliVM.SelectedReporterIndex[3]].Content;
      CaliVM.SelectedCl3rContent = CaliVM.ReporterItems[CaliVM.SelectedCl3rIndex].Content;
    }
  }

  public static void TranslateDashboardVM()
  {
    var DashVM = DashboardViewModel.Instance;
    if (DashVM != null)
    {
      DashVM.SpeedItems[0].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Normal), _curCulture);
      DashVM.SpeedItems[1].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Hi_Speed), _curCulture);
      DashVM.SpeedItems[2].Content = _rm.GetString(nameof(Language.Resources.Dropdown_Hi_Sens), _curCulture);
      DashVM.SelectedSpeedContent = DashVM.SpeedItems[(int)DashVM.SelectedSpeedIndex].Content;

      DashVM.OrderItems[0].Content = _rm.GetString(nameof(Language.Resources.Column), _curCulture);
      DashVM.OrderItems[1].Content = _rm.GetString(nameof(Language.Resources.Row), _curCulture);
      DashVM.SelectedOrderContent = DashVM.OrderItems[(int)DashVM.SelectedOrderIndex].Content;

      DashVM.SysControlItems[0].Content = _rm.GetString(nameof(Language.Resources.Experiment_Manual), _curCulture);
      DashVM.SysControlItems[1].Content = _rm.GetString(nameof(Language.Resources.Experiment_Work_Order), _curCulture);
      //DashVM.SysControlItems[2].Content = RM.GetString(nameof(Language.Resources.Experiment_Work_Order_Plus_Bcode), curCulture);
      DashVM.SelectedSysControlContent = DashVM.SysControlItems[DashVM.SelectedSystemControlIndex].Content;

      DashVM.EndReadItems[0].Content = _rm.GetString(nameof(Language.Resources.Experiment_Min_Per_Reg), _curCulture);
      DashVM.EndReadItems[1].Content = _rm.GetString(nameof(Language.Resources.Experiment_Total_Events), _curCulture);
      DashVM.EndReadItems[2].Content = _rm.GetString(nameof(Language.Resources.Experiment_End_of_Sample), _curCulture);
      DashVM.EndReadItems[3].Content = _rm.GetString(nameof(Language.Resources.Experiment_Timer), _curCulture);
      DashVM.SelectedEndReadContent = DashVM.EndReadItems[DashVM.SelectedEndReadIndex].Content;
    }
  }

  public static void TranslateChannelsVM()
  {
    var ChannelsVM = ChannelsViewModel.Instance;
    if (ChannelsVM != null)
    {
      if (App.ChannelRedirectionEnabled)
      {
        ChannelsVM.Labels[0] = _rm.GetString(nameof(Language.Resources.Channels_Green_A), _curCulture);
        ChannelsVM.Labels[1] = _rm.GetString(nameof(Language.Resources.Channels_OEM_Green_B), _curCulture);
        ChannelsVM.Labels[2] = _rm.GetString(nameof(Language.Resources.Channels_OEM_Green_C), _curCulture);
        ChannelsVM.Labels[3] = _rm.GetString(nameof(Language.Resources.Channels_OEM_RedA), _curCulture);
        ChannelsVM.Labels[4] = _rm.GetString(nameof(Language.Resources.Channels_Red_B), _curCulture);
        ChannelsVM.Labels[5] = _rm.GetString(nameof(Language.Resources.Channels_OEM_RedC), _curCulture);
        ChannelsVM.Labels[6] = _rm.GetString(nameof(Language.Resources.Channels_OEM_RedD), _curCulture);
        ChannelsVM.Labels[7] = _rm.GetString(nameof(Language.Resources.Channels_Green_D), _curCulture);
      }
      else
      {
        ChannelsVM.Labels[0] = _rm.GetString(nameof(Language.Resources.Channels_Green_A), _curCulture);
        ChannelsVM.Labels[1] = _rm.GetString(nameof(Language.Resources.Channels_Green_B), _curCulture);
        ChannelsVM.Labels[2] = _rm.GetString(nameof(Language.Resources.Channels_Green_C), _curCulture);
        ChannelsVM.Labels[3] = _rm.GetString(nameof(Language.Resources.Channels_Red_A), _curCulture);
        ChannelsVM.Labels[4] = _rm.GetString(nameof(Language.Resources.Channels_Red_B), _curCulture);
        ChannelsVM.Labels[5] = _rm.GetString(nameof(Language.Resources.Channels_Red_C), _curCulture);
        ChannelsVM.Labels[6] = _rm.GetString(nameof(Language.Resources.Channels_Red_D), _curCulture);
        ChannelsVM.Labels[7] = _rm.GetString(nameof(Language.Resources.Channels_Green_D), _curCulture);
      }
    }
  }

  public static void TranslateVerificationVM()
  {
    var VerVM = VerificationViewModel.Instance;
    if (VerVM != null)
    {
      VerVM.VerificationWarningItems[0].Content = _rm.GetString(nameof(Language.Resources.Daily), _curCulture);
      VerVM.VerificationWarningItems[1].Content = _rm.GetString(nameof(Language.Resources.Weekly), _curCulture);
      VerVM.VerificationWarningItems[2].Content = _rm.GetString(nameof(Language.Resources.Monthly), _curCulture);
      VerVM.VerificationWarningItems[3].Content = _rm.GetString(nameof(Language.Resources.Quarterly), _curCulture);
      VerVM.VerificationWarningItems[4].Content = _rm.GetString(nameof(Language.Resources.Yearly), _curCulture);
      VerVM.SelectedVerificationWarningContent = VerVM.VerificationWarningItems[Settings.Default.VerificationWarningIndex].Content;
    }
  }

  public static void TranslateTemplateSelectVM()
  {
    var TemplVM = TemplateSelectViewModel.Instance;
    if (TemplVM != null)
    {
      if (TemplVM.TemplateSaveName[0] == _rm.GetString(nameof(Language.Resources.DefaultTemplateName), _exCulture))
        TemplVM.TemplateSaveName[0] = _rm.GetString(nameof(Language.Resources.DefaultTemplateName), _curCulture);
    }
  }

  public static void TranslateExperimentVM()
  {
    var ExpVM = ExperimentViewModel.Instance;
    if (ExpVM != null)
    {
      if (ExpVM.CurrentTemplateName == _rm.GetString(nameof(Language.Resources.TemplateName_None), _exCulture))
        ExpVM.CurrentTemplateName = _rm.GetString(nameof(Language.Resources.TemplateName_None), _curCulture);
    }
  }

  public static void TranslateResultsVM()
  {
    var ResVM = ResultsViewModel.Instance;
    if (ResVM != null)
    {
      if (ResVM.PlexButtonString == _rm.GetString(nameof(Language.Resources.Experiment_Active_Regions), _exCulture))
        ResVM.PlexButtonString = _rm.GetString(nameof(Language.Resources.Experiment_Active_Regions), _curCulture);
      else if (ResVM.PlexButtonString == _rm.GetString(nameof(Language.Resources.Experiment_Stats), _exCulture))
        ResVM.PlexButtonString = _rm.GetString(nameof(Language.Resources.Experiment_Stats), _curCulture);
    }
  }

  public static void TranslateStatisticsTableVM()
  {
    var StVM = StatisticsTableViewModel.Instance;
    if (StVM != null)
    {
      if (App.ChannelRedirectionEnabled)
      {
        StVM.StatisticsLabels[0] = _rm.GetString(nameof(Language.Resources.Channels_Green_A), _curCulture);
        StVM.StatisticsLabels[1] = _rm.GetString(nameof(Language.Resources.Statistics_OEM_Red_C), _curCulture);//
        StVM.StatisticsLabels[2] = _rm.GetString(nameof(Language.Resources.Statistics_OEM_Red_D), _curCulture);//
        StVM.StatisticsLabels[3] = _rm.GetString(nameof(Language.Resources.DataAn_Red_SSC), _curCulture);
        StVM.StatisticsLabels[4] = _rm.GetString(nameof(Language.Resources.Statistics_OEM_GreenB), _curCulture);//
        StVM.StatisticsLabels[5] = _rm.GetString(nameof(Language.Resources.Statistics_OEM_GreenC), _curCulture);//
        StVM.StatisticsLabels[6] = _rm.GetString(nameof(Language.Resources.CL3), _curCulture);
        StVM.StatisticsLabels[7] = _rm.GetString(nameof(Language.Resources.Channels_Green_D), _curCulture);
      }
      else
      {
        StVM.StatisticsLabels[0] = _rm.GetString(nameof(Language.Resources.Channels_Green_A), _curCulture);
        StVM.StatisticsLabels[1] = _rm.GetString(nameof(Language.Resources.Channels_Green_B), _curCulture);
        StVM.StatisticsLabels[2] = _rm.GetString(nameof(Language.Resources.Channels_Green_C), _curCulture);
        StVM.StatisticsLabels[3] = _rm.GetString(nameof(Language.Resources.DataAn_Red_SSC), _curCulture);
        StVM.StatisticsLabels[4] = _rm.GetString(nameof(Language.Resources.CL1), _curCulture);
        StVM.StatisticsLabels[5] = _rm.GetString(nameof(Language.Resources.CL2), _curCulture);
        StVM.StatisticsLabels[6] = _rm.GetString(nameof(Language.Resources.CL3), _curCulture);
        StVM.StatisticsLabels[7] = _rm.GetString(nameof(Language.Resources.Channels_Green_D), _curCulture);
      }
    }
  }
}