using DIOS.Core;

namespace DIOS.Application;

public class WellResults
{
  public Well Well { get; private set; }
  private readonly ReporterResultManager _reporterManager = new ();
  private readonly StatsAccumulator _calibrationStatsAccumulator = new ();
  private readonly BackgroundStatsAccumulator _backgroundStatsAccumulator = new ();
  private readonly ResultingWellStatsData _measuredWellStats = new();

  internal void Reset(Well well)
  {
    Well = new Well(well);
    _calibrationStatsAccumulator.Reset();
    _backgroundStatsAccumulator.Reset();
    _reporterManager.Reset(well.ActiveRegions);
    _measuredWellStats.Reset();
  }

  internal int Add(in ProcessedBead bead)
  {
    //accum stats for run as a whole, used during aligment and QC
    _calibrationStatsAccumulator.Add(in bead);
    _backgroundStatsAccumulator.Add(in bead);
    //WellResults is a list of region numbers that are active
    //each entry has a list of rp1 values from each bead in that region
    return _reporterManager.Add(in bead);
  }

  internal void AddWellStats(WellStats stats)
  {
    _measuredWellStats.Add(stats.ToString());
  }
    
  internal List<RegionReporterResultVolatile> GetResultsClone()
  {
    return _reporterManager.GetResultsClone();
  }

  public ChannelsCalibrationStats GetChannelStats()
  {
    return _calibrationStatsAccumulator.CalculateStats();
  }

  public ChannelsAveragesStats GetBackgroundChannelsAverages()
  {
    return _backgroundStatsAccumulator.CalculateAverages();
  }

  public string PublishWellStats()
  {
    return _measuredWellStats.Publish();
  }

  /// <summary>
  /// Checks if MinPerRegion Condition is met. Not thread safe. Supposed to be called after the well is read or in the measurement sequence thread
  /// </summary>
  /// <returns>A positive number or 0, if MinPerRegions is met; otherwise returns a negative number of lacking beads</returns>
  internal int MinPerAllRegionsAchieved(int minPerRegion)
  {
    return _reporterManager.MinPerAllRegionsAchieved(minPerRegion);
  }
}