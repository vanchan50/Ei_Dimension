using Ei_Dimension.Core;

namespace Ei_Dimension.Models
{
  public class HeatMapData : ObservableObject
  {
    public int X { get; set; }
    public int Y { get; set; }
    public byte intensity { get; set; }
    public HeatMapData(int x, int y)
    {
      X = x;
      Y = y;
      intensity = 0;
    }
  }
}
