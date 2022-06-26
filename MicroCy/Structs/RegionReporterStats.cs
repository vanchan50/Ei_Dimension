using System;

namespace DIOS.Core
{
  [Serializable]
  public class RegionReporterStats
  {
    public ushort Region;
    public int Count;
    public float MedFi;
    public float MeanFi;
    public float CoeffVar;

    public RegionReporterStats(RegionReporterResult regionNumber, Well well)
    {
      Count = regionNumber.ReporterValues.Count;
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
}