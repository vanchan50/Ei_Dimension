using System;
using System.Collections.Generic;
using System.Linq;

namespace DIOS.Core
{
  internal class StatsAccumulator
  {
    private readonly List<float> greenssc = new List<float>(MAXSIZE);
    private readonly List<float> greenB = new List<float>(MAXSIZE);
    private readonly List<float> greenC = new List<float>(MAXSIZE);
    private readonly List<float> redssc = new List<float>(MAXSIZE);
    private readonly List<float> cl1 = new List<float>(MAXSIZE);
    private readonly List<float> cl2 = new List<float>(MAXSIZE);
    private readonly List<float> cl3 = new List<float>(MAXSIZE);
    private readonly List<float> violetssc = new List<float>(MAXSIZE);
    private readonly List<float> cl0 = new List<float>(MAXSIZE);
    private readonly List<float> fsc = new List<float>(MAXSIZE);

    private readonly List<float> greensscBg = new List<float>(MAXSIZE);
    private readonly List<float> greenBBg = new List<float>(MAXSIZE);
    private readonly List<float> greenCBg = new List<float>(MAXSIZE);
    private readonly List<float> redsscBg = new List<float>(MAXSIZE);
    private readonly List<float> cl1Bg = new List<float>(MAXSIZE);
    private readonly List<float> cl2Bg = new List<float>(MAXSIZE);
    private readonly List<float> cl3Bg = new List<float>(MAXSIZE);
    private readonly List<float> violetsscBg = new List<float>(MAXSIZE);
    private readonly List<float> cl0Bg = new List<float>(MAXSIZE);
    private readonly List<float> fscBg = new List<float>(MAXSIZE);
    private const int MAXSIZE = 80000;

    public void Add(in BeadInfoStruct bead)
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

      greensscBg.Add(bead.gssc_bg);
      greenBBg.Add(bead.greenB_bg);
      greenCBg.Add(bead.greenC_bg);
      redsscBg.Add(bead.rssc_bg);
      cl1Bg.Add(bead.cl1_bg);
      cl2Bg.Add(bead.cl2_bg);
      cl3Bg.Add(bead.cl3_bg);
      violetsscBg.Add(bead.vssc_bg);
      cl0Bg.Add(bead.cl0_bg);
      fscBg.Add(bead.fsc_bg);
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

      greensscBg.Clear();
      greenBBg.Clear();
      greenCBg.Clear();
      redsscBg.Clear();
      cl1Bg.Clear();
      cl2Bg.Clear();
      cl3Bg.Clear();
      violetsscBg.Clear();
      cl0Bg.Clear();
      fscBg.Clear();
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

    public BackgroundStats CalculateBackgroundAverages()
    {
      BackgroundStats ret;
      try
      {
        ret = new BackgroundStats
        (
          greensscBg.Average(),
          greenBBg.Average(),
          greenCBg.Average(),
          redsscBg.Average(),
          cl1Bg.Average(),
          cl2Bg.Average(),
          cl3Bg.Average(),
          violetsscBg.Average(),
          cl0Bg.Average(),
          fscBg.Average()
        );
      }
      catch(InvalidOperationException)
      {
        ret = new BackgroundStats();
      }

      return ret;
    }
  }
}
