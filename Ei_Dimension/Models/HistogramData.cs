using Ei_Dimension.Core;

namespace Ei_Dimension.Models
{
  public class HistogramData : ObservableObject
  {
    public int Value
    {
      get => _value;
      set
      {
        _value = value;
        OnPropertyChanged();
      }
    }
    public int Argument
    {
      get => _argument;
      set
      {
        _argument = value;
        OnPropertyChanged();
      }
    }
    private int _value;
    private int _argument;
    public HistogramData(int val, int arg)
    {
      _value = val;
      _argument = arg;
    }
  }
}
