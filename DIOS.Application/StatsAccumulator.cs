using DIOS.Core;

namespace DIOS.Application;

public class StatsAccumulator
{
  protected readonly List<float> greenA   =  new(MAXSIZE);
  protected readonly List<float> greenB     =  new(MAXSIZE);
  protected readonly List<float> greenC     =  new(MAXSIZE);
  protected readonly List<float> redB     =  new(MAXSIZE);
  protected readonly List<float> cl1        =  new(MAXSIZE);
  protected readonly List<float> cl2        =  new(MAXSIZE);
  protected readonly List<float> redA        =  new(MAXSIZE);
  protected readonly List<float> greenD        =  new(MAXSIZE);
  private const int MAXSIZE = 80000;

  public virtual void Add(in ProcessedBead bead)
  {
    greenA.Add(bead.greenssc);
    greenB.Add(bead.greenB);
    greenC.Add(bead.greenC);
    redB.Add(bead.redssc);
    cl1.Add(bead.cl1);
    cl2.Add(bead.cl2);
    redA.Add(bead.redA);
    greenD.Add(bead.greenD);
  }

  public void Reset()
  {
    greenA.Clear();
    greenB.Clear();
    greenC.Clear();
    redB.Clear();
    cl1.Clear();
    cl2.Clear();
    redA.Clear();
    greenD.Clear();
  }

  public ChannelsCalibrationStats CalculateStats()
  {
    return new ChannelsCalibrationStats
    (
      greenA.GetDistributionStatistics(),
      greenB.GetDistributionStatistics(),
      greenC.GetDistributionStatistics(),
      redB.GetDistributionStatistics(),
      cl1.GetDistributionStatistics(),
      cl2.GetDistributionStatistics(),
      redA.GetDistributionStatistics(),
      greenD.GetDistributionStatistics()
    );
  }

  public ChannelsAveragesStats CalculateAverages()
  {
    ChannelsAveragesStats ret;
    try
    {
      ret = new ChannelsAveragesStats
      (
        greenA.Average(),
        greenB.Average(),
        greenC.Average(),
        redB.Average(),
        cl1.Average(),
        cl2.Average(),
        redA.Average(),
        greenD.Average()
      );
    }
    catch (InvalidOperationException)
    {
      ret = new ChannelsAveragesStats
      (
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      );
    }

    return ret;
  }
}