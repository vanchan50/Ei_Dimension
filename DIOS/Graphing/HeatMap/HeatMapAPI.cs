using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using DevExpress.Xpf.Charts;
using Ei_Dimension.Core;
using Ei_Dimension.ViewModels;

namespace Ei_Dimension.Graphing.HeatMap;

/// <summary>
/// The class holds raw data points for 256x256 XY graph.
/// <br>Initial Setup required with SetupChart()</br>
/// <br>Add points and then call ReDraw()</br>
/// </summary>
internal class HeatMapAPI
{
  public static HeatMapAPI API
  {
    get
    {
      if(_instance != null)
        return _instance;
      return new HeatMapAPI();
    }
  }

  private Dictionary<(int x, int y), HeatMapPoint> DisplayedMap { get; set; }
  private HeatMapDataContainer _data = new();

  private readonly double[] _colorBins;
  private static readonly SolidColorBrush[] _heatColors = new SolidColorBrush[13];
  private readonly List<SeriesPoint> _pointsCache = new (50000);
  private static HeatMapAPI _instance;
  private IHeatMapChart _heatMapChart;
  private bool _chartAssigned;

  private HeatMapAPI()
  {
    DisplayedMap = _data.GetAccordingDictionary(MapIndex.CL12, current:true);

    _heatColors[0] = Brushes.Black;
    _heatColors[1] = new SolidColorBrush(Color.FromRgb(0x4a, 0x00, 0x6a));
    _heatColors[2] = Brushes.DarkViolet;
    _heatColors[3] = new SolidColorBrush(Color.FromRgb(0x4f, 0x37, 0xbf));
    _heatColors[4] = new SolidColorBrush(Color.FromRgb(0x0a, 0x6d, 0xaa));
    _heatColors[5] = new SolidColorBrush(Color.FromRgb(0x05, 0x9d, 0x7a));
    _heatColors[6] = new SolidColorBrush(Color.FromRgb(0x00, 0xcc, 0x49));
    _heatColors[7] = new SolidColorBrush(Color.FromRgb(0x80, 0xb9, 0x25));
    _heatColors[8] = Brushes.Orange;
    _heatColors[9] = new SolidColorBrush(Color.FromRgb(0xff, 0x75, 0x00));
    _heatColors[10] = Brushes.OrangeRed;
    _heatColors[11] = new SolidColorBrush(Color.FromRgb(0xff, 0x23, 0x00));
    _heatColors[12] = Brushes.Red;

    _colorBins = new double[_heatColors.Length];
    _instance = this;
  }

  /// <summary>
  /// Inject an actual chart pointer. To be called only once
  /// </summary>
  /// <param name="chart"></param>
  /// <exception cref="Exception"></exception>
  public void SetupChart(IHeatMapChart chart)
  {
    if (_chartAssigned)
      throw new Exception("Chart is already set");

    _heatMapChart = chart;
    _chartAssigned = true;
  }

  public void ClearData(bool current)
  {
    _data.ClearData(current);
  }
  /// <summary>
  /// Add a point to graph data container.
  /// <br>Expects a point in CL space</br>
  /// </summary>
  /// <param name="pointInClSpace"></param>
  /// <param name="bins">Hi resolution or low resolution bins for point conversion to real space</param>
  /// <param name="mapIndex"></param>
  /// <param name="current"></param>
  public void AddDataPoint((int x, int y) pointInClSpace, double[] bins, MapIndex mapIndex, bool current)
  {
    _data.Add(pointInClSpace, bins, mapIndex, current);
  }

  public void ChangeDisplayedMap(MapIndex mapIndex, bool current)
  {
    DisplayedMap = _data.GetAccordingDictionary(mapIndex, current);
  }

  public Task ReDraw(bool hiRez = false)
  {
    if (DisplayedMap == null)
      return null;

    var heatMapList = DisplayedMap.Values; //DisplayedMap May change whenever, caching here is necessary
    if (heatMapList.Count > 0)
    {
      int max = heatMapList.Select(point => point.Amplitude).Max();

      DataProcessor.GenerateLogSpaceD(1, max + 1, _heatColors.Length, _colorBins, true);
      App.Current.Dispatcher.Invoke(_heatMapChart.ClearHeatMaps);
      foreach (var heatMapPoint in heatMapList)
      {
        if (heatMapPoint.Amplitude <= 1) //transparent single member beads == 1  //Actual amplitude starts from 0
          continue;
        var X = heatMapPoint.X;
        var Y = heatMapPoint.Y;
        if (ResultsViewModel.Instance.WrldMap.Flipped)
        {
          X = heatMapPoint.Y;
          Y = heatMapPoint.X;
        }
        CacheColorizedPoint(heatMapPoint.Amplitude, X, Y, hiRez);
      }

      var t1 = DrawCallAsync_UI(hiRez);
      return t1.ContinueWith(x =>
        {
          _pointsCache.Clear();
        });
    }
    else if (heatMapList.Count == 0)
    {
      return App.Current.Dispatcher.BeginInvoke(() =>
      {
        try
        {
          _heatMapChart.ClearHeatMaps();
        }
        catch
        {

        }
      }).Task;
    }

    return null;
  }

  public List<HeatMapPoint> GetCache(MapIndex mapIndex)
  {
    var ret = _data.GetAccordingDictionary(mapIndex);
    if (ret == null)
      return new List<HeatMapPoint>();
    return ret.Values.ToList();
  }

  private void CacheColorizedPoint(int Amplitude, int X, int Y, bool hiRez)
  {
    for (var j = 0; j < _heatColors.Length; j++)
    {
      if (Amplitude <= _colorBins[j])
      {
        if (!hiRez && j == 0) //Cutoff for smallXY
          break;
        var chartPoint = new SeriesPoint(X, Y);
        chartPoint.Brush = _heatColors[j];
        _pointsCache.Add(chartPoint);
        break;
      }
    }
  }

  private Task DrawCallAsync_UI(bool hiRez)
  {
    return App.Current.Dispatcher.BeginInvoke(() =>
    {
      try
      {
        _heatMapChart.AddXYPointToHeatMap(_pointsCache, hiRez);
      }
      catch(Exception e)
      {
        App.Logger.Log($"Heatmap DrawCall exception {e.Message}");
        App.Logger.Log($"trace: {e.StackTrace}");
      }
    }).Task;
  }
}