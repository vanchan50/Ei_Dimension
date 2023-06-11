using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ei_Dimension.Controllers;
using DIOS.Application;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class MainButtonsViewModel
  {
    public virtual bool StartButtonEnabled { get; set; }
    public static MainButtonsViewModel Instance { get; private set; }
    public virtual ObservableCollection<string> ActiveList { get; set; }
    public virtual ObservableCollection<string> Flavor { get; set; }
    protected MainButtonsViewModel()
    {
      StartButtonEnabled = true;
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

    public void StartButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      //TODO: contains 0 can never happen here
      if (App.DiosApp.Terminator.TerminationType == Termination.MinPerRegion
          && !MapRegionsController.AreThereActiveRegions())
      {
        var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_MinPerReg_RequiresAtLeast1),
          Language.TranslationSource.Instance.CurrentCulture);
        Notification.Show(msg);
        return;
      }

      var wells = WellsSelectViewModel.Instance.OutputWells();
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

      StartButtonEnabled = false;
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
        if (DashboardViewModel.Instance.SelectedSystemControlIndex == 1)
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
}