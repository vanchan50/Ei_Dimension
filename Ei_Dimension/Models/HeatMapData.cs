using Ei_Dimension.Core;

namespace Ei_Dimension.Models
{
  public class HeatMapData : ObservableObject
  {
    public int X { get; set; }
    public int Y { get; set; }
    public byte IntensityX { get; set; }
    public byte IntensityY { get; set; }
    public HeatMapData(int x, int y)
    {
      X = x;
      Y = y;
      IntensityX = 0;
      IntensityY = 0;
    }
  }
}
