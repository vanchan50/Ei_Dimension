using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCy
{
  public class ValidationStats
  {
    public int Region { get; }
    public List<Gstats> Stats = new List<Gstats>(3);
    private readonly float[,] _sfi = new float[5000, 3];
    private int _count;

    public ValidationStats(int reg)
    {
      Region = reg;
    }

    public void FillCalibrationStatsRow(in BeadInfoStruct outbead)
    {
      _sfi[_count, 0] = outbead.reporter;
      _sfi[_count, 1] = outbead.cl1;
      _sfi[_count, 2] = outbead.cl2;
      _count++;
    }

    public void CalculateResults()
    {
      for (int i = 0; i < 3; i++)
      {
        double sumit = 0;
        for (int beads = 0; beads < _count; beads++)
        {
          sumit += _sfi[beads, i];
        }
        double robustcnt = _count; //start with total bead count
        double mean = sumit / robustcnt;
        //find high and low bounds
        double min = mean * 0.5;
        double max = mean * 2;
        sumit = 0;
        for (int beads = 0; beads < _count; beads++)
        {
          if ((_sfi[beads, i] > min) && (_sfi[beads, i] < max))
            sumit += _sfi[beads, i];
          else
          {
            _sfi[beads, i] = 0;
            robustcnt--;
          }
        }
        mean = sumit / robustcnt;
        double sumsq = 0;
        for (int beads = 0; beads < _count; beads++)
        {
          if (_sfi[beads, i] == 0)
            continue;
          sumsq += Math.Pow(mean - _sfi[beads, i], 2);
        }
        double stddev = Math.Sqrt(sumsq / (robustcnt - 1));

        double gcv = (stddev / mean) * 100;
        if (double.IsNaN(gcv))
          gcv = 0;
        Stats.Add(new Gstats
        {
          mfi = mean,
          cv = gcv
        });
      }
    }
  }
}