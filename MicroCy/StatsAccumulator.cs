using System;
using System.Collections.Generic;
using System.Linq;

namespace DIOS.Core
{
  public class StatsAccumulator
  {
    protected readonly List<float> greenssc = new List<float>(MAXSIZE);
    protected readonly List<float> greenB = new List<float>(MAXSIZE);
    protected readonly List<float> greenC = new List<float>(MAXSIZE);
    protected readonly List<float> redssc = new List<float>(MAXSIZE);
    protected readonly List<float> cl1 = new List<float>(MAXSIZE);
    protected readonly List<float> cl2 = new List<float>(MAXSIZE);
    protected readonly List<float> cl3 = new List<float>(MAXSIZE);
    protected readonly List<float> violetssc = new List<float>(MAXSIZE);
    protected readonly List<float> cl0 = new List<float>(MAXSIZE);
    protected readonly List<float> fsc = new List<float>(MAXSIZE);
    private const int MAXSIZE = 80000;

    public virtual void Add(in BeadInfoStruct bead)
    {
      greenssc.Add(bead.greenssc);
      greenB.Add(bead.greenB);
      greenC.Add(bead.greenC);
      redssc.Add(bead.redssc);
      cl1.Add(bead.cl1);
      cl2.Add(bead.cl2);
      cl3.Add(bead.cl3);
      violetssc.Add(bead.violetssc);
      cl0.Add(bead.cl0);
      fsc.Add(bead.fsc);
    }

    public void Reset()
    {
      greenssc.Clear();
      greenB.Clear();
      greenC.Clear();
      redssc.Clear();
      cl1.Clear();
      cl2.Clear();
      cl3.Clear();
      violetssc.Clear();
      cl0.Clear();
      fsc.Clear();
    }

    public CalibrationStats CalculateStats()
    {
      return new CalibrationStats
      (
        greenssc.GetDistributionStatistics(),
        greenB.GetDistributionStatistics(),
        greenC.GetDistributionStatistics(),
        redssc.GetDistributionStatistics(),
        cl1.GetDistributionStatistics(),
        cl2.GetDistributionStatistics(),
        cl3.GetDistributionStatistics(),
        violetssc.GetDistributionStatistics(),
        cl0.GetDistributionStatistics(),
        fsc.GetDistributionStatistics()
      );
    }

    public AveragesStats CalculateAverages()
    {
      AveragesStats ret;
      try
      {
        ret = new AveragesStats
        (
          greenssc.Average(),
          greenB.Average(),
          greenC.Average(),
          redssc.Average(),
          cl1.Average(),
          cl2.Average(),
          cl3.Average(),
          violetssc.Average(),
          cl0.Average(),
          fsc.Average()
        );
      }
      catch (InvalidOperationException)
      {
        ret = new AveragesStats
        (
          0,
          0,
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
}
