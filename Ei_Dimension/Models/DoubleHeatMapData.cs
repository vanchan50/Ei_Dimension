using Ei_Dimension.Core;
using System.Collections.Generic;

namespace Ei_Dimension.Models
{
  public class DoubleHeatMapData : ObservableObject
  {
    public double X
    {
      get => _x;
      set
      {
        _x = value;
        OnPropertyChanged();
      }
    }
    public double Y
    {
      get => _y;
      set
      {
        _y = value;
        OnPropertyChanged();
      }
    }
    public double A
    {
      get => _a;
      set
      {
        _a = value;
        OnPropertyChanged();
      }
    }
    private double _x;
    private double _y;
    private double _a;
    public static double[] bins { get; }
    public DoubleHeatMapData(double x, double y, double a)
    {
      X = x;
      Y = y;
      A = a;
    }
  }
}
