using System.Collections.Generic;
using Ei_Dimension.Core;

namespace Ei_Dimension.Graphing.HeatMap;

internal class HeatMapDataContainer
{

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

  public void ClearData(bool current)
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

  public void Add((int x, int y) pointInClSpace, double[] bins, MapIndex mapIndex, bool current)
  {
    var dict = GetAccordingDictionary(mapIndex, current);
    if (dict == null)
      return;

    //TODO: preallocate all the points as (0,0); Mutate them instead of allocating new. (256x256 points max);
    //TODO: use fill counter, so you don't have to traverse (0,0) points that are left as extra
    if (!dict.ContainsKey(pointInClSpace))
    {
      var x = DataProcessor.FromCLSpaceToReal(pointInClSpace.x, bins);
      var y = DataProcessor.FromCLSpaceToReal(pointInClSpace.y, bins);
      var newPoint = new HeatMapPoint(x, y);
      dict.Add(pointInClSpace, newPoint);
      return;
    }
    dict[pointInClSpace].Amplitude++;
  }

  public Dictionary<(int x, int y), HeatMapPoint> GetAccordingDictionary(MapIndex mapIndex, bool current = true)
  {
    Dictionary<(int x, int y), HeatMapPoint> dict = null;
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
      }
    }
    return dict;
  }
}