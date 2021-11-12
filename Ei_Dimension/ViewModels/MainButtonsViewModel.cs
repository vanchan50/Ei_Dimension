using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class MainButtonsViewModel
  {
    public virtual bool StartButtonEnabled { get; set; }
    public static MainButtonsViewModel Instance { get; private set; }
    public virtual ObservableCollection<string> ActiveList { get; set; }
    protected MainButtonsViewModel()
    {
      StartButtonEnabled = true;
      ActiveList = new ObservableCollection<string>();
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

      //read section of plate
      App.Device.MainCommand("Get FProperty", code: 0x58);
      App.Device.MainCommand("Get FProperty", code: 0x68);
      App.Device.PlateReport = new MicroCy.PlateReport(); //TODO: optimize, not needed here
      App.Device.MainCommand("Get FProperty", code: 0x20); //get high dnr property
      App.Device.ReadActive = true;

      StartButtonEnabled = false;
      ResultsViewModel.Instance.ClearGraphs();
      ResultsViewModel.Instance.PlotCurrent();
      ResultsViewModel.Instance.PlatePictogram.Clear();
      ResultsViewModel.Instance.PlatePictogram.SetWellsForReading(App.Device.WellsInOrder);

      App.Device.SetAspirateParamsForWell(0);  //setup for first read
      App.Device.SetReadingParamsForWell(0);
      App.Device.MainCommand("Set Property", code: 0x19, parameter: 1); //bubble detect on
      App.Device.MainCommand("Position Well Plate");   //move motors. next position is set in properties 0xad and 0xae
      App.Device.MainCommand("Aspirate Syringe A"); //handles down and pickup sample
      App.Device.WellNext();   //save well numbers for file name
      App.Device.InitBeadRead(App.Device.ReadingRow, App.Device.ReadingCol);   //gets output file redy
      App.Device.ClearSummary();

      if (App.Device.WellsToRead == 0)    //only one well in region
        App.Device.MainCommand("Read A");
      else
      {
        App.Device.SetAspirateParamsForWell(1);
        App.Device.MainCommand("Read A Aspirate B");
      }
      App.Device.CurrentWellIdx = 0;
      if (App.Device.TerminationType != 1)    //set some limit for running to eos or if regions are wrong
        App.Device.BeadsToCapture = 100000;
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
  }
}