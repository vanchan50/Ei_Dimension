using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCy
{
  public class Validator
  {
    public List<ValidationStats> RegionalStats = new List<ValidationStats>();
    public int TotalClassifiedBeads;
    private Dictionary<int, int> _dict = new Dictionary<int, int>();
    public Validator(List<int> regions)
    {
      RegionalStats = new List<ValidationStats>(regions.Count);
      TotalClassifiedBeads = 0;
      foreach (var reg in regions)
      {
        _dict.Add(reg, RegionalStats.Count);
        RegionalStats.Add(new ValidationStats(reg));
      }
    }

    public void FillStats(in BeadInfoStruct outbead)
    {
      int index;
      if (_dict.TryGetValue(outbead.region, out index))
      {
        RegionalStats[index].FillCalibrationStatsRow(in outbead);
      }
    }

    public void CalculateResults()
    {
      foreach (var s in RegionalStats)
      {
        s.CalculateResults();
        TotalClassifiedBeads += s.Count;
      }
    }
  }
}