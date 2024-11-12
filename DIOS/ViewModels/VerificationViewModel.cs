using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using DIOS.Application;
using DIOS.Application.FileIO;
using DIOS.Application.FileIO.Verification;
using System.Windows;
using Ei_Dimension.Graphing.HeatMap;
using Ei_Dimension.Core;

namespace Ei_Dimension.ViewModels;

[POCOViewModel]
public class VerificationViewModel
{
  public virtual ObservableCollection<DropDownButtonContents> VerificationWarningItems { get; set; }
  public virtual string SelectedVerificationWarningContent { get; set; }
  public virtual ObservableCollection<string> MinCount { get; set; } = new(){ "" };
  public static VerificationViewModel Instance { get; private set; }
  public virtual Visibility DetailsVisibility { get; set; } = Visibility.Hidden;
  protected VerificationViewModel()
  {
    var RM = Language.Resources.ResourceManager;
    var curCulture = Language.TranslationSource.Instance.CurrentCulture;
    VerificationWarningItems = new ObservableCollection<DropDownButtonContents>
    {
      new(RM.GetString(nameof(Language.Resources.Daily), curCulture), this),
      new(RM.GetString(nameof(Language.Resources.Weekly), curCulture), this),
      new(RM.GetString(nameof(Language.Resources.Monthly), curCulture), this),
      new(RM.GetString(nameof(Language.Resources.Quarterly), curCulture), this),
      new(RM.GetString(nameof(Language.Resources.Yearly), curCulture), this)
    };
    SelectedVerificationWarningContent = VerificationWarningItems[Settings.Default.VerificationWarningIndex].Content;
    Instance = this;
  }

  public static VerificationViewModel Create()
  {
    return ViewModelSource.Create(() => new VerificationViewModel());
  }

  public void LoadClick(bool fromCode = false)
  {
    DetailsVisibility = Visibility.Hidden;
    var map = App.DiosApp.MapController.LoadMapByName(App.DiosApp.MapController.ActiveMap.mapName);

    for (var i = 0; i < map.regions.Count; i++)
    {
      var isActive = map.regions[i].isValidator;
        App.MapRegions.ActivateVerificationTextBox(map.regions[i].Number, isActive);
    }

    if (!fromCode)
    {
      var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Ver_Regions_Loaded),
        Language.TranslationSource.Instance.CurrentCulture);
      Notification.Show(msg);
    }
  }

  public void SaveClick()
  {
    UserInputHandler.InputSanityCheck();

    var res = App.DiosApp.MapController.SaveCalValsToCurrentMap(new MapCalParameters
    {
      TempGreenD = -1,
      TempCl1 = -1,
      TempCl2 = -1,
      TempCl3 = -1,
      TempRedSsc = -1,
      TempGreenSsc = -1,
      TempRpMaj = -1,
      TempRpMin = -1,
      Compensation = -1,
      Gating = -1,
      Height = -1,
      MinSSC = -1,
      MaxSSC = -1,
      DNRCoef = -1,
      DNRTrans = -1,
      Attenuation = -1,
      CL0 = -1,
      CL1 = -1,
      CL2 = -1,
      CL3 = -1,
      RP1 = -1,
      Caldate = null,
      Valdate = null
    });
    if (!res)
    {
      App.Current.Dispatcher.Invoke(() =>
        Notification.Show("Save failed"));
      return;
    }
    var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Ver_Regions_Saved),
      Language.TranslationSource.Instance.CurrentCulture);
    Notification.Show(msg);
  }

  public static void VerificationSuccess()
  {
    App.DiosApp.MapController.SaveCalValsToCurrentMap(new MapCalParameters
    {
      TempGreenD = -1,
      TempCl1 = -1,
      TempCl2 = -1,
      TempCl3 = -1,
      TempRedSsc = -1,
      TempGreenSsc = -1,
      TempRpMaj = -1,
      TempRpMin = -1,
      Compensation = -1,
      Gating = -1,
      Height = -1,
      MinSSC = -1,
      MaxSSC = -1,
      DNRCoef = -1,
      DNRTrans = -1,
      Attenuation = -1,
      CL0 = -1,
      CL1 = -1,
      CL2 = -1,
      CL3 = -1,
      RP1 = -1,
      Caldate = null,
      Valdate = DateTime.Now.ToString("dd.MM.yyyy", new System.Globalization.CultureInfo("en-GB"))
    });
    DashboardViewModel.Instance.SetValidationDate(App.DiosApp.MapController.ActiveMap.valtime);
    Notification.ShowLocalizedSuccess(nameof(Language.Resources.Validation_Success));
  }

  /// <summary>
  /// Checks that verification regions are properly setup
  /// </summary>
  /// <returns></returns>
  public bool ValMapInfoReady()
  {
    if (App.DiosApp.MapController.ActiveMap.regions.Count(x => x.isValidator) == 0)
    {
      var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_VerRegions_NotSelected),
        Language.TranslationSource.Instance.CurrentCulture);
      Notification.Show(msg);
      return false;
    }

    foreach (var region in App.DiosApp.MapController.ActiveMap.regions)
    {
      if (region.isValidator && region.VerificationTargetReporter < 0)
      {
        var msg1 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Ver_Region),
          Language.TranslationSource.Instance.CurrentCulture);
        var msg2 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Reporter_Not_Specified),
          Language.TranslationSource.Instance.CurrentCulture);
        Notification.Show($"{msg1} {region.Number} {msg2}");
        //any failure - instant error output
        return false;
      }
    }
    //all is actually good
    return true;
  }

  public VerificationReport FormNewVerificationReport(Verificator verificator)
  {
    var firmwareVersion = App.DiosApp.Device.FirmwareVersion;
    var appVersion = App.DiosApp.BUILD;
    var dnrCoefficient = float.Parse(CalibrationViewModel.Instance.DNRContents[0]);
    var dnrTransition = float.Parse(CalibrationViewModel.Instance.DNRContents[1]);
    var channelConfig = ComponentsViewModel.Instance.SelectedChConfigIndex.ToString();
    var eventHeight = float.Parse(CalibrationViewModel.Instance.EventTriggerContents[0]);
    var lowGate = float.Parse(CalibrationViewModel.Instance.EventTriggerContents[1]);
    var highGate = float.Parse(CalibrationViewModel.Instance.EventTriggerContents[2]);
    var minCount = int.Parse(MinCount[0]);
    var report = new VerificationReport(firmwareVersion, appVersion, dnrCoefficient, dnrTransition,
      channelConfig, eventHeight, lowGate, highGate, minCount);

    var list = App.DiosApp.MapController.ActiveMap.regions.Where(x => x.isValidator);
    foreach (var validatorRegion in list)
    {
      var verStats = verificator.GetRegionStats(validatorRegion.Number);
      verStats.CalculateResultingStats();

      var stats = verStats.Stats[0];
      var greenSSC = new VerificationReportChannelData(stats.Mean, stats.CoeffVar,
        validatorRegion.MaxCV.GreenSSC, validatorRegion.MeanTolerance.GreenSSC, 8500);

      stats = verStats.Stats[1];
      var redSSC = new VerificationReportChannelData(stats.Mean, stats.CoeffVar,
        validatorRegion.MaxCV.RedSSC, validatorRegion.MeanTolerance.RedSSC, 8500);

      var targetCl1 = MapRegion.FromCLSpaceToReal(validatorRegion.Center.x, HeatMapPoint.bins);
      stats = verStats.Stats[2];
      var cl1 = new VerificationReportChannelData(stats.Mean, stats.CoeffVar,
        validatorRegion.MaxCV.Cl1, validatorRegion.MeanTolerance.Cl1, targetCl1);

      var targetCl2 = MapRegion.FromCLSpaceToReal(validatorRegion.Center.y, HeatMapPoint.bins);
      stats = verStats.Stats[3];
      var cl2 = new VerificationReportChannelData(stats.Mean, stats.CoeffVar,
        validatorRegion.MaxCV.Cl2, validatorRegion.MeanTolerance.Cl2, targetCl2); 

      stats = verStats.Stats[4];
      var reporter = new VerificationReportChannelData(stats.Mean, stats.CoeffVar,
        validatorRegion.MaxCV.Reporter, validatorRegion.MeanTolerance.Reporter, validatorRegion.VerificationTargetReporter);
      var data = new VerificationReportRegionData(validatorRegion.Number.ToString(),
        greenSSC, redSSC, cl1, cl2, reporter, verStats.Count);
      report.regionsData.Add(data);
    }

    return report;
  }

  public void OnMapChanged(MapModel map)
  {
    MinCount[0] = map.minVerBeads.ToString();
  }

  public void DropPress()
  {
    UserInputHandler.InputSanityCheck();
  }

  public void TextChanged(TextChangedEventArgs e)
  {
    UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
  }

  public void FocusedBox(int num)
  {
    var minCountTb = Views.VerificationView.Instance.minCountTb;
    switch (num)
    {
      case 0:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(MinCount)), this, 0,
          minCountTb);
        MainViewModel.Instance.NumpadToggleButton(minCountTb);
        break;
    }
  }

  public class DropDownButtonContents : Core.ObservableObject
  {
    public string Content
    {
      get => _content;
      set
      {
        _content = value;
        OnPropertyChanged();
      }
    }
    public byte Index { get; set; }
    private static byte _nextIndex = 0;
    private string _content;
    private static VerificationViewModel _vm;
    public DropDownButtonContents(string content, VerificationViewModel vm = null)
    {
      if (_vm == null)
      {
        _vm = vm;
      }
      Content = content;
      Index = _nextIndex++;
    }

    public void Click()
    {
      _vm.SelectedVerificationWarningContent = Content;
      Settings.Default.VerificationWarningIndex = Index;
      Settings.Default.Save();
    }
  }
}