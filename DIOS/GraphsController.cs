using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DIOS.Core;
using Ei_Dimension.Graphing.HeatMap;

namespace Ei_Dimension;

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
  private readonly List<ProcessedBead> _tempBeadInfoList = new (1000);
  private static readonly List<Task> _tasksCache = new(5);

  public void Update()
  {
    if (Interlocked.CompareExchange(ref _uiUpdateIsActive, 1, 0) == 1)
      return;

    //UpdateBinfoList();
    App.DiosApp.Results.CycleSpanSize();
    App.DiosApp.Results.FixSpanSize();
    var t1 = Task.Run(() =>
    {
      var span = App.DiosApp.Results.GetNewBeadsAsSpan();
      //var span = CollectionsMarshal.AsSpan(_tempBeadInfoList);
      Core.DataProcessor.BinScatterData(span);

      var t2 = ViewModels.ScatterChartViewModel.Instance.ScttrData.FillCurrentDataASYNC_UI();
      _tasksCache.Add(t2);
    });
    _tasksCache.Add(t1);


    var t3 = Task.Run(() =>
    {
      var span = App.DiosApp.Results.GetNewBeadsAsSpan();
      Core.DataProcessor.BinMapData(span, current: true);
      if (!ViewModels.ResultsViewModel.Instance.DisplaysCurrentmap)
      {
        return;
      }
      var t4 = HeatMapAPI.API.ReDraw();
      if (t4 is not null)
      {
        _tasksCache.Add(t4);
      }
    });
    _tasksCache.Add(t3);
    Task.WhenAll(_tasksCache).ContinueWith(x =>
    {
      _uiUpdateIsActive = 0;
    });
  }

  private void UpdateBinfoList()
  {
    _tempBeadInfoList.Clear();
    try
    {
      foreach (var bead in App.DiosApp.Results.GetNewBeads())
      {
        _tempBeadInfoList.Add(bead);
      }
    }
    catch (ArgumentOutOfRangeException e)
    {
      //Should only be able to happen if WellResults.Reset() happens during enumeration, access to 
      App.Logger.Log($"[WARNING]\t{nameof(UpdateBinfoList)} {e.Message}");
    }
  }
}