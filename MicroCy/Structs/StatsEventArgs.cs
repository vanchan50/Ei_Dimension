using System.Collections.Generic;

namespace DIOS.Core
{
  public class StatsEventArgs
  {
    public List<Gstats> GStats { get; }
    public List<double> AvgBg { get; }

    internal StatsEventArgs(BeadProcessor beadProcessor)
    {
      GStats = beadProcessor.Stats;
      AvgBg = beadProcessor.AvgBg;
    }
  }
}
