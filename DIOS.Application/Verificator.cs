﻿using System;
using System.Collections.Generic;
using System.Globalization;
using DIOS.Core;

namespace DIOS.Application
{
  public class Verificator
  {
    private readonly List<VerificationStats> RegionalStats = new List<VerificationStats>(50);
    private Dictionary<int, int> _classifiedRegionsDict = new Dictionary<int, int>();  //region,index
    private Dictionary<int, int> _unclassifiedRegionsDict = new Dictionary<int, int>();  //region,count
    private int _highestCount = 0;
    private int _lowestCount = int.MaxValue;
    private int _lowestCountRegion = -1;
    private int _highestCountRegion = -1;
    private int _totalClassifiedBeads;
    private int _totalBeads;
    private int _totalUnclassifiedBeads;
    private int _highestUnclassifiedCount; 
    private int _highestUnclassifiedCountRegion = -1;
    private readonly VerificationReportPublisher _publisher;
    private CultureInfo _culture;
    private VerificationReport _report;
    private readonly ILogger _logger;
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

    public Verificator(ILogger logger)
    {
      _logger = logger;
      _publisher = new VerificationReportPublisher(_logger);
    }

    public void Reset(List<(int regionNum, double InputReporter)> regions)
    {
      RegionalStats.Clear();
      _totalClassifiedBeads = 0;
      _totalUnclassifiedBeads = 0;
      _highestUnclassifiedCount = 0;
      _totalBeads = 0;
      _highestCount = 0;
      _lowestCount = int.MaxValue;
      _lowestCountRegion = -1;
      _highestCountRegion = -1;
      _highestUnclassifiedCountRegion = -1;
      _classifiedRegionsDict.Clear();
      _unclassifiedRegionsDict.Clear();
      foreach (var reg in regions)
      {
        _classifiedRegionsDict.Add(reg.regionNum, RegionalStats.Count);
        RegionalStats.Add(new VerificationStats(reg.regionNum, reg.InputReporter));
      }

      _culture = new CultureInfo("en-US");
      _publisher.Reset();
      _report = new VerificationReport();
    }

    public void PublishReport()
    {
      _publisher.PublishReport();
    }

    public bool ReporterToleranceTest(double errorThresholdPercent, out string msg)
    {
      msg = null;
      if (errorThresholdPercent < 0)
        throw new ArgumentException("Error threshold must not be negative");
      _report.Tolerance1 = errorThresholdPercent;
      bool passed = true;
      var thresholdMultiplier = errorThresholdPercent <= 100 ? 1 - (errorThresholdPercent / 100) : (errorThresholdPercent / 100);  //reverse percentage
      var problematicRegions = new List<(int region, double errorPercentage)>();
      foreach (var reg in RegionalStats)
      {
        var ActualReporterMedian = GetMedianReporterForRegion(reg.Region);
        //_logger.Log($"Verification Fail. Test 1 Reporter tolerance\nReporter value ({ReporterMedian.ToString()}) deviation is more than Threshold is {errorThresholdPercent.ToString($"{0:0.00}")}% from the target ({reg.InputReporter})");
        var errorPercentage = 100 * (reg.InputReporter - ActualReporterMedian) / reg.InputReporter;  //how much Actual is less than InputReporter
        if (ActualReporterMedian <= reg.InputReporter * thresholdMultiplier)
        {
          problematicRegions.Add((reg.Region, errorPercentage));
          passed = false;
        }
        _report.Test1regions.Add((reg.Region, -1 * errorPercentage));
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
      _report.Passed = passed;
      return passed;
    }

    public bool ClassificationToleranceTest(double errorThresholdPercent, out string msg)
    {
      msg = null;
      if (errorThresholdPercent < 0)
        throw new ArgumentException("Error threshold must not be negative");
      _report.Tolerance2 = errorThresholdPercent;
      bool passed = true;

      //var difference = (_highestCount - _lowestCount) / (double)_lowestCount;
      if (_totalClassifiedBeads == 0)
      {
        msg = GetCulturedMsg("Test2_Fail");
        _publisher.AddData("\nClassification Tolerance Test:\n");
        _publisher.AddData("No beads were classified\n");
        _report.UnclassifiedBeadsPercentage = 100;
        _report.Passed = false;
        return false;
      }
      //var difference = (double)_totalUnclassifiedBeads / _totalClassifiedBeads;     //Old technique
      var difference = (_totalBeads - _totalClassifiedBeads) / (double)_totalBeads;
      var difPercent = difference * 100;
      if (difPercent > errorThresholdPercent)
      {
        passed = false;
        //_logger.Log($"Verification Fail. Test 2 Classification tolerance\nMax difference between region counts is {difPercent.ToString()}%, Threshold is {errorThresholdPercent.ToString($"{0:0.00}")}");
        msg = $"{GetCulturedMsg("Test2_1")}: {difPercent.ToString()}% {GetCulturedMsg("Test2_2")}";
        _publisher.AddData("\nClassification Tolerance Test:\n");
        _publisher.AddData($"{difPercent.ToString()}% of verification events outside target regions\n");
      }
      _report.UnclassifiedBeadsPercentage = difPercent;
      _report.Passed = passed && _report.Passed;
      return passed;
    }

    public bool MisclassificationToleranceTest(double errorThresholdPercent, out string msg)
    {
      msg = null;
      if (errorThresholdPercent < 0)
        throw new ArgumentException("Error threshold must not be negative");
      _report.Tolerance3 = errorThresholdPercent;
      //var thresholdMultiplier = errorThresholdPercent <= 100 ? 1 - (errorThresholdPercent / 100) : (errorThresholdPercent / 100);  //reverse percentage
      bool passed = true;

      var problematicRegions = new List<int>();

      foreach (var reg in _unclassifiedRegionsDict)
      {
        if ( ((reg.Value / (double)_lowestCount) * 100 ) > errorThresholdPercent)
        {
          passed = false;
          //_logger.Log($"Verification Fail. Test 3 Misclassification tolerance\nRegion #{reg.Key} Count is higher than the threshold {errorThresholdPercent.ToString($"{0:0.00}")}%");
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

      _report.Passed = passed && _report.Passed;
      return passed;
    }

    /// <summary>
    /// Hack to have a message in different language
    /// </summary>
    /// <param name="culture"></param>
    public void SetCulture(CultureInfo culture)
    {
      _culture = culture;
    }

    public  void PublishResult(string path)
    {
      _report.Publish(path);
    }

    /// <summary>
    /// Called on every Read from USB, in Verification mode. Not used in other modes
    /// </summary>
    /// <param name="outbead"></param>
    public void FillStats(in ProcessedBead bead)
    {
      _totalBeads++;
      //if region is classified
      if (_classifiedRegionsDict.TryGetValue(bead.region, out var index))
      {
        RegionalStats[index].FillCalibrationStatsRow(in bead);
        return;
      }

      //if region is UNclassified
      if (bead.region == 0)
        return;
      //region 0 should not be counted here
      if (_unclassifiedRegionsDict.ContainsKey(bead.region))
      {
        _unclassifiedRegionsDict[bead.region]++;
        return;
      }
      _unclassifiedRegionsDict.Add(bead.region, 1);
    }

    public void CalculateResults(MapModel activeMap)
    {
      //RegionalStats holds regions with defined Reporter target
      foreach (var region in RegionalStats)
      {
        region.CalculateResultingStats();
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
        if (entry.Value > _highestUnclassifiedCount && entry.Key != 0)
        {
          _highestUnclassifiedCount = entry.Value;
          _highestUnclassifiedCountRegion = entry.Key;
        }
      }
      _report.Test3HighestUnclassifiedCountRegion = _highestUnclassifiedCountRegion;
      _report.Test3HighestUnclassifiedCount = _highestUnclassifiedCount;
      _report.TotalBeads = _totalBeads;

      //TODO: botched like crazy
      var list = new List<MapRegion>(4);
      foreach (var p in _classifiedRegionsDict)
      {
        list.Add(activeMap.Regions[p.Key]);
      }

      if (activeMap.Regions.TryGetValue(_highestUnclassifiedCountRegion, out var mapRegion))
      {
        var nearestVerifRegion = mapRegion.FindNearestRegionFrom(list);
        _report.Test3NearestClassifiedCountRegion = nearestVerifRegion.Number;
        _report.Test3NearestClassifiedCount = RegionalStats[_classifiedRegionsDict[nearestVerifRegion.Number]].Count;
      }
      else
      {
        _report.Test3NearestClassifiedCountRegion = -1;
        _report.Test3NearestClassifiedCount = -1;
      }
      if (_report.Test3NearestClassifiedCount > 0)
        _report.Test3MisclassificationsPercentage = 100 * (double) _report.Test3HighestUnclassifiedCount / _report.Test3NearestClassifiedCount;
      else
        _report.Test3MisclassificationsPercentage = 100;

    }

    private double GetMedianReporterForRegion(int regionNum)
    {
      var index = RegionalStats.FindIndex(x => x.Region == regionNum);
      if (double.IsNaN(RegionalStats[index].Stats[0].Median))
        return 0;
      return RegionalStats[index].Stats[0].Median;
    }

    private string GetCulturedMsg(string str)
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