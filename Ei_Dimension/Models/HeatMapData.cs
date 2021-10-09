using Ei_Dimension.Core;

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
    public byte IntensityX { get; set; }
    public byte IntensityY { get; set; }
    private int _x;
    private int _y;
    public HeatMapData(int x, int y)
    {
      X = x;
      Y = y;
      IntensityX = 0;
      IntensityY = 0;
    }
  }
}
