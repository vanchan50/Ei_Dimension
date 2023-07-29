using DIOS.Core;

namespace DIOS.Application;

public class WellResults
{
  public Well Well { get; private set; }
  private readonly ReporterResultManager _reporterManager = new ();
  private readonly StatsAccumulator _calibrationStatsAccumulator = new ();
  private readonly BackgroundStatsAccumulator _backgroundStatsAccumulator = new ();

  internal void Reset(Well well, IReadOnlyCollection<int> regions)
  {
    Well = new Well(well);
    _calibrationStatsAccumulator.Reset();
    _backgroundStatsAccumulator.Reset();
    _reporterManager.Reset(regions);
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
    
  internal List<RegionReporterResultVolatile> GetResultsClone()
  {
    return _reporterManager.GetResultsClone();
  }

  public ChannelsCalibrationStats GetStats()
  {
    return _calibrationStatsAccumulator.CalculateStats();
  }

  public ChannelsAveragesStats GetBackgroundAverages()
  {
    return _backgroundStatsAccumulator.CalculateAverages();
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