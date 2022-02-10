using System;
using System.Collections.Generic;

namespace MicroCy
{
  public static class Validator
  {
    public static readonly List<ValidationStats> RegionalStats = new List<ValidationStats>(50);
    public static int TotalClassifiedBeads;
    private static Dictionary<int, int> _dict = new Dictionary<int, int>();
    private static Dictionary<int, int> _unclassifiedRegionsDict = new Dictionary<int, int>();
    private static int _highestCount = 0;
    private static int _lowestCount = int.MaxValue;

    public static void Reset(List<(int regionNum, double InputReporter)> regions)
    {
      RegionalStats.Clear();
      TotalClassifiedBeads = 0;
      _highestCount = 0;
      _lowestCount = int.MaxValue;
      _dict.Clear();
      _unclassifiedRegionsDict.Clear();
      foreach (var reg in regions)
      {
        _dict.Add(reg.regionNum, RegionalStats.Count);
        RegionalStats.Add(new ValidationStats(reg.regionNum, reg.InputReporter));
      }
    }

    public static double GetMedianReporterForRegion(int regionNum)
    {
      var index = RegionalStats.FindIndex(x => x.Region == regionNum);
      return RegionalStats[index].Stats[0].mfi;
    }

    public static void FillStats(in BeadInfoStruct outbead)
    {
      if (_dict.TryGetValue(outbead.region, out var index))
      {
        RegionalStats[index].FillCalibrationStatsRow(in outbead);
        return;
      }

      if (_unclassifiedRegionsDict.ContainsKey(outbead.region))
      {
        _unclassifiedRegionsDict[outbead.region]++;
        return;
      }
      _unclassifiedRegionsDict.Add(outbead.region, 1);
    }

    public static void CalculateResults()
    {
      foreach (var s in RegionalStats)
      {
        s.CalculateResults();
        TotalClassifiedBeads += s.Count;
      }
    }

    public static bool ReporterToleranceTest(double errorThresholdPercent)
    {
      if (errorThresholdPercent < 0 || errorThresholdPercent > 100)
        throw new ArgumentException("Error threshold must be in range [0,100]");
      bool passed = true;
      var thresholdMultiplier = 1 - (errorThresholdPercent / 100);  //reverse percentage
      foreach (var reg in RegionalStats)
      {
        var ReporterMedian = GetMedianReporterForRegion(reg.Region);
        if (ReporterMedian <= reg.InputReporter * thresholdMultiplier)
        {
          Console.WriteLine($"Validation Fail. Test 1 Reporter tolerance\nReporter value ({ReporterMedian.ToString()}) deviation is more than Threshold is {errorThresholdPercent.ToString($"{0:0.00}")}% from the target ({reg.InputReporter})");
          passed = false;
        }
      }
      return passed;
    }

    public static bool ClassificationToleranceTest(double errorThresholdPercent)
    {
      if (errorThresholdPercent < 0)
        throw new ArgumentException("Error threshold must not be negative");
      bool passed = true;
      foreach (var region in RegionalStats)
      {
        if (region.Count > _highestCount)
        {
          _highestCount = region.Count;
          continue;
        }
        if (region.Count < _lowestCount)
          _lowestCount = region.Count;
      }

      var difference = (_highestCount - _lowestCount) / (double)_lowestCount;
      var difPercent = difference * 100;
      if (difPercent > errorThresholdPercent)
      {
        passed = false;
        Console.WriteLine($"Validation Fail. Test 2 Classification tolerance\nMax difference between region counts is {difPercent.ToString()}%, Threshold is {errorThresholdPercent.ToString($"{0:0.00}")}");
      }
      return passed;
    }

    public static bool MisclassificationToleranceTest(double errorThresholdPercent)
    {
      if (errorThresholdPercent < 0 || errorThresholdPercent > 100)
        throw new ArgumentException("Error threshold must be in range [0,100]");
      var thresholdMultiplier = 1 - (errorThresholdPercent / 100);  //reverse percentage
      bool passed = true;


      foreach (var reg in _unclassifiedRegionsDict.Keys)
      {
        var UnclassifiedRegionCount = _unclassifiedRegionsDict[reg];
        
        if (UnclassifiedRegionCount > _highestCount * thresholdMultiplier)
        {
          passed = false;
          Console.WriteLine($"Validation Fail. Test 3 Misclassification tolerance\nRegion #{reg} Count is higher than the threshold {errorThresholdPercent.ToString($"{0:0.00}")}%");
        }
      }
      return passed;
    }
  }
}