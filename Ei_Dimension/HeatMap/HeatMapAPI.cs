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

    private Dictionary<(int x, int y), HeatMapPoint> DisplayedMap { get; set; }
    private readonly Dictionary<(int x, int y), HeatMapPoint> _currentCL01Dict = new Dictionary<(int x, int y), HeatMapPoint>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), HeatMapPoint> _currentCL02Dict = new Dictionary<(int x, int y), HeatMapPoint>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), HeatMapPoint> _currentCL03Dict = new Dictionary<(int x, int y), HeatMapPoint>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), HeatMapPoint> _currentCL12Dict = new Dictionary<(int x, int y), HeatMapPoint>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), HeatMapPoint> _currentCL13Dict = new Dictionary<(int x, int y), HeatMapPoint>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), HeatMapPoint> _currentCL23Dict = new Dictionary<(int x, int y), HeatMapPoint>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), HeatMapPoint> _backingCL01Dict = new Dictionary<(int x, int y), HeatMapPoint>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), HeatMapPoint> _backingCL02Dict = new Dictionary<(int x, int y), HeatMapPoint>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), HeatMapPoint> _backingCL03Dict = new Dictionary<(int x, int y), HeatMapPoint>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), HeatMapPoint> _backingCL12Dict = new Dictionary<(int x, int y), HeatMapPoint>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), HeatMapPoint> _backingCL13Dict = new Dictionary<(int x, int y), HeatMapPoint>(XYMAPCAPACITY);
    private readonly Dictionary<(int x, int y), HeatMapPoint> _backingCL23Dict = new Dictionary<(int x, int y), HeatMapPoint>(XYMAPCAPACITY);
    public const int XYMAPCAPACITY = 50000;  //max possible capacity is 256x256. Realistic 3/4 is ~49k

    private readonly double[] _bins;
    private readonly SolidColorBrush[] _heatColors;
    private static HeatMapAPI _instance;
    private IHeatMapChart _heatMapChart;
    private bool _chartAssigned;

    private HeatMapAPI()
    {
      DisplayedMap = _currentCL12Dict;

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
        _currentCL01Dict.Clear();
        _currentCL02Dict.Clear();
        _currentCL03Dict.Clear();
        _currentCL12Dict.Clear();
        _currentCL13Dict.Clear();
        _currentCL23Dict.Clear();
      }
      else
      {
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
      Dictionary<(int x, int y), HeatMapPoint> dict;

      if (current)
      {
        switch (mapIndex)
        {
          case MapIndex.CL01:
            dict = _currentCL01Dict;
            break;
          case MapIndex.CL02:
            dict = _currentCL02Dict;
            break;
          case MapIndex.CL03:
            dict = _currentCL03Dict;
            break;
          case MapIndex.CL12:
            dict = _currentCL12Dict;
            break;
          case MapIndex.CL13:
            dict = _currentCL13Dict;
            break;
          case MapIndex.CL23:
            dict = _currentCL23Dict;
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
            break;
          case MapIndex.CL02:
            dict = _backingCL02Dict;
            break;
          case MapIndex.CL03:
            dict = _backingCL03Dict;
            break;
          case MapIndex.CL12:
            dict = _backingCL12Dict;
            break;
          case MapIndex.CL13:
            dict = _backingCL13Dict;
            break;
          case MapIndex.CL23:
            dict = _backingCL23Dict;
            break;
          default:
            return;
        }
      }
      //TODO: preallocate all the points as (0,0); Mutate them instead of allocating new (256x256 points max);
      //TODO: use fill counter, so you don't have to traverse (0,0) points that are left as extra
      if (!dict.ContainsKey(pointInClSpace))
      {
        var newPoint = new HeatMapPoint((int)bins[pointInClSpace.x], (int)bins[pointInClSpace.y]);
        dict.Add(pointInClSpace, newPoint);
        return;
      }
      dict[pointInClSpace].Amplitude++;
    }

    public void Display(MapIndex mapIndex, bool current = true)
    {
      if (current)
      {
        switch (mapIndex)
        {
          case MapIndex.CL01:
            DisplayedMap = _currentCL01Dict;
            break;
          case MapIndex.CL02:
            DisplayedMap = _currentCL02Dict;
            break;
          case MapIndex.CL03:
            DisplayedMap = _currentCL03Dict;
            break;
          case MapIndex.CL12:
            DisplayedMap = _currentCL12Dict;
            break;
          case MapIndex.CL13:
            DisplayedMap = _currentCL13Dict;
            break;
          case MapIndex.CL23:
            DisplayedMap = _currentCL23Dict;
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
            DisplayedMap = _backingCL01Dict;
            break;
          case MapIndex.CL02:
            DisplayedMap = _backingCL02Dict;
            break;
          case MapIndex.CL03:
            DisplayedMap = _backingCL03Dict;
            break;
          case MapIndex.CL12:
            DisplayedMap = _backingCL12Dict;
            break;
          case MapIndex.CL13:
            DisplayedMap = _backingCL13Dict;
            break;
          case MapIndex.CL23:
            DisplayedMap = _backingCL23Dict;
            break;
          default:
            DisplayedMap = null;
            break;
        }
      }
    }

    public void AnalyzeHeatMap(bool hiRez = false)
    {
      if (DisplayedMap == null)
        return;

      var heatMapList = DisplayedMap.Values; //DisplayedMap May change whenever, caching here is necessary
      if (heatMapList.Count > 0)
      {
        int max = heatMapList.Select(point => point.Amplitude).Max();

        DataProcessor.GenerateLogSpaceD(1, max + 1, _heatColors.Length, _bins, true);
        _heatMapChart.ClearHeatMaps();
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
          PutColorizedPointOnHeatMapGraph(heatMapPoint.Amplitude, X, Y, hiRez);
        }
      }
      else if (heatMapList.Count == 0)
      {
        _heatMapChart.ClearHeatMaps();
      }
    }

    public List<HeatMapPoint> GetCache(MapIndex mapIndex)
    {
      Dictionary<(int x, int y), HeatMapPoint> ret;
      switch (mapIndex)
      {
        case MapIndex.CL01:
          ret = _currentCL01Dict;
          break;
        case MapIndex.CL02:
          ret = _currentCL02Dict;
          break;
        case MapIndex.CL03:
          ret = _currentCL03Dict;
          break;
        case MapIndex.CL12:
          ret = _currentCL12Dict;
          break;
        case MapIndex.CL13:
          ret = _currentCL13Dict;
          break;
        case MapIndex.CL23:
          ret = _currentCL23Dict;
          break;
        default:
          ret = null;
          break;
      }
      return ret.Values.ToList();
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