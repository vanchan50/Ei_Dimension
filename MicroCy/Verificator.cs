using System;
using System.Collections.Generic;

namespace DIOS.Core
{
  public static class Verificator
  {
    private static readonly List<ValidationStats> RegionalStats = new List<ValidationStats>(50);
    private static Dictionary<int, int> _classifiedRegionsDict = new Dictionary<int, int>();
    private static Dictionary<int, int> _unclassifiedRegionsDict = new Dictionary<int, int>();  //region,count
    private static int _highestCount = 0;
    private static int _lowestCount = int.MaxValue;
    private static int _lowestCountRegion = -1;
    private static int _highestCountRegion = -1;
    private static int _totalClassifiedBeads;
    private static int _totalUnclassifiedBeads;

    public static void Reset(List<(int regionNum, double InputReporter)> regions)
    {
      RegionalStats.Clear();
      _totalClassifiedBeads = 0;
      _totalUnclassifiedBeads = 0;
      _highestCount = 0;
      _lowestCount = int.MaxValue;
      _lowestCountRegion = -1;
      _highestCountRegion = -1;
      _classifiedRegionsDict.Clear();
      _unclassifiedRegionsDict.Clear();
      foreach (var reg in regions)
      {
        _classifiedRegionsDict.Add(reg.regionNum, RegionalStats.Count);
        RegionalStats.Add(new ValidationStats(reg.regionNum, reg.InputReporter));
      }
    }

    public static bool ReporterToleranceTest(double errorThresholdPercent, out string msg)
    {
      msg = null;
      if (errorThresholdPercent < 0)
        throw new ArgumentException("Error threshold must not be negative");
      bool passed = true;
      var thresholdMultiplier = errorThresholdPercent <= 100 ? 1 - (errorThresholdPercent / 100) : (errorThresholdPercent / 100);  //reverse percentage
      var problematicRegions = new List<int>();
      foreach (var reg in RegionalStats)
      {
        var ReporterMedian = GetMedianReporterForRegion(reg.Region);
        if (ReporterMedian <= reg.InputReporter * thresholdMultiplier)
        {
          //Console.WriteLine($"Verification Fail. Test 1 Reporter tolerance\nReporter value ({ReporterMedian.ToString()}) deviation is more than Threshold is {errorThresholdPercent.ToString($"{0:0.00}")}% from the target ({reg.InputReporter})");
          problematicRegions.Add(reg.Region);
          passed = false;
        }
      }

      if (problematicRegions.Count != 0)
      {
        msg = "Test1 Regions: ";
        foreach (var region in problematicRegions)
        {
          msg = msg + region.ToString() + ",";
        }
        msg = msg.Remove(msg.Length - 1);
        msg = msg += $" measured {thresholdMultiplier}% below target reporter value";
      }
      return passed;
    }

    public static bool ClassificationToleranceTest(double errorThresholdPercent, out string msg)
    {
      msg = null;
      if (errorThresholdPercent < 0)
        throw new ArgumentException("Error threshold must not be negative");
      bool passed = true;

      //var difference = (_highestCount - _lowestCount) / (double)_lowestCount;
      var difference = _totalUnclassifiedBeads / _totalClassifiedBeads;
      var difPercent = difference * 100;
      if (difPercent > errorThresholdPercent)
      {
        passed = false;
        //Console.WriteLine($"Verification Fail. Test 2 Classification tolerance\nMax difference between region counts is {difPercent.ToString()}%, Threshold is {errorThresholdPercent.ToString($"{0:0.00}")}");
        msg = $"Test2 {difPercent.ToString()}% of verification events outside target regions";
      }
      return passed;
    }

    public static bool MisclassificationToleranceTest(double errorThresholdPercent, out string msg)
    {
      msg = null;
      if (errorThresholdPercent < 0)
        throw new ArgumentException("Error threshold must not be negative");
      //var thresholdMultiplier = errorThresholdPercent <= 100 ? 1 - (errorThresholdPercent / 100) : (errorThresholdPercent / 100);  //reverse percentage
      bool passed = true;

      var problematicRegions = new List<int>();

      foreach (var reg in _unclassifiedRegionsDict)
      {
        if ( ((reg.Value / (double)_lowestCount) * 100 ) > errorThresholdPercent)
        {
          passed = false;
          //Console.WriteLine($"Verification Fail. Test 3 Misclassification tolerance\nRegion #{reg.Key} Count is higher than the threshold {errorThresholdPercent.ToString($"{0:0.00}")}%");
          problematicRegions.Add(reg.Key);
        }
      }


      if (problematicRegions.Count != 0)
      {
        msg = $"Test3 {errorThresholdPercent}% of region {_lowestCountRegion} events misclassified into regions : ";
        foreach (var region in problematicRegions)
        {
          msg = msg + region.ToString() + ",";
        }
        msg = msg.Remove(msg.Length - 1);
      }
      return passed;
    }

    /// <summary>
    /// Called on every Read from USB, in Verification mode. Not used in other modes
    /// </summary>
    /// <param name="outbead"></param>
    internal static void FillStats(in BeadInfoStruct outbead)
    {
      //if region is classified
      if (_classifiedRegionsDict.TryGetValue(outbead.region, out var index))
      {
        RegionalStats[index].FillCalibrationStatsRow(in outbead);
        return;
      }

      //if region is UNclassified
      if (_unclassifiedRegionsDict.ContainsKey(outbead.region))
      {
        _unclassifiedRegionsDict[outbead.region]++;
        return;
      }
      _unclassifiedRegionsDict.Add(outbead.region, 1);
    }

    internal static void CalculateResults()
    {
      //RegionalStats holds regions with defined Reporter target
      foreach (var region in RegionalStats)
      {
        region.CalculateResults();
        _totalClassifiedBeads += region.Count;

        //calculate highest and lowest count for classified regions
        if (region.Count > _highestCount)
        {
          _highestCount = region.Count;
          _highestCountRegion = region.Region;
          continue;
        }

        if (region.Count < _lowestCount)
        {
          _lowestCount = region.Count;
          _lowestCountRegion = region.Region;
        }
      }

      foreach (var entry in _unclassifiedRegionsDict)
      {
        _totalUnclassifiedBeads += entry.Value;
      }
    }
    private static double GetMedianReporterForRegion(int regionNum)
    {
      var index = RegionalStats.FindIndex(x => x.Region == regionNum);
      return RegionalStats[index].Stats[0].mfi;
    }
  }
}