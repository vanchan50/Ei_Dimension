using System;
using System.Collections.Generic;
using System.Linq;

namespace DIOS.Core.Tests
{
  public class StatisticsUnitTests
  { /*
    [Fact]
    public void Test1()
    {
      var rand = new Random();
      var val1 = new List<float>(10000);
      var val2 = new List<float>(10000);
      var val3 = new List<float>(10000);
      var val4 = new List<float>(10000);
      for (var i = 0; i < 10000; i++)
      {
        var t =(float) rand.NextDouble() * 500000;
        val1.Add(t);
        val2.Add(t);
        val3.Add(t);
        val4.Add(t);
      }

      var res1 = val1.GetDistributionStatistics().CoeffVar;
      var res2 = OldDistributionStatistics(val2).CoeffVar;
      var res3 = TwiceOldSlightlyDifferentCalculateStatistics(val3).CoeffVar;
      var res4 = OldReporterStats(val4).CoeffVar;
    }

    private static DistributionStats OldDistributionStatistics(List<float> values)
    {
      float mean = 0;
      float coeffVar = 0;
      float median = 0;
      var count = values.Count;
      if (count >= 20)
      {
        values.Sort();
        int quarter = count / 4;
        RemoveDistributionTails(values, quarter);
        median = values[quarter];
        mean = values.Average();

        double sumsq = values.Sum(dataout => Math.Pow(dataout - mean, 2));
        double stddev = Math.Sqrt(sumsq / values.Count() - 1);

        coeffVar = (float)stddev / mean * 100;
        if (double.IsNaN(coeffVar))
          coeffVar = 0;
      }
      else if (count > 0)
      {
        mean = values.Average();
      }

      return new DistributionStats
      {
        Median = median,
        Mean = mean,
        CoeffVar = coeffVar
      };
    }

    /// <summary>
    /// Removes a range of elements from both sides of the list
    /// </summary>
    /// <param name="values"></param>
    /// <param name="length">length of tails to remove</param>
    private static void RemoveDistributionTails(List<float> values, int length)
    {
      values.RemoveRange(values.Count - length, length);
      values.RemoveRange(0, length);
    }

    private static DistributionStats TwiceOldSlightlyDifferentCalculateStatistics(List<float> list)
    {
      double[] MfiStats = new double[10];
      double[] CvStats = new double[10];
      float[,] Sfi = new float[5000, 10];
      var maxBeads = list.Count > 5000 ? 5000 : list.Count;
      for (var i = 0; i < maxBeads; i++)
      {
        Sfi[i, 0] = list[i];
        Sfi[i, 1] = list[i];
        Sfi[i, 2] = list[i];
        Sfi[i, 3] = list[i];
        Sfi[i, 4] = list[i];
        Sfi[i, 5] = list[i];
        Sfi[i, 6] = list[i];
        Sfi[i, 7] = list[i];
        Sfi[i, 8] = list[i];
        Sfi[i, 9] = list[i];
      }
      for (int finx = 0; finx < 10; finx++)
      {
        double sumit = 0;
        for (int beads = 0; beads < maxBeads; beads++)
        {
          sumit += Sfi[beads, finx];
        }
        double robustcnt = maxBeads; //start with total bead count
        double mean = sumit / robustcnt;
        //find high and low bounds
        double min = mean * 0.5;
        double max = mean * 2;
        sumit = 0;
        for (int beads = 0; beads < maxBeads; beads++)
        {
          if ((Sfi[beads, finx] > min) && (Sfi[beads, finx] < max))
            sumit += Sfi[beads, finx];
          else
          {
            Sfi[beads, finx] = 0;
            robustcnt--;
          }
        }
        mean = sumit / robustcnt;
        double sumsq = 0;
        for (int beads = 0; beads < maxBeads; beads++)
        {
          if (Sfi[beads, finx] == 0)
            continue;
          sumsq += Math.Pow(mean - Sfi[beads, finx], 2);
        }
        double stdDev = Math.Sqrt(sumsq / (robustcnt - 1));

        double gcv = (stdDev / mean) * 100;
        if (double.IsNaN(gcv))
          gcv = 0;

        MfiStats[finx] = mean;
        CvStats[finx] = gcv;
      }

      return new DistributionStats
      { 
        CoeffVar = (float)CvStats[0],
        Mean = (float)MfiStats[0]
      };
    }

    private DistributionStats OldReporterStats(List<float> list)
    {
      float mean = 0;
      float coeffVar = 0;
      float median = 0;
      var count = list.Count;
      float avg = list.Average();
      if (count >= 20)
      {
        list.Sort();
        int quarterIndex = count / 4;
        float sum = 0;
        for (var i = quarterIndex; i < count - quarterIndex; i++)
        {
          sum += list[i];
        }

        mean = sum / (count - 2 * quarterIndex);

        median = (float)Math.Round(list[count / 2]);

        double sumsq = list.Sum(dataout => Math.Pow(dataout - mean, 2));
        double stddev = Math.Sqrt(sumsq / list.Count() - 1);
        coeffVar = (float)stddev / mean * 100;
        if (double.IsNaN(coeffVar))
          coeffVar = 0;
      }
      else if (count > 0)
      {
        mean = list.Average();
      }

      return new DistributionStats
      {
        Median = median,
        Mean = mean,
        CoeffVar = coeffVar
      };
    }
    */
  }
}