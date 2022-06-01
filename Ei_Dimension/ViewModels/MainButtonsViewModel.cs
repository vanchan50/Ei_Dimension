﻿using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Core;
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
      if (App.Device.TerminationType == Termination.MinPerRegion && MapRegionsController.ActiveRegionNums.Count == 0)
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
      App.Device.WellController.Init(wells);

      switch (App.Device.Mode)
      {
        case OperationMode.Normal:
          if (MapRegionsController.ActiveRegionNums.Count == 0)
          {
            SelectNullRegion();
          }
          //DefaultRegionNaming();
          if (!App.Device.Results.SetupRunRegions(MapRegionsController.ActiveRegionNums))
          {

          }
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
      ResultsViewModel.Instance.PlatePictogram.SetWellsForReading(wells);
      for(var i = 0; i < 10; i++)
      {
        ResultsViewModel.Instance.CurrentMfiItems[i] = "";
        ResultsViewModel.Instance.CurrentCvItems[i] = "";
      }
      MainViewModel.Instance.NavigationSelector(1);
      if (App.Device.IsNormalizationEnabled)
        Console.WriteLine("Normalization Enabled");
      else
        Console.WriteLine("Normalization Disabled");
      App.Device.StartOperation();
    }

    public void EndButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      if (!App.Device.IsMeasurementGoing)  //end button press before start, cancel work order
      {
        App.Device.MainCommand("Set Property", code: 0x17); //leds off
        if (DashboardViewModel.Instance.SelectedSystemControlIndex == 1)
        {
          DashboardViewModel.Instance.WorkOrder[0] = ""; //actually questionable if not in workorder operation
        }
      }
      else
      {
        App.Device.PrematureStop();
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