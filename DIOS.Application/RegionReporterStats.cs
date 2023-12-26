namespace DIOS.Application;

[Serializable]
public class RegionReporterStats
{
  public readonly int Region;
  public readonly int Count;
  public readonly float MedFi;
  public readonly float MeanFi;
  public readonly float CoeffVar;

  //  TODO: duplicates RegionReporterResultVolatile.MakeStats() behavior
  public RegionReporterStats(RegionReporterResult regionNumber)
  {
    Count = regionNumber.Count;
    Region = regionNumber.regionNumber;
    var stats = regionNumber.ReporterValues.GetDistributionStatistics();
    MeanFi = stats.Mean;
    CoeffVar = stats.CoeffVar;
    MedFi = stats.Median;
  }

  public override string ToString()
  {
    return $"{Region.ToString()},{Count.ToString()},{MedFi.ToString()},{MeanFi.ToString("F3")},{CoeffVar.ToString("F3")}";
  }
}