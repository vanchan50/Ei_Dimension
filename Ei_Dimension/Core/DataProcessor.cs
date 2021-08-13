using Ei_Dimension.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ei_Dimension.Core
{
  public class DataProcessor
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

    static public List<HistogramData> LinearizeDictionary(SortedDictionary<int,int> dict)
    {
      var result = new List<HistogramData>();
      foreach (var x in dict)
      {
        result.Add(new HistogramData(x.Value, x.Key));
      }
      return result;
    }

    static public List<(int, int, int)> IdentifyPeaks(List<HistogramData> list)
    {
      var Peaks = new List<(int, int, int)>();
      int start = list[0].Value;
      int top = list[0].Value;
      int end = list[0].Value;
      int startIndex = 0;
      int topIndex = 0;
      int endIndex = 0;
      int threshold = 19; //arbitrary threshold to not include meaningless peaks
      for (var i = 1; i < list.Count - 1; i++)
      {
        if (list[i].Value < list[i - 1].Value && start == top)
        {
          end = list[i].Value;
          start = list[i].Value;
          top = list[i].Value;
          startIndex = i;
          topIndex = i;
          endIndex = i;
          continue;
        }
        if (list[i].Value > top)
        {
          top = list[i].Value;
          topIndex = i;
          continue;
        }
        if (list[i].Value < start)
        {
          end = list[i].Value;
          endIndex = i;
          if (top - start > threshold)
          {
            Peaks.Add((list[startIndex].Argument, list[topIndex].Argument, list[endIndex].Argument));
          }
          start = list[i].Value;
          startIndex = i;
          top = list[i].Value;
          topIndex = i;
          continue;
        }
      }
      return Peaks;
    }
  }
}
