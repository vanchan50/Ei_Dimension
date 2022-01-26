using MicroCy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

          action = () =>
          {
            ViewModels.ResultsViewModel.Instance.CurrentAnalysis12Map.Clear();
            if (NullWellResults.Count == 0)
            {
              App.MapRegions.CurrentActiveRegionsCount[0] = "0";
              App.MapRegions.CurrentActiveRegionsMean[0] = "0";
            }
            else
            {
              App.MapRegions.CurrentActiveRegionsCount[0] = NullWellResults.Count.ToString();
              App.MapRegions.CurrentActiveRegionsMean[0] = NullWellResults.Average().ToString("0,0");
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
              var index = App.MapRegions.RegionsList.IndexOf(result.region.ToString());
              if (index < 0)
                continue;
              float avg = 0;
              if (result.vals.Length == 0)
              {
                App.MapRegions.CurrentActiveRegionsCount[index] = "0";
                App.MapRegions.CurrentActiveRegionsMean[index] = "0";
              }
              else
              {
                avg = result.vals.Average();
                App.MapRegions.CurrentActiveRegionsCount[index] = result.vals.Length.ToString();
                App.MapRegions.CurrentActiveRegionsMean[index] = avg.ToString("0,0");
                Array.Clear(result.vals, 0, result.vals.Length); //Crutch. Explicit clear needed for some reason
              }
              if (index != 0)
                Reporter3DGraphHandler(index - 1, avg); // -1 accounts for region = 0
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