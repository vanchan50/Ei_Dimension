using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Application;
using DIOS.Core;
using Ei_Dimension.Controllers;

namespace Ei_Dimension.ViewModels;

[POCOViewModel]
public class NormalizationViewModel
{
  public virtual ObservableCollection<string> NormalizationFactor { get; set; } = new() { "" };
  public virtual ObservableCollection<bool> NormalizationEnabled { get; set; } = new() { true };
  public virtual ObservableCollection<bool> CompensationEnabled { get; set; } = new() { true };
  public virtual ObservableCollection<string> CompensationMatrix { get; set; } = new() { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", };
  public static NormalizationViewModel Instance { get; private set; }

  protected NormalizationViewModel()
  {
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
        MapRegionsController.RegionsList[idx].MFIValue[0] = value.medianFi.ToString();
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
      App.DiosApp.Normalization.Enable();
    else
      App.DiosApp.Normalization.Disable();
    NormalizationEnabled[0] = state;
    MainViewModel.Instance.SetNormalizationMarker(state);
  }

  public void CompensationMatrixCheckedBox(bool state)
  {
    UserInputHandler.InputSanityCheck();
    App.DiosApp._beadProcessor.BeadCompensationEnabled = state;
    CompensationEnabled[0] = state;
  }

  public void FocusedBox(int num)
  {
    switch (num)
    {
      case 0:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(NormalizationFactor)), this, 0, Views.NormalizationView.Instance.TB0);
        MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.TB0);
        break;
      case 10:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationMatrix)), this, 0, Views.NormalizationView.Instance.CMatrix0);
        MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.CMatrix0);
        break;
      case 11:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationMatrix)), this, 1, Views.NormalizationView.Instance.CMatrix1);
        MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.CMatrix1);
        break;
      case 12:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationMatrix)), this, 2, Views.NormalizationView.Instance.CMatrix2);
        MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.CMatrix2);
        break;
      case 13:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationMatrix)), this, 3, Views.NormalizationView.Instance.CMatrix3);
        MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.CMatrix3);
        break;
      case 14:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationMatrix)), this, 4, Views.NormalizationView.Instance.CMatrix4);
        MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.CMatrix4);
        break;
      case 15:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationMatrix)), this, 5, Views.NormalizationView.Instance.CMatrix5);
        MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.CMatrix5);
        break;
      case 16:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationMatrix)), this, 6, Views.NormalizationView.Instance.CMatrix6);
        MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.CMatrix6);
        break;
      case 17:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationMatrix)), this, 7, Views.NormalizationView.Instance.CMatrix7);
        MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.CMatrix7);
        break;
      case 18:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationMatrix)), this, 8, Views.NormalizationView.Instance.CMatrix8);
        MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.CMatrix8);
        break;
      case 19:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationMatrix)), this, 9, Views.NormalizationView.Instance.CMatrix9);
        MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.CMatrix9);
        break;
      case 20:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationMatrix)), this, 10, Views.NormalizationView.Instance.CMatrix10);
        MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.CMatrix10);
        break;
      case 21:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationMatrix)), this, 11, Views.NormalizationView.Instance.CMatrix11);
        MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.CMatrix11);
        break;
      case 22:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationMatrix)), this, 12, Views.NormalizationView.Instance.CMatrix12);
        MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.CMatrix12);
        break;
      case 23:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationMatrix)), this, 13, Views.NormalizationView.Instance.CMatrix13);
        MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.CMatrix13);
        break;
      case 24:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationMatrix)), this, 14, Views.NormalizationView.Instance.CMatrix14);
        MainViewModel.Instance.NumpadToggleButton(Views.NormalizationView.Instance.CMatrix14);
        break;
    }
  }

  public void TextChanged(TextChangedEventArgs e)
  {
    UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
  }

  public void OnMapChanged(MapModel map)
  {
    NormalizationFactor[0] = map.factor.ToString();
    CompensationMatrixCheckedBox(map.CMatrixEnabled);
    CompensationMatrix[0] = map.CMatrix.GreenB1.ToString("F3");
    CompensationMatrix[1] = map.CMatrix.GreenB2.ToString("F3");
    CompensationMatrix[2] = map.CMatrix.GreenB3.ToString("F3");
    CompensationMatrix[3] = map.CMatrix.GreenB4.ToString("F3");
    CompensationMatrix[4] = map.CMatrix.GreenB5.ToString("F3");

    CompensationMatrix[5] = map.CMatrix.GreenC1.ToString("F3");
    CompensationMatrix[6] = map.CMatrix.GreenC2.ToString("F3");
    CompensationMatrix[7] = map.CMatrix.GreenC3.ToString("F3");
    CompensationMatrix[8] = map.CMatrix.GreenC4.ToString("F3");

    CompensationMatrix[9] = map.CMatrix.GreenD1.ToString("F3");
    CompensationMatrix[10] =map.CMatrix.GreenD2.ToString("F3");
    CompensationMatrix[11] =map.CMatrix.GreenD3.ToString("F3");

    CompensationMatrix[12] = map.CMatrix.RedC1.ToString("F3");
    CompensationMatrix[13] = map.CMatrix.RedC2.ToString("F3");

    CompensationMatrix[14] = map.CMatrix.RedD1.ToString("F3");
  }
}