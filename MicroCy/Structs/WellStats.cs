using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace DIOS.Core.Structs
{
  [JsonObject(MemberSerialization.Fields)]
  internal class WellStats
  {
    [JsonProperty("Well")]
    private readonly Well _well;
    [JsonProperty("Results")]
    private readonly List<RegionReporterStats> _results = new List<RegionReporterStats>(101);
    [JsonIgnore]
    private int _wellCount;
    [JsonIgnore]
    private static readonly char[] Alphabet = Enumerable.Range('A', 16).Select(x => (char)x).ToArray();

    public WellStats(Well well, List<RegionResultVolatile> results, int totalBeadCount)
    {
      _well = new Well(well);
      foreach (var regionResult in results)
      {
        var stats = new RegionReporterStats(regionResult, _well);
        _results.Add(stats);
      }
      _results.Sort((x, y) => x.Region.CompareTo(y.Region));
      _wellCount += totalBeadCount;
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

    public string ToStringLegacy(FieldInfo property)
    {
      var bldr = new StringBuilder();

      var row = Alphabet[_well.RowIdx].ToString();
      var col = _well.ColIdx + 1; //columns are 1 based
      bldr.Append($"\"{row}{col}\",");
      string Sample = "";
      bldr.Append($"\"{Sample}\",");
      foreach (var result in _results)
      {
        var value = property.GetValue(result);
        bldr.Append($"\"{value}\",");
      }
      bldr.Append($"\"{_wellCount}\",");
      string Notes = "";
      bldr.Append($"\"{Notes}\"");
      return bldr.ToString();
    }
  }
}
