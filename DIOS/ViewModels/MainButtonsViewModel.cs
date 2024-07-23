using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Ei_Dimension.Controllers;
using DIOS.Application;
using System;
using System.Windows;
using System.Threading;
using System.Windows.Media;
using System.Linq;
using System.Threading.Tasks;

namespace Ei_Dimension.ViewModels;

[POCOViewModel]
public class MainButtonsViewModel
{
  public virtual bool StartButtonEnabled { get; set; } = true;
  public static MainButtonsViewModel Instance { get; private set; }
  public virtual ObservableCollection<string> ActiveList { get; set; }
  public virtual ObservableCollection<string> Flavor { get; set; }
  public virtual Visibility ScanButtonVisibility { get; set; } = Visibility.Collapsed;
  public virtual SolidColorBrush StartButtonColor { get; set; } = _inactiveColorBrush;
  private static SolidColorBrush _inactiveColorBrush = Brushes.DimGray;
  private static SolidColorBrush _activeColorBrush = Brushes.ForestGreen;
  private int _scanOngoing;
  protected MainButtonsViewModel()
  {
    ActiveList = new ObservableCollection<string>();
    Flavor = new ObservableCollection<string> { null };
    Instance = this;
  }

  public static MainButtonsViewModel Create()
  {
    return ViewModelSource.Create(() => new MainButtonsViewModel());
  }

  public async Task LoadButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    await App.DiosApp.Device.LoadPlate();
  }

  public async Task EjectButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    await App.DiosApp.Device.EjectPlate();
  }

  public async void ScanButtonClick()
  {
    if (Interlocked.CompareExchange(ref _scanOngoing, 1, 0) == 1)
      return;

    UserInputHandler.InputSanityCheck();
    string data = null;
    try
    {
      data = await App.DiosApp.BarcodeReader.QueryReadAsync(2000);
    }
    catch(IOException)
    {
      Notification.ShowError("BCR not available");
      _scanOngoing = 0;
      return;
    }

    var success = !string.IsNullOrEmpty(data);

    if (success)
    {
      DashboardViewModel.Instance.WorkOrderID[0] = data;
      EnableStartButton(true);
    }
    _scanOngoing = 0;
  }

  public void ShowScanButton()
  {
    UserInputHandler.InputSanityCheck();
    ScanButtonVisibility = Visibility.Visible;
  }

  public void HideScanButton()
  {
    UserInputHandler.InputSanityCheck();
    ScanButtonVisibility = Visibility.Collapsed;
  }

  public void EnableStartButton(bool enable)
  {
    StartButtonEnabled = enable;
    StartButtonColor = enable ? _activeColorBrush : _inactiveColorBrush;
  }

  public async Task StartButtonClick(IReadOnlyCollection<Well> wellList = null)
  {
    UserInputHandler.InputSanityCheck();
    HideScanButton();
    var wells = wellList;
    wells ??= ValidateInputs();
    if (wells.Count == 0)
      return;

    MainViewModel.Instance.NavigationSelector(1);

    EnableStartButton(false);
    ResultsViewModel.Instance.ClearGraphs();
    PlatePictogramViewModel.Instance.PlatePictogram.Clear();
    ResultsViewModel.Instance.PlotCurrent();
    PlatePictogramViewModel.Instance.PlatePictogram.SetWellsForReading(wells);
    StatisticsTableViewModel.Instance.ClearCurrentCalibrationStats();
    if (App.DiosApp.Control == SystemControl.WorkOrder && !string.IsNullOrEmpty(DashboardViewModel.Instance.WorkOrderID[0]))
    {
      await App.DiosApp.StartOperation(wells, WellsSelectViewModel.Instance.CurrentPlate, DashboardViewModel.Instance.WorkOrderID[0]);
    }
    else
    {
      await App.DiosApp.StartOperation(wells, WellsSelectViewModel.Instance.CurrentPlate);
    }
  }

  private IReadOnlyCollection<Well> ValidateInputs()
  {
    //TODO: contains 0 can never happen here
    if (App.DiosApp.Terminator.TerminationType == Termination.MinPerRegion
        && !MapRegionsController.AreThereActiveRegions())
    {
      var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_MinPerReg_RequiresAtLeast1),
        Language.TranslationSource.Instance.CurrentCulture);
      Notification.Show(msg);
      return Array.Empty<Well>();
    }

    IReadOnlyCollection<(int Number, string Name)> regions = null;
    switch (App.DiosApp.Device.Mode)
    {
      case OperationMode.Normal:
        if (!MapRegionsController.AreThereActiveRegions())
        {
          //this adds region0 to ActiveRegionNums
          DisplayNullregionInUI();
        }
        MapRegionsController.ActiveRegionNums.Add(0);
        //DefaultRegionNaming();
        var sregions = new HashSet<(int Number, string Name)>();
        foreach (var activeRegionNum in MapRegionsController.ActiveRegionNums)
        {
          var index = MapRegionsController.GetMapRegionIndex(activeRegionNum);
          var mapRegion = MapRegionsController.RegionsList[index];
          sregions.Add((mapRegion.Number, mapRegion.Name[0]));
        }
        regions = sregions;
        break;
      case OperationMode.Calibration:
        App.MapRegions.RemoveNullTextBoxes();
        CalibrationViewModel.Instance.CalJustFailed = true;
        ResultsViewModel.Instance.ShowSinglePlexResults();
        break;
      case OperationMode.Verification:
        App.MapRegions.RemoveNullTextBoxes();
        List<(int regionNum, string regionName)> valRegions = new();
        var list = App.DiosApp.MapController.ActiveMap.regions.Where(x => x.isValidator);
        foreach (var validationRegion in list)
        {
          valRegions.Add((validationRegion.Number, validationRegion.Number.ToString()));
        }
        regions = valRegions;
        break;
    }

    IReadOnlyList<Well> wells;
    //if (App.DiosApp.Control == SystemControl.WorkOrder)
    //{
    //  if (App.DiosApp.WorkOrderController.TryGetWorkOrderById("1", out var wo))
    //  {
    //    App.CurrentWorkOrder = wo;
    //  }
    //  //fill wells from work order
    //  wells = App.CurrentWorkOrder.Wells;
    //}
    //else
    //{
    regions ??= new HashSet<(int Number, string Name)>();
    wells = WellsSelectViewModel.Instance.OutputWells(regions);
    //}
    if (wells.Count == 0)
    {
      var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_NoWellsOrTube_Selected),
        Language.TranslationSource.Instance.CurrentCulture);
      Notification.Show(msg);
    }

    return wells;
  }

  public void EndButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    if (App.DiosApp.Device.IsMeasurementGoing)  //end button press before start, cancel work order
    {
      if (App.DiosApp.RunPlateContinuously)
      {
        ComponentsViewModel.Instance.ContinuousModeOn = false;
        ComponentsViewModel.Instance.ContinuousModeToggle();
      }
      App.DiosApp.Device.PrematureStop();
    }
  }

  private static void DefaultRegionNaming()
  {
    for (var i = 1; i < MapRegionsController.RegionsList.Count; i++)
    {
      if (MapRegionsController.ActiveRegionNums.Contains(MapRegionsController.RegionsList[i].Number) && MapRegionsController.RegionsList[i].Name[0] == "")
      {
        MapRegionsController.RegionsList[i].Name[0] = MapRegionsController.RegionsList[i].NumberString;
      }
    }
  }

  private static void DisplayNullregionInUI()
  {
    App.MapRegions.ShowNullTextBoxes();
  }
}