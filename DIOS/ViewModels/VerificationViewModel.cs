using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Core;
using System;
using System.Collections.ObjectModel;
using Ei_Dimension.Controllers;
using System.Windows.Controls;

namespace Ei_Dimension.ViewModels;

[POCOViewModel]
public class VerificationViewModel
{
  public virtual ObservableCollection<DropDownButtonContents> VerificationWarningItems { get; set; }
  public virtual ObservableCollection<string> ToleranceItems { get; set; } = new(){"", "", "", "", ""};
  public virtual ObservableCollection<string> MaxCVItems { get; set; } = new() { "", "", "", "", "" };
  public virtual string SelectedVerificationWarningContent { get; set; }
  public static VerificationViewModel Instance { get; private set; }
  public bool isActivePage { get; set; }
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
    isActivePage = false;
  }

  public static VerificationViewModel Create()
  {
    return ViewModelSource.Create(() => new VerificationViewModel());
  }

  public void AddValidationRegion(byte num)
  {
    //App.MapRegions.AddValidationRegion(num);
  }

  public void LoadClick(bool fromCode = false)
  {
    var map = App.DiosApp.MapController.GetMapByName(App.DiosApp.MapController.ActiveMap.mapName);
    for (var i = 0; i < map.regions.Count; i++)
    {
      //Reset all Verification Regions
      MapRegionsController.ActiveVerificationRegionNums.Add(map.regions[i].Number);
      App.MapRegions.AddValidationRegion(map.regions[i].Number);
      MapRegionsController.RegionsList[i + 1].TargetReporterValue[0] = "";

      if (map.regions[i].isValidator)
      {
        App.MapRegions.AddValidationRegion(map.regions[i].Number);
        if(map.regions[i].VerificationTargetReporter > -0.1)
          MapRegionsController.RegionsList[i + 1].TargetReporterValue[0] = map.regions[i].VerificationTargetReporter.ToString();
      }
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
    var map = App.DiosApp.MapController.GetMapByName(App.DiosApp.MapController.ActiveMap.mapName);
    for (var i = 1; i < MapRegionsController.RegionsList.Count; i++)
    {
      var index = map.regions.FindIndex(x => x.Number == MapRegionsController.RegionsList[i].Number);
      if(index != -1)
      {
        double temp = -1;
        if (MapRegionsController.ActiveVerificationRegionNums.Contains(MapRegionsController.RegionsList[i].Number))
        {
          map.regions[index].isValidator = true;
          if (MapRegionsController.RegionsList[i].TargetReporterValue[0] != "" && double.TryParse(MapRegionsController.RegionsList[i].TargetReporterValue[0], out temp))
          {
            if(temp < 0)
              map.regions[index].VerificationTargetReporter = -1;
            else
              map.regions[index].VerificationTargetReporter = temp;
          }
          else
            map.regions[index].VerificationTargetReporter = temp;
        }
        else
        {
          map.regions[index].isValidator = false;
          map.regions[index].VerificationTargetReporter = -1;
        }
      }
    }
    var res = App.DiosApp.MapController.SaveCalVals(new MapCalParameters
    {
      TempCl0 = -1,
      TempCl1 = -1,
      TempCl2 = -1,
      TempCl3 = -1,
      TempRedSsc = -1,
      TempGreenSsc = -1,
      TempVioletSsc = -1,
      TempRpMaj = -1,
      TempRpMin = -1,
      TempFsc = -1,
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
    App.DiosApp.MapController.SaveCalVals(new MapCalParameters
    {
      TempCl0 = -1,
      TempCl1 = -1,
      TempCl2 = -1,
      TempCl3 = -1,
      TempRedSsc = -1,
      TempGreenSsc = -1,
      TempVioletSsc = -1,
      TempRpMaj = -1,
      TempRpMin = -1,
      TempFsc = -1,
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

  public bool ValMapInfoReady()
  {
    if (MapRegionsController.ActiveVerificationRegionNums.Count == 0)
    {
      var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_VerRegions_NotSelected),
        Language.TranslationSource.Instance.CurrentCulture);
      Notification.Show(msg);
      return false;
    }

    for (var i = 1; i < MapRegionsController.RegionsList.Count; i++)
    {
      if (MapRegionsController.ActiveVerificationRegionNums.Contains(MapRegionsController.RegionsList[i].Number)
          && MapRegionsController.RegionsList[i].TargetReporterValue[0] == "")
      {
        var msg1 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Ver_Region),
          Language.TranslationSource.Instance.CurrentCulture);
        var msg2 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Reporter_Not_Specified),
          Language.TranslationSource.Instance.CurrentCulture);
        Notification.Show($"{msg1} {MapRegionsController.RegionsList[i]} {msg2}");
        return false;
      }
    }
    return true;
  }

  public bool AnalyzeVerificationResults(out string errorMsg)
  {
    errorMsg = null;
    App.DiosApp.Verificator.SetCulture(Language.TranslationSource.Instance.CurrentCulture);
    var passed1 = App.DiosApp.Verificator.ReporterToleranceTest(Settings.Default.ValidatorToleranceReporter, out var msg1);
    var passed2 = App.DiosApp.Verificator.ClassificationToleranceTest(Settings.Default.ValidatorToleranceClassification, out var msg2);
    var passed3 = App.DiosApp.Verificator.MisclassificationToleranceTest(Settings.Default.ValidatorToleranceMisclassification, out var msg3);
    App.DiosApp.Verificator.PublishResult($"{App.DiosApp.RootDirectory.FullName}\\SystemLogs\\VerificationLogs.txt");
    if (msg1 != null)
      errorMsg = msg1;
    if (msg2 != null)
      errorMsg = errorMsg + Environment.NewLine + msg2;
    if (msg3 != null)
      errorMsg = errorMsg + Environment.NewLine + msg3;
    return passed1 && passed2 && passed3;
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
    var toleranceSP = Views.VerificationView.Instance.toleranceSP.Children;
    var maxCV_SP = Views.VerificationView.Instance.maxCV_SP.Children;
    switch (num)
    {
      case 0:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ToleranceItems)), this, 0, toleranceSP[0] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(toleranceSP[0] as TextBox);
        break;
      case 1:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ToleranceItems)), this, 1, toleranceSP[1] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(toleranceSP[1] as TextBox);
        break;
      case 2:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ToleranceItems)), this, 2, toleranceSP[2] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(toleranceSP[2] as TextBox);
        break;
      case 3:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ToleranceItems)), this, 3, toleranceSP[3] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(toleranceSP[3] as TextBox);
        break;
      case 4:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ToleranceItems)), this, 4, toleranceSP[4] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(toleranceSP[4] as TextBox);
        break;
      case 5:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(MaxCVItems)), this, 0, maxCV_SP[0] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(maxCV_SP[0] as TextBox);
        break;
      case 6:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(MaxCVItems)), this, 1, maxCV_SP[1] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(maxCV_SP[1] as TextBox);
        break;
      case 7:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(MaxCVItems)), this, 2, maxCV_SP[2] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(maxCV_SP[2] as TextBox);
        break;
      case 8:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(MaxCVItems)), this, 3, maxCV_SP[3] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(maxCV_SP[3] as TextBox);
        break;
      case 9:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(MaxCVItems)), this, 4, maxCV_SP[4] as TextBox);
        MainViewModel.Instance.NumpadToggleButton(maxCV_SP[4] as TextBox);
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