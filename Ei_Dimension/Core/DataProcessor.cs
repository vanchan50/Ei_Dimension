using Ei_Dimension.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ei_Dimension.Core
{
  public static class DataProcessor
  //helper class for ResultsViewModel
  {
    static public async Task<List<string>> GetDataFromFileAsync(string path)
    {
      var str = new List<string>();
      using (System.IO.FileStream fin = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
      using (System.IO.StreamReader sr = new System.IO.StreamReader(fin))
      {
        sr.ReadLine();
        while (!sr.EndOfStream)
        {
          str.Add(await sr.ReadLineAsync());
        }
      }
      return str;
    }

    static public MicroCy.BeadInfoStruct ParseRow(string data)
    {
      MicroCy.BeadInfoStruct Binfo;
      string[] words = data.Split(',');
      Binfo.Header = uint.Parse(words[0]);
      Binfo.EventTime = uint.Parse(words[1]);
      Binfo.fsc_bg = byte.Parse(words[2]);
      Binfo.vssc_bg = byte.Parse(words[3]);
      Binfo.cl0_bg = byte.Parse(words[4]);
      Binfo.cl1_bg = byte.Parse(words[5]);
      Binfo.cl2_bg = byte.Parse(words[6]);
      Binfo.cl3_bg = byte.Parse(words[7]);
      Binfo.rssc_bg = byte.Parse(words[8]);
      Binfo.gssc_bg = byte.Parse(words[9]);
      Binfo.greenB_bg = ushort.Parse(words[10]);
      Binfo.greenC_bg = ushort.Parse(words[11]);
      Binfo.greenB = ushort.Parse(words[12]);
      Binfo.greenC = ushort.Parse(words[13]);
      Binfo.l_offset_rg = byte.Parse(words[14]);
      Binfo.l_offset_gv = byte.Parse(words[15]);
      Binfo.region = ushort.Parse(words[16]);
      Binfo.fsc = float.Parse(words[17], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      Binfo.violetssc = float.Parse(words[18], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      Binfo.cl0 = float.Parse(words[19], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      Binfo.redssc = float.Parse(words[20], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      Binfo.cl1 = float.Parse(words[21], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      Binfo.cl2 = float.Parse(words[22], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      Binfo.cl3 = float.Parse(words[23], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      Binfo.greenssc = float.Parse(words[24], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      Binfo.reporter = float.Parse(words[25], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      return Binfo;
    }

    static public List<HistogramData<int,int>> MovAvgFilter(List<HistogramData<int, int>> data)
    {
      int max = data.Count - 1;
      int avg = 0;
      List<HistogramData<int, int>> averaged = new List<HistogramData<int, int>> (data);
      for (var i = 4; i < max; i++)
      {
        avg = (averaged[i - 4].Value + averaged[i - 3].Value + averaged[i - 2].Value + averaged[i - 1].Value + averaged[i].Value) / 5;
        averaged[i].Value = avg;
      }
      return averaged;
    }

    static public List<HistogramData<int, int>> LinearizeDictionary(SortedDictionary<int,int> dict)
    {
      var result = new List<HistogramData<int, int>>();
      foreach (var x in dict)
      {
        result.Add(new HistogramData<int, int>(x.Value, x.Key));
      }
      return result;
    }

    static public List<(int, int, int)> IdentifyPeaks(List<HistogramData<int, int>> list)
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

    static public List<SortedDictionary<int, int>> MakeDictionariesFromData(List<MicroCy.BeadInfoStruct> bsList)
    {
      SortedDictionary<int, int> dictForward = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictVioletssc = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictRedssc = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictGreenssc = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictReporter = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictCl0 = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictCl1 = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictCl2 = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictCl3 = new SortedDictionary<int, int>(); //value,bin


      int binsize = 50;
      int maxlength = 10050;
      for (var i = binsize; i < maxlength; i += binsize)
      {
        dictCl0.Add(i, 0);
        dictCl1.Add(i, 0);
        dictCl2.Add(i, 0);
        dictCl3.Add(i, 0);
      }

      int key;
      foreach (var bs in bsList)
      {
        key = (int)bs.reporter;
        if (dictReporter.ContainsKey(key))
        {
          dictReporter[key]++;
        }
        else
        {
          dictReporter.Add(key, 1);
        }

        key = (int)bs.fsc;
        if (dictForward.ContainsKey(key))
        {
          dictForward[key]++;
        }
        else
        {
          dictForward.Add(key, 1);
        }

        key = (int)bs.violetssc;
        if (dictVioletssc.ContainsKey(key))
        {
          dictVioletssc[key]++;
        }
        else
        {
          dictVioletssc.Add(key, 1);
        }

        key = (int)bs.redssc;
        if (dictRedssc.ContainsKey(key))
        {
          dictRedssc[key]++;
        }
        else
        {
          dictRedssc.Add(key, 1);
        }

        key = (int)bs.greenssc;
        if (dictGreenssc.ContainsKey(key))
        {
          dictGreenssc[key]++;
        }
        else
        {
          dictGreenssc.Add(key, 1);
        }

        //bin these into binsize point bins
        for (var i = binsize; i < maxlength; i += binsize)
        {
          if (bs.cl0 > i)
          {
            continue;
          }
          dictCl0[i]++;
          break;
        }
        for (var i = binsize; i < maxlength; i += binsize)
        {
          if (bs.cl1 > i)
          {
            continue;
          }
          dictCl1[i]++;
          break;
        }
        for (var i = binsize; i < maxlength; i += binsize)
        {
          if (bs.cl2 > i)
          {
            continue;
          }
          dictCl2[i]++;
          break;
        }
        for (var i = binsize; i < maxlength; i += binsize)
        {
          if (bs.cl3 > i)
          {
            continue;
          }
          dictCl3[i]++;
          break;
        }
        //  key = (int)bs.cl0;
        //  if (dictCl0.ContainsKey(key))
        //  {
        //    dictCl0[key]++;
        //  }
        //  else
        //  {
        //    dictCl0.Add(key, 1);
        //  }

        //  key = (int)bs.cl1;
        //  if (dictCl1.ContainsKey(key))
        //  {
        //    dictCl1[key]++;
        //  }
        //  else
        //  {
        //    dictCl1.Add(key, 1);
        //  }
        //
        //  key = (int)bs.cl2;
        //  if (dictCl2.ContainsKey(key))
        //  {
        //    dictCl2[key]++;
        //  }
        //  else
        //  {
        //    dictCl2.Add(key, 1);
        //  }
        //
        //  key = (int)bs.cl3;
        //  if (dictCl3.ContainsKey(key))
        //  {
        //    dictCl3[key]++;
        //  }
        //  else
        //  {
        //    dictCl3.Add(key, 1);
        //  }
      }

      var lst = new List<SortedDictionary<int, int>>();
      lst.Add(dictForward);     //0
      lst.Add(dictVioletssc);   //1
      lst.Add(dictRedssc);      //2
      lst.Add(dictGreenssc);    //3
      lst.Add(dictReporter);    //4
      lst.Add(dictCl0);         //5
      lst.Add(dictCl1);         //6
      lst.Add(dictCl2);         //7
      lst.Add(dictCl3);         //8
      return lst;
    }

    static public byte AssignIntensity(int point, (int,int,int)peak, double[] heatLvl)
    {
      //check if in bounds
      if (point > peak.Item1)
      {
        //in 30%
        if (point > peak.Item2 - heatLvl[0] * (peak.Item2 - peak.Item1) && point < peak.Item2 + heatLvl[0] * (peak.Item3 - peak.Item2))
        {
          return 5;
        }
        //in 50%
        if (point > peak.Item2 - heatLvl[1] * (peak.Item2 - peak.Item1) && point < peak.Item2 + heatLvl[1] * (peak.Item3 - peak.Item2))
        {
          return 4;
        }
        //in 70%
        if (point > peak.Item2 - heatLvl[2] * (peak.Item2 - peak.Item1) && point < peak.Item2 + heatLvl[2] * (peak.Item3 - peak.Item2))
        {
          return 3;
        }
        //in bin at all
        if (point > peak.Item2 - heatLvl[3] * (peak.Item2 - peak.Item1) && point < peak.Item2 + heatLvl[3] * (peak.Item3 - peak.Item2))
        {
          return 2;
        }
      }
    //less than start - assign to min
    return 1;
    }
    public static double[] GenerateLogSpace(int min, int max, int logBins)
    {
      double logarithmicBase = 10;
      double logMin = Math.Log10(min);
      double logMax = Math.Log10(max);
      double delta = (logMax - logMin) / logBins;
      double accDelta = 0;
      double[] Result = new double[logBins + 1];
      for (int i = 0; i <= logBins; ++i)
      {
        Result[i] = Math.Pow(logarithmicBase, logMin + accDelta);
        accDelta += delta;// accDelta = delta * i
      }
      return Result;
    }
  }
}
