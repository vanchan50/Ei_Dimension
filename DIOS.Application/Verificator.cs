using DIOS.Core;

namespace DIOS.Application;

public class Verificator
{
  private readonly Dictionary<int, VerificationStats> RegionalStats = new(10);

  public void Reset(IEnumerable<int> regions)
  {
    RegionalStats.Clear();
    foreach (var reg in regions)
    {
      RegionalStats.Add(reg, new VerificationStats(reg));
    }
  }

  /// <summary>
  /// Called on every Read from USB, in Verification mode. Not used in other modes
  /// </summary>
  /// <param name="bead"></param>
  public void Add(in ProcessedBead bead)
  {
    if (RegionalStats.TryGetValue(bead.region, out var stats))
    {
      stats.AddData(bead);
    }
  }

  public VerificationStats GetRegionStats(int regionNumber)
  {
    if (RegionalStats.TryGetValue(regionNumber, out var stats))
    {
      return stats;
    }
    return null;
  }
}