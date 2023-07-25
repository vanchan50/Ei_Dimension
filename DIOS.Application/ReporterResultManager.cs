using System;
using System.Collections.Generic;
using DIOS.Core;

namespace DIOS.Application;

public class ReporterResultManager
{
  public int RegionsCount { get; private set; }
  public IReadOnlyCollection<int> CurrentActiveRegions { get; private set; }

  private readonly List<RegionReporterResult> _reporterPerRegion = new List<RegionReporterResult>();
  private readonly SortedDictionary<int, int> _regionIndexDictionary = new SortedDictionary<int, int>();//region,position
  private int _non0RegionsCount;  //cached data for optimization. discard region 0 from calculation. only for minPerReg case

  public void Reset(IReadOnlyCollection<int> regions)
  {
    _reporterPerRegion.Clear();
    _regionIndexDictionary.Clear();
    CurrentActiveRegions = regions;

    bool containsRegion0 = false;

    foreach (var region in regions)
    {
      //skip region0 to make it the last one. if it is there at all
      if (region == 0)
      {
        containsRegion0 = true;
        continue;
      }
      _regionIndexDictionary.Add((ushort)region, _reporterPerRegion.Count);
      _reporterPerRegion.Add(new RegionReporterResult(region));
    }
    //region 0 has to be the last in _wellResults
    if (containsRegion0)
    {
      _regionIndexDictionary.Add(0, _reporterPerRegion.Count);
      _reporterPerRegion.Add(new RegionReporterResult(0));
    }
    RegionsCount = _reporterPerRegion.Count;

    _non0RegionsCount = 0;
    if (RegionsCount != 0 && containsRegion0)
      _non0RegionsCount = RegionsCount - 1;
  }

  /// <summary>
  /// Adds reporter value to the corresponding region.
  /// <br>If the bead region had not been setup, the value is discarded</br> 
  /// </summary>
  /// <param name="bead"></param>
  /// <returns></returns>
  public int Add(in ProcessedBead bead)
  {
    if (_regionIndexDictionary.TryGetValue(bead.region, out var index))
    {
      _reporterPerRegion[index].Add(bead.reporter);
      return _reporterPerRegion[index].Count;
    }
    //return negative count, so the check is passed
    return -1;
  }

  public RegionReporterResult GetReporterResultsByRegion(int region)
  {
    if (_regionIndexDictionary.TryGetValue(region, out var index))
    {
      return _reporterPerRegion[index];
    }
    throw new IndexOutOfRangeException($"Region #{region} is not set up");
  }

  internal List<RegionReporterResultVolatile> GetResultsClone()
  {
    var copy = new List<RegionReporterResultVolatile>(RegionsCount);
    for (var i = 0; i < RegionsCount; i++)
    {
      var r = new RegionReporterResultVolatile(_reporterPerRegion[i]);
      copy.Add(r);
    }
    return copy;
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