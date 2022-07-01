using System.Collections.Generic;

namespace DIOS.Core
{
  public class StatsEventArgs
  {
    public ChannelsCalibrationStats Stats { get; }
    public ChannelsAveragesStats BgStats { get; }

    internal StatsEventArgs(ChannelsCalibrationStats stats, ChannelsAveragesStats avg)
    {
      Stats = stats;
      BgStats = avg;
    }
  }
}
