using Ei_Dimension.Core;

namespace Ei_Dimension.Models
{
  public class HistogramData<T1, T2> : ObservableObject
  {
    public T1 Value
    {
      get => _value;
      set
      {
        _value = value;
        OnPropertyChanged();
      }
    }
    public T2 Argument
    {
      get => _argument;
      set
      {
        _argument = value;
        OnPropertyChanged();
      }
    }
    private T1 _value;
    private T2 _argument;
    public HistogramData(T1 val, T2 arg)
    {
      _value = val;
      _argument = arg;
    }
  }
}
