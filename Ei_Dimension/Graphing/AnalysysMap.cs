using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Core;
using Ei_Dimension.Controllers;
using Ei_Dimension.Graphing.HeatMap;
using Ei_Dimension.Models;

namespace Ei_Dimension.Graphing
{
  [POCOViewModel]
  public class AnalysysMap
  {
    public List<RegionReporterResult> BackingWResults { get; set; }
    public virtual ObservableCollection<DoubleHeatMapData> DisplayedAnalysisMap { get; set; }
    public virtual ObservableCollection<DoubleHeatMapData> CurrentAnalysis01Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> CurrentAnalysis02Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> CurrentAnalysis03Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> CurrentAnalysis12Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> CurrentAnalysis13Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> CurrentAnalysis23Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> BackingAnalysis01Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> BackingAnalysis02Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> BackingAnalysis03Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> BackingAnalysis12Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> BackingAnalysis13Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> BackingAnalysis23Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();

    protected AnalysysMap()
    {
      DisplayedAnalysisMap = CurrentAnalysis12Map;
      BackingWResults = new List<RegionReporterResult>();
    }

    public static AnalysysMap Create()
    {
      ViewModels.ResultsViewModel.Instance.AnalysisMap = ViewModelSource.Create(() => new AnalysysMap());
      return ViewModels.ResultsViewModel.Instance.AnalysisMap;
    }

    public void ClearData(bool current)
    {
      if (current)
      {
        CurrentAnalysis01Map.Clear();
        CurrentAnalysis02Map.Clear();
        CurrentAnalysis03Map.Clear();
        CurrentAnalysis12Map.Clear();
        CurrentAnalysis13Map.Clear();
        CurrentAnalysis23Map.Clear();
      }
      else
      {
        BackingAnalysis01Map.Clear();
        BackingAnalysis02Map.Clear();
        BackingAnalysis03Map.Clear();
        lock (BackingAnalysis12Map)
        {
          BackingAnalysis12Map.Clear();
        }
        BackingAnalysis13Map.Clear();
        BackingAnalysis23Map.Clear();
      }
    }

    public void InitBackingWellResults()
    {
      BackingWResults.Clear();
      if (MapRegionsController.ActiveRegionNums.Count > 0)
      {
        foreach (var reg in MapRegionsController.ActiveRegionNums)
        {
          if (reg == 0)
            continue;
          BackingWResults.Add(new RegionReporterResult { regionNumber = (ushort)reg });
        }
      }
    }

    public void FillBackingAnalysisMap()
    {
      foreach (var result in BackingWResults)
      {
        if (App.Device.MapCtroller.ActiveMap.Regions.TryGetValue(result.regionNumber, out var region))
        {
          var x = HeatMapPoint.bins[region.Center.x];
          var y = HeatMapPoint.bins[region.Center.y];
          lock (BackingAnalysis12Map)
          {
            if (result.ReporterValues.Count > 0)
            {
              BackingAnalysis12Map.Add(new DoubleHeatMapData(x, y, result.ReporterValues.Average()));
            }
          }
        }
      }
    }

    public void AddDataPoint(int regionIndex, double reporterAvg)
    {
      var x = HeatMapPoint.bins[App.Device.MapCtroller.ActiveMap.regions[regionIndex].Center.x];
      var y = HeatMapPoint.bins[App.Device.MapCtroller.ActiveMap.regions[regionIndex].Center.y];
      CurrentAnalysis12Map.Add(new Models.DoubleHeatMapData(x, y, reporterAvg));
    }

    public void ChangeDisplayedMap(MapIndex mapIndex, bool current)
    {
      DisplayedAnalysisMap = null;
      if (current)
      {
        switch (mapIndex)
        {
          case MapIndex.CL01:
            DisplayedAnalysisMap = CurrentAnalysis01Map;
            break;
          case MapIndex.CL02:
            DisplayedAnalysisMap = CurrentAnalysis02Map;
            break;
          case MapIndex.CL03:
            DisplayedAnalysisMap = CurrentAnalysis03Map;
            break;
          case MapIndex.CL12:
            DisplayedAnalysisMap = CurrentAnalysis12Map;
            break;
          case MapIndex.CL13:
            DisplayedAnalysisMap = CurrentAnalysis13Map;
            break;
          case MapIndex.CL23:
            DisplayedAnalysisMap = CurrentAnalysis23Map;
            break;
        }
      }
      else
      {
        switch (mapIndex)
        {
          case MapIndex.CL01:
            DisplayedAnalysisMap = BackingAnalysis01Map;
            break;
          case MapIndex.CL02:
            DisplayedAnalysisMap = BackingAnalysis02Map;
            break;
          case MapIndex.CL03:
            DisplayedAnalysisMap = BackingAnalysis03Map;
            break;
          case MapIndex.CL12:
            DisplayedAnalysisMap = BackingAnalysis12Map;
            break;
          case MapIndex.CL13:
            DisplayedAnalysisMap = BackingAnalysis13Map;
            break;
          case MapIndex.CL23:
            DisplayedAnalysisMap = BackingAnalysis23Map;
            break;
        }
      }
    }
  }
}
