using Ei_Dimension.Models;
using System;
using System.Collections.Generic;
using DIOS.Application;
using DIOS.Core;
using Ei_Dimension.Controllers;
using Ei_Dimension.ViewModels;
using Ei_Dimension.Graphing.HeatMap;
using System.Linq;

namespace Ei_Dimension.Core;

public static class DataProcessor
{
  private const string _100plexAMapName = "D100Aplex";
  private const string _100plexBMapName = "D100Bplex";
  private static readonly HashSet<int> WeightedRegions = new HashSet<int> {1,2,3,4,5,6,7,8,10,11,16,17,23,24,31,32,40,41,50,60,71};

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

  public static ChannelsCalibrationStats CalculateStatistics(ReadOnlySpan<ProcessedBead> list)
  {
    var accumulator = new StatsAccumulator();
    foreach (var bead in list)
    {
      accumulator.Add(bead);
    }
    return accumulator.CalculateStats();
  }

  public static void BinScatterData(ReadOnlySpan<ProcessedBead> inputBeadList, bool fromFile = false)
  {
    int[] reporter, fsc, red, green, violet;
    //NULL Region is included in RegionsList
    List<RegionReporterResult> RegionsReporter = null;  //for mean and count 
    if (fromFile)
    {
      RegionsReporter = new List<RegionReporterResult>(MapRegionsController.RegionsList.Count);
      reporter = ScatterData.bReporter;
      fsc = ScatterData.bFsc;
      red = ScatterData.bRed;
      green = ScatterData.bGreen;
      violet = ScatterData.bViolet;

      for (var i = 0; i < MapRegionsController.RegionsList.Count; i++)
      {
        RegionsReporter.Add(new RegionReporterResult(MapRegionsController.RegionsList[i].Number));
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
    foreach (var processedBead in inputBeadList)
    {
      FillBinArray(fsc, processedBead.fsc);
      FillBinArray(red, processedBead.redssc);
      FillBinArray(green, processedBead.greenssc);
      FillBinArray(violet, processedBead.violetssc);
      FillBinArray(reporter, processedBead.reporter);

      if (fromFile)
      {
        var index = MapRegionsController.GetMapRegionIndex(processedBead.region);
        if (index != -1)
          RegionsReporter[index].Add(processedBead.reporter);
        //else if (bead.region == 0)
        //  continue;
        //else
        //  failed = true;
      }
    }
    //if (failed)
    //  System.Windows.MessageBox.Show("An error occured during Well File Read");

    if (fromFile)
    {
      var Stats = new List<RegionReporterStats>(RegionsReporter.Count);
      foreach (var reporterValues in RegionsReporter)
      {
        Stats.Add(new RegionReporterStats(reporterValues));
      }

      ActiveRegionsStatsController.Instance.UpdateBackgroundStats(Stats);
    }
  }

  public static void FillBinArray(int[] array, float signal)
  {
    //overflow protection
    signal = signal < HistogramData.UPPERLIMIT ? signal : HistogramData.UPPERLIMIT;
    int index = FindPlaceInBin(signal);
    array[index]++;
  }

  private static int FindPlaceInBin(float value)
  {
    var binNumber = Array.BinarySearch(HistogramData.Bins, (int)value);
    if (binNumber < 0)
      binNumber = ~binNumber;
    return binNumber;
  }

  public static int GetSignalPeak(int[] array)
  {
    var index = FindPeakIndex(array);
    return FindPeak(index);
  }

  private static int FindPeakIndex(int[] array)
  {
    return Array.IndexOf(array, array.Max());
  }

  private static int FindPeak(int peakIndex)
  {
    return HistogramData.Bins[peakIndex];
  }

  public static void BinMapData(ReadOnlySpan<ProcessedBead> beadInfoList, bool current = true, bool hiRez = false)
  {
    //Puts points to Lists instead of filling 256x256 arrays.
    //traversing [,] array would be a downside, and condition check would also be included for every step.
    //Traversing a list is better.
    var analysisMap = ResultsViewModel.Instance.AnalysisMap;
    var bins = hiRez ? HeatMapPoint.HiRezBins : HeatMapPoint.bins;
    var boundary = hiRez ? HeatMapPoint.HiRezBins.Length - 1 : HeatMapPoint.bins.Length - 1;
    var i = 0;

    while (i < beadInfoList.Length)
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
        analysisMap.BackingWResults.Add(beadInfoList[i]);
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