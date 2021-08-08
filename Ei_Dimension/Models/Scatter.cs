using Ei_Dimension.Core;

namespace Ei_Dimension.Models
{
  public class Scatter : ObservableObject
  {
    public int Wavelength
    {
      get => _wavelength;
      set
      {
        _wavelength = value;
        OnPropertyChanged();
      }
    }
    public int Intensity
    {
      get => _intensity;
      set
      {
        _intensity = value;
        OnPropertyChanged();
      }
    }
    private int _wavelength;
    private int _intensity;
    public Scatter(int wl, int intens)
    {
      Wavelength = wl;
      Intensity = intens;
    }

  }
}
