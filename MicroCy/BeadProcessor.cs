using System;
using System.Collections.Generic;

namespace DIOS.Core
{
  internal class BeadProcessor
  {
    public List<Gstats> Stats { get; } = new List<Gstats>(10);
    public List<double> AvgBg { get; } = new List<double>(10);
    internal int SavBeadCount { get; set; }
    private byte _actPrimaryIndex;
    private byte _actSecondaryIndex;
    private float _greenMin;
    private float _greenMaj;
    private static readonly double[] ClassificationBins;
    private readonly float[,] Sfi = new float[80000, 10];
    private int[,] _classificationMap;
    private ushort[,] _bgValues = new ushort[10, 80000];
    private Device _device;

    public BeadProcessor(Device device)
    {
      _device = device;
    }

    static BeadProcessor()
    {
      ClassificationBins = GenerateLogSpace(1, 60000, 256);
    }

    public void CalculateBeadParams(ref BeadInfoStruct outbead)
    {
      //greenMaj is the hi dyn range channel, greenMin is the high sensitivity channel(depends on filter placement)
      if (_device.SensitivityChannel == HiSensitivityChannel.B)
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
      outbead.fsc = (float)Math.Pow(10, outbead.fsc);
      var reg = (ushort)ClassifyBeadToRegion(cl);
      outbead.region = reg;
      //handle HI dnr channel
      var reporter = _greenMin > _device.HdnrTrans ? _greenMaj * _device.HDnrCoef : _greenMin;
      outbead.reporter = (reporter / _device.ReporterScaling);
      if (_device.Normalization)
      {
        NormalizeReporter(ref outbead, reg);
      }
    }

    private int ClassifyBeadToRegion((float cl0, float cl1, float cl2, float cl3) cl)
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

    private void NormalizeReporter(ref BeadInfoStruct outbead, int region)
    {
      var idx = _device.MapCtroller.GetMapRegionIndex(region);
      var rep = (float)(_device.MapCtroller.ActiveMap.factor * _device.MapCtroller.ActiveMap.regions[idx].NormalizationMFI);
      outbead.reporter -= rep;
      outbead.reporter = outbead.reporter >= 0 ? outbead.reporter : 0;
    }

    public void FillBackgroundAverages(in BeadInfoStruct outbead)
    {
      if (_device.BeadCount < 80000)
      {
        _bgValues[0, _device.BeadCount] = outbead.gssc_bg;
        _bgValues[1, _device.BeadCount] = outbead.greenB_bg;
        _bgValues[2, _device.BeadCount] = outbead.greenC_bg;
        _bgValues[3, _device.BeadCount] = outbead.cl3_bg;
        _bgValues[4, _device.BeadCount] = outbead.rssc_bg;
        _bgValues[5, _device.BeadCount] = outbead.cl1_bg;
        _bgValues[6, _device.BeadCount] = outbead.cl2_bg;
        _bgValues[7, _device.BeadCount] = outbead.vssc_bg;
        _bgValues[8, _device.BeadCount] = outbead.cl0_bg;
        _bgValues[9, _device.BeadCount] = outbead.fsc_bg;
      }
    }

    public void CalculateBackgroundAverages()
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

    public void FillCalibrationStatsRow(in BeadInfoStruct outbead)
    {
      if (_device.BeadCount < 80000)
      {
        Sfi[_device.BeadCount, 0] = outbead.greenssc;
        Sfi[_device.BeadCount, 1] = outbead.greenB;
        Sfi[_device.BeadCount, 2] = outbead.greenC;
        Sfi[_device.BeadCount, 3] = outbead.redssc;
        Sfi[_device.BeadCount, 4] = outbead.cl1;
        Sfi[_device.BeadCount, 5] = outbead.cl2;
        Sfi[_device.BeadCount, 6] = outbead.cl3;
        Sfi[_device.BeadCount, 7] = outbead.violetssc;
        Sfi[_device.BeadCount, 8] = outbead.cl0;
        Sfi[_device.BeadCount, 9] = outbead.fsc;
      }
    }

    public void CalculateGStats()
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

    private (float cl0, float cl1, float cl2, float cl3) MakeClArr(in BeadInfoStruct outbead)
    {
      var cl1comp = _greenMaj * _device.Compensation / 100;
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

    public void ConstructClassificationMap(CustomMap cMap)
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
