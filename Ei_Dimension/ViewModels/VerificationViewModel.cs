using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class VerificationViewModel
  {
    public static VerificationViewModel Instance { get; private set; }
    protected VerificationViewModel()
    {
      Instance = this;
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

      var idx = App.Device.MapList.FindIndex(x => x.mapName == App.Device.ActiveMap.mapName);
      var map = App.Device.MapList[idx];
      for (var i = 0; i < map.regions.Count; i++)
      {
        App.MapRegions.VerificationRegions[i] = true;
        App.MapRegions.AddValidationRegion(i);
        App.MapRegions.VerificationReporterList[i] = "";
        if (map.regions[i].isValidator)
        {
          App.MapRegions.AddValidationRegion(i);
          if(map.regions[i].VerificationTargetReporter > 0.01)
            App.MapRegions.VerificationReporterList[i] = map.regions[i].VerificationTargetReporter.ToString();
        }
      }
    }

    public void SaveClick()
    {
      var idx = App.Device.MapList.FindIndex(x => x.mapName == App.Device.ActiveMap.mapName);
      var map = App.Device.MapList[idx];
      for (var i = 0; i < App.MapRegions.RegionsList.Count; i++)
      {
        if (App.MapRegions.VerificationRegions[i])
        {
          var index = map.regions.FindIndex(x => x.Number == int.Parse(App.MapRegions.RegionsList[i]));
          if(index != -1)
          {
            map.regions[index].isValidator = true;
            double temp;
            if (double.TryParse(App.MapRegions.VerificationReporterList[i], out temp))
              map.regions[index].VerificationTargetReporter = temp;
          }
        }
      }
      App.Device.SaveCalVals(new MicroCy.MapCalParameters
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
        MinSSC = -1,
        MaxSSC = -1,
        Attenuation = -1,
        Caldate = null,
        Valdate = null
      });
    }

    public void VerificationSuccess()
    {
      App.Device.SaveCalVals(new MicroCy.MapCalParameters
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
        MinSSC = -1,
        MaxSSC = -1,
        Attenuation = -1,
        Caldate = null,
        Valdate = DateTime.Now.ToString("dd.MM.yyyy", new System.Globalization.CultureInfo("en-GB"))
      });
      DashboardViewModel.Instance.ValidDateBox[0] = App.Device.ActiveMap.valtime;
      App.ShowLocalizedNotification(nameof(Language.Resources.Validation_Success), System.Windows.Media.Brushes.Green);
    }

    public bool ValMapInfoReady()
    {
      if (App.MapRegions.VerificationRegionNums.Count == 0)
      {
        App.ShowNotification("No Verification regions selected");
        return false;
      }

      for (var i = 0; i < App.MapRegions.RegionsList.Count; i++)
      {
        if (App.MapRegions.VerificationRegions[i])
        {
          if (App.MapRegions.VerificationReporterList[i] == "")
          {
            App.ShowNotification($"Verification region {App.MapRegions.RegionsList[i]} Reporter is not specified");
            return false;
          }
        }
      }
      return true;
    }
  }
}