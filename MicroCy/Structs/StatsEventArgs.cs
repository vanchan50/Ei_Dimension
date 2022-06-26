using System.Collections.Generic;

namespace DIOS.Core
{
  public class StatsEventArgs
  {
    public CalibrationStats Stats { get; }
    public BackgroundStats BgStats { get; }

    internal StatsEventArgs(CalibrationStats stats, BackgroundStats avg)
    {
      Stats = stats;
      BgStats = avg;
    }
  }
}
