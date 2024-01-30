using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DIOS.Core;
using Ei_Dimension.Graphing.HeatMap;
using Ei_Dimension.ViewModels;

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

  public void Update()
  {
    if (Interlocked.CompareExchange(ref _uiUpdateIsActive, 1, 0) == 1)
      return;

    _ = Task.Run(()=>
    {
      UpdateBinfoList();
      _ = Task.Run(() =>
      {
        var span = CollectionsMarshal.AsSpan(_tempBeadInfoList);
        Core.DataProcessor.BinScatterData(span);

        _ = App.Current.Dispatcher.BeginInvoke(() =>
        {
          ScatterChartViewModel.Instance.ScttrData.FillCurrentData();
        });
      });

      Core.DataProcessor.BinMapData(_tempBeadInfoList, current: true);
      if (!ViewModels.ResultsViewModel.Instance.DisplaysCurrentmap)
      {
        _uiUpdateIsActive = 0;
        return;
      }
      _ = App.Current.Dispatcher.BeginInvoke(() =>
      {
        HeatMapAPI.API.ReDraw();
        _uiUpdateIsActive = 0;
      });
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