using System.Collections.Generic;
using DIOS.Core;

namespace DIOS.Application
{
  public class WellResults
  {
    public Well Well { get; private set; }
    internal BeadEventsData BeadEventsData { get; } = new BeadEventsData();
    private readonly ReporterResultManager _reporterManager = new ReporterResultManager();
    private readonly StatsAccumulator _calibrationStatsAccumulator = new StatsAccumulator();
    private readonly BackgroundStatsAccumulator _backgroundStatsAccumulator = new BackgroundStatsAccumulator();
    private uint _size;
    private int _nextNewBeadIndex;

    internal void Reset(Well well, IReadOnlyCollection<int> regions)
    {
      Well = new Well(well);
      BeadEventsData.Reset();
      _size = 0;
      _nextNewBeadIndex = 0;
      _calibrationStatsAccumulator.Reset();
      _backgroundStatsAccumulator.Reset();
      _reporterManager.Reset(regions);
    }

    internal int Add(in ProcessedBead bead)
    {
      BeadEventsData.Add(in bead);
      _size++;
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

    public IEnumerable<ProcessedBead> GetNewBeads()
    {
      if (_size == 0)
      {
        yield break;
      }

      if (_nextNewBeadIndex >= _size)
      {
        yield break;
      }

      yield return BeadEventsData[_nextNewBeadIndex++];
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
}