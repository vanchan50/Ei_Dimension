using System;
using System.Collections.Generic;

namespace DIOS.Core.Structs
{
  internal class WellResults
  {
    public Well Well { get; private set; }
    private readonly List<RegionResult> _wellResults = new List<RegionResult>();
    private readonly SortedDictionary<ushort, int> _regionIndexDictionary = new SortedDictionary<ushort, int>();
    private int _non0RegionsCount;  //cached data for optimization. discard region 0 from calculation. only for minPerReg case

    internal void Reset(Well well, ICollection<int> regions)
    {
      Well = new Well(well);
      _wellResults.Clear();
      _regionIndexDictionary.Clear();
      foreach (var region in regions)
      {
        //skip region0 to make it the last one. if it is there at all
        if(region == 0)
          continue;
        _regionIndexDictionary.Add((ushort)region, _wellResults.Count);
        _wellResults.Add(new RegionResult { regionNumber = (ushort)region });
      }
      //region 0 has to be the last in _wellResults
      if (regions.Contains(0))
      {
        _regionIndexDictionary.Add(0, _wellResults.Count);
        _wellResults.Add(new RegionResult { regionNumber = 0 });
      }
      _non0RegionsCount = _wellResults.Count;
      if (_non0RegionsCount != 0 && regions.Contains(0))
        _non0RegionsCount -= 1;
    }

    internal int Add(in BeadInfoStruct outBead)
    {
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
    
    internal List<RegionResult> GetResults()
    {
      //TODO: cache previous copy per well somehow, dont make new allocation all the time
      var copy = new List<RegionResult>(_wellResults.Count);
      for (var i = 0; i < _wellResults.Count; i++)
      {
        var r = new RegionResult();
        r.regionNumber = _wellResults[i].regionNumber;
        var count = _wellResults[i].ReporterValues.Count < RegionResult.CAPACITY ? _wellResults[i].ReporterValues.Count : RegionResult.CAPACITY;
        for (var j = 0; j < count; j++)
        {
          r.ReporterValues.Add(_wellResults[i].ReporterValues[j]);
        }
        copy.Add(r);
      }
      return copy;
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
