using Ei_Dimension.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Ei_Dimension.Core
{
  public static class DataProcessor
  {
    private static SolidColorBrush[] _heatColors;

    static  DataProcessor()
    {
      _heatColors = new SolidColorBrush[7];
      _heatColors[0] = Brushes.Black;
      _heatColors[1] = Brushes.DarkViolet;
      _heatColors[2] = new SolidColorBrush(Color.FromRgb(0x0a, 0x6d, 0xaa));
      _heatColors[3] = new SolidColorBrush(Color.FromRgb(0x00, 0xcc, 0x49));
      _heatColors[4] = Brushes.Orange;
      _heatColors[5] = Brushes.OrangeRed;
      _heatColors[6] = Brushes.Red;
    }

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

    static public List<HistogramData> LinearizeDictionary(SortedDictionary<int,int> dict)
    {
      var result = new List<HistogramData>();
      foreach (var x in dict)
      {
        result.Add(new HistogramData(x.Value, x.Key));
      }
      return result;
    }

    public static int[] GenerateLogSpace(int min, int max, int logBins, bool baseE = false)
    {
      double logarithmicBase = 10;
      double logMin = Math.Log10(min);
      double logMax = Math.Log10(max);
      if (baseE)
      {
        logarithmicBase = Math.E;
        logMin = Math.Log(min);
        logMax = Math.Log(max);
      }
      double delta = (logMax - logMin) / logBins;
      double accDelta = delta;
      int[] Result = new int[logBins];
      for (int i = 1; i <= logBins; ++i)
      {
        Result[i - 1] = (int)Math.Round(Math.Pow(logarithmicBase, logMin + accDelta));
        accDelta += delta;
      }
      return Result;
    }
    public static double[] GenerateLogSpaceD(int min, int max, int logBins, bool baseE = false)
    {
      double logarithmicBase = 10;
      double logMin = Math.Log10(min);
      double logMax = Math.Log10(max);
      if (baseE)
      {
        logarithmicBase = Math.E;
        logMin = Math.Log(min);
        logMax = Math.Log(max);
      }
      double delta = (logMax - logMin) / logBins;
      double accDelta = delta;
      double[] Result = new double[logBins];
      for (int i = 1; i <= logBins; ++i)
      {
        Result[i - 1] = Math.Pow(logarithmicBase, logMin + accDelta);
        accDelta += delta;
      }
      return Result;
    }

    public static void BinScatterData(List<MicroCy.BeadInfoStruct> list, bool fromFile = false)
    {
      var ResVM = ViewModels.ResultsViewModel.Instance;
      var MaxValue = ResVM.CurrentReporter[ResVM.CurrentReporter.Count - 1].Argument;
      var reporter = new int[ResVM.CurrentReporter.Count];
      var fsc = new int[ResVM.CurrentReporter.Count];
      var red = new int[ResVM.CurrentReporter.Count];
      var green = new int[ResVM.CurrentReporter.Count];
      var violet = new int[ResVM.CurrentReporter.Count];

      List<List<float>> ActiveRegionsStats = new List<List<float>>();  //for mean and count
      foreach (var region in App.Device.ActiveMap.regions)
      {
        ActiveRegionsStats.Add(new List<float>());
      }
      //bool failed = false;
      foreach (var beadD in list)
      {
        var bead = beadD;
        //overflow protection
        bead.fsc = bead.fsc < MaxValue ? bead.fsc : MaxValue;
        bead.violetssc = bead.violetssc < MaxValue ? bead.violetssc : MaxValue;
        bead.redssc = bead.redssc < MaxValue ? bead.redssc : MaxValue;
        bead.greenssc = bead.greenssc < MaxValue ? bead.greenssc : MaxValue;
        bead.reporter = bead.reporter < MaxValue ? bead.reporter : MaxValue;

        int j = 0;
        j = Array.BinarySearch(HistogramData.Bins, (int)bead.fsc);
        if (j < 0)
          j = ~j;
        fsc[j]++;
        j = Array.BinarySearch(HistogramData.Bins, (int)bead.redssc);
        if (j < 0)
          j = ~j;
        red[j]++;
        j = Array.BinarySearch(HistogramData.Bins, (int)bead.greenssc);
        if (j < 0)
          j = ~j;
        green[j]++;
        j = Array.BinarySearch(HistogramData.Bins, (int)bead.violetssc);
        if (j < 0)
          j = ~j;
        violet[j]++;
        j = Array.BinarySearch(HistogramData.Bins, (int)bead.reporter);
        if (j < 0)
          j = ~j;
        reporter[j]++;

        var index = App.MapRegions.RegionsList.IndexOf(beadD.region.ToString());
        if (index != -1)
          ActiveRegionsStats[index].Add(beadD.reporter);
        else if (beadD.region == 0)
          continue;
        //else
        //  failed = true;
      }
      //if (failed)
      //  System.Windows.MessageBox.Show("An error occured during Well File Read");

      _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        if (fromFile)
        {
          for (var i = 0; i < ResVM.BackingReporter.Count; i++)
          {
            ResVM.BackingReporter[i].Value += reporter[i];
            ResVM.BackingForwardSsc[i].Value += fsc[i];
            ResVM.BackingRedSsc[i].Value += red[i];
            ResVM.BackingGreenSsc[i].Value += green[i];
            ResVM.BackingVioletSsc[i].Value += violet[i];

          }
          var j = 0;
          foreach (var lst in ActiveRegionsStats)
          {
            if(lst.Count > 0)
            {
              App.MapRegions.BackingActiveRegionsCount[j] = lst.Count.ToString();
              App.MapRegions.BackingActiveRegionsMean[j] = lst.Average().ToString("0,0");
            }
            j++;
          }
          ResVM.ResultsWaitIndicatorVisibility = false;
        }
        else
        {
          for (var i = 0; i < ResVM.CurrentReporter.Count; i++)
          {
            ResVM.CurrentReporter[i].Value += reporter[i];
            ResVM.CurrentForwardSsc[i].Value += fsc[i];
            ResVM.CurrentRedSsc[i].Value += red[i];
            ResVM.CurrentGreenSsc[i].Value += green[i];
            ResVM.CurrentVioletSsc[i].Value += violet[i];
          }
        }
      }));
    }

    public static void AnalyzeHeatMap(List<HeatMapData> heatmap, bool hiRez = false)
    {
      if (heatmap != null && heatmap.Count > 0)
      {
        int max = 0;
        
        foreach (var p in heatmap)
        {
          if (p.A > max)
            max = p.A;
        }
        double[] bins = GenerateLogSpaceD(1, max + 1, _heatColors.Length, true);
        Views.ResultsView.Instance.ClearPoints();
        for (var i = 0; i < heatmap.Count; i++)
        {
          for (var j = 0; j < _heatColors.Length; j++)
          {
            //int cutoff = (heatmap[i].X > 100 && heatmap[i].Y > 100) ? ViewModels.ResultsViewModel.Instance.XYCutoff : 2;
            if (heatmap[i].A <= bins[j])
            {
              if (!hiRez && j == 0)
                break;
              if (ViewModels.ResultsViewModel.Instance.FlipMapAnalysis)
                Views.ResultsView.Instance.AddXYPoint(heatmap[i].Y, heatmap[i].X, _heatColors[j], hiRez);
              else
                Views.ResultsView.Instance.AddXYPoint(heatmap[i].X, heatmap[i].Y, _heatColors[j], hiRez);
              break;
            }
          }
        }
      }
    }

    public static void BinMapData(List<MicroCy.BeadInfoStruct> BeadInfoList, bool current = true, bool hiRez = false)
    {
      var ResVM = ViewModels.ResultsViewModel.Instance;
      var bins = hiRez ? HeatMapData.HiRezBins : HeatMapData.bins;
      var boundary = hiRez ? HeatMapData.HiRezBins.Length - 1 : HeatMapData.bins.Length - 1;
      foreach (var bead in BeadInfoList)
      {
        /*
        int cl0 = Array.BinarySearch(bins, bead.cl0);
        if (cl0 < 0)
          cl0 = ~cl0;
        cl0 = cl0 > boundary ? boundary : cl0;
        */
        int cl1 = Array.BinarySearch(bins, bead.cl1);
        if (cl1 < 0)
          cl1 = ~cl1;
        cl1 = cl1 > boundary ? boundary : cl1;
        int cl2 = Array.BinarySearch(bins, bead.cl2);
        if (cl2 < 0)
          cl2 = ~cl2;
        cl2 = cl2 > boundary ? boundary : cl2;
        /*
        int cl3 = Array.BinarySearch(bins, bead.cl3);
        if (cl3 < 0)
          cl3 = ~cl3;
        cl3 = cl3 > boundary ? boundary : cl3;
        */

        if (current)
        {
          /*
          //01
          if (!ResVM.CurrentCL01Dict.ContainsKey((cl0, cl1)))
          {
            ResVM.CurrentCL01Dict.Add((cl0, cl1), ResVM.CurrentCL01Map.Count);
            ResVM.CurrentCL01Map.Add(new HeatMapData((int)bins[cl0], (int)bins[cl1]));
          }
          else
          {
            ResVM.CurrentCL01Map[ResVM.CurrentCL01Dict[(cl0, cl1)]].A++;
          }
          //02
          if (!ResVM.CurrentCL02Dict.ContainsKey((cl0, cl2)))
          {
            ResVM.CurrentCL02Dict.Add((cl0, cl2), ResVM.CurrentCL02Map.Count);
            ResVM.CurrentCL02Map.Add(new HeatMapData((int)bins[cl0], (int)bins[cl2]));
          }
          else
          {
            ResVM.CurrentCL02Map[ResVM.CurrentCL02Dict[(cl0, cl2)]].A++;
          }
          //03
          if (!ResVM.CurrentCL03Dict.ContainsKey((cl0, cl3)))
          {
            ResVM.CurrentCL03Dict.Add((cl0, cl3), ResVM.CurrentCL03Map.Count);
            ResVM.CurrentCL03Map.Add(new HeatMapData((int)bins[cl0], (int)bins[cl3]));
          }
          else
          {
            ResVM.CurrentCL03Map[ResVM.CurrentCL03Dict[(cl0, cl3)]].A++;
          }
          */
          //12
          if (!ResVM.CurrentCL12Dict.ContainsKey((cl1, cl2)))
          {
            ResVM.CurrentCL12Dict.Add((cl1, cl2), ResVM.CurrentCL12Map.Count);
            ResVM.CurrentCL12Map.Add(new HeatMapData((int)bins[cl1], (int)bins[cl2]));
          }
          else
          {
            ResVM.CurrentCL12Map[ResVM.CurrentCL12Dict[(cl1, cl2)]].A++;
          }
          /*
          //13
          if (!ResVM.CurrentCL13Dict.ContainsKey((cl1, cl3)))
          {
            ResVM.CurrentCL13Dict.Add((cl1, cl3), ResVM.CurrentCL13Map.Count);
            ResVM.CurrentCL13Map.Add(new HeatMapData((int)bins[cl1], (int)bins[cl3]));
          }
          else
          {
            ResVM.CurrentCL13Map[ResVM.CurrentCL13Dict[(cl1, cl3)]].A++;
          }
          //23
          if (!ResVM.CurrentCL23Dict.ContainsKey((cl2, cl3)))
          {
            ResVM.CurrentCL23Dict.Add((cl2, cl3), ResVM.CurrentCL23Map.Count);
            ResVM.CurrentCL23Map.Add(new HeatMapData((int)bins[cl2], (int)bins[cl3]));
          }
          else
          {
            ResVM.CurrentCL23Map[ResVM.CurrentCL23Dict[(cl2, cl3)]].A++;
          }
          */
        }
        else
        {
          /*
          //01
          if (!ResVM.BackingCL01Dict.ContainsKey((cl0, cl1)))
          {
            ResVM.BackingCL01Dict.Add((cl0, cl1), ResVM.BackingCL01Map.Count);
            ResVM.BackingCL01Map.Add(new HeatMapData((int)bins[cl0], (int)bins[cl1]));
          }
          else
          {
            ResVM.BackingCL01Map[ResVM.BackingCL01Dict[(cl0, cl1)]].A++;
          }
          //02
          if (!ResVM.BackingCL02Dict.ContainsKey((cl0, cl2)))
          {
            ResVM.BackingCL02Dict.Add((cl0, cl2), ResVM.BackingCL02Map.Count);
            ResVM.BackingCL02Map.Add(new HeatMapData((int)bins[cl0], (int)bins[cl2]));
          }
          else
          {
            ResVM.BackingCL02Map[ResVM.BackingCL02Dict[(cl0, cl2)]].A++;
          }
          //03
          if (!ResVM.BackingCL03Dict.ContainsKey((cl0, cl3)))
          {
            ResVM.BackingCL03Dict.Add((cl0, cl3), ResVM.BackingCL03Map.Count);
            ResVM.BackingCL03Map.Add(new HeatMapData((int)bins[cl0], (int)bins[cl3]));
          }
          else
          {
            ResVM.BackingCL03Map[ResVM.BackingCL03Dict[(cl0, cl3)]].A++;
          }
          */
          //12
          if (!ResVM.BackingCL12Dict.ContainsKey((cl1, cl2)))
          {
            ResVM.BackingCL12Dict.Add((cl1, cl2), ResVM.BackingCL12Map.Count);
            ResVM.BackingCL12Map.Add(new HeatMapData((int)bins[cl1], (int)bins[cl2]));
          }
          else
          {
            ResVM.BackingCL12Map[ResVM.BackingCL12Dict[(cl1, cl2)]].A++;
          }
          /*
          //13
          if (!ResVM.BackingCL13Dict.ContainsKey((cl1, cl3)))
          {
            ResVM.BackingCL13Dict.Add((cl1, cl3), ResVM.BackingCL13Map.Count);
            ResVM.BackingCL13Map.Add(new HeatMapData((int)bins[cl1], (int)bins[cl3]));
          }
          else
          {
            ResVM.BackingCL13Map[ResVM.BackingCL13Dict[(cl1, cl3)]].A++;
          }
          //23
          if (!ResVM.BackingCL23Dict.ContainsKey((cl2, cl3)))
          {
            ResVM.BackingCL23Dict.Add((cl2, cl3), ResVM.BackingCL23Map.Count);
            ResVM.BackingCL23Map.Add(new HeatMapData((int)bins[cl2], (int)bins[cl3]));
          }
          else
          {
            ResVM.BackingCL23Map[ResVM.BackingCL23Dict[(cl2, cl3)]].A++;
          }
          */

          //3DReporterPlot
          var index = ResVM.BackingWResults.FindIndex(x => x.regionNumber == bead.region);
          if(index != -1)
          {
            ResVM.BackingWResults[index].RP1vals.Add(bead.reporter);
          }
          
        }
      }
    }
  }
}