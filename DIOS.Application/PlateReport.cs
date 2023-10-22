using System.Text;
using DIOS.Core.HardwareIntercom;
using Newtonsoft.Json;

namespace DIOS.Application;

[JsonObject(MemberSerialization.Fields)]
public class PlateReport
{
  public string plateID;
  public Guid beadMapId;
  public DateTime completedDateTime;
  [JsonProperty("Wells")]
  private List<WellStats> _wellStatsList = new(384);
  [JsonIgnore]
  private PlateSize _plateSize;

  [JsonIgnore] private readonly SortedSet<(int Number, string Name)> _allActiveRegionsInPlate;  //used for legacy report header

  public PlateReport()
  {
    var comp = Comparer<(int Number, string Name)>
      .Create((a, b) =>
        a.Number.CompareTo(b.Number));
    _allActiveRegionsInPlate = new(comp);
  }
  /// <summary>
  /// Get the reporter Mean values. Works only for the first well (tube)
  /// </summary>
  /// <returns>a list of reporter means for the respective regions</returns>
  public List<(int region, int medianFi)> GetRegionalReporterMFI()
  {
    if (_wellStatsList.Count == 0 || _wellStatsList[0] == null)
      return null;
    return _wellStatsList[0].GetReporterMeanFi();
  }

  internal void Add(WellStats stats)
  {
    _wellStatsList.Add(stats);
    foreach (var region in stats.Well.ActiveRegions)
    {
      _allActiveRegionsInPlate.Add(region);
    }
  }

  public void Reset(PlateSize plateSize, string plateId = null)
  {
    plateID = plateId;
    _plateSize = plateSize;
    beadMapId = Guid.Empty;
    completedDateTime = DateTime.MinValue;
    _wellStatsList.Clear();
    _allActiveRegionsInPlate.Clear();
  }

  public string LegacyReport(bool includeRegion0)
  {
    var header = GetLegacyReportHeader(includeRegion0);
    var median = typeof(RegionReporterStats).GetField(nameof(RegionReporterStats.MedFi));
    var mean = typeof(RegionReporterStats).GetField(nameof(RegionReporterStats.MeanFi));
    var count = typeof(RegionReporterStats).GetField(nameof(RegionReporterStats.Count));
    var coeffVar = typeof(RegionReporterStats).GetField(nameof(RegionReporterStats.CoeffVar));
    var bldr = new StringBuilder();
    bldr.AppendLine("Results\n");
    bldr.AppendLine("Data Type:,\"Median\"");
    bldr.AppendLine($"{header}");
    foreach (var well in _wellStatsList)
    {
      bldr.AppendLine(well.ToStringLegacy(median, includeRegion0, _allActiveRegionsInPlate));
    }
    bldr.AppendLine($"Data Type:,\"Mean {100 * StatisticsExtension.TailDiscardPercentage}% Trim\"");
    bldr.AppendLine($"{header}");
    foreach (var well in _wellStatsList)
    {
      bldr.AppendLine(well.ToStringLegacy(mean, includeRegion0, _allActiveRegionsInPlate));
    }
    bldr.AppendLine("Data Type:,\"Count\"");
    bldr.AppendLine($"{header}");
    foreach (var well in _wellStatsList)
    {
      bldr.AppendLine(well.ToStringLegacy(count, includeRegion0, _allActiveRegionsInPlate));
    }
    bldr.AppendLine("Data Type:,\"TRIMMED % CV\"");
    bldr.AppendLine($"{header}");
    foreach (var well in _wellStatsList)
    {
      bldr.AppendLine(well.ToStringLegacy(coeffVar, includeRegion0, _allActiveRegionsInPlate));
    }
    return bldr.ToString();
  }

  private string GetLegacyReportHeader(bool includeRegion0)
  {
    var bldr = new StringBuilder();
    bldr.Append("\"Location\",");
    bldr.Append("\"Sample\",");
    foreach (var region in _allActiveRegionsInPlate)
    {
      if (region.Number == 0)
      {
        if (includeRegion0)
        {
          bldr.Append($"\"{GetRegionLegacyHeader(region)}\",");
        }
        continue;
      }
      bldr.Append($"\"{GetRegionLegacyHeader(region)}\",");
    }
    bldr.Append("\"Total Events\",");
    bldr.Append("\"Notes\"");
    return bldr.ToString();
  }

  private string GetRegionLegacyHeader((int Number, string Name) region)
  {
    return $"{region.Name} ({region.Number})";
  }

  public string JSONify()
  {
    return JsonConvert.SerializeObject(this);
  }
}