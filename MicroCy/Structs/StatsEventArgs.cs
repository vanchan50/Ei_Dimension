using System.Collections.Generic;

namespace DIOS.Core
{
  public class StatsEventArgs
  {
    public List<Gstats> GStats { get; }
    public List<double> AvgBg { get; }

    internal StatsEventArgs(List<Gstats> stats, List<double> avg)
    {
      GStats = stats;
      AvgBg = avg;
    }
  }
}
