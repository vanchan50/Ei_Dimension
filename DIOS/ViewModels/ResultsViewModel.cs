﻿using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DIOS.Core;
using DIOS.Application.FileIO;
using Ei_Dimension.Controllers;
using Ei_Dimension.Graphing;
using Ei_Dimension.Graphing.HeatMap;
using System.Threading;
using Ei_Dimension.Models;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace Ei_Dimension.ViewModels;

[POCOViewModel]
public class ResultsViewModel
{
  public bool DisplaysCurrentmap { get; private set; }
  public virtual System.Windows.Visibility MultiPlexVisible { get; set; }
  public virtual System.Windows.Visibility SinglePlexVisible { get; set; }
  public virtual System.Windows.Visibility ValidationCoverVisible { get; set; }
  public virtual bool ResultsWaitIndicatorVisibility { get; set; }
  public virtual bool ChartWaitIndicatorVisibility { get; set; }
  public WorldMap WrldMap { get; set; }
  public AnalysysMap AnalysisMap { get; set; }
  public virtual System.Windows.Visibility AnalysisVisible { get; set; }
  public virtual System.Windows.Visibility Analysis2DVisible { get; set; }
  public virtual System.Windows.Visibility Analysis3DVisible { get; set; }
  public virtual string PlexButtonString { get; set; }
  public virtual ObservableCollection<bool> CLButtonsChecked { get; set; }
  public virtual ObservableCollection<string> CLAxis { get; set; }
  //Strings to make channel swap line up with names
  public virtual string XYPlot_DataChannel1Name { get; set; } = "CL1";
  public virtual string XYPlot_DataChannel2Name { get; set; } = "CL2";
  public static ResultsViewModel Instance { get; private set; }
  private int _fillDataActive;
  public const int HIREZDEFINITION = 512;
  private readonly List<ProcessedBead> _cachedBeadStructsForLoadedData = new(2_000_000);

  protected ResultsViewModel()
  {
    DisplaysCurrentmap = true;
    ResultsWaitIndicatorVisibility = false;
    ChartWaitIndicatorVisibility = false;
    MultiPlexVisible = System.Windows.Visibility.Visible;
    SinglePlexVisible = System.Windows.Visibility.Hidden;
    ValidationCoverVisible = System.Windows.Visibility.Hidden;
    PlexButtonString = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Experiment_Active_Regions),
      Language.TranslationSource.Instance.CurrentCulture);

    Instance = this;

    WrldMap = WorldMap.Create();
    AnalysisMap = AnalysysMap.Create();

    CLButtonsChecked = new(){ false, false, true, false, false, true, false, false };
    CLAxis = new(){ "CL1", "CL2" };
    AnalysisVisible = System.Windows.Visibility.Hidden;
    Analysis2DVisible = System.Windows.Visibility.Visible;
    Analysis3DVisible = System.Windows.Visibility.Hidden;
  }

  public static ResultsViewModel Create()
  {
    return ViewModelSource.Create(() => new ResultsViewModel());
  }

  public void CLButtonClick(int CL)
  {
    if (CL < 4)
    {
      CLButtonsChecked[0] = false;
      CLButtonsChecked[1] = false;
      CLButtonsChecked[2] = false;
      CLButtonsChecked[3] = false;
      CLAxis[1] = $"CL {CL}";
    }
    else
    {
      CLButtonsChecked[4] = false;
      CLButtonsChecked[5] = false;
      CLButtonsChecked[6] = false;
      CLButtonsChecked[7] = false;
      CLAxis[0] = $"CL {CL - 4}";
    }
    CLButtonsChecked[CL] = true;
    SetDisplayedMap();
    HeatMapAPI.API.ReDraw();
  }

  public void ClearGraphs(bool current = true)
  {
    ScatterChartViewModel.Instance.ScttrData.ClearData(current);
    HeatMapAPI.API.ClearData(current);
    AnalysisMap.ClearData(current);
    if (!current)
    {
      ActiveRegionsStatsController.Instance.ResetBackingDisplayedStats();
    }
    //else
    //do not ResetCurrentActiveRegionsDisplayedStats() here. This function can be called during runtime -> can loose data
    //ResetCurrentActiveRegionsDisplayedStats() is called in the App.StartingToReadWellEventHandler()
    Views.ResultsView.Instance.ClearHeatMaps();
  }

  public void SwapXYNamesToOEM(bool On)
  {
    if (On)
    {
      XYPlot_DataChannel1Name = "CL4";
      XYPlot_DataChannel2Name = "CL5";
    }
    else
    {
      XYPlot_DataChannel1Name = "CL1";
      XYPlot_DataChannel2Name = "CL2";
    }
  }

  /// <summary>
  /// Task to fill XY plot with data from file
  /// </summary>
  public void FillAllData()
  {
    if (Interlocked.CompareExchange(ref _fillDataActive, 1, 0) == 1)
    {
      Notification.Show("Please wait for the previous well to load");
      return;
    }

    var hiRez = AnalysisVisible == System.Windows.Visibility.Visible;
    _ = Task.Run(() =>
    {
      if(!LoadBeadEventsFile())
        return;

      _ = FillScatterChartFromFileAsync();
      _ = FillCalibrationStatsFromFileAsync();

      var allBeadsSpan = CollectionsMarshal.AsSpan(_cachedBeadStructsForLoadedData);
      Core.DataProcessor.BinMapData(allBeadsSpan, current: false, hiRez);
      //DisplayedMap.Sort((x, y) => x.A.CompareTo(y.A));
      var t = FillBackingMapFromFileAsync(hiRez);
      HeatMapAPI.API.ReDraw(hiRez);
      MainViewModel.Instance.EventCountLocal[0] = _cachedBeadStructsForLoadedData.Count.ToString();
    });
  }

  private bool LoadBeadEventsFile()
  {
    try
    {
      var path = PlatePictogramViewModel.Instance.PlatePictogram.GetSelectedFilePath(); //@"C:\Emissioninc\KEIZ0R-LEGION\AcquisitionData\xxxA1_01.05.2023.13-26-29.csv";//
      if (!BeadEventFileExists(path)) //rowtest1A1_0  //BeadAssayA1_19 //val speed test 2E7_0
        return false;

      _cachedBeadStructsForLoadedData.Clear();
      if (!BeadParser.ParseBeadInfoFile(path, _cachedBeadStructsForLoadedData))
      {
#if DEBUG
        App.Logger.Log($"LOADING FILE FAILED, FILE \"{path}\" is empty");
#endif
        Notification.ShowLocalized(nameof(Language.Resources.Notification_Empty_File));
        ResultsWaitIndicatorVisibility = false;
        ChartWaitIndicatorVisibility = false;
        _fillDataActive = 0;
        return false;
      }
    }
    catch (Exception e)
    {
      if (e.GetType() == typeof(OverflowException))
      {//can bubble up from ParseBeadInfo()
        App.Logger.Log("[PROBLEM] LOADING FILE FAILED; FILE overflow");
        App.Current.Dispatcher.Invoke(() =>
          Notification.ShowError("Overflow error while reading from file\nPlease report this issue"));
      }
      else
      {
        App.Logger.Log($"[PROBLEM] LOADING FILE FAILED; exception: {e.Message}");
      }
      ResultsWaitIndicatorVisibility = false;
      ChartWaitIndicatorVisibility = false;
      _fillDataActive = 0;
      return false;
    }

    return true;
  }

  private bool BeadEventFileExists(string path)
  {
    if (!System.IO.File.Exists(path))
    {
#if DEBUG
      App.Logger.Log($"LOADING FILE FAILED, FILE \"{path}\" doesn't exist");
#endif
      Notification.ShowLocalized(nameof(Language.Resources.Notification_File_Inexistent));
      ResultsWaitIndicatorVisibility = false;
      ChartWaitIndicatorVisibility = false;
      _fillDataActive = 0;
      return false;
    }
    return true;
  }

  private Task FillScatterChartFromFileAsync()
  {
    AnalysisMap.InitBackingWellResults();
    return Task.Run(() =>
    {
      var allBeadsSpan = CollectionsMarshal.AsSpan(_cachedBeadStructsForLoadedData);
      Core.DataProcessor.BinScatterData(allBeadsSpan, fromFile: true);

      ScatterChartViewModel.Instance.ScttrData.FillCurrentDataASYNC_UI(fromFile: true);
    });
  }

  private Task FillCalibrationStatsFromFileAsync()
  {
    return Task.Run(() =>
    {
      var allBeadsSpan = CollectionsMarshal.AsSpan(_cachedBeadStructsForLoadedData);
      var histogramPeaks = HistogramBinner.BinData(allBeadsSpan);

      var stats = Core.DataProcessor.CalculateStatistics(allBeadsSpan);
      _ = App.Current.Dispatcher.BeginInvoke(() =>
      {
        StatisticsTableViewModel.Instance.DecodeCalibrationStats(stats, histogramPeaks, current: false);
      });
    });
  }

  private DispatcherOperation FillBackingMapFromFileAsync(bool hiRez)
  {
    return App.Current.Dispatcher.BeginInvoke(() =>
    {
      try
      {
        AnalysisMap.FillBackingMap();
      }
      catch (Exception e)
      {
        if (Language.TranslationSource.Instance.CurrentCulture.TextInfo.CultureName == "zh-CN")
        {
          Notification.Show($"请将此问题报告给制造商\n {e.Message}");
        }
        else
        {
          Notification.Show($"Something went wrong during File loading.\nPlease report this issue to the manufacturer\n {e.Message}");
        }
      }
      finally
      {
        ChartWaitIndicatorVisibility = false;
        _fillDataActive = 0;
      }
    });
  }

  public void PlotCurrent(bool current = true)
  {
    DisplaysCurrentmap = current;
    App.Current.Dispatcher.Invoke(SetDisplayedMap);
    ScatterChartViewModel.Instance.ScttrData.DisplayCurrent(current);
    if (current)
    {
      App.Current.Dispatcher.Invoke(Views.PlatePictogramView.Instance.DrawingPlate.UnselectAllCells);
      if (App.MapRegions != null)
      {
        ActiveRegionsStatsController.Instance.DisplayCurrentBeadStats();
      }

      HeatMapAPI.API.ReDraw();
      MainViewModel.Instance.EventCountField = MainViewModel.Instance.EventCountCurrent;
      StatisticsTableViewModel.Instance.SelectDisplayedStatsType();
      return;
    }

    if (App.MapRegions != null)
    {
      ResultsWaitIndicatorVisibility = true;
      ChartWaitIndicatorVisibility = true;
      ActiveRegionsStatsController.Instance.DisplayCurrentBeadStats(current: false);
    }
    MainViewModel.Instance.EventCountField = MainViewModel.Instance.EventCountLocal;
    StatisticsTableViewModel.Instance.SelectDisplayedStatsType();
  }

  /// <summary>
  /// Called on Activemap change to fill all world maps with data from files
  /// </summary>
  public void FillWorldMaps()
  {
    _ = App.Current.Dispatcher.BeginInvoke(() =>
    {
      WrldMap.InitMaps();
      ResultsWaitIndicatorVisibility = false;
      ChartWaitIndicatorVisibility = false;
    }).Task.ContinueWith(x =>
    {
      PlotCurrent(DisplaysCurrentmap);
      ResultsWaitIndicatorVisibility = false;
      ChartWaitIndicatorVisibility = false;
    });
  }

  private void SetDisplayedMap()
  {
    WrldMap.Flipped = false;
    MapIndex mapIndex = MapIndex.Empty;
    if (CLButtonsChecked[4])
    {
      if (CLButtonsChecked[1])
      {
        mapIndex = MapIndex.CL01;
      }
      else if (CLButtonsChecked[2])
      {
        mapIndex = MapIndex.CL02;
      }
      else if (CLButtonsChecked[3])
      {
        mapIndex = MapIndex.CL03;
      }
      else
      {
        Views.ResultsView.Instance.ClearHeatMaps();
        WrldMap.DisplayedWorldMap.Clear();
        mapIndex = MapIndex.Empty;
      }
    }
    else if (CLButtonsChecked[5])
    {
      if (CLButtonsChecked[0])
      {
        WrldMap.Flipped = true;
        mapIndex = MapIndex.CL01;
      }
      else if (CLButtonsChecked[2])
      {
        mapIndex = MapIndex.CL12;
      }
      else if (CLButtonsChecked[3])
      {
        mapIndex = MapIndex.CL13;
      }
      else
      {
        Views.ResultsView.Instance.ClearHeatMaps();
        WrldMap.DisplayedWorldMap.Clear();
      }
    }
    else if (CLButtonsChecked[6])
    {
      if (CLButtonsChecked[0])
      {
        WrldMap.Flipped = true;
        mapIndex = MapIndex.CL02;
      }
      else if (CLButtonsChecked[1])
      {
        WrldMap.Flipped = true;
        mapIndex = MapIndex.CL12;
      }
      else if (CLButtonsChecked[3])
      {
        mapIndex = MapIndex.CL23;
      }
      else
      {
        Views.ResultsView.Instance.ClearHeatMaps();
        WrldMap.DisplayedWorldMap.Clear();
        mapIndex = MapIndex.Empty;
      }
    }
    else if (CLButtonsChecked[7])
    {
      if (CLButtonsChecked[0])
      {
        WrldMap.Flipped = true;
        mapIndex = MapIndex.CL03;
      }
      else if (CLButtonsChecked[1])
      {
        WrldMap.Flipped = true;
        mapIndex = MapIndex.CL13;
      }
      else if (CLButtonsChecked[2])
      {
        WrldMap.Flipped = true;
        mapIndex = MapIndex.CL23;
      }
      else
      {
        Views.ResultsView.Instance.ClearHeatMaps();
        WrldMap.DisplayedWorldMap.Clear();
        mapIndex = MapIndex.Empty;
      }
    }
    HeatMapAPI.API.ChangeDisplayedMap(mapIndex, DisplaysCurrentmap);
    AnalysisMap.ChangeDisplayedMap(mapIndex, DisplaysCurrentmap);
    WrldMap.DisplayedWmap = mapIndex;
    WrldMap.FillDisplayedWorldMap();
  }

  public void PlexButtonClick()
  {
    if (MultiPlexVisible == System.Windows.Visibility.Visible)
    {
      ShowSinglePlexResults();
      return;
    }
    ShowMultiPlexResults();
  }

  public void ShowSinglePlexResults()
  {
    PlexButtonString = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Experiment_Stats), Language.TranslationSource.Instance.CurrentCulture);
    MultiPlexVisible = System.Windows.Visibility.Hidden;
    SinglePlexVisible = System.Windows.Visibility.Visible;
  }

  public void ShowMultiPlexResults()
  {
    PlexButtonString = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Experiment_Active_Regions), Language.TranslationSource.Instance.CurrentCulture);
    MultiPlexVisible = System.Windows.Visibility.Visible;
    SinglePlexVisible = System.Windows.Visibility.Hidden;
  }

  public void Show3D()
  {
    Analysis3DVisible = System.Windows.Visibility.Visible;
    Analysis2DVisible = System.Windows.Visibility.Hidden;
  }

  public void Show2D()
  {
    Views.ResultsView.Instance.ShowLargeXYPlot();
    Analysis2DVisible = System.Windows.Visibility.Visible;
    Analysis3DVisible = System.Windows.Visibility.Hidden;
  }

  public void ShowAnalysis()
  {
    Views.ResultsView.Instance.Plot2DButton.IsChecked = true;
    AnalysisVisible = System.Windows.Visibility.Visible;
    Show2D();
  }

  public void ShowResults()
  {
    Views.ResultsView.Instance.ShowSmallXYPlot();
    AnalysisVisible = System.Windows.Visibility.Hidden;
    Analysis2DVisible = System.Windows.Visibility.Visible;
    Analysis3DVisible = System.Windows.Visibility.Hidden;
  }

  public void XYprint()
  {
    Views.ResultsView.Instance.PrintXY();
  }

  public void AnalysisPrint()
  {
    Views.ResultsView.Instance.Print3D();
  }
}