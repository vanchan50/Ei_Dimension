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
    private static readonly List<(ushort region, float[] vals)> TempWellResults = new List<(ushort region, float[] vals)>(100);

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
        _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
        {
          ViewModels.ResultsViewModel.Instance.CurrentAnalysis12Map.Clear();
          foreach (var result in TempWellResults)
          {
            var index = App.MapRegions.RegionsList.IndexOf(result.region.ToString());
            if (index == -1)
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
              Array.Clear(result.vals, 0, result.vals.Length);  //Crutch. Explicit clear needed for some reason
            }
            Reporter3DGraphHandler(index, avg);
          }
          TempWellResults.Clear();
          _activeRegionsUpdateGoing = false;
        }));
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
