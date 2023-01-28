using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Core;
using Ei_Dimension.Controllers;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class NormalizationViewModel
  {
    public virtual ObservableCollection<string> NormalizationFactor { get; set; }
    public virtual ObservableCollection<bool> NormalizationEnabled { get; set; }
    public static NormalizationViewModel Instance { get; private set; }

    protected NormalizationViewModel()
    {
      NormalizationFactor = new ObservableCollection<string> {""};
      NormalizationEnabled = new ObservableCollection<bool> { true };
      MainViewModel.Instance.SetNormalizationMarker(true);
      Instance = this;
    }

    public static NormalizationViewModel Create()
    {
      return ViewModelSource.Create(() => new NormalizationViewModel());
    }

    public void Load()
    {
      var map = App.DiosApp.MapController.ActiveMap;
      foreach (var regionData in MapRegionsController.RegionsList)
      {
        if (map.Regions.TryGetValue(regionData.Number, out var region))
        {
          regionData.MFIValue[0] = region.NormalizationMFI.ToString();
        }
      }
    }

    public void PostClick()
    {
      if (App.DiosApp.Device.IsMeasurementGoing)
      {
        Notification.ShowError("Please wait until the measurement is finished");
        return;
      }

      var list = App.DiosApp.Results.PlateReport.GetRegionalReporterMFI();

      if (list == null)
      {
        Notification.ShowError("Normalization results are not available");
        return;
      }

      foreach (var value in list)
      {
        if (MapRegionsController.ActiveRegionNums.Contains(value.region))
        {
          var idx = MapRegionsController.GetMapRegionIndex(value.region);
          MapRegionsController.RegionsList[idx].MFIValue[0] = value.mfi.ToString();
        }
      }
    }

    public void ClearClick()
    {
      foreach (var regionData in MapRegionsController.RegionsList)
      {
        regionData.MFIValue[0] = 0.ToString();
      }
    }

    public void SaveClick()
    {
      UserInputHandler.InputSanityCheck();
      var map = App.DiosApp.MapController.ActiveMap;

      var newRegions = map.regions;
      for (var i = 0; i < map.regions.Count; i++)
      {
        if (int.TryParse(MapRegionsController.RegionsList[i + 1].MFIValue[0], out var iRes))
        {
          newRegions[i].NormalizationMFI = iRes;
          continue;
        }
        Notification.ShowError("Error Saving To Map");  //TODO: should not happen
        return;
      }

      if (!App.DiosApp.MapController.SaveRegions(newRegions))
      {
        Notification.ShowError("Error Saving To Map");
        return;
      }
      Notification.Show("Normalization Data is Saved To Map");

      try
      {
        App.DiosApp.MapController.SaveNormVals(double.Parse(NormalizationFactor[0]));
      }
      catch (ArgumentException e)
      {
        Notification.ShowError(e.Message);
      }
    }

    public void CheckedBox(bool state)
    {
      UserInputHandler.InputSanityCheck();
      if(state)
        App.DiosApp.Device.Normalization.Enable();
      else
        App.DiosApp.Device.Normalization.Disable();
      NormalizationEnabled[0] = state;
      MainViewModel.Instance.SetNormalizationMarker(state);
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(NormalizationFactor)), this, 0, Views.NormalizationView.Instance.TB0);
          MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.TB0);
          break;
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }

    public void OnMapChanged(CustomMap map)
    {
      NormalizationFactor[0] = map.factor.ToString();
    }
  }
}