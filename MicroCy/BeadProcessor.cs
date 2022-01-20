using MicroCy.InstrumentParameters;
using System;
using System.Collections.Generic;

namespace MicroCy
{
  internal class BeadProcessor
  {
    public static List<Gstats> Stats { get; } = new List<Gstats>(10);
    public static List<double> AvgBg { get; } = new List<double>(10);
    internal static int SavBeadCount { get; set; }
    private static byte _actPrimaryIndex;
    private static byte _actSecondaryIndex;
    private static float _greenMin;
    private static float _greenMaj;
    private static readonly double[] ClassificationBins;
    private static readonly float[,] Sfi = new float[80000, 10];
    private static int[,] _classificationMap;
    private static ushort[,] _bgValues = new ushort[10, 80000];

    static BeadProcessor()
    {
      ClassificationBins = GenerateLogSpace(1, 60000, 256);
    }

    public static void CalculateBeadParams(ref BeadInfoStruct outbead)
    {
      //greenMaj is the hi dyn range channel, greenMin is the high sensitivity channel(depends on filter placement)
      if (MicroCyDevice.ChannelBIsHiSensitivity)
      {
        _greenMaj = outbead.greenC;
        _greenMin = outbead.greenB;
      }
      else
      {
        _greenMaj = outbead.greenB;
        _greenMin = outbead.greenC;
      }
      var cl = MakeClArr(in outbead);
      //each well can have a different  classification map
      outbead.cl1 = cl.cl1;
      outbead.cl2 = cl.cl2;
      outbead.region = (ushort)ClassifyBeadToRegion(cl);
      //handle HI dnr channel
      outbead.reporter = _greenMin > Calibration.HdnrTrans ? _greenMaj * Calibration.HDnrCoef : _greenMin;
    }

    private static int ClassifyBeadToRegion((float cl0, float cl1, float cl2, float cl3) cl)
    {
      //_actPrimaryIndex and _actSecondaryIndex should define _classimap index in a previous call,
      //and produce an index for the selection of classiMap. For cl0 and cl3 map compatibility
      int x = Array.BinarySearch(ClassificationBins, cl.cl1);
      if (x < 0)
        x = ~x;
      int y = Array.BinarySearch(ClassificationBins, cl.cl2);
      if (y < 0)
        y = ~y;
      x = x < byte.MaxValue ? x : byte.MaxValue;
      y = y < byte.MaxValue ? y : byte.MaxValue;
      return _classificationMap[x, y];
    }

    public static void FillBackgroundAverages(in BeadInfoStruct outbead)
    {
      if (MicroCyDevice.BeadCount < 80000)
      {
        _bgValues[0, MicroCyDevice.BeadCount] = outbead.gssc_bg;
        _bgValues[1, MicroCyDevice.BeadCount] = outbead.greenB_bg;
        _bgValues[2, MicroCyDevice.BeadCount] = outbead.greenC_bg;
        _bgValues[3, MicroCyDevice.BeadCount] = outbead.rssc_bg;
        _bgValues[4, MicroCyDevice.BeadCount] = outbead.cl1_bg;
        _bgValues[5, MicroCyDevice.BeadCount] = outbead.cl2_bg;
        _bgValues[6, MicroCyDevice.BeadCount] = outbead.cl3_bg;
        _bgValues[7, MicroCyDevice.BeadCount] = outbead.vssc_bg;
        _bgValues[8, MicroCyDevice.BeadCount] = outbead.cl0_bg;
        _bgValues[9, MicroCyDevice.BeadCount] = outbead.fsc_bg;
      }
    }

    public static void CalculateBackgroundAverages()
    {
      AvgBg.Clear();
      var Count = SavBeadCount > 80000 ? 80000 : SavBeadCount;
      for (int i = 0; i < 10; i++)
      {
        double sum = 0;

        for (int j = 0; j < Count; j++)
        {
          sum += _bgValues[i, j];
        }
        var avg = sum / Count;
        AvgBg.Add(avg);
      }
    }

    public static void FillCalibrationStatsRow(in BeadInfoStruct outbead)
    {
      if (MicroCyDevice.BeadCount < 80000)
      {
        Sfi[MicroCyDevice.BeadCount, 0] = outbead.greenssc;
        Sfi[MicroCyDevice.BeadCount, 1] = outbead.greenB;
        Sfi[MicroCyDevice.BeadCount, 2] = outbead.greenC;
        Sfi[MicroCyDevice.BeadCount, 3] = outbead.redssc;
        Sfi[MicroCyDevice.BeadCount, 4] = outbead.cl1;
        Sfi[MicroCyDevice.BeadCount, 5] = outbead.cl2;
        Sfi[MicroCyDevice.BeadCount, 6] = outbead.cl3;
        Sfi[MicroCyDevice.BeadCount, 7] = outbead.violetssc;
        Sfi[MicroCyDevice.BeadCount, 8] = outbead.cl0;
        Sfi[MicroCyDevice.BeadCount, 9] = outbead.fsc;
      }
    }

    public static void CalculateGStats()
    {
      Stats.Clear();
      var Count = SavBeadCount > 80000 ? 80000 : SavBeadCount;
      for (int i = 0; i < 10; i++)
      {
        double sumit = 0;
        for (int beads = 0; beads < Count; beads++)
        {
          sumit += Sfi[beads, i];
        }
        double robustcnt = Count; //start with total bead count
        double mean = sumit / robustcnt;
        //find high and low bounds
        double min = mean * 0.5;
        double max = mean * 2;
        sumit = 0;
        for (int beads = 0; beads < Count; beads++)
        {
          if ((Sfi[beads, i] > min) && (Sfi[beads, i] < max))
            sumit += Sfi[beads, i];
          else
          {
            Sfi[beads, i] = 0;
            robustcnt--;
          }
        }
        mean = sumit / robustcnt;
        double sumsq = 0;
        for (int beads = 0; beads < Count; beads++)
        {
          if (Sfi[beads, i] == 0)
            continue;
          sumsq += Math.Pow(mean - Sfi[beads, i], 2);
        }
        double stdDev = Math.Sqrt(sumsq / (robustcnt - 1));

        double gcv = (stdDev / mean) * 100;
        if (double.IsNaN(gcv))
          gcv = 0;
        Stats.Add(new Gstats
        {
          mfi = mean,
          cv = gcv
        });
      }
    }

    private static (float cl0, float cl1, float cl2, float cl3) MakeClArr(in BeadInfoStruct outbead)
    {
      var cl1comp = _greenMaj * Calibration.Compensation / 100;
      var cl2comp = cl1comp * 0.26f;
      return (
        outbead.cl0,
        outbead.cl1 - cl1comp,  //Compensation
        outbead.cl2 - cl2comp,  //Compensation
        outbead.cl3
      );
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

    public static void ConstructClassificationMap(CustomMap cMap)
    {
      _actPrimaryIndex = (byte)cMap.midorderidx; //what channel cl0 - cl3?
      _actSecondaryIndex = (byte)cMap.loworderidx;

      _classificationMap = new int[256, 256];
      foreach (var region in cMap.regions)
      {
        foreach(var point in region.Points)
        {
          _classificationMap[point.x, point.y] = region.Number;
        }
      }
    }

  }
}
