using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ValidationViewModel
  {
    public static ValidationViewModel Instance { get; private set; }
    protected ValidationViewModel()
    {
      Instance = this;
    }

    public static ValidationViewModel Create()
    {
      return ViewModelSource.Create(() => new ValidationViewModel());
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
        App.MapRegions.ValidationRegions[i] = true;
        App.MapRegions.AddValidationRegion(i);
        App.MapRegions.ValidationReporterList[i] = "";
        if (map.regions[i].isValidator)
        {
          App.MapRegions.AddValidationRegion(i);
          if(map.regions[i].ValidationTargetReporter > 0.01)
            App.MapRegions.ValidationReporterList[i] = map.regions[i].ValidationTargetReporter.ToString();
        }
      }
    }

    public void SaveClick()
    {
      var idx = App.Device.MapList.FindIndex(x => x.mapName == App.Device.ActiveMap.mapName);
      var map = App.Device.MapList[idx];
      for (var i = 0; i < App.MapRegions.RegionsList.Count; i++)
      {
        if (App.MapRegions.ValidationRegions[i])
        {
          var index = map.regions.FindIndex(x => x.Number == int.Parse(App.MapRegions.RegionsList[i]));
          if(index != -1)
          {
            map.regions[index].isValidator = true;
            double temp;
            if (double.TryParse(App.MapRegions.ValidationReporterList[i], out temp))
              map.regions[index].ValidationTargetReporter = temp;
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

    public void ValidationSuccess()
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
      if (App.MapRegions.ValidationRegionNums.Count == 0)
      {
        App.ShowNotification("No Validation regions selected");
        return false;
      }

      for (var i = 0; i < App.MapRegions.RegionsList.Count; i++)
      {
        if (App.MapRegions.ValidationRegions[i])
        {
          if (App.MapRegions.ValidationReporterList[i] == "")
          {
            App.ShowNotification($"Validation region {App.MapRegions.RegionsList[i]} Reporter is not specified");
            return false;
          }
        }
      }
      return true;
    }
  }
}