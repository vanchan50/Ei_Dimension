﻿using System.Collections.Frozen;
using DIOS.Core;

namespace DIOS.Application.Domain;

internal class ClassificationMap
{
  private int[,] _classificationMap = new int[CLASSIFICATIONMAPSIZE, CLASSIFICATIONMAPSIZE];
  private byte _actPrimaryIndex;
  private byte _actSecondaryIndex;
  private static readonly double[] ClassificationBins;
  private const int CLASSIFICATIONMAPSIZE = 256;
  private byte _param1Shift = _shiftData["RedC"];
  private byte _param2Shift = _shiftData["RedD"];
  internal static readonly FrozenDictionary<string, byte> _shiftData = new Dictionary<string, byte>()
  {
    ["GreenA"] = 68,
    ["GreenB"] = 20,
    ["GreenC"] = 24,
    ["GreenD"] = 48,
    ["RedA"] = 64,
    ["RedB"] = 52,
    ["RedC"] = 56,
    ["RedD"] = 60,
  }.ToFrozenDictionary();

  static ClassificationMap()
  {
    ClassificationBins = GenerateLogSpace(1, 60000, CLASSIFICATIONMAPSIZE);
  }

  public void ConstructClassificationMap(MapModel cMap)
  {
    _actPrimaryIndex = (byte)cMap.midorderidx; //what channel cl0 - cl3?
    _actSecondaryIndex = (byte)cMap.loworderidx;
    Array.Clear(_classificationMap, 0, _classificationMap.Length);

    foreach (var region in cMap.Regions)
    {
      foreach (var point in region.Value.Points)
      {
        _classificationMap[point.x, point.y] = region.Key;
      }
    }
    ChooseProperClassification(cMap.ClassificationParameter1, cMap.ClassificationParameter2);
  }

  public int ClassifyBeadToRegion(in ProcessedBead bead)
  {
    //_actPrimaryIndex and _actSecondaryIndex should define _classimap index in a previous call,
    //and produce an index for the selection of classiMap. For cl0 and cl3 map compatibility
    int x = Array.BinarySearch(ClassificationBins, GetClassificationParam1(bead));
    if (x < 0)
      x = ~x;
    int y = Array.BinarySearch(ClassificationBins, GetClassificationParam2(bead));
    if (y < 0)
      y = ~y;
    x = x < byte.MaxValue ? x : byte.MaxValue;
    y = y < byte.MaxValue ? y : byte.MaxValue;
    return _classificationMap[x, y];
  }

  public void ChooseProperClassification(string param1, string param2)
  {
    try
    {
      _param1Shift = _shiftData[param1];
    }
    catch
    {
      _param1Shift = _shiftData["RedC"];
    }
    try
    {
      _param2Shift = _shiftData[param2];
    }
    catch
    {
      _param2Shift = _shiftData["RedD"];
    }
  }

  public float GetClassificationParam1(in ProcessedBead bead)
  {
    unsafe
    {
      fixed (ProcessedBead* pBead = &bead)
      {
        return *(float*)((byte*)pBead + _param1Shift);
      }
    }
  }

  private float GetClassificationParam2(in ProcessedBead bead)
  {
    unsafe
    {
      fixed (ProcessedBead* pBead = &bead)
      {
        return *(float*)((byte*)pBead + _param2Shift);
      }
    }
  }

  private static double[] GenerateLogSpace(int min, int max, int logBins, bool baseE = false)
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
}