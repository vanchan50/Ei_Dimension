using Ei_Dimension.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ei_Dimension.Core
{
  public class HeatmapDataProcessor
  {
    static public List<HistogramData> MovAvgFilter(List<HistogramData> data)
    {
      int max = data.Count - 1;
      int avg = 0;
      List<HistogramData> averaged = new List<HistogramData>(data);
      for (var i = 4; i < max; i++)
      {
        avg = (averaged[i - 4].Value + averaged[i - 3].Value + averaged[i - 2].Value + averaged[i - 1].Value + averaged[i].Value) / 5;
        averaged[i].Value = avg;
      }
      return averaged;
    }
  }
}
