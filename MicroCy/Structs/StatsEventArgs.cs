using System.Collections.Generic;

namespace DIOS.Core
{
  public class StatsEventArgs
  {
    public CalibrationStats Stats { get; }
    public AveragesStats BgStats { get; }

    internal StatsEventArgs(CalibrationStats stats, AveragesStats avg)
    {
      Stats = stats;
      BgStats = avg;
    }
  }
}
