using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using MicroCy;
using System;
using System.Collections.ObjectModel;

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

    public void LoadClick()
    {
      var idx = App.Device.MapCtroller.MapList.FindIndex(x => x.mapName == App.Device.MapCtroller.ActiveMap.mapName);
      var map = App.Device.MapCtroller.MapList[idx];
      for (var i = 0; i < map.regions.Count; i++)
      {
        App.MapRegions.VerificationRegions[i] = true;
        App.MapRegions.AddValidationRegion(i);
        App.MapRegions.VerificationReporterList[i] = "";
        if (map.regions[i].isValidator)
        {
          App.MapRegions.AddValidationRegion(i);
          if(map.regions[i].VerificationTargetReporter > -0.1)
            App.MapRegions.VerificationReporterList[i] = map.regions[i].VerificationTargetReporter.ToString();
        }
      }
    }

    public void SaveClick()
    {
      UserInputHandler.InputSanityCheck();
      var idx = App.Device.MapCtroller.MapList.FindIndex(x => x.mapName == App.Device.MapCtroller.ActiveMap.mapName);
      var map = App.Device.MapCtroller.MapList[idx];
      for (var i = 0; i < App.MapRegions.RegionsList.Count; i++)
      {
        var index = map.regions.FindIndex(x => x.Number == int.Parse(App.MapRegions.RegionsList[i]));
        if(index != -1)
        {
          double temp = -1;
          if (App.MapRegions.VerificationRegions[i])
          {
            map.regions[index].isValidator = true;
            if (App.MapRegions.VerificationReporterList[i] != "" && double.TryParse(App.MapRegions.VerificationReporterList[i], out temp))
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
      App.Device.MapCtroller.SaveCalVals(new MicroCy.MapCalParameters
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
    }

    public static void VerificationSuccess()
    {
      App.Device.MapCtroller.SaveCalVals(new MicroCy.MapCalParameters
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
      DashboardViewModel.Instance.ValidDateBox[0] = App.Device.MapCtroller.ActiveMap.valtime;
      Notification.ShowLocalized(nameof(Language.Resources.Validation_Success), System.Windows.Media.Brushes.Green);
    }

    public bool ValMapInfoReady()
    {
      if (App.MapRegions.VerificationRegionNums.Count == 0)
      {
        Notification.Show("No Verification regions selected");
        return false;
      }

      for (var i = 0; i < App.MapRegions.RegionsList.Count; i++)
      {
        if (App.MapRegions.VerificationRegions[i])
        {
          if (App.MapRegions.VerificationReporterList[i] == "")
          {
            Notification.Show($"Verification region {App.MapRegions.RegionsList[i]} Reporter is not specified");
            return false;
          }
        }
      }
      return true;
    }

    public static bool AnalyzeVerificationResults()
    {
      bool passed = true;
      if (Validator.TotalClassifiedBeads < MicroCyDevice.TotalBeads * 0.8)
      {
        Console.WriteLine("Verification Fail: Less than 80% of beads hit the regions");
        passed = false;
      }

      double reporterErrorMargin = 0.2;
      for (var i = 0; i < App.MapRegions.RegionsList.Count; i++)
      {
        if (App.MapRegions.VerificationRegions[i])
        {
          int regionNum = int.Parse(App.MapRegions.RegionsList[i]);
          double inputReporter = double.Parse(App.MapRegions.VerificationReporterList[i]);
          int validatorIndex = Validator.RegionalStats.FindIndex(x => x.Region == regionNum);

          if (Validator.RegionalStats[validatorIndex].Stats[0].mfi <= inputReporter * (1 - reporterErrorMargin) &&
              Validator.RegionalStats[validatorIndex].Stats[0].mfi >= inputReporter * (1 + reporterErrorMargin))
          {
            Console.WriteLine($"Verification Fail: Reporter value ({Validator.RegionalStats[validatorIndex].Stats[0].mfi.ToString()}) deviation is more than 20% from the target ({App.MapRegions.VerificationReporterList[i]})");
            passed = false;
          }
        }
      }
      return passed;
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