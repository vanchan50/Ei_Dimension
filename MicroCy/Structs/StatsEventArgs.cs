using System.Collections.Generic;

namespace DIOS.Core
{
  public class StatsEventArgs
  {
    public List<Gstats> GStats { get; }
    public List<double> AvgBg { get; }
    public StatsEventArgs(List<Gstats> stats = null, List<double> averageBackgrounds = null)
    {
      GStats = stats;
      AvgBg = averageBackgrounds;
    }
  }
}
