using DIOS.Core;

namespace DIOS.Application;

public class VerificationStats
{
  public int Region { get; }
  public List<DistributionStats> Stats { get; } = new(5);
  public int Count { get; private set; }
  private readonly List<float> greenSsc = new(100000);
  private readonly List<float> redSsc = new(100000);
  private readonly List<float> _cl1 = new(100000);
  private readonly List<float> _cl2 = new(100000);
  private readonly List<float> _reporter = new(100000);

  public VerificationStats(int regionNum)
  {
    Region = regionNum;
  }

  public void AddData(in ProcessedBead bead)
  {
    greenSsc.Add(bead.greenssc);
    redSsc.Add(bead.redssc);
    _cl1.Add(bead.cl1);
    _cl2.Add(bead.cl2);
    _reporter.Add(bead.reporter);
    Count++;
  }

  public void CalculateResultingStats()
  {
    Stats.Add(greenSsc.GetDistributionStatistics());
    Stats.Add(redSsc.GetDistributionStatistics());
    Stats.Add(_cl1.GetDistributionStatistics());
    Stats.Add(_cl2.GetDistributionStatistics());
    Stats.Add(_reporter.GetDistributionStatistics());
  }
}