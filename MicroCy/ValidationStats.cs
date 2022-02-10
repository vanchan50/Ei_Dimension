using System;
using System.Collections.Generic;

namespace MicroCy
{
  public class ValidationStats
  {
    public int Region { get; }
    internal double InputReporter { get; }
    public List<Gstats> Stats { get; } = new List<Gstats>(3);
    public int Count { get; private set; }
    private readonly float[,] _sfi = new float[100000, 3];

    public ValidationStats(int regionNum, double inputReporter)
    {
      Region = regionNum;
      InputReporter = inputReporter;
    }

    public void FillCalibrationStatsRow(in BeadInfoStruct outbead)
    {
      _sfi[Count, 0] = outbead.reporter;
      _sfi[Count, 1] = outbead.cl1;
      _sfi[Count, 2] = outbead.cl2;
      Count++;
    }

    public void CalculateResults()
    {
      for (int i = 0; i < 3; i++)
      {
        double sumit = 0;
        for (int beads = 0; beads < Count; beads++)
        {
          sumit += _sfi[beads, i];
        }
        double robustcnt = Count; //start with total bead count
        double mean = sumit / robustcnt;
        //find high and low bounds
        double min = mean * 0.5;
        double max = mean * 2;
        sumit = 0;
        for (int beads = 0; beads < Count; beads++)
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
        for (int beads = 0; beads < Count; beads++)
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