using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using DevExpress.Xpf.Charts;
using Ei_Dimension.Core;
using Ei_Dimension.ViewModels;

namespace Ei_Dimension.HeatMap
{
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

    private List<HeatMapPoint> DisplayedMap { get; set; }
    private readonly List<HeatMapPoint> _currentCL01Map = new List<HeatMapPoint>(XYMAPCAPACITY);
    private readonly List<HeatMapPoint> _currentCL02Map = new List<HeatMapPoint>(XYMAPCAPACITY);
    private readonly List<HeatMapPoint> _currentCL03Map = new List<HeatMapPoint>(XYMAPCAPACITY);
    private readonly List<HeatMapPoint> _currentCL12Map = new List<HeatMapPoint>(XYMAPCAPACITY);
    private readonly List<HeatMapPoint> _currentCL13Map = new List<HeatMapPoint>(XYMAPCAPACITY);
    private readonly List<HeatMapPoint> _currentCL23Map = new List<HeatMapPoint>(XYMAPCAPACITY);
    private readonly List<HeatMapPoint> _backingCL01Map = new List<HeatMapPoint>(XYMAPCAPACITY);
    private readonly List<HeatMapPoint> _backingCL02Map = new List<HeatMapPoint>(XYMAPCAPACITY);
    private readonly List<HeatMapPoint> _backingCL03Map = new List<HeatMapPoint>(XYMAPCAPACITY);
    private readonly List<HeatMapPoint> _backingCL12Map = new List<HeatMapPoint>(XYMAPCAPACITY);
    private readonly List<HeatMapPoint> _backingCL13Map = new List<HeatMapPoint>(XYMAPCAPACITY);
    private readonly List<HeatMapPoint> _backingCL23Map = new List<HeatMapPoint>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), int> _currentCL01Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), int> _currentCL02Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), int> _currentCL03Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), int> _currentCL12Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), int> _currentCL13Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), int> _currentCL23Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), int> _backingCL01Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), int> _backingCL02Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), int> _backingCL03Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), int> _backingCL12Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), int> _backingCL13Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), int> _backingCL23Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
    public const int XYMAPCAPACITY = 50000;  //max possible capacity is 256x256. Realistic 3/4 is ~49k

    private readonly double[] _bins;
    private readonly SolidColorBrush[] _heatColors;
    private static HeatMapAPI _instance;
    private IHeatMapChart _heatMapChart;
    private bool _chartAssigned;

    private HeatMapAPI()
    {
      DisplayedMap = _currentCL12Map;

      _heatColors = new SolidColorBrush[13];
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

      _bins = new double[_heatColors.Length];
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

    public void Clear(bool current = true)
    {
      if (current)
      {
        _currentCL01Map.Clear();
        _currentCL02Map.Clear();
        _currentCL03Map.Clear();
        _currentCL12Map.Clear();
        _currentCL13Map.Clear();
        _currentCL23Map.Clear();
        _currentCL01Dict.Clear();
        _currentCL02Dict.Clear();
        _currentCL03Dict.Clear();
        _currentCL12Dict.Clear();
        _currentCL13Dict.Clear();
        _currentCL23Dict.Clear();
      }
      else
      {
        _backingCL01Map.Clear();
        _backingCL02Map.Clear();
        _backingCL03Map.Clear();
        _backingCL12Map.Clear();
        _backingCL13Map.Clear();
        _backingCL23Map.Clear();
        _backingCL01Dict.Clear();
        _backingCL02Dict.Clear();
        _backingCL03Dict.Clear();
        _backingCL12Dict.Clear();
        _backingCL13Dict.Clear();
        _backingCL23Dict.Clear();
      }
    }

    public void AddPoint((int x, int y) pointInClSpace, double[] bins, MapIndex mapIndex, bool current = true)
    {
      Dictionary<(int x, int y), int> dict;
      List<HeatMapPoint> map;

      if (current)
      {
        switch (mapIndex)
        {
          case MapIndex.CL01:
            dict = _currentCL01Dict;
            map = _currentCL01Map;
            break;
          case MapIndex.CL02:
            dict = _currentCL02Dict;
            map = _currentCL02Map;
            break;
          case MapIndex.CL03:
            dict = _currentCL03Dict;
            map = _currentCL03Map;
            break;
          case MapIndex.CL12:
            dict = _currentCL12Dict;
            map = _currentCL12Map;
            break;
          case MapIndex.CL13:
            dict = _currentCL13Dict;
            map = _currentCL13Map;
            break;
          case MapIndex.CL23:
            dict = _currentCL23Dict;
            map = _currentCL23Map;
            break;
          default:
            return;
        }
      }
      else
      {
        switch (mapIndex)
        {
          case MapIndex.CL01:
            dict = _backingCL01Dict;
            map = _backingCL01Map;
            break;
          case MapIndex.CL02:
            dict = _backingCL02Dict;
            map = _backingCL02Map;
            break;
          case MapIndex.CL03:
            dict = _backingCL03Dict;
            map = _backingCL03Map;
            break;
          case MapIndex.CL12:
            dict = _backingCL12Dict;
            map = _backingCL12Map;
            break;
          case MapIndex.CL13:
            dict = _backingCL13Dict;
            map = _backingCL13Map;
            break;
          case MapIndex.CL23:
            dict = _backingCL23Dict;
            map = _backingCL23Map;
            break;
          default:
            return;
        }
      }
      //TODO: preallocate all the points as (0,0); Mutate them instead of allocating new;
      //TODO: use fill counter, so you don't have to traverse (0,0) points that are left as extra
      if (!dict.ContainsKey(pointInClSpace))
      {
        dict.Add(pointInClSpace, map.Count);
        map.Add(new HeatMapPoint((int)bins[pointInClSpace.x], (int)bins[pointInClSpace.y]));
      }
      else
      {
        map[dict[pointInClSpace]].Amplitude++;
      }
    }

    public void Display(MapIndex mapIndex, bool current = true)
    {
      if (current)
      {
        switch (mapIndex)
        {
          case MapIndex.CL01:
            DisplayedMap = _currentCL01Map;
            break;
          case MapIndex.CL02:
            DisplayedMap = _currentCL02Map;
            break;
          case MapIndex.CL03:
            DisplayedMap = _currentCL03Map;
            break;
          case MapIndex.CL12:
            DisplayedMap = _currentCL12Map;
            break;
          case MapIndex.CL13:
            DisplayedMap = _currentCL13Map;
            break;
          case MapIndex.CL23:
            DisplayedMap = _currentCL23Map;
            break;
          default:
            DisplayedMap = null;
            break;
        }
      }
      else
      {
        switch (mapIndex)
        {
          case MapIndex.CL01:
            DisplayedMap = _backingCL01Map;
            break;
          case MapIndex.CL02:
            DisplayedMap = _backingCL02Map;
            break;
          case MapIndex.CL03:
            DisplayedMap = _backingCL03Map;
            break;
          case MapIndex.CL12:
            DisplayedMap = _backingCL12Map;
            break;
          case MapIndex.CL13:
            DisplayedMap = _backingCL13Map;
            break;
          case MapIndex.CL23:
            DisplayedMap = _backingCL23Map;
            break;
          default:
            DisplayedMap = null;
            break;
        }
      }
    }

    public void AnalyzeHeatMap(bool hiRez = false)
    {
      var heatMapList = DisplayedMap; //DisplayedMap May change whenever, caching here is necessary
      if (heatMapList != null && heatMapList.Count > 0)
      {
        int max = heatMapList.Select(point => point.Amplitude).Max();

        DataProcessor.GenerateLogSpaceD(1, max + 1, _heatColors.Length, _bins, true);
        _heatMapChart.ClearHeatMaps();
        for (var i = 0; i < heatMapList.Count; i++)
        {
          var heatMap = heatMapList[i];
          if (heatMap.Amplitude <= 1) //transparent single member beads == 1  //Actual amplitude starts from 0
            continue;
          var X = heatMap.X;
          var Y = heatMap.Y;
          if (ResultsViewModel.Instance.WrldMap.Flipped)
          {
            X = heatMap.Y;
            Y = heatMap.X;
          }
          PutColorizedPointOnHeatMapGraph(heatMap.Amplitude, X, Y, hiRez);
        }
      }
      else if (heatMapList != null && heatMapList.Count == 0)
      {
        _heatMapChart.ClearHeatMaps();
      }
    }

    public List<HeatMapPoint> GetCache(MapIndex mapIndex)
    {
      List<HeatMapPoint> ret;
      switch (mapIndex)
      {
        case MapIndex.CL01:
          ret = _currentCL01Map;
          break;
        case MapIndex.CL02:
          ret = _currentCL02Map;
          break;
        case MapIndex.CL03:
          ret = _currentCL03Map;
          break;
        case MapIndex.CL12:
          ret = _currentCL12Map;
          break;
        case MapIndex.CL13:
          ret = _currentCL13Map;
          break;
        case MapIndex.CL23:
          ret = _currentCL23Map;
          break;
        default:
          ret = null;
          break;
      }
      return ret;
    }

    private void PutColorizedPointOnHeatMapGraph(int Amplitude, int X, int Y, bool hiRez)
    {
      for (var j = 0; j < _heatColors.Length; j++)
      {
        if (Amplitude <= _bins[j])
        {
          if (!hiRez && j == 0) //Cutoff for smallXY
            break;
          var chartPoint = new SeriesPoint(X, Y);
          chartPoint.Brush = _heatColors[j];

          _heatMapChart.AddXYPointToHeatMap(chartPoint, hiRez);
          break;
        }
      }
    }
  }
}