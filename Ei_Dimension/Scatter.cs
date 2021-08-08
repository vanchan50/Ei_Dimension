using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ei_Dimension
{
  public class Scatter
  {
    public int Wavelength { get; set; }
    public int Intensity { get; set; }
    public Scatter(int wl, int intens)
    {
      Wavelength = wl;
      Intensity = intens;
    }
  }
}
