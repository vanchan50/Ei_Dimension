using System.Collections.Generic;
using System.Linq;
using DIOS.Core;

namespace DIOS.Application
{
  public class WellResults
  {
    public Well Well { get; private set; }
    public BeadEventsData BeadEventsData { get; } = new BeadEventsData();
    public uint Size { get; private set; }
    private readonly List<RegionReporterResult> _reporterPerRegion = new List<RegionReporterResult>();
    private readonly SortedDictionary<int, int> _regionIndexDictionary = new SortedDictionary<int, int>();
    private readonly StatsAccumulator _calibrationStatsAccumulator = new StatsAccumulator();
    private readonly BackgroundStatsAccumulator _backgroundStatsAccumulator = new BackgroundStatsAccumulator();
    private int _non0RegionsCount;  //cached data for optimization. discard region 0 from calculation. only for minPerReg case

    internal void Reset(Well well, IReadOnlyCollection<int> regions)
    {
      Well = new Well(well);
      _reporterPerRegion.Clear();
      BeadEventsData.Reset();
      Size = 0;
      _calibrationStatsAccumulator.Reset();
      _backgroundStatsAccumulator.Reset();
      _regionIndexDictionary.Clear();
      foreach (var region in regions)
      {
        //skip region0 to make it the last one. if it is there at all
        if(region == 0)
          continue;
        _regionIndexDictionary.Add((ushort)region, _reporterPerRegion.Count);
        _reporterPerRegion.Add(new RegionReporterResult(region));
      }
      //region 0 has to be the last in _wellResults
      if (regions.Contains(0))
      {
        _regionIndexDictionary.Add(0, _reporterPerRegion.Count);
        _reporterPerRegion.Add(new RegionReporterResult(0));
      }
      _non0RegionsCount = _reporterPerRegion.Count;
      if (_non0RegionsCount != 0 && regions.Contains(0))
        _non0RegionsCount -= 1;
    }

    internal int Add(in ProcessedBead bead)
    {
      BeadEventsData.Add(in bead);
      Size++;
      //accum stats for run as a whole, used during aligment and QC
      _calibrationStatsAccumulator.Add(in bead);
      _backgroundStatsAccumulator.Add(in bead);
      //WellResults is a list of region numbers that are active
      //each entry has a list of rp1 values from each bead in that region
      if (_regionIndexDictionary.TryGetValue(bead.region, out var index))
      {
        _reporterPerRegion[index].Add(bead.reporter);
        return _reporterPerRegion[index].Count;
      }
      //return negative count, so the check is passed
      return -1;
    }
    
    internal List<RegionReporterResultVolatile> GetResultsClone()
    {
      var resultsCount = _reporterPerRegion.Count;
      var copy = new List<RegionReporterResultVolatile>(resultsCount);
      for (var i = 0; i < resultsCount; i++)
      {
        var r = new RegionReporterResultVolatile(_reporterPerRegion[i]);
        copy.Add(r);
      }
      return copy;
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
      int res = int.MaxValue;
      for (var i = 0; i < _non0RegionsCount; i++)
      {
        var diff = _reporterPerRegion[i].Count - minPerRegion;
        res = diff < res ? diff : res;
      }
      return res;
    }
  }
}