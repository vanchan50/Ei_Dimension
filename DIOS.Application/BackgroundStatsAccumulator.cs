using DIOS.Core;

namespace DIOS.Application;

public class BackgroundStatsAccumulator : StatsAccumulator
{
  public override void Add(in ProcessedBead bead)
  {
    greenA.Add(bead.gssc_bg);
    greenB.Add(bead.greenB_bg);
    greenC.Add(bead.greenC_bg);
    redB.Add(bead.rssc_bg);
    cl1.Add(bead.cl1_bg);
    cl2.Add(bead.cl2_bg);
    redA.Add(bead.redA_bg);
    greenD.Add(bead.greenD_bg);
  }
}