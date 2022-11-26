using Ei_Dimension.Models;
using System;
using System.Collections.Generic;
using DIOS.Core;
using Ei_Dimension.Controllers;
using Ei_Dimension.ViewModels;
using Ei_Dimension.Graphing.HeatMap;

namespace Ei_Dimension.Core
{
  public static class DataProcessor
  {
    private static readonly char[] _separator = {','};
    private const string _100plexAMapName = "D100Aplex";
    private const string _100plexBMapName = "D100Bplex";
    private static readonly HashSet<int> WeightedRegions = new HashSet<int> {1,2,3,4,5,6,7,8,10,11,16,17,23,24,31,32,40,41,50,60,71};

    public static int FromCLSpaceToReal(int pointInClSpace, double[] bins)
    {
      return (int)bins[pointInClSpace];
    }

    public static List<string> GetDataFromFile(string path)
    {
      var str = new List<string>(100000);
      using (var fin = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read,
               System.IO.FileShare.Read))
      using (var sr = new System.IO.StreamReader(fin))
      {
        // ReSharper disable once MethodHasAsyncOverload
        sr.ReadLine();
        while (!sr.EndOfStream)
        {
          str.Add(sr.ReadLine());
        }
      }
      return str;
    }

    public static ProcessedBead ParseRow(string data)
    {
      var numFormat = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
      string[] words = data.Split(_separator);
      int i = 0;
      ProcessedBead binfo = new ProcessedBead
      {
        EventTime = uint.Parse(words[i++]),
        fsc_bg = byte.Parse(words[i++]),
        vssc_bg = byte.Parse(words[i++]),
        cl0_bg = byte.Parse(words[i++]),
        cl1_bg = byte.Parse(words[i++]),
        cl2_bg = byte.Parse(words[i++]),
        cl3_bg = byte.Parse(words[i++]),
        rssc_bg = byte.Parse(words[i++]),
        gssc_bg = byte.Parse(words[i++]),
        greenB_bg = ushort.Parse(words[i++]),
        greenC_bg = ushort.Parse(words[i++]),
        greenB = ushort.Parse(words[i++]),
        greenC = ushort.Parse(words[i++]),
        l_offset_rg = byte.Parse(words[i++]),
        l_offset_gv = byte.Parse(words[i++]),

        region = (ushort.Parse(words[i])) % ProcessedBead.ZONEOFFSET, //16123
        zone = (ushort.Parse(words[i++])) / ProcessedBead.ZONEOFFSET,

        fsc = float.Parse(words[i++], numFormat),
        violetssc = float.Parse(words[i++], numFormat),
        cl0 = float.Parse(words[i++], numFormat),
        redssc = float.Parse(words[i++], numFormat),
        cl1 = float.Parse(words[i++], numFormat),
        cl2 = float.Parse(words[i++], numFormat),
        cl3 = float.Parse(words[i++], numFormat),
        greenssc = float.Parse(words[i++], numFormat),
        reporter = float.Parse(words[i], numFormat),
      };
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

    public static void GenerateLogSpaceD(int min, int max, int logBins, double[] results, bool baseE = false)
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
      if (results == null || results.Length != logBins)
        throw new Exception("results array is wrong size or null");
      for (int i = 1; i <= logBins; ++i)
      {
        results[i - 1] = Math.Pow(logarithmicBase, logMin + accDelta);
        accDelta += delta;
      }
    }

    public static void CalculateStatistics(List<ProcessedBead> list)
    {
      var accumulator = new StatsAccumulator();
      foreach (var bead in list)
      {
        accumulator.Add(bead);
      }
      var stats = accumulator.CalculateStats();
      _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        ResultsViewModel.Instance.DecodeCalibrationStats(stats, current: false);
      }));
    }

    public static void BinScatterData(List<ProcessedBead> list, bool fromFile = false)
    {
      var ScatterDataCount = ScatterData.CurrentReporter.Count;
      var MaxValue = ScatterData.CurrentReporter[ScatterDataCount - 1].Argument;
      int[] reporter, fsc, red, green, violet;
      //NULL Region is included in RegionsList
      var RegionsReporter = new List<RegionReporterResult>(MapRegionsController.RegionsList.Count);  //for mean and count 
      if (fromFile)
      {
        reporter = ScatterData.bReporter;
        fsc = ScatterData.bFsc;
        red = ScatterData.bRed;
        green = ScatterData.bGreen;
        violet = ScatterData.bViolet;

        for (var i = 0; i < MapRegionsController.RegionsList.Count; i++)
        {
          RegionsReporter.Add(new RegionReporterResult
          {
            regionNumber = MapRegionsController.RegionsList[i].Number
          });
        }
      }
      else
      {
        reporter = ScatterData.cReporter;
        fsc = ScatterData.cFsc;
        red = ScatterData.cRed;
        green = ScatterData.cGreen;
        violet = ScatterData.cViolet;
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

        int i = Array.BinarySearch(HistogramData.Bins, (int)bead.fsc);
        if (i < 0)
          i = ~i;
        int j = Array.BinarySearch(HistogramData.Bins, (int)bead.redssc);
        if (j < 0)
          j = ~j;
        int k = Array.BinarySearch(HistogramData.Bins, (int)bead.greenssc);
        if (k < 0)
          k = ~k;
        int o = Array.BinarySearch(HistogramData.Bins, (int)bead.violetssc);
        if (o < 0)
          o = ~o;
        int l = Array.BinarySearch(HistogramData.Bins, (int)bead.reporter);
        if (l < 0)
          l = ~l;

        fsc[i]++;
        red[j]++;
        green[k]++;
        violet[o]++;
        reporter[l]++;


        if (fromFile)
        {
          var index = MapRegionsController.GetMapRegionIndex(bead.region);
          if (index != -1)
            RegionsReporter[index].ReporterValues.Add(bead.reporter);
          //else if (bead.region == 0)
          //  continue;
          //else
          //  failed = true;
        }
      }
      //if (failed)
      //  System.Windows.MessageBox.Show("An error occured during Well File Read");

      var Stats = new List<RegionReporterStats>(RegionsReporter.Count);
      if (fromFile)
      {
        foreach (var reporterValues in RegionsReporter)
        {
          Stats.Add(new RegionReporterStats(reporterValues));
        }
      }

      _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        ScatterChartViewModel.Instance.ScttrData.FillCurrentData(fromFile);
        if (fromFile)
        {
          var j = 0;
          foreach (var stat in Stats)
          {
            ActiveRegionsStatsController.Instance.BackingCount[j] = stat.Count.ToString();
            ActiveRegionsStatsController.Instance.BackingMean[j] = stat.MeanFi.ToString("0.0");
            j++;
          }
          ResultsViewModel.Instance.ResultsWaitIndicatorVisibility = false;
        }
      }));
    }

    public static void BinMapData(List<ProcessedBead> beadInfoList, bool current = true, bool hiRez = false)
    {
      //Puts points to Lists instead of filling 256x256 arrays.
      //traversing [,] array would be a downside, and condition check would also be included for every step.
      //Traversing a list is better.
      var analysisMap = ResultsViewModel.Instance.AnalysisMap;
      var bins = hiRez ? HeatMapPoint.HiRezBins : HeatMapPoint.bins;
      var boundary = hiRez ? HeatMapPoint.HiRezBins.Length - 1 : HeatMapPoint.bins.Length - 1;
      var i = 0;

      while (i < beadInfoList.Count)
      {
        var region = beadInfoList[i].region;

        int cl0 = Array.BinarySearch(bins, beadInfoList[i].cl0);
        if (cl0 < 0)
          cl0 = ~cl0;
        cl0 = cl0 > boundary ? boundary : cl0;

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

        //foreach (MapIndex mapIndex in (MapIndex[])Enum.GetValues(typeof(MapIndex)))
        //{
        //  HeatMap.AddPoint((cl1, cl2), bins, mapIndex, current);
        //}
        HeatMapAPI.API.AddDataPoint((cl0, cl1), bins, MapIndex.CL01, current);
        HeatMapAPI.API.AddDataPoint((cl0, cl2), bins, MapIndex.CL02, current);
        HeatMapAPI.API.AddDataPoint((cl1, cl2), bins, MapIndex.CL12, current);

        //More weight to points on 100Plex lowerleft regions
        if (AddWeightToRegions(region))
        {
          HeatMapAPI.API.AddDataPoint((cl1, cl2), bins, MapIndex.CL12, current);
          HeatMapAPI.API.AddDataPoint((cl1, cl2), bins, MapIndex.CL12, current);
        }

        //3DReporterPlot
        if (!current)
        {
          var index = analysisMap.BackingWResults.FindIndex(x => x.regionNumber == beadInfoList[i].region);
          if (index != -1)
          {
            analysisMap.BackingWResults[index].ReporterValues.Add(beadInfoList[i].reporter);
          }
        }
        i++;
      }
    }

    private static bool AddWeightToRegions(int region)
    {
      if (App.DiosApp.MapController.ActiveMap.mapName != _100plexAMapName
          && App.DiosApp.MapController.ActiveMap.mapName != _100plexBMapName)
        return false;
      if(WeightedRegions.Contains(region))
        return true;
      return false;
    }
  }
}