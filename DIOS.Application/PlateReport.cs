using System.Text;
using Newtonsoft.Json;

namespace DIOS.Application;

[JsonObject(MemberSerialization.Fields)]
public class PlateReport
{
  public Guid plateID;
  public Guid beadMapId;
  public DateTime completedDateTime;
  [JsonProperty("Wells")]
  private List<WellStats> _wells = new(384);
  [JsonIgnore]
  private int _size;

  /// <summary>
  /// Get the reporter Mean values. Works only for the first well (tube)
  /// </summary>
  /// <returns>a list of reporter means for the respective regions</returns>
  public List<(int region, int mfi)> GetRegionalReporterMFI()
  {
    if (_wells.Count == 0 || _wells[0] == null)
      return null;
    return _wells[0].GetReporterMFI();
  }

  internal void Add(WellStats stats)
  {
    _wells.Add(stats);
    _size++;
  }

  public void Reset()
  {
    plateID = Guid.Empty;
    beadMapId = Guid.Empty;
    completedDateTime = DateTime.MinValue;
    _wells.Clear();
    _size = 0;
  }

  public string LegacyReport(string header, bool includeRegion0)
  {
    var median = typeof(RegionReporterStats).GetField(nameof(RegionReporterStats.MedFi));
    var mean = typeof(RegionReporterStats).GetField(nameof(RegionReporterStats.MeanFi));
    var count = typeof(RegionReporterStats).GetField(nameof(RegionReporterStats.Count));
    var coeffVar = typeof(RegionReporterStats).GetField(nameof(RegionReporterStats.CoeffVar));
    var bldr = new StringBuilder();
    bldr.AppendLine("Results\n");
    bldr.AppendLine("Data Type:,\"Median\"");
    bldr.AppendLine($"{header}");
    foreach (var well in _wells)
    {
      bldr.AppendLine(well.ToStringLegacy(median, includeRegion0));
    }
    bldr.AppendLine($"Data Type:,\"Mean {100 * StatisticsExtension.TailDiscardPercentage}% Trim\"");
    bldr.AppendLine($"{header}");
    foreach (var well in _wells)
    {
      bldr.AppendLine(well.ToStringLegacy(mean, includeRegion0));
    }
    bldr.AppendLine("Data Type:,\"Count\"");
    bldr.AppendLine($"{header}");
    foreach (var well in _wells)
    {
      bldr.AppendLine(well.ToStringLegacy(count, includeRegion0));
    }
    bldr.AppendLine("Data Type:,\"TRIMMED % CV\"");
    bldr.AppendLine($"{header}");
    foreach (var well in _wells)
    {
      bldr.AppendLine(well.ToStringLegacy(coeffVar, includeRegion0));
    }
    return bldr.ToString();
  }

  public string JSONify()
  {
    return JsonConvert.SerializeObject(this);
  }
}