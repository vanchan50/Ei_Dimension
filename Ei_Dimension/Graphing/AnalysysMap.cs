﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Application;
using DIOS.Core;
using Ei_Dimension.Controllers;
using Ei_Dimension.Graphing.HeatMap;
using Ei_Dimension.Models;

namespace Ei_Dimension.Graphing
{
  [POCOViewModel]
  public class AnalysysMap
  {
    public ReporterResultManager BackingWResults { get; protected set; } = new ReporterResultManager();
    public virtual ObservableCollection<DoubleHeatMapData> DisplayedMap { get; set; }
    public virtual ObservableCollection<DoubleHeatMapData> Current01Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> Current02Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> Current03Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> Current12Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> Current13Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> Current23Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> Backing01Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> Backing02Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> Backing03Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> Backing12Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> Backing13Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();
    public virtual ObservableCollection<DoubleHeatMapData> Backing23Map { get; protected set; } = new ObservableCollection<DoubleHeatMapData>();

    private object _lock = new object();
    private MapModel _activeMap;
    protected AnalysysMap()
    {
      DisplayedMap = Current12Map;
    }

    public static AnalysysMap Create()
    {
      return ViewModelSource.Create(() => new AnalysysMap());
    }

    public void ClearData(bool current)
    {
      if (current)
      {
        Current01Map.Clear();
        Current02Map.Clear();
        Current03Map.Clear();
        Current12Map.Clear();
        Current13Map.Clear();
        Current23Map.Clear();
      }
      else
      {
        Backing01Map.Clear();
        Backing02Map.Clear();
        Backing03Map.Clear();
        lock (_lock)
        {
          Backing12Map.Clear();
        }
        Backing13Map.Clear();
        Backing23Map.Clear();
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
          BackingWResults.Add(new RegionReporterResult(reg));
        }
      }
    }

    public void FillBackingMap()
    {
      foreach (var result in BackingWResults)
      {
        if (_activeMap.Regions.TryGetValue(result.regionNumber, out var region))
        {
          var x = HeatMapPoint.bins[region.Center.x];
          var y = HeatMapPoint.bins[region.Center.y];
          lock (_lock)
          {
            if (result.Count > 0)
            {
              Backing12Map.Add(new DoubleHeatMapData(x, y, result.Average()));
            }
          }
        }
      }
    }

    public void AddDataPoint(int regionIndex, double reporterAvg)
    {
      var region = _activeMap.regions[regionIndex];
      var x = HeatMapPoint.bins[region.Center.x];
      var y = HeatMapPoint.bins[region.Center.y];
      Current12Map.Add(new DoubleHeatMapData(x, y, reporterAvg));
    }

    public void ChangeDisplayedMap(MapIndex mapIndex, bool current)
    {
      DisplayedMap = null;
      if (current)
      {
        switch (mapIndex)
        {
          case MapIndex.CL01:
            DisplayedMap = Current01Map;
            break;
          case MapIndex.CL02:
            DisplayedMap = Current02Map;
            break;
          case MapIndex.CL03:
            DisplayedMap = Current03Map;
            break;
          case MapIndex.CL12:
            DisplayedMap = Current12Map;
            break;
          case MapIndex.CL13:
            DisplayedMap = Current13Map;
            break;
          case MapIndex.CL23:
            DisplayedMap = Current23Map;
            break;
        }
      }
      else
      {
        switch (mapIndex)
        {
          case MapIndex.CL01:
            DisplayedMap = Backing01Map;
            break;
          case MapIndex.CL02:
            DisplayedMap = Backing02Map;
            break;
          case MapIndex.CL03:
            DisplayedMap = Backing03Map;
            break;
          case MapIndex.CL12:
            DisplayedMap = Backing12Map;
            break;
          case MapIndex.CL13:
            DisplayedMap = Backing13Map;
            break;
          case MapIndex.CL23:
            DisplayedMap = Backing23Map;
            break;
        }
      }
    }

    public void OnMapChanged(MapModel map)
    {
      _activeMap = map;
    }
  }
}
