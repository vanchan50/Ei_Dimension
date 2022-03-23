using System;
using System.Collections.Generic;

namespace DIOS.Core
{
  public static class Verificator
  {
    public static int TotalClassifiedBeads;
    private static readonly List<ValidationStats> RegionalStats = new List<ValidationStats>(50);
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
          Console.WriteLine($"Verification Fail. Test 1 Reporter tolerance\nReporter value ({ReporterMedian.ToString()}) deviation is more than Threshold is {errorThresholdPercent.ToString($"{0:0.00}")}% from the target ({reg.InputReporter})");
          problematicRegions.Add(reg.Region);
          passed = false;
        }
      }

      if (problematicRegions.Count != 0)
      {
        msg = "Test1 Failed Regions: ";
        foreach (var region in problematicRegions)
        {
          msg = msg + region.ToString() + ",";
        }
        msg = msg.Remove(msg.Length - 1);
      }
      return passed;
    }

    public static bool ClassificationToleranceTest(double errorThresholdPercent, out string msg)
    {
      msg = null;
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
        Console.WriteLine($"Verification Fail. Test 2 Classification tolerance\nMax difference between region counts is {difPercent.ToString()}%, Threshold is {errorThresholdPercent.ToString($"{0:0.00}")}");
        msg = $"Test2 Max difference is {difPercent.ToString()}%";
      }
      return passed;
    }

    public static bool MisclassificationToleranceTest(double errorThresholdPercent, out string msg)
    {
      msg = null;
      if (errorThresholdPercent < 0)
        throw new ArgumentException("Error threshold must not be negative");
      var thresholdMultiplier = errorThresholdPercent <= 100 ? 1 - (errorThresholdPercent / 100) : (errorThresholdPercent / 100);  //reverse percentage
      bool passed = true;

      var problematicRegions = new List<int>();

      foreach (var reg in _unclassifiedRegionsDict.Keys)
      {
        var UnclassifiedRegionCount = _unclassifiedRegionsDict[reg];
        
        if (UnclassifiedRegionCount > _highestCount * thresholdMultiplier)
        {
          passed = false;
          Console.WriteLine($"Verification Fail. Test 3 Misclassification tolerance\nRegion #{reg} Count is higher than the threshold {errorThresholdPercent.ToString($"{0:0.00}")}%");
          problematicRegions.Add(reg);
        }
      }


      if (problematicRegions.Count != 0)
      {
        msg = "Test3 Failed Regions: ";
        foreach (var region in problematicRegions)
        {
          msg = msg + region.ToString() + ",";
        }
        msg = msg.Remove(msg.Length - 1);
      }
      return passed;
    }

    internal static void FillStats(in BeadInfoStruct outbead)
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

    internal static void CalculateResults()
    {
      foreach (var s in RegionalStats)
      {
        s.CalculateResults();
        TotalClassifiedBeads += s.Count;
      }
    }
    private static double GetMedianReporterForRegion(int regionNum)
    {
      var index = RegionalStats.FindIndex(x => x.Region == regionNum);
      return RegionalStats[index].Stats[0].mfi;
    }
  }
}