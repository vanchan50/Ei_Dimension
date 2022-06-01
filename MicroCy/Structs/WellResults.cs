using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIOS.Core.Structs
{
  public class WellResults
  {
    private Well _well;
    private List<RegionResult> _wellResults = new List<RegionResult>();
    private SortedDictionary<ushort, int> _regionIndexDictionary = new SortedDictionary<ushort, int>();
    private int _minPerRegCount;  //discard region 0 from calculation

    internal void Reset(Well well, ICollection<int> regions, bool includeRegion0)
    {
      _well = new Well(well);
      _wellResults.Clear();
      _regionIndexDictionary.Clear();
      foreach (var region in regions)
      {
        _regionIndexDictionary.Add((ushort)region, _wellResults.Count);
        _wellResults.Add(new RegionResult { regionNumber = (ushort)region });
      }
      //region 0 has to be the last in _wellResults
      if (includeRegion0)  //TODO: remove the IF from here and don't damage the rest of reg0stats logic
      {
        _regionIndexDictionary.Add(0, _wellResults.Count);
        _wellResults.Add(new RegionResult { regionNumber = 0 });
      }
      _minPerRegCount = regions.Count;
      if (_minPerRegCount != 0 && includeRegion0)
        _minPerRegCount -= 1;
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
      throw new Exception($"WellResults.Add() could not find region {outBead.region}");
    }

    //TODO:bad design to have it public. get rid somehow
    internal List<RegionResult> MakeDeepCopy()
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
      for (var i = 0; i < _minPerRegCount; i++)
      {
        var diff = _wellResults[i].ReporterValues.Count - minPerRegion;
        res = diff < res ? diff : res;
      }
      return res;
    }
  }
}
