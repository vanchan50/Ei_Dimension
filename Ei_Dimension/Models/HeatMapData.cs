using Ei_Dimension.Core;
using System.Collections.Generic;

namespace Ei_Dimension.Models
{
  public class HeatMapData : ObservableObject
  {
    public int X {
      get => _x;
      set
      {
        _x = value;
        OnPropertyChanged();
      }
    }
    public int Y
    {
      get => _y;
      set
      {
        _y = value;
        OnPropertyChanged();
      }
    }
    public int A
    {
      get => _a;
      set
      {
        _a = value;
        OnPropertyChanged();
      }
    }
    private int _x;
    private int _y;
    private int _a;
    public static double[] bins { get; }
    public static Dictionary<(int x, int y), int> Dict { get; } = new Dictionary<(int x, int y), int>();
    public HeatMapData(int x, int y)
    {
      X = x;
      Y = y;
      _a = 0;
    }
    static HeatMapData()
    {
      bins = DataProcessor.GenerateLogSpaceD(1, 50000, 256);
    }
  }
}
