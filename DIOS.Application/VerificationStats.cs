using System.Collections.Generic;
using DIOS.Core;

namespace DIOS.Application;

public class VerificationStats
{
  public int Region { get; }
  public double InputReporter { get; }
  public List<DistributionStats> Stats { get; } = new List<DistributionStats>(3);
  public int Count { get; private set; }
  private readonly List<float> _reporter = new List<float>(100000);
  private readonly List<float> _cl1 = new List<float>(100000);
  private readonly List<float> _cl2 = new List<float>(100000);

  public VerificationStats(int regionNum, double inputReporter)
  {
    Region = regionNum;
    InputReporter = inputReporter;
  }

  public void FillCalibrationStatsRow(in ProcessedBead outbead)
  {
    _reporter.Add(outbead.reporter);
    _cl1.Add(outbead.cl1);
    _cl2.Add(outbead.cl2);
    Count++;
  }

  public void CalculateResultingStats()
  {
    Stats.Add(_reporter.GetDistributionStatistics());
    Stats.Add(_cl1.GetDistributionStatistics());
    Stats.Add(_cl2.GetDistributionStatistics());
  }
}