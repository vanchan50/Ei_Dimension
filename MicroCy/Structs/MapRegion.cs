using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCy
{
  [Serializable]
  public class MapRegion
  {
    public int Number;
    public double ValidationTargetReporter;
    public bool isValidator;
    public (int x, int y) Center; //coords in 256x256 space
    public List<(int x, int y)> Points; //contains coords in 256x256 space for region numbers
  }
}