using System.IO;
using System.Text;

namespace DIOS.Core
{
  internal class WellStatsData
  {
    private readonly StringBuilder _dataOut = new StringBuilder();
    public const string HEADER = "Region,Bead Count,Median FI,Trimmed Mean FI,CV%\r";

    public void Add(string stats)
    {
      _dataOut.AppendLine(stats);
    }

    public void Reset()
    {
      _ = _dataOut.Clear();
    }

    public string Publish(bool includeReg0)
    {
      var result = _dataOut.ToString();
      if (!includeReg0)
        RemoveRegion0(ref result);
      return result;
    }

    private void RemoveRegion0(ref string output)
    {
      var sb = new StringBuilder();
      var r = new StringReader(output);
      string s = r.ReadLine();
      while (s != null)
      {
        if (!s.StartsWith("0"))
        {
          sb.AppendLine(s);
        }
        s = r.ReadLine();
      }
      output = sb.ToString();
    }
  }
}