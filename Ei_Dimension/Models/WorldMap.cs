using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using MicroCy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ei_Dimension.Controllers;

namespace Ei_Dimension.Models
{
  [POCOViewModel]
  public class WorldMap
  {
    public virtual ObservableCollection<HeatMapData> DisplayedWorldMap { get; set; }
    public List<HeatMapData> Map01 { get; }
    public List<HeatMapData> Map02 { get; }
    public List<HeatMapData> Map03 { get; }
    public List<HeatMapData> Map12 { get; }
    public List<HeatMapData> Map13 { get; }
    public List<HeatMapData> Map23 { get; }
    public List<HeatMapData> CalibrationMap { get; set; }
    public MapIndex DisplayedWmap { get; set; }
    public bool Flipped { get; set; }
    public const int WORLDMAPCAPACITY = 15000;

    protected WorldMap()
    {
      Flipped = false;
      DisplayedWmap = MapIndex.CL12;
      Map01 = new List<HeatMapData>(WORLDMAPCAPACITY);
      Map02 = new List<HeatMapData>(WORLDMAPCAPACITY);
      Map03 = new List<HeatMapData>(WORLDMAPCAPACITY);
      Map12 = new List<HeatMapData>(WORLDMAPCAPACITY);
      Map13 = new List<HeatMapData>(WORLDMAPCAPACITY);
      Map23 = new List<HeatMapData>(WORLDMAPCAPACITY);

      DisplayedWorldMap = new ObservableCollection<HeatMapData>();
    }

    public static WorldMap Create()
    {
      ViewModels.ResultsViewModel.Instance.WrldMap = ViewModelSource.Create(() => new WorldMap());
      return ViewModels.ResultsViewModel.Instance.WrldMap;
    }

    public void ClearMaps()
    {
      Map01.Clear();
      Map02.Clear();
      Map03.Clear();
      Map12.Clear();
      Map13.Clear();
      Map23.Clear();
    }

    public void InitMaps()
    {
      ClearMaps();
      foreach (var region in App.Device.MapCtroller.ActiveMap.regions)
      {
        foreach (var point in region.Points)
        {
          Map12.Add(new HeatMapData((int)HeatMapData.bins[point.x], (int)HeatMapData.bins[point.y], region.Number));
        }
      }
    }

    public void FillDisplayedMap()
    {
      Action BuildWmap = null;
      List<HeatMapData> Map = GetCurrentMap();
      switch (App.Device.Mode)
      {
        case OperationMode.Normal:
          BuildWmap = () => {
            foreach (var point in Map)
            {
              if (MapRegionsController.ActiveRegionNums.Contains(point.Region))
              {
                if (Flipped)
                  DisplayedWorldMap.Add(new HeatMapData(point.Y, point.X));
                else
                  DisplayedWorldMap.Add(new HeatMapData(point.X, point.Y));
              }
            }
          };
          break;
        case OperationMode.Calibration:
          BuildWmap = () => {
            foreach (var point in CalibrationMap)
            {
              DisplayedWorldMap.Add(new HeatMapData(point.X, point.Y));
            }
          };
          break;
        case OperationMode.Verification:
          BuildWmap = () => {
            foreach (var point in Map)
            {
              if (MapRegionsController.ActiveVerificationRegionNums.Contains(point.Region))
              {
                if (Flipped)
                  DisplayedWorldMap.Add(new HeatMapData(point.Y, point.X));
                else
                  DisplayedWorldMap.Add(new HeatMapData(point.X, point.Y));
              }
            }
          };
          break;
      }
      //cal worldmap is unique instance, that is produced by special function.
      //regular maps are produced in a regular way, so they can be switched with cl0-cl3 switches
      if (Map != null)
      {
        DisplayedWorldMap.Clear();
        _ = App.Current.Dispatcher.BeginInvoke(BuildWmap);
      }
    }

    private List<HeatMapData> GetCurrentMap()
    {
      List<HeatMapData> map = null;
      switch (DisplayedWmap)
      {
        case MapIndex.CL01:
          map = Map01;
          break;
        case MapIndex.CL02:
          map = Map02;
          break;
        case MapIndex.CL03:
          map = Map03;
          break;
        case MapIndex.CL12:
          map = Map12;
          break;
        case MapIndex.CL13:
          map = Map13;
          break;
        case MapIndex.CL23:
          map = Map23;
          break;
        case MapIndex.Empty:
          map = null;
          break;
      }
      return map;
    }
  }
}