using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DIOS.Core;

namespace Ei_Dimension
{
  /// <summary>
  /// Updates Scatter and XY plots with Current measurement data.
  /// Unpacks the DataOut queue of the Device
  /// </summary>
  internal sealed class GraphsController
  {
    public static GraphsController Instance
    {
      get
      {
        if (_instance != null)
          return _instance;
        return _instance = new GraphsController();
      }
    }

    private static GraphsController _instance;
    private static int _uiUpdateIsActive;
    private readonly List<BeadInfoStruct> TempBeadInfoList = new List<BeadInfoStruct>(300);

    public void Update()
    {
      if (Interlocked.CompareExchange(ref _uiUpdateIsActive, 1, 0) == 1)
        return;

      _ = Task.Run(()=>
      {
        UpdateBinfoList();
        _ = Task.Run(() => { Core.DataProcessor.BinScatterData(TempBeadInfoList); });
        Core.DataProcessor.BinMapData(TempBeadInfoList, current: true);
        if (!ViewModels.ResultsViewModel.Instance.DisplaysCurrentmap)
        {
          _uiUpdateIsActive = 0;
          return;
        }
        _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
        {
          Core.DataProcessor.AnalyzeHeatMap();
          _uiUpdateIsActive = 0;
        }));
      });
    }

    private void UpdateBinfoList()
    {
      TempBeadInfoList.Clear();
      while (App.Device.DataOut.TryDequeue(out BeadInfoStruct bead))
      {
        TempBeadInfoList.Add(bead);
      }
    }
  }
}
