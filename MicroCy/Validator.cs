using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCy
{
  public static class Validator
  {
    public static List<ValidationStats> RegionalStats = new List<ValidationStats>(50);
    public static int TotalClassifiedBeads;
    private static Dictionary<int, int> _dict = new Dictionary<int, int>();

    public static void Reset(List<int> regions)
    {
      RegionalStats.Clear();
      TotalClassifiedBeads = 0;
      _dict.Clear();
      foreach (var reg in regions)
      {
        _dict.Add(reg, RegionalStats.Count);
        RegionalStats.Add(new ValidationStats(reg));
      }
    }

    public static void FillStats(in BeadInfoStruct outbead)
    {
      int index;
      if (_dict.TryGetValue(outbead.region, out index))
      {
        RegionalStats[index].FillCalibrationStatsRow(in outbead);
      }
    }

    public static void CalculateResults()
    {
      foreach (var s in RegionalStats)
      {
        s.CalculateResults();
        TotalClassifiedBeads += s.Count;
      }
    }
  }
}