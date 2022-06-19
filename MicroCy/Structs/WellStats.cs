using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace DIOS.Core.Structs
{
  [JsonObject(MemberSerialization.Fields)]
  internal class WellStats
  {
    private readonly Well _well;
    private readonly List<RegionReporterStats> _results = new List<RegionReporterStats>(101);
    [NonSerialized]
    private static readonly char[] Alphabet = Enumerable.Range('A', 16).Select(x => (char)x).ToArray();

    public WellStats(Well well, List<RegionResultVolatile> results)
    {
      _well = new Well(well);
      foreach (var regionResult in results)
      {
        _results.Add(new RegionReporterStats(regionResult, _well));
      }
    }

    public List<(int region, int mfi)> GetReporterMFI()
    {
      List<(int region, int mfi)> list = new List<(int region, int mfi)>(100);

      foreach (var regionReport in _results)
      {
        if (regionReport.Region == 0)
        {
          continue;
        }
        list.Add((regionReport.Region, (int)regionReport.MeanFi));
      }
      return list;
    }

    public override string ToString()
    {
      var bldr = new StringBuilder();
      var row = Alphabet[_well.RowIdx].ToString();
      var col = _well.ColIdx + 1; //columns are 1 based
      bldr.AppendLine($"Well {row}{col}:"); 
      foreach (var result in _results)
      {
        bldr.AppendLine(result.ToString());
      }
      return bldr.ToString();
    }
  }
}
