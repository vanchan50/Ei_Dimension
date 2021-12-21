using Ei_Dimension.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Ei_Dimension.ViewModels;

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

    public static async Task<List<string>> GetDataFromFileAsync(string path)
    {
      var str = new List<string>();
      using (var fin = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read,
               System.IO.FileShare.Read))
      using (var sr = new System.IO.StreamReader(fin))
      {
        // ReSharper disable once MethodHasAsyncOverload
        sr.ReadLine();
        while (!sr.EndOfStream)
        {
          str.Add(await sr.ReadLineAsync());
        }
      }
      return str;
    }

    public static MicroCy.BeadInfoStruct ParseRow(string data)
    {
      MicroCy.BeadInfoStruct binfo;
      string[] words = data.Split(',');
      binfo.Header = uint.Parse(words[0]);
      binfo.EventTime = uint.Parse(words[1]);
      binfo.fsc_bg = byte.Parse(words[2]);
      binfo.vssc_bg = byte.Parse(words[3]);
      binfo.cl0_bg = byte.Parse(words[4]);
      binfo.cl1_bg = byte.Parse(words[5]);
      binfo.cl2_bg = byte.Parse(words[6]);
      binfo.cl3_bg = byte.Parse(words[7]);
      binfo.rssc_bg = byte.Parse(words[8]);
      binfo.gssc_bg = byte.Parse(words[9]);
      binfo.greenB_bg = ushort.Parse(words[10]);
      binfo.greenC_bg = ushort.Parse(words[11]);
      binfo.greenB = ushort.Parse(words[12]);
      binfo.greenC = ushort.Parse(words[13]);
      binfo.l_offset_rg = byte.Parse(words[14]);
      binfo.l_offset_gv = byte.Parse(words[15]);
      binfo.region = ushort.Parse(words[16]);
      binfo.fsc = float.Parse(words[17], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      binfo.violetssc = float.Parse(words[18], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      binfo.cl0 = float.Parse(words[19], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      binfo.redssc = float.Parse(words[20], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      binfo.cl1 = float.Parse(words[21], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      binfo.cl2 = float.Parse(words[22], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      binfo.cl3 = float.Parse(words[23], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      binfo.greenssc = float.Parse(words[24], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      binfo.reporter = float.Parse(words[25], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      return binfo;
    }

    public static List<HistogramData> LinearizeDictionary(SortedDictionary<int,int> dict)
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
      int[] result = new int[logBins];
      for (var i = 1; i <= logBins; ++i)
      {
        result[i - 1] = (int)Math.Round(Math.Pow(logarithmicBase, logMin + accDelta));
        accDelta += delta;
      }
      return result;
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
      var ScatterData = ResVM.ScttrData;
      var ScatterDataCount = ScatterData.CurrentReporter.Count;
      var MaxValue = ScatterData.CurrentReporter[ScatterDataCount - 1].Argument;
      int[] reporter, fsc, red, green, violet;
      if (fromFile)
      {
        reporter = ScatterData.bReporter;
        fsc = ScatterData.bFsc;
        red = ScatterData.bRed;
        green = ScatterData.bGreen;
        violet = ScatterData.bViolet;
      }
      else
      {
        reporter = ScatterData.cReporter;
        fsc = ScatterData.cFsc;
        red = ScatterData.cRed;
        green = ScatterData.cGreen;
        violet = ScatterData.cViolet;
      }

      List<List<float>> activeRegionsStats = new List<List<float>>();  //for mean and count
      foreach (var region in App.Device.ActiveMap.regions)
      {
        activeRegionsStats.Add(new List<float>());
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
          activeRegionsStats[index].Add(beadD.reporter);
        //else if (beadD.region == 0)
        //  continue;
        //else
        //  failed = true;
      }
      //if (failed)
      //  System.Windows.MessageBox.Show("An error occured during Well File Read");

      _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        ResVM.ScttrData.FillCurrentData(fromFile);
        if (fromFile)
        {
          var j = 0;
          foreach (var lst in activeRegionsStats)
          {
            if (lst.Count > 0)
            {
              App.MapRegions.BackingActiveRegionsCount[j] = lst.Count.ToString();
              App.MapRegions.BackingActiveRegionsMean[j] = lst.Average().ToString("0,0");
            }
            j++;
          }
          ResVM.ResultsWaitIndicatorVisibility = false;
        }
      }));
    }

    public static void AnalyzeHeatMap(bool hiRez = false)
    {
      var heatMap = ResultsViewModel.Instance.DisplayedMap;
      if (heatMap != null && heatMap.Count > 0)
      {
        int max = heatMap.Select(p => p.A).Max();

        var bins = GenerateLogSpaceD(1, max + 1, _heatColors.Length, true);
        Views.ResultsView.Instance.ClearPoints();
        for (var i = 0; i < heatMap.Count; i++)
        {
          for (var j = 0; j < _heatColors.Length; j++)
          {
            //int cutoff = (heatMap[i].X > 100 && heatMap[i].Y > 100) ? ViewModels.ResultsViewModel.Instance.XYCutoff : 2;
            if (heatMap[i].A <= bins[j])
            {
              if (!hiRez && j == 0)
                break;
              if (ResultsViewModel.Instance.WrldMap.Flipped)
                Views.ResultsView.Instance.AddXYPoint(heatMap[i].Y, heatMap[i].X, _heatColors[j], hiRez);
              else
                Views.ResultsView.Instance.AddXYPoint(heatMap[i].X, heatMap[i].Y, _heatColors[j], hiRez);
              break;
            }
          }
        }
      }
    }

    public static void BinMapData(List<MicroCy.BeadInfoStruct> beadInfoList, bool current = true, bool hiRez = false)
    {
      var resVm = ResultsViewModel.Instance;
      var bins = hiRez ? HeatMapData.HiRezBins : HeatMapData.bins;
      var boundary = hiRez ? HeatMapData.HiRezBins.Length - 1 : HeatMapData.bins.Length - 1;
      var i = 0;
      while (i < beadInfoList.Count)
      {
        /*
        int cl0 = Array.BinarySearch(bins, bead.cl0);
        if (cl0 < 0)
          cl0 = ~cl0;
        cl0 = cl0 > boundary ? boundary : cl0;
        */
        int cl1 = Array.BinarySearch(bins, beadInfoList[i].cl1);
        if (cl1 < 0)
          cl1 = ~cl1;
        cl1 = cl1 > boundary ? boundary : cl1;
        int cl2 = Array.BinarySearch(bins, beadInfoList[i].cl2);
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
          if (!resVm.CurrentCL12Dict.ContainsKey((cl1, cl2)))
          {
            resVm.CurrentCL12Dict.Add((cl1, cl2), resVm.CurrentCL12Map.Count);
            resVm.CurrentCL12Map.Add(new HeatMapData((int)bins[cl1], (int)bins[cl2]));
          }
          else
          {
            resVm.CurrentCL12Map[resVm.CurrentCL12Dict[(cl1, cl2)]].A++;
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
          if (!resVm.BackingCL12Dict.ContainsKey((cl1, cl2)))
          {
            resVm.BackingCL12Dict.Add((cl1, cl2), resVm.BackingCL12Map.Count);
            resVm.BackingCL12Map.Add(new HeatMapData((int)bins[cl1], (int)bins[cl2]));
          }
          else
          {
            resVm.BackingCL12Map[resVm.BackingCL12Dict[(cl1, cl2)]].A++;
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
          var index = resVm.BackingWResults.FindIndex(x => x.regionNumber == beadInfoList[i].region);
          if(index != -1)
          {
            resVm.BackingWResults[index].RP1vals.Add(beadInfoList[i].reporter);
          }
        }
        i++;
      }
    }
  }
}