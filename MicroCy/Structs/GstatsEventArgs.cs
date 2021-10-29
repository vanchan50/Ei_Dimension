using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCy
{
  public class GstatsEventArgs
  {
    public List<Gstats> GStats { get; private set; }
    public GstatsEventArgs(List<Gstats> stats)
    {
      GStats = stats;
    }
  }
}
