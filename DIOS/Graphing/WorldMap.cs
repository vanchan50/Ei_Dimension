using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ei_Dimension.Controllers;
using Ei_Dimension.Core;
using Ei_Dimension.Graphing.HeatMap;

namespace Ei_Dimension.Graphing
{
  [POCOViewModel]
  public class WorldMap
  {
    public virtual ObservableCollection<HeatMapPoint> DisplayedWorldMap { get; protected set; } = new ObservableCollection<HeatMapPoint>();
    public List<HeatMapPoint> Map01 { get; } = new List<HeatMapPoint>(WORLDMAPCAPACITY);
    public List<HeatMapPoint> Map02 { get; } = new List<HeatMapPoint>(WORLDMAPCAPACITY);
    public List<HeatMapPoint> Map03 { get; } = new List<HeatMapPoint>(WORLDMAPCAPACITY);
    public List<HeatMapPoint> Map12 { get; } = new List<HeatMapPoint>(WORLDMAPCAPACITY);
    public List<HeatMapPoint> Map12Flipped { get; } = new List<HeatMapPoint>(WORLDMAPCAPACITY);
    public List<HeatMapPoint> Map13 { get; } = new List<HeatMapPoint>(WORLDMAPCAPACITY);
    public List<HeatMapPoint> Map23 { get; } = new List<HeatMapPoint>(WORLDMAPCAPACITY);
    public List<HeatMapPoint> CalibrationMap { get; set; }
    public MapIndex DisplayedWmap { get; set; }
    public bool Flipped { get; set; }
    public const int WORLDMAPCAPACITY = 15000;

    protected WorldMap()
    {
      DisplayedWmap = MapIndex.CL12;
    }

    public static WorldMap Create()
    {
      return ViewModelSource.Create(() => new WorldMap());
    }

    public void ClearData()
    {
      Map01.Clear();
      Map02.Clear();
      Map03.Clear();
      Map12.Clear();
      Map13.Clear();
      Map23.Clear();
      //TODO: Only map12Flipped implemented for now
      Map12Flipped.Clear();
    }

    public void InitMaps()
    {
      ClearData();
      foreach (var region in App.DiosApp.MapController.ActiveMap.Regions)
      {
        foreach (var point in region.Value.Points)
        {
          var x = DataProcessor.FromCLSpaceToReal(point.x, HeatMapPoint.bins);
          var y = DataProcessor.FromCLSpaceToReal(point.y, HeatMapPoint.bins);
          Map12.Add(new HeatMapPoint(x, y, region.Key));
          Map12Flipped.Add(new HeatMapPoint(y, x, region.Key));
        }
      }
    }

    public void FillDisplayedWorldMap()
    {
      Action BuildWmap = null;
      List<HeatMapPoint> Map = GetCurrentMap();
      switch (App.DiosApp.Device.Mode)
      {
        case OperationMode.Normal:
          BuildWmap = () => {
            foreach (var point in Map)
            {
              if (MapRegionsController.ActiveRegionNums.Contains(point.Region))
              {
                DisplayedWorldMap.Add(point);
              }
            }
          };
          break;
        case OperationMode.Calibration:
          BuildWmap = () => {
            foreach (var point in CalibrationMap)
            {
              DisplayedWorldMap.Add(point);
            }
          };
          break;
        case OperationMode.Verification:
          BuildWmap = () => {
            foreach (var point in Map)
            {
              if (MapRegionsController.ActiveVerificationRegionNums.Contains(point.Region))
              {
                DisplayedWorldMap.Add(point);
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

    private List<HeatMapPoint> GetCurrentMap()
    {
      List<HeatMapPoint> map = null;
      if (!Flipped)
      {
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
      }
      else  //TODO: Only map12Flipped implemented for now
      {
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
            map = Map12Flipped;
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
      }
      return map;
    }
  }
}