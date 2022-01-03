using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroCy;

namespace Ei_Dimension
{
  internal class GraphsHandler
  {
    private static bool _histogramUpdateGoing;
    private static readonly List<BeadInfoStruct> TempBeadInfoList = new List<BeadInfoStruct>(100);

    public static void Update()
    {
      if (!_histogramUpdateGoing)
      {
        _histogramUpdateGoing = true;
        _ = Task.Run(()=>
        {
          TempBeadInfoList.Clear();
          while (MicroCyDevice.DataOut.TryDequeue(out BeadInfoStruct bead))
          {
            TempBeadInfoList.Add(bead);
          }
          _ = Task.Run(() => { Core.DataProcessor.BinScatterData(TempBeadInfoList); });
          Core.DataProcessor.BinMapData(TempBeadInfoList, current: true);
          if (ViewModels.ResultsViewModel.Instance.DisplaysCurrentmap)
          {
            _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
              Core.DataProcessor.AnalyzeHeatMap();
              _histogramUpdateGoing = false;
            }));
          }
          else
            _histogramUpdateGoing = false;
        });
      }
    }
  }
}
