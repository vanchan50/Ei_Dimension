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

    public string Publish()
    {
      return _dataOut.ToString();
    }
  }
}