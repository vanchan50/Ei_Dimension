using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DIOS.Core;
using Ei_Dimension.HeatMap;

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
    private readonly List<BeadInfoStruct> _tempBeadInfoList = new List<BeadInfoStruct>(1000);

    public void Update()
    {
      if (Interlocked.CompareExchange(ref _uiUpdateIsActive, 1, 0) == 1)
        return;

      _ = Task.Run(()=>
      {
        UpdateBinfoList();
        _ = Task.Run(() => { Core.DataProcessor.BinScatterData(_tempBeadInfoList); });
        Core.DataProcessor.BinMapData(_tempBeadInfoList, current: true);
        if (!ViewModels.ResultsViewModel.Instance.DisplaysCurrentmap)
        {
          _uiUpdateIsActive = 0;
          return;
        }
        _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
        {
          HeatMapAPI.API.ReDraw();
          _uiUpdateIsActive = 0;
        }));
      });
    }

    private void UpdateBinfoList()
    {
      _tempBeadInfoList.Clear();
      while (App.Device.DataOut.TryDequeue(out BeadInfoStruct bead))
      {
        _tempBeadInfoList.Add(bead);
      }
    }
  }
}
