using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCy
{
  public class Validator
  {
    public List<ValidationStats> VStats = new List<ValidationStats>();
    private Dictionary<int, int> _dict = new Dictionary<int, int>();
    public Validator(List<int> regions)
    {
      VStats = new List<ValidationStats>(regions.Count);
      foreach (var reg in regions)
      {
        VStats.Add(new ValidationStats(reg));
        _dict.Add(reg, VStats.Count);
      }
    }

    public void FillStats(in BeadInfoStruct outbead)
    {
      int index;
      if (_dict.TryGetValue(outbead.region, out index))
      {
        VStats[index].FillCalibrationStatsRow(in outbead);
      }
    }

    public void CalculateResults()
    {
      foreach (var s in VStats)
      {
        s.CalculateResults();
      }
    }
  }
}