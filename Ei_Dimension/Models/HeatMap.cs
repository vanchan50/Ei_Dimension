using System.Collections.Generic;

namespace Ei_Dimension.Models
{
  internal class HeatMap
  {
    public static List<HeatMapData> DisplayedMap { get; set; }
    private static List<HeatMapData> CurrentCL01Map { get;}
    private static List<HeatMapData> CurrentCL02Map { get;}
    private static List<HeatMapData> CurrentCL03Map { get;}
    private static List<HeatMapData> CurrentCL12Map { get;}
    private static List<HeatMapData> CurrentCL13Map { get;}
    private static List<HeatMapData> CurrentCL23Map { get;}
    private static List<HeatMapData> BackingCL01Map { get;}
    private static List<HeatMapData> BackingCL02Map { get;}
    private static List<HeatMapData> BackingCL03Map { get;}
    private static List<HeatMapData> BackingCL12Map { get;}
    private static List<HeatMapData> BackingCL13Map { get;}
    private static List<HeatMapData> BackingCL23Map { get;}
    private static Dictionary<(int x, int y), int> CurrentCL01Dict { get;}
    private static Dictionary<(int x, int y), int> CurrentCL02Dict { get;}
    private static Dictionary<(int x, int y), int> CurrentCL03Dict { get;}
    private static Dictionary<(int x, int y), int> CurrentCL12Dict { get;}
    private static Dictionary<(int x, int y), int> CurrentCL13Dict { get;}
    private static Dictionary<(int x, int y), int> CurrentCL23Dict { get;}
    private static Dictionary<(int x, int y), int> BackingCL01Dict { get;}
    private static Dictionary<(int x, int y), int> BackingCL02Dict { get;}
    private static Dictionary<(int x, int y), int> BackingCL03Dict { get;}
    private static Dictionary<(int x, int y), int> BackingCL12Dict { get;}
    private static Dictionary<(int x, int y), int> BackingCL13Dict { get;}
    private static Dictionary<(int x, int y), int> BackingCL23Dict { get;}
    public const int XYMAPCAPACITY = 50000;  //max possible capacity is 256x256. Realistic 3/4 is ~49k

    static HeatMap()
    {
      CurrentCL01Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
      CurrentCL02Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
      CurrentCL03Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
      CurrentCL12Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
      CurrentCL13Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
      CurrentCL23Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
      BackingCL01Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
      BackingCL02Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
      BackingCL03Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
      BackingCL12Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
      BackingCL13Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);
      BackingCL23Dict = new Dictionary<(int x, int y), int>(XYMAPCAPACITY);


      CurrentCL01Map = new List<HeatMapData>(XYMAPCAPACITY);
      CurrentCL02Map = new List<HeatMapData>(XYMAPCAPACITY);
      CurrentCL03Map = new List<HeatMapData>(XYMAPCAPACITY);
      CurrentCL12Map = new List<HeatMapData>(XYMAPCAPACITY);
      CurrentCL13Map = new List<HeatMapData>(XYMAPCAPACITY);
      CurrentCL23Map = new List<HeatMapData>(XYMAPCAPACITY);
      BackingCL01Map = new List<HeatMapData>(XYMAPCAPACITY);
      BackingCL02Map = new List<HeatMapData>(XYMAPCAPACITY);
      BackingCL03Map = new List<HeatMapData>(XYMAPCAPACITY);
      BackingCL12Map = new List<HeatMapData>(XYMAPCAPACITY);
      BackingCL13Map = new List<HeatMapData>(XYMAPCAPACITY);
      BackingCL23Map = new List<HeatMapData>(XYMAPCAPACITY);

      DisplayedMap = CurrentCL12Map;
    }

    public static void Clear(bool current = true)
    {
      if (current)
      {
        CurrentCL01Map.Clear();
        CurrentCL02Map.Clear();
        CurrentCL03Map.Clear();
        CurrentCL12Map.Clear();
        CurrentCL13Map.Clear();
        CurrentCL23Map.Clear();
        CurrentCL01Dict.Clear();
        CurrentCL02Dict.Clear();
        CurrentCL03Dict.Clear();
        CurrentCL12Dict.Clear();
        CurrentCL13Dict.Clear();
        CurrentCL23Dict.Clear();
      }
      else
      {
        BackingCL01Map.Clear();
        BackingCL02Map.Clear();
        BackingCL03Map.Clear();
        BackingCL12Map.Clear();
        BackingCL13Map.Clear();
        BackingCL23Map.Clear();
        BackingCL01Dict.Clear();
        BackingCL02Dict.Clear();
        BackingCL03Dict.Clear();
        BackingCL12Dict.Clear();
        BackingCL13Dict.Clear();
        BackingCL23Dict.Clear();
      }
    }

    public static void AddPoint((int x, int y) point, double[] bins, MapIndex mapIndex, bool current = true)
    {
      Dictionary<(int x, int y), int> dict;
      List<HeatMapData> map;

      if (current)
      {
        switch (mapIndex)
        {
          case MapIndex.CL01:
            dict = CurrentCL01Dict;
            map = CurrentCL01Map;
            break;
          case MapIndex.CL02:
            dict = CurrentCL02Dict;
            map = CurrentCL02Map;
            break;
          case MapIndex.CL03:
            dict = CurrentCL03Dict;
            map = CurrentCL03Map;
            break;
          case MapIndex.CL12:
            dict = CurrentCL12Dict;
            map = CurrentCL12Map;
            break;
          case MapIndex.CL13:
            dict = CurrentCL13Dict;
            map = CurrentCL13Map;
            break;
          case MapIndex.CL23:
            dict = CurrentCL23Dict;
            map = CurrentCL23Map;
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
            dict = BackingCL01Dict;
            map = BackingCL01Map;
            break;
          case MapIndex.CL02:
            dict = BackingCL02Dict;
            map = BackingCL02Map;
            break;
          case MapIndex.CL03:
            dict = BackingCL03Dict;
            map = BackingCL03Map;
            break;
          case MapIndex.CL12:
            dict = BackingCL12Dict;
            map = BackingCL12Map;
            break;
          case MapIndex.CL13:
            dict = BackingCL13Dict;
            map = BackingCL13Map;
            break;
          case MapIndex.CL23:
            dict = BackingCL23Dict;
            map = BackingCL23Map;
            break;
          default:
            return;
        }
      }

      if (!dict.ContainsKey(point))
      {
        dict.Add(point, map.Count);
        map.Add(new HeatMapData((int)bins[point.x], (int)bins[point.y]));
      }
      else
      {
        map[dict[point]].A++;
      }
    }

    public static void Display(MapIndex mapIndex, bool current = true)
    {
      if (current)
      {
        switch (mapIndex)
        {
          case MapIndex.CL01:
            DisplayedMap = CurrentCL01Map;
            break;
          case MapIndex.CL02:
            DisplayedMap = CurrentCL02Map;
            break;
          case MapIndex.CL03:
            DisplayedMap = CurrentCL03Map;
            break;
          case MapIndex.CL12:
            DisplayedMap = CurrentCL12Map;
            break;
          case MapIndex.CL13:
            DisplayedMap = CurrentCL13Map;
            break;
          case MapIndex.CL23:
            DisplayedMap = CurrentCL23Map;
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
            DisplayedMap = BackingCL01Map;
            break;
          case MapIndex.CL02:
            DisplayedMap = BackingCL02Map;
            break;
          case MapIndex.CL03:
            DisplayedMap = BackingCL03Map;
            break;
          case MapIndex.CL12:
            DisplayedMap = BackingCL12Map;
            break;
          case MapIndex.CL13:
            DisplayedMap = BackingCL13Map;
            break;
          case MapIndex.CL23:
            DisplayedMap = BackingCL23Map;
            break;
          default:
            DisplayedMap = null;
            break;
        }
      }
    }

    public static List<HeatMapData> GetCache(MapIndex mapIndex)
    {
      List<HeatMapData> ret;
      switch (mapIndex)
      {
        case MapIndex.CL01:
          ret = CurrentCL01Map;
          break;
        case MapIndex.CL02:
          ret = CurrentCL02Map;
          break;
        case MapIndex.CL03:
          ret = CurrentCL03Map;
          break;
        case MapIndex.CL12:
          ret = CurrentCL12Map;
          break;
        case MapIndex.CL13:
          ret = CurrentCL13Map;
          break;
        case MapIndex.CL23:
          ret = CurrentCL23Map;
          break;
        default:
          ret = null;
          break;
      }
      return ret;
    }
  }
}