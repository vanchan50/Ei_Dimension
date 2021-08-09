using Ei_Dimension.Core;

namespace Ei_Dimension.Models
{
  public class HistogramData : ObservableObject
  {
    public int Value { get; set; }
    public int Argument { get; set; }
    public HistogramData(int val, int arg)
    {
      Value = val;
      Argument = arg;
    }
  }
}
