using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DIOS.Core;
using Newtonsoft.Json;

namespace DIOS.Application;

/// <summary>
/// A class for output.
/// <br> Contains the region Reporter stats data per well</br>
/// </summary>
[JsonObject(MemberSerialization.Fields)]
internal class WellStats
{
  [JsonProperty("Well")]
  public Well Well { get; }
  [JsonIgnore]
  public int TotalCount { get; }

  [JsonProperty("Results")]
  private readonly List<RegionReporterStats> _results = new List<RegionReporterStats>(101);

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

  public List<(int region, int mfi)> GetReporterMFI()
  {
    List<(int region, int mfi)> list = new List<(int region, int mfi)>(100);

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

  public string ToStringLegacy(FieldInfo property, bool includeRegion0)
  {
    var bldr = new StringBuilder();
    bldr.Append($"\"{Well.CoordinatesString()}\",");
    string Sample = "";
    bldr.Append($"\"{Sample}\",");
    foreach (var result in _results)
    {
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
}