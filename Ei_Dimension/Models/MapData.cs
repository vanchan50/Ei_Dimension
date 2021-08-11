using Ei_Dimension.Core;

namespace Ei_Dimension.Models
{
  public class MapData : ObservableObject
  {
    public int X { get; set; }
    public int Y { get; set; }
    public MapData(int x, int y)
    {
      X = x;
      Y = y;
    }
  }
}
