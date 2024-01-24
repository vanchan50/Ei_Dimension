using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Application;

namespace Ei_Dimension.Controllers;

/// <summary>
/// This class Holds the statistical data for every map region.
/// The data is split between the Current measurement and Backing (for selected file)
/// </summary>
[POCOViewModel]
public class ActiveRegionsStatsController
{
  //pointers to storages of mean and count
  public virtual ObservableCollection<string> DisplayedActiveRegionsCount { get; protected set; }
  public virtual ObservableCollection<string> DisplayedActiveRegionsMean { get; protected set; }
  //storage for mean and count of all existing regions. For current reading and backing file select
  public ObservableCollection<string> CurrentCount { get; } = new();
  public ObservableCollection<string> CurrentMean { get; } = new();
  public ObservableCollection<string> BackingCount { get; } = new();
  public ObservableCollection<string> BackingMean { get; } = new();
  public static ActiveRegionsStatsController Instance
  {
    get
    {
      if (_instance != null)
        return _instance;
      return _instance = ViewModelSource.Create(() => new ActiveRegionsStatsController());
    }
  }
  public const int NULLREGIONCAPACITY = 100000;
  public bool IsMeasurementGoing { get; set; } = false;

  private static ActiveRegionsStatsController _instance;
  private static int _activeRegionsUpdateGoing;
  private static readonly RegionReporterResultVolatile _nullWellRegionResult = new(0);

  protected ActiveRegionsStatsController()
  {
    _nullWellRegionResult.ReporterValues.Capacity = NULLREGIONCAPACITY;
  }

  public void AddNewEntry()
  {
    CurrentCount.Add("0");
    CurrentMean.Add("0");
    BackingCount.Add("0");
    BackingMean.Add("0");
  }

  public void ResetCurrentActiveRegionsDisplayedStats()
  {
    for (var i = 0; i < CurrentCount.Count; i++)
    {
      CurrentCount[i] = "0";
      CurrentMean[i] = "0";
    }
  }

  public void ResetBackingDisplayedStats()
  {
    for (var i = 0; i < BackingCount.Count; i++)
    {
      BackingCount[i] = "0";
      BackingMean[i] = "0";
    }
  }

  public void Clear()
  {
    CurrentCount.Clear();
    CurrentMean.Clear();
    BackingCount.Clear();
    BackingMean.Clear();
  }

  public void DisplayCurrentBeadStats(bool current = true)
  {
    if (current)
    {
      DisplayedActiveRegionsCount = CurrentCount;
      DisplayedActiveRegionsMean = CurrentMean;
      return;
    }
    DisplayedActiveRegionsCount = BackingCount;
    DisplayedActiveRegionsMean = BackingMean;
  }

  public void UpdateCurrentStats()
  {
    if (!IsMeasurementGoing)
      return;
    
    if (Interlocked.CompareExchange(ref _activeRegionsUpdateGoing, 1, 0) > 0)
      return;
    
    Action action;
    if (App.MapRegions.IsNullRegionActive)
    {
      var copy = App.DiosApp.Results.MakeWellResultsClone(); //TODO:hotpath allocations
      action = UpdateNullRegionProcedure(copy);
    }
    else
    {
      var cache = App.DiosApp.Results.GetReporterMeansAndCount();
      action = UpdateRegionsProcedure(cache);
    }
    _ = App.Current.Dispatcher.BeginInvoke(action)
      .Task.ContinueWith(x=>
      {
        _activeRegionsUpdateGoing = 0;
      });
  }

  private Action UpdateRegionsProcedure(IReadOnlyDictionary<int, (int, float)> meanCountCache)
  {
    return () =>
    {
      foreach (var cache in meanCountCache)
      {
        var index = MapRegionsController.GetMapRegionIndex(cache.Key);
        if (index < 0)
          continue;
        var count = cache.Value.Item1;
        var mean = cache.Value.Item2;
          
        CurrentCount[index] = count.ToString();
        CurrentMean[index] = mean.ToString("F1");
      }
    };
  }

  public void FinalUpdateRegionsProcedure(WellStats wellStats)
  {
    while (Interlocked.CompareExchange(ref _activeRegionsUpdateGoing, 2, 0) > 0)
    {
      Thread.Sleep(50);
    }

    foreach (var result in wellStats)
    {
      var index = MapRegionsController.GetMapRegionIndex(result.Region);
      if (index < 0)
        continue;
      var count = result.Count;
      var mean = result.MeanFi;

      CurrentCount[index] = count.ToString();
      CurrentMean[index] = mean.ToString("F1");
    }
    _activeRegionsUpdateGoing = 0;
  }

  private Action UpdateNullRegionProcedure(List<RegionReporterResultVolatile> wellresults)
  {
    foreach (var result in wellresults)
    {
      if (result.Count > 0)
      {
        _nullWellRegionResult.ReporterValues.InsertRange(_nullWellRegionResult.Count, result.ReporterValues);
      }
    }

    _nullWellRegionResult.MakeStats(out var count, out var mean);
    _nullWellRegionResult.ReporterValues.Clear();

    return () =>
    {
      CurrentCount[0] = count.ToString();
      CurrentMean[0] = mean.ToString("0.0");
    };
  }
}