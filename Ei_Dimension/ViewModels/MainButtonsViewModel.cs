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
      DashboardViewModel.Instance.SetWellsInOrder();
      if (App.Device.WellsInOrder.Count < 1)
        return;

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
      App.Device.StartOperation();
      switch (App.Device.Mode)
      {
        case MicroCy.OperationMode.Normal:
          break;
        case MicroCy.OperationMode.Calibration:
          ResultsViewModel.Instance.ShowSinglePlexResults();
          break;
        case MicroCy.OperationMode.Validation:
          MakeNewValidator();
          break;
      }

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
        if (App.MapRegions.ValidationRegions[i])
        {
          int reg = int.Parse(App.MapRegions.RegionsList[i]);
          regions.Add(reg);
        }
      }
      App.Device.Validator = new MicroCy.Validator(regions);
    }
  }
}