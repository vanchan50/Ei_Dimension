using System;
using System.Collections.Generic;

namespace DIOS.Core
{
  internal class BeadProcessor
  {
    internal int SavBeadCount { get; set; }
    private byte _actPrimaryIndex;
    private byte _actSecondaryIndex;
    private float _greenMin;
    private float _greenMaj;
    private static readonly double[] ClassificationBins;
    private int[,] _classificationMap = new int[CLASSIFICATIONMAPSIZE, CLASSIFICATIONMAPSIZE];
    private Device _device;
    private const int CLASSIFICATIONMAPSIZE = 256;

    public BeadProcessor(Device device)
    {
      _device = device;
    }

    static BeadProcessor()
    {
      ClassificationBins = GenerateLogSpace(1, 60000, CLASSIFICATIONMAPSIZE);
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
      //outbead.fsc = (float)Math.Pow(10, outbead.fsc);
      var reg = (ushort)ClassifyBeadToRegion(cl);
      outbead.region = reg;
      //handle HI dnr channel
      var reporter = _greenMin > _device.HdnrTrans ? _greenMaj * _device.HDnrCoef : _greenMin;
      outbead.reporter = (reporter / _device.ReporterScaling);
      if (_device.IsNormalizationEnabled)
      {
        NormalizeReporter(ref outbead);
      }
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

    private void NormalizeReporter(ref BeadInfoStruct outbead)
    {
      if (outbead.region == 0)
        return;
      var rep = (float)(_device.MapCtroller.ActiveMap.factor * _device.MapCtroller.ActiveMap.Regions[outbead.region].NormalizationMFI);
      outbead.reporter -= rep;
      outbead.reporter = outbead.reporter >= 0 ? outbead.reporter : 0;
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
  }
}
