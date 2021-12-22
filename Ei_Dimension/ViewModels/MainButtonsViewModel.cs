using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
      App.Device.MainCommand("Load Plate");
    }

    public void EjectButtonClick()
    {
      App.Device.MainCommand("Eject Plate");
    }

    public void StartButtonClick()
    {
      if (App.Device.TerminationType == 0 && App.MapRegions.ActiveRegionNums.Count == 0)
      {
        App.ShowNotification("\"Min Per Region\" End of Read requires at least 1 active region");
        return;
      }

      DashboardViewModel.Instance.SetWellsInOrder();
      if (App.Device.WellsInOrder.Count < 1)
      {
        App.ShowNotification("No wells or Tube selected");
        return;
      }
#if DEBUG
      App.Device.MainCommand("Set FProperty", code: 0x06);
#endif
      HashSet<int> startArg = null;
      switch (App.Device.Mode)
      {
        case MicroCy.OperationMode.Normal:
          if (App.MapRegions.ActiveRegionNums.Count == 0)
          {
            App.ShowNotification("No Active regions selected");
            for (var i = 0; i < App.MapRegions.ActiveRegions.Count; i++)
            {
              App.MapRegions.AddActiveRegion(i);
            }
          }
          DefaultRegionNaming();
          startArg = App.MapRegions.ActiveRegionNums;
          break;
        case MicroCy.OperationMode.Calibration:
          CalibrationViewModel.Instance.CalJustFailed = true;
          ResultsViewModel.Instance.ShowSinglePlexResults();
          break;
        case MicroCy.OperationMode.Verification:
          MakeNewValidator();
          break;
      }
      StartButtonEnabled = false;
      ResultsViewModel.Instance.ClearGraphs();
      ResultsViewModel.Instance.PlatePictogram.Clear();
      ResultsViewModel.Instance.PlotCurrent();
      ResultsViewModel.Instance.PlatePictogram.SetWellsForReading(App.Device.WellsInOrder);
      for(var i = 0; i < 10; i++)
      {
        ResultsViewModel.Instance.MfiItems[i] = "";
        ResultsViewModel.Instance.CvItems[i] = "";
      }
      App.Device.StartOperation(startArg);
      MainViewModel.Instance.NavigationSelector(1);
    }

    public void EndButtonClick()
    {
      if (!App.Device.ReadActive)  //end button press before start, cancel work order
      {
        App.Device.MainCommand("Set Property", code: 0x17); //leds off
        DashboardViewModel.Instance.WorkOrder[0] = "";
      }
      else
      {
        App.Device.ReadActive = false;
        App.Device.EndState = 1;
        if (App.Device.WellsToRead > 0) //if end read on tube or single well, nothing else is aspirated otherwise
          App.Device.WellsToRead = App.Device.CurrentWellIdx + 1; //just read the next well in order since it is already aspirated
      }
    }

    private void MakeNewValidator()
    {
      var regions = new List<int>();
      for (var i = 0; i < App.MapRegions.RegionsList.Count; i++)
      {
        if (App.MapRegions.VerificationRegions[i])
        {
          int reg = int.Parse(App.MapRegions.RegionsList[i]);
          regions.Add(reg);
        }
      }
      App.Device.Verificator = new MicroCy.Validator(regions);
    }

    public void DefaultRegionNaming()
    {
      for (var i = 0; i < App.MapRegions.RegionsList.Count; i++)
      {
        if (App.MapRegions.ActiveRegions[i])
        {
          if (App.MapRegions.RegionsNamesList[i] == "")
            App.MapRegions.RegionsNamesList[i] = App.MapRegions.RegionsList[i];
        }
      }
    }
  }
}