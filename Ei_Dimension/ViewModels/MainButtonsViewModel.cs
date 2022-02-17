using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using MicroCy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ei_Dimension.Controllers;

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
      App.Device.MainCommand("Load Plate");
    }

    public void EjectButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      App.Device.MainCommand("Eject Plate");
    }

    public void StartButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      if (MicroCyDevice.TerminationType == 0 && MapRegionsController.ActiveRegionNums.Count == 0)
      {
        Notification.Show("\"Min Per Region\" End of Read requires at least 1 active region");
        return;
      }

      DashboardViewModel.Instance.SetWellsInOrder();
      if (MicroCyDevice.WellsInOrder.Count < 1)
      {
        Notification.Show("No wells or Tube selected");
        return;
      }
      App.Device.MainCommand("Set FProperty", code: 0x06);
      HashSet<int> startArg = null;
      switch (MicroCyDevice.Mode)
      {
        case OperationMode.Normal:
          if (MapRegionsController.ActiveRegionNums.Count == 0)
          {
            SelectNullRegion();
          }
          //DefaultRegionNaming();
          startArg = MapRegionsController.ActiveRegionNums;
          break;
        case OperationMode.Calibration:
          App.MapRegions.RemoveNullTextBoxes();
          CalibrationViewModel.Instance.CalJustFailed = true;
          ResultsViewModel.Instance.ShowSinglePlexResults();
          break;
        case OperationMode.Verification:
          App.MapRegions.RemoveNullTextBoxes();
          MakeNewValidator();
          break;
      }
      StartButtonEnabled = false;
      ResultsViewModel.Instance.ClearGraphs();
      ResultsViewModel.Instance.PlatePictogram.Clear();
      ResultsViewModel.Instance.PlotCurrent();
      ResultsViewModel.Instance.PlatePictogram.SetWellsForReading(MicroCyDevice.WellsInOrder);
      for(var i = 0; i < 10; i++)
      {
        ResultsViewModel.Instance.CurrentMfiItems[i] = "";
        ResultsViewModel.Instance.CurrentCvItems[i] = "";
      }
      App.Device.StartOperation(startArg);
      MainViewModel.Instance.NavigationSelector(1);
    }

    public void EndButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      if (!MicroCyDevice.IsMeasurementGoing)  //end button press before start, cancel work order
      {
        App.Device.MainCommand("Set Property", code: 0x17); //leds off
        if (DashboardViewModel.Instance.SelectedSystemControlIndex == 1)
        {
          DashboardViewModel.Instance.WorkOrder[0] = ""; //actually questionable if not in workorder operation
        }
      }
      else
      {
        App.Device.StartStateMachine();
        if (MicroCyDevice.WellsToRead > 0) //if end read on tube or single well, nothing else is aspirated otherwise
          MicroCyDevice.WellsToRead = MicroCyDevice.CurrentWellIdx + 1; //just read the next well in order since it is already aspirated
      }
    }

    private static void MakeNewValidator()
    {
      var regions = new List<(int regionNum, double InputReporter)>();
      for (var i = 1; i < MapRegionsController.RegionsList.Count; i++)
      {
        if (MapRegionsController.ActiveVerificationRegionNums.Contains(MapRegionsController.RegionsList[i].Number))
        {
          int reg = MapRegionsController.RegionsList[i].Number;
          var inputReporter = double.Parse(MapRegionsController.RegionsList[i].TargetReporterValue[0]);
          inputReporter /= App.Device.ReporterScaling;  //adjust for scaling factor
          regions.Add((reg, inputReporter));
        }
      }
      Verificator.Reset(regions);
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

    private static void SelectNullRegion()
    {
      App.MapRegions.ShowNullTextBoxes();
    }
  }
}