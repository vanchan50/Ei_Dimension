using System.Collections;
using System.Reflection;
using System.Text;
using DIOS.Core;
using Newtonsoft.Json;

namespace DIOS.Application;

/// <summary>
/// A class for output.
/// <br> Contains the region Reporter stats data per well</br>
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class WellStats : IEnumerable<RegionReporterStats>
{
  [JsonProperty("Well")]
  public readonly Well Well;

  public int TotalCount { get; }
  [JsonProperty("Results")]
  private readonly List<RegionReporterStats> _results = new(101);

  public WellStats(Well well, List<RegionReporterResultVolatile> results, int totalBead)
  {
    Well = new Well(well);
    foreach (var regionResult in results)
    {
      var stats = new RegionReporterStats(regionResult);
      _results.Add(stats);
    }
    _results.Sort((x, y) => x.Region.CompareTo(y.Region));
    TotalCount = totalBead;
  }

  public List<(int region, int medianFi)> GetReporterMeanFi()
  {
    var list = new List<(int region, int mfi)>(100);

    foreach (var regionReport in _results)
    {
      if (regionReport.Region == 0)
      {
        continue;
      }
      list.Add((regionReport.Region, (int)regionReport.MeanFi));
    }
    return list;
  }

  public override string ToString()
  {
    var bldr = new StringBuilder();
    bldr.AppendLine($"Well {Well.CoordinatesString()}:");
    foreach (var result in _results)
    {
      bldr.AppendLine(result.ToString());
    }
    return bldr.ToString();
  }

  public string ToStringLegacy(FieldInfo property, bool includeRegion0, IReadOnlySet<(int Number, string Name)> allRegionsInPlate)
  {
    var bldr = new StringBuilder();
    bldr.Append($"\"{Well.CoordinatesString()}\",");
    string Sample = "";
    bldr.Append($"\"{Sample}\",");
    foreach (var region in allRegionsInPlate)
    {
      var result = _results.Find(x => x.Region == region.Number);
      if (result is null)
      {
        bldr.Append("\"#\",");
        continue;
      }
      if (result.Region == 0 && !includeRegion0)
      {
        continue;
      }
      var value = property.GetValue(result);
      bldr.Append($"\"{value}\",");
    }
    bldr.Append($"\"{TotalCount}\",");
    string Notes = "";
    bldr.Append($"\"{Notes}\"");
    return bldr.ToString();
  }

  public IEnumerator<RegionReporterStats> GetEnumerator()
  {
    return ((IEnumerable<RegionReporterStats>)_results).GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return ((IEnumerable)_results).GetEnumerator();
  }
}