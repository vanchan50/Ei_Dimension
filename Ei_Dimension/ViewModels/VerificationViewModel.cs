using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Core;
using System;
using System.Collections.ObjectModel;
using Ei_Dimension.Controllers;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class VerificationViewModel
  {
    public virtual ObservableCollection<DropDownButtonContents> VerificationWarningItems { get; set; }
    public virtual string SelectedVerificationWarningContent { get; set; }
    public static VerificationViewModel Instance { get; private set; }
    public bool isActivePage { get; set; }
    protected VerificationViewModel()
    {
      var RM = Language.Resources.ResourceManager;
      var curCulture = Language.TranslationSource.Instance.CurrentCulture;
      VerificationWarningItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Daily), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Weekly), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Monthly), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Quarterly), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Yearly), curCulture), this)
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
      var idx = App.Device.MapCtroller.MapList.FindIndex(x => x.mapName == App.Device.MapCtroller.ActiveMap.mapName);
      var map = App.Device.MapCtroller.MapList[idx];
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
      var idx = App.Device.MapCtroller.MapList.FindIndex(x => x.mapName == App.Device.MapCtroller.ActiveMap.mapName);
      var map = App.Device.MapCtroller.MapList[idx];
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
      var res = App.Device.MapCtroller.SaveCalVals(new MapCalParameters
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
      App.Device.MapCtroller.SaveCalVals(new MapCalParameters
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
      DashboardViewModel.Instance.SetValidationDate(App.Device.MapCtroller.ActiveMap.valtime);
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

    public static bool AnalyzeVerificationResults(out string errorMsg)
    {
      errorMsg = null;
      Verificator.SetCulture(Language.TranslationSource.Instance.CurrentCulture);
      var passed1 = Verificator.ReporterToleranceTest(Settings.Default.ValidatorToleranceReporter, out var msg1);
      var passed2 = Verificator.ClassificationToleranceTest(Settings.Default.ValidatorToleranceClassification, out var msg2);
      var passed3 = Verificator.MisclassificationToleranceTest(Settings.Default.ValidatorToleranceMisclassification, out var msg3);
      Verificator.PublishResult();
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
}