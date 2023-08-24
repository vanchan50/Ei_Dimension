using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Ei_Dimension.Controllers;
using DIOS.Application;
using System.Windows;
using System.Threading;
using System.Windows.Media;

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

  public void LoadButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    App.DiosApp.Device.LoadPlate();
  }

  public void EjectButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    App.DiosApp.Device.EjectPlate();
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
      DashboardViewModel.Instance.WorkOrder[0] = data;
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

  public void StartButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    HideScanButton();
    //TODO: contains 0 can never happen here
    if (App.DiosApp.Terminator.TerminationType == Termination.MinPerRegion
        && !MapRegionsController.AreThereActiveRegions())
    {
      var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_MinPerReg_RequiresAtLeast1),
        Language.TranslationSource.Instance.CurrentCulture);
      Notification.Show(msg);
      return;
    }

    IReadOnlyCollection<Well> wells;
    if (App.DiosApp.Control == SystemControl.WorkOrder)
    {
      if (App.DiosApp.WorkOrderController.TryGetWorkOrderById("1", out var wo))
      {
        App.CurrentWorkOrder = wo;
      }
      //fill wells from work order
      wells = App.CurrentWorkOrder.Wells;
    }
    else
    {
      wells = WellsSelectViewModel.Instance.OutputWells();
    }

    if (wells.Count == 0)
    {
      var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_NoWellsOrTube_Selected),
        Language.TranslationSource.Instance.CurrentCulture);
      Notification.Show(msg);
      return;
    }

    HashSet<int> regions = null;
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
        regions = MapRegionsController.ActiveRegionNums;
        break;
      case OperationMode.Calibration:
        App.MapRegions.RemoveNullTextBoxes();
        CalibrationViewModel.Instance.CalJustFailed = true;
        ResultsViewModel.Instance.ShowSinglePlexResults();
        break;
      case OperationMode.Verification:
        if (MapRegionsController.ActiveVerificationRegionNums.Count != 4)
        {
          Notification.ShowError($"{MapRegionsController.ActiveVerificationRegionNums.Count} " +
                                 "out of 4 Verification Regions selected\nPlease select 4 Verification Regions");
          return;
        }
        App.MapRegions.RemoveNullTextBoxes();
        var verificationRegions = MapRegionsController.MakeVerificationList();
        App.DiosApp.Verificator.Reset(verificationRegions);
        break;
    }
    MainViewModel.Instance.NavigationSelector(1);

    EnableStartButton(false);
    ResultsViewModel.Instance.ClearGraphs();
    PlatePictogramViewModel.Instance.PlatePictogram.Clear();
    ResultsViewModel.Instance.PlotCurrent();
    PlatePictogramViewModel.Instance.PlatePictogram.SetWellsForReading(wells);
    ResultsViewModel.Instance.ClearCurrentCalibrationStats();
    App.DiosApp.StartOperation(regions, wells);
  }

  public void EndButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    if (!App.DiosApp.Device.IsMeasurementGoing)  //end button press before start, cancel work order
    {
      if (App.DiosApp.Control == SystemControl.WorkOrder)
      {
        DashboardViewModel.Instance.WorkOrder[0] = ""; //actually questionable if not in workorder operation
      }
    }
    else
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