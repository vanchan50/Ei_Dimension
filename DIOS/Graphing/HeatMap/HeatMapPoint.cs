using Ei_Dimension.Core;

namespace Ei_Dimension.Graphing.HeatMap
{
  public class HeatMapPoint : ObservableObject
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
    public int Amplitude
    {
      get => _amplitude;
      set
      {
        _amplitude = value;
        OnPropertyChanged();
      }
    }

    public int Region { get; } = -1;
    private int _x;
    private int _y;
    private int _amplitude;
    public static double[] bins { get; }
    public static double[] HiRezBins { get; }
    public static double[] HalfPrecisionBins { get; }
    public HeatMapPoint(int x, int y)
    {
      X = x;
      Y = y;
      _amplitude = 0;
    }

    public HeatMapPoint(int x, int y, int region) : this(x,y)
    {
      Region = region;
    }

    static HeatMapPoint()
    {
      bins = new double[256];
      DataProcessor.GenerateLogSpaceD(1, 60000, 256, bins);
      HiRezBins = new double[ViewModels.ResultsViewModel.HIREZDEFINITION];
      DataProcessor.GenerateLogSpaceD(1, 60000, ViewModels.ResultsViewModel.HIREZDEFINITION, HiRezBins);
      HalfPrecisionBins = new double[256 / 2];
      DataProcessor.GenerateLogSpaceD(1, 60000, 256/2, HalfPrecisionBins);
    }
  }
}
