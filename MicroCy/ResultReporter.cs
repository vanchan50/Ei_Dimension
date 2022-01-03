using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCy
{
  public class ResultReporter
  {
    private static StringBuilder _dataout = new StringBuilder();
    private const string Bheader = "Preamble,Time(1 us Tick),FSC bg,Viol SSC bg,CL0 bg,CL1 bg,CL2 bg,CL3 bg,Red SSC bg,Green SSC bg," +
                                   "Green Maj bg, Green Min bg,Green Major,Green Minor,Red-Grn Offset,Grn-Viol Offset,Region,Forward Scatter,Violet SSC,CL0," +
                                   "Red SSC,CL1,CL2,CL3,Green SSC,Reporter\r ";

    public static void StartNewWellReport()
    {
      _ = _dataout.Clear();
      _ = _dataout.Append(Bheader);
    }

    public static void AddBeadStats(in BeadInfoStruct beadInfo)
    {
      _ = _dataout.Append(beadInfo.ToString());
    }

    public static string GetWellReport()
    {
      return _dataout.ToString();
    }
  }
}
