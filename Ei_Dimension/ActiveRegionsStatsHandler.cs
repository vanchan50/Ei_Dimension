using MicroCy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ei_Dimension
{
  internal class ActiveRegionsStatsHandler
  {
    private static bool _activeRegionsUpdateGoing;
    private static readonly List<(ushort region, float[] vals)> TempWellResults = new List<(ushort region, float[] vals)>(101);
    private static readonly List<float> NullWellResults = new List<float>(100000);

    public static void Update()
    {
      if (!_activeRegionsUpdateGoing)
      {
        _activeRegionsUpdateGoing = true;
        for (var i = 0; i < MicroCyDevice.WellResults.Count; i++)
        {
          var rp1 = new float[MicroCyDevice.WellResults[i].RP1vals.Count];
          MicroCyDevice.WellResults[i].RP1vals.CopyTo(0, rp1, 0, rp1.Length);
          TempWellResults.Add((MicroCyDevice.WellResults[i].regionNumber, rp1));
        }
        Action action;
        if (App.MapRegions.IsNullRegionActive)
        {
          foreach (var result in TempWellResults)
          {
            if (result.vals.Length > 0)
            {
              NullWellResults.InsertRange(NullWellResults.Count, result.vals);
              Array.Clear(result.vals, 0, result.vals.Length); //Crutch. Explicit clear needed for some reason
            }
          }
          TempWellResults.Clear();
          
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

          action = () =>
          {
            ViewModels.ResultsViewModel.Instance.CurrentAnalysis12Map.Clear();
            if (count == 0)
            {
              App.MapRegions.CurrentActiveRegionsCount[0] = "0";
              App.MapRegions.CurrentActiveRegionsMean[0] = "0";
            }
            else
            {
              App.MapRegions.CurrentActiveRegionsCount[0] = count.ToString();
              App.MapRegions.CurrentActiveRegionsMean[0] = mean.ToString("0.0");
            }
            NullWellResults.Clear();
            _activeRegionsUpdateGoing = false;
          };
        }
        else
        {
          action = () =>
          {
            ViewModels.ResultsViewModel.Instance.CurrentAnalysis12Map.Clear();
            foreach (var result in TempWellResults)
            {
              var index = App.MapRegions.GetMapRegionIndex(result.region);
              if (index < 0)
                continue;
              float avg = 0;
              float mean = 0;
              if (result.vals.Length == 0)
              {
                App.MapRegions.CurrentActiveRegionsCount[index] = "0";
                App.MapRegions.CurrentActiveRegionsMean[index] = "0";
              }
              else
              {
                avg = result.vals.Average();
                var count = result.vals.Length;
                if (count >= 20)
                {
                  Array.Sort(result.vals);
                  int quarterIndex = count / 4;

                  float sum = 0;
                  for (var i = quarterIndex; i < count - quarterIndex; i++)
                  {
                    sum += result.vals[i];
                  }
                  mean = sum / (count - 2 * quarterIndex);
                }
                else
                  mean = avg;

                App.MapRegions.CurrentActiveRegionsCount[index] = count.ToString();
                App.MapRegions.CurrentActiveRegionsMean[index] = mean.ToString("0.0");
                Array.Clear(result.vals, 0, result.vals.Length); //Crutch. Explicit clear needed for some reason
              }
              if (index != 0)
                Reporter3DGraphHandler(index - 1, mean); // -1 accounts for region = 0
            }

            TempWellResults.Clear();
            _activeRegionsUpdateGoing = false;
          };
        }
        _ = App.Current.Dispatcher.BeginInvoke(action);
      }
    }
    private static void Reporter3DGraphHandler(int regionIndex, double reporterAvg)
    {
      var x = Models.HeatMapData.bins[App.Device.MapCtroller.ActiveMap.regions[regionIndex].Center.x];
      var y = Models.HeatMapData.bins[App.Device.MapCtroller.ActiveMap.regions[regionIndex].Center.y];
      ViewModels.ResultsViewModel.Instance.CurrentAnalysis12Map.Add(new Models.DoubleHeatMapData(x, y, reporterAvg));
    }
  }
}