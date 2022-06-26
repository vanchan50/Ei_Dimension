using System.Collections.Generic;

namespace DIOS.Core.Structs
{
  internal class MeasurementResults
  {
    public Well Well { get; private set; }
    public BeadEventsData BeadEventsData { get; } = new BeadEventsData();
    private readonly List<RegionReporterResult> _wellResults = new List<RegionReporterResult>();
    private readonly SortedDictionary<ushort, int> _regionIndexDictionary = new SortedDictionary<ushort, int>();
    private readonly StatsAccumulator _statsAccumulator = new StatsAccumulator();
    private int _non0RegionsCount;  //cached data for optimization. discard region 0 from calculation. only for minPerReg case

    internal void Reset(Well well, ICollection<int> regions)
    {
      Well = new Well(well);
      _wellResults.Clear();
      BeadEventsData.Reset();
      _statsAccumulator.Reset();
      _regionIndexDictionary.Clear();
      foreach (var region in regions)
      {
        //skip region0 to make it the last one. if it is there at all
        if(region == 0)
          continue;
        _regionIndexDictionary.Add((ushort)region, _wellResults.Count);
        _wellResults.Add(new RegionReporterResult { regionNumber = (ushort)region });
      }
      //region 0 has to be the last in _wellResults
      if (regions.Contains(0))
      {
        _regionIndexDictionary.Add(0, _wellResults.Count);
        _wellResults.Add(new RegionReporterResult { regionNumber = 0 });
      }
      _non0RegionsCount = _wellResults.Count;
      if (_non0RegionsCount != 0 && regions.Contains(0))
        _non0RegionsCount -= 1;
    }

    internal int Add(in BeadInfoStruct outBead)
    {
      BeadEventsData.Add(in outBead);
      //accum stats for run as a whole, used during aligment and QC
      _statsAccumulator.Add(in outBead);
      //WellResults is a list of region numbers that are active
      //each entry has a list of rp1 values from each bead in that region
      if (_regionIndexDictionary.TryGetValue(outBead.region, out var index))
      {
        _wellResults[index].ReporterValues.Add(outBead.reporter);
        return _wellResults[index].ReporterValues.Count;
      }
      //return negative count, so the check is passed
      return -1;
    }
    
    internal List<RegionReporterResultVolatile> GetResults()
    {
      var resultsCount = _wellResults.Count;
      var copy = new List<RegionReporterResultVolatile>(resultsCount);
      for (var i = 0; i < resultsCount; i++)
      {
        var r = new RegionReporterResultVolatile(_wellResults[i]);
        copy.Add(r);
      }
      return copy;
    }

    internal CalibrationStats GetStats()
    {
      return _statsAccumulator.CalculateStats();
    }

    internal BackgroundStats GetBackgroundAverages()
    {
      return _statsAccumulator.CalculateBackgroundAverages();
    }
    
    internal int MinPerAllRegionsAchieved(int minPerRegion)
    {
      int res = int.MaxValue;
      for (var i = 0; i < _non0RegionsCount; i++)
      {
        var diff = _wellResults[i].ReporterValues.Count - minPerRegion;
        res = diff < res ? diff : res;
      }
      return res;
    }
  }
}
