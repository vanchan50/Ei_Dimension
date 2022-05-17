﻿using System;
using System.Collections.Generic;
using System.Globalization;
using DIOS.Core.FileIO;

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
    private static readonly VerificationReportPublisher _publisher = new VerificationReportPublisher();
    private static CultureInfo _culture;
    private static readonly Dictionary<string, (string en, string zh)> _culturedText = new Dictionary<string, (string en, string zh)>
    {
      ["Test1_1"] = ("Test1: Regions", "测试1：区域"),
      ["Test1_2"] = ("measured below target reporter value", "报告分子低于目标MFI值"),
      ["Test2_Fail"] = ("Test2: no beads were classified", "测试2：未检出编码微球"),
      ["Test2_1"] = ("Test2", "测试2"),
      ["Test2_2"] = ("of verification events outside target regions", "验证微球未落在目标区域内"),
      ["Test3_1"] = ("Test3", "测试3"),
      ["Test3_2"] = ("of region", "区域"),
      ["Test3_3"] = ("events misclassified into regions", "微球错误分类至区域")
    };

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

      _culture = new CultureInfo("en-US");
      _publisher.Reset();
    }

    public static void PublishReport()
    {
      _publisher.PublishReport();
    }

    public static bool ReporterToleranceTest(double errorThresholdPercent, out string msg)
    {
      msg = null;
      if (errorThresholdPercent < 0)
        throw new ArgumentException("Error threshold must not be negative");
      bool passed = true;
      var thresholdMultiplier = errorThresholdPercent <= 100 ? 1 - (errorThresholdPercent / 100) : (errorThresholdPercent / 100);  //reverse percentage
      var problematicRegions = new List<(int region, double errorPercentage)>();
      foreach (var reg in RegionalStats)
      {
        var ActualReporterMedian = GetMedianReporterForRegion(reg.Region);
        if (ActualReporterMedian <= reg.InputReporter * thresholdMultiplier)
        {
          //Console.WriteLine($"Verification Fail. Test 1 Reporter tolerance\nReporter value ({ReporterMedian.ToString()}) deviation is more than Threshold is {errorThresholdPercent.ToString($"{0:0.00}")}% from the target ({reg.InputReporter})");
          var errorPercentage =  100 * (reg.InputReporter - ActualReporterMedian) / reg.InputReporter;  //how much Actual is less than InputReporter
          problematicRegions.Add((reg.Region, errorPercentage));
          passed = false;
        }
      }

      if (problematicRegions.Count != 0)
      {
        msg = $"{GetCulturedMsg("Test1_1")}: ";
        _publisher.AddData("Reporter Tolerance Test Failed Regions:\n");
        foreach (var region in problematicRegions)
        {
          msg = msg + region.region.ToString() + ",";
          _publisher.AddData($"Region #{region.region}\t\t{region.errorPercentage}%\n");
        }
        msg = msg.Remove(msg.Length - 1); //remove last ","
        msg = msg += $" {GetCulturedMsg("Test1_2")}";
        _publisher.AddData("\n");
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
      if (_totalClassifiedBeads == 0)
      {
        msg = GetCulturedMsg("Test2_Fail");
        _publisher.AddData("\nClassification Tolerance Test:\n");
        _publisher.AddData("no beads were classified\n");
        return false;
      }
      var difference = _totalUnclassifiedBeads / _totalClassifiedBeads;
      var difPercent = difference * 100;
      if (difPercent > errorThresholdPercent)
      {
        passed = false;
        //Console.WriteLine($"Verification Fail. Test 2 Classification tolerance\nMax difference between region counts is {difPercent.ToString()}%, Threshold is {errorThresholdPercent.ToString($"{0:0.00}")}");
        msg = $"{GetCulturedMsg("Test2_1")}: {difPercent.ToString()}% {GetCulturedMsg("Test2_2")}";
        _publisher.AddData("\nClassification Tolerance Test:\n");
        _publisher.AddData($"{difPercent.ToString()}% of verification events outside target regions\n");
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
        msg = $"{_culturedText["Test3_1"].en}: {errorThresholdPercent}% {_culturedText["Test3_2"].en} {_lowestCountRegion} {_culturedText["Test3_3"].en} : ";
        if (_culture.Name.StartsWith("zh"))
        {
          msg = $"{_culturedText["Test3_1"].zh}: {_culturedText["Test3_2"].zh} {_lowestCountRegion} 有 {errorThresholdPercent}% {_culturedText["Test3_3"].zh} : ";
        }
        _publisher.AddData("\nMisclassification Tolerance Test:\n");
        foreach (var region in problematicRegions)
        {
          msg = msg + region.ToString() + ",";
          _publisher.AddData($"{region.ToString()}, ");
        }
        msg = msg.Remove(msg.Length - 1);
      }
      return passed;
    }

    /// <summary>
    /// Hack to have a message in different language
    /// </summary>
    /// <param name="culture"></param>
    public static void SetCulture(CultureInfo culture)
    {
      _culture = culture;
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
      if (outbead.region == 0)
        return;
      //region 0 should not be counted here
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
      if (double.IsNaN(RegionalStats[index].Stats[0].mfi))
        return 0;
      return RegionalStats[index].Stats[0].mfi;
    }

    private static string GetCulturedMsg(string str)
    {
      string msg = null;
      var resource = _culturedText[str];
      if (_culture.Name.StartsWith("zh"))
      {
        msg = resource.zh;
      }
      if (_culture.Name.StartsWith("en"))
      {
        msg = resource.en;
      }
      return msg;
    }
  }
}