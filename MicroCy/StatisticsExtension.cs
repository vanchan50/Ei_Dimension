using System;
using System.Collections.Generic;
using System.Linq;

namespace DIOS.Core
{
  public static class StatisticsExtension
  {
    public static DistributionStats GetDistributionStatistics(this List<float> values)
    {
      float mean = 0;
      float coeffVar = 0;
      float median = 0;
      var count = values.Count;
      if (count >= 20)
      {
        values.Sort();
        int quarter = count / 4;
        values.RemoveDistributionTails(quarter);
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
    private static void RemoveDistributionTails(this List<float> values, int length)
    {
      values.RemoveRange(values.Count - length, length);
      values.RemoveRange(0, length);
    }
  }
}
