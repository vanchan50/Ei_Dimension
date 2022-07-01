using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIOS.Core
{
  public class BackgroundStatsAccumulator : StatsAccumulator
  {
    public override void Add(in BeadInfoStruct bead)
    {
      greenssc.Add(bead.gssc_bg);
      greenB.Add(bead.greenB_bg);
      greenC.Add(bead.greenC_bg);
      redssc.Add(bead.rssc_bg);
      cl1.Add(bead.cl1_bg);
      cl2.Add(bead.cl2_bg);
      cl3.Add(bead.cl3_bg);
      violetssc.Add(bead.vssc_bg);
      cl0.Add(bead.cl0_bg);
      fsc.Add(bead.fsc_bg);
    }
  }
}