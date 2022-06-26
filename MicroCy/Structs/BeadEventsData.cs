using System.Collections.Generic;
using System.Text;

namespace DIOS.Core
{
  public class BeadEventsData
  {
    private readonly List<BeadInfoStruct> _list = new List<BeadInfoStruct>(500000);
    private readonly StringBuilder _dataOut = new StringBuilder();
    private const string HEADER = "Time(1 us Tick),FSC bg,Viol SSC bg,CL0 bg,CL1 bg,CL2 bg,CL3 bg,Red SSC bg,Green SSC bg," +
                                   "Green B bg,Green C bg,Green B,Green C,Red-Grn Offset,Grn-Viol Offset,Region,Forward Scatter,Violet SSC,CL0," +
                                   "Red SSC,CL1,CL2,CL3,Green SSC,Reporter\r";

    public void Add(in BeadInfoStruct bead)
    {
      _list.Add(bead);
    }

    public void Reset()
    {
      _list.Clear();
      _ = _dataOut.Clear();
      _ = _dataOut.Append(HEADER);
    }

    public string Publish(bool onlyClassified)
    {
      foreach (var bead in _list)
      {
        if (bead.region == 0 && onlyClassified)
          continue;
        _dataOut.AppendLine(bead.ToString());
      }
      return _dataOut.ToString();
    }
  }
}
