using System;
using System.Collections.Generic;
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