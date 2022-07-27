﻿using System;

namespace DIOS.Core
{
  internal class ClassificationMap
  {
    private int[,] _classificationMap = new int[CLASSIFICATIONMAPSIZE, CLASSIFICATIONMAPSIZE];
    private byte _actPrimaryIndex;
    private byte _actSecondaryIndex;
    private static readonly double[] ClassificationBins;
    private const int CLASSIFICATIONMAPSIZE = 256;

    static ClassificationMap()
    {
      ClassificationBins = GenerateLogSpace(1, 60000, CLASSIFICATIONMAPSIZE);
    }

    public void ConstructClassificationMap(CustomMap cMap)
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
    }

    public int ClassifyBeadToRegion(in RawBead bead)
    {
      //_actPrimaryIndex and _actSecondaryIndex should define _classimap index in a previous call,
      //and produce an index for the selection of classiMap. For cl0 and cl3 map compatibility
      int x = Array.BinarySearch(ClassificationBins, bead.cl1);
      if (x < 0)
        x = ~x;
      int y = Array.BinarySearch(ClassificationBins, bead.cl2);
      if (y < 0)
        y = ~y;
      x = x < byte.MaxValue ? x : byte.MaxValue;
      y = y < byte.MaxValue ? y : byte.MaxValue;
      return _classificationMap[x, y];
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
}
