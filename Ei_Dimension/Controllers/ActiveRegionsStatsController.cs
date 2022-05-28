using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Core;

namespace Ei_Dimension.Controllers
{
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
    public ObservableCollection<string> CurrentCount { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> CurrentMean { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> BackingCount { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> BackingMean { get; } = new ObservableCollection<string>();
    public static ActiveRegionsStatsController Instance
    {
      get
      {
        if (_instance != null)
          return _instance;
        return _instance = ViewModelSource.Create(() => new ActiveRegionsStatsController());
      }
    }

    private static ActiveRegionsStatsController _instance;
    private static bool _activeRegionsUpdateGoing;
    private static readonly List<float> NullWellResults = new List<float>(100000);

    protected ActiveRegionsStatsController()
    {

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
      if (!_activeRegionsUpdateGoing)
      {
        _activeRegionsUpdateGoing = true;
        var copy = App.Device.Results.MakeDeepCopy();//TODO:hotpath allocations
        var action = App.MapRegions.IsNullRegionActive ? UpdateNullRegionProcedure(copy) : UpdateRegionsProcedure(copy);
        _ = App.Current.Dispatcher.BeginInvoke(action);
      }
    }

    private Action UpdateRegionsProcedure(List<RegionResult> wellresults)
    {
      return () =>
      {
        ViewModels.ResultsViewModel.Instance.CurrentAnalysis12Map.Clear();
        foreach (var result in wellresults)
        {
          var index = MapRegionsController.GetMapRegionIndex(result.regionNumber);
          if (index < 0)
            continue;
          float avg = 0;
          float mean = 0;
          if (result.RP1vals.Count == 0)
          {
            CurrentCount[index] = "0";
            CurrentMean[index] = "0";
          }
          else
          {
            avg = result.RP1vals.Average();
            var count = result.RP1vals.Count;
            if (count >= 20)
            {
              result.RP1vals.Sort();
              int quarterIndex = count / 4;

              float sum = 0;
              for (var i = quarterIndex; i < count - quarterIndex; i++)
              {
                sum += result.RP1vals[i];
              }

              mean = sum / (count - 2 * quarterIndex);
            }
            else
              mean = avg;

            CurrentCount[index] = count.ToString();
            CurrentMean[index] = mean.ToString("0.0");
          }

          if (index != 0)
            Reporter3DGraphHandler(index - 1, mean); // -1 accounts for region = 0
        }

        _activeRegionsUpdateGoing = false;
      };
    }

    private Action UpdateNullRegionProcedure(List<RegionResult> wellresults)
    {
      foreach (var result in wellresults)
      {
        if (result.RP1vals.Count > 0)
        {
          NullWellResults.InsertRange(NullWellResults.Count, result.RP1vals);
        }
      }

      var count = NullWellResults.Count;
      float mean = 0;
      if (count > 0)
      {
        float avg = NullWellResults.Average();
        if (count >= 20)
        {
          NullWellResults.Sort();
          int quarterIndex = count / 4;

          float sum = 0;
          for (var i = quarterIndex; i < count - quarterIndex; i++)
          {
            sum += NullWellResults[i];
          }
          mean = sum / (count - 2 * quarterIndex);
        }
        else
          mean = avg;
      }

      return () =>
      {
        ViewModels.ResultsViewModel.Instance.CurrentAnalysis12Map.Clear();
        if (count == 0)
        {
          CurrentCount[0] = "0";
          CurrentMean[0] = "0";
        }
        else
        {
          CurrentCount[0] = count.ToString();
          CurrentMean[0] = mean.ToString("0.0");
        }
        NullWellResults.Clear();
        _activeRegionsUpdateGoing = false;
      };
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

    private static void Reporter3DGraphHandler(int regionIndex, double reporterAvg)
    {
      var x = Models.HeatMapData.bins[App.Device.MapCtroller.ActiveMap.regions[regionIndex].Center.x];
      var y = Models.HeatMapData.bins[App.Device.MapCtroller.ActiveMap.regions[regionIndex].Center.y];
      ViewModels.ResultsViewModel.Instance.CurrentAnalysis12Map.Add(new Models.DoubleHeatMapData(x, y, reporterAvg));
    }

  }
}