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
    public int Region { get; }
    private int _x;
    private int _y;
    private int _a;
    public static double[] bins { get; }
    public HeatMapData(int x, int y, int r = -1)
    {
      X = x;
      Y = y;
      _a = 0;
      Region = r;
    }
    static HeatMapData()
    {
      bins = DataProcessor.GenerateLogSpaceD(1, 50000, 256);
    }
  }
}
