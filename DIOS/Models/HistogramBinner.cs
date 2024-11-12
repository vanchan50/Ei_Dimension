using DIOS.Core;
using System;
using DIOS.Application;
using Ei_Dimension.Core;

namespace Ei_Dimension.Models;

public static class HistogramBinner
{
  private static int[] GreenA = new int[HistogramData.Bins.Length];
  private static int[] GreenB = new int[HistogramData.Bins.Length];//RedC
  private static int[] GreenC = new int[HistogramData.Bins.Length];//RedD
  private static int[] RedSSC = new int[HistogramData.Bins.Length];
  private static int[] CL1 = new int[HistogramData.Bins.Length];//GreenB
  private static int[] CL2 = new int[HistogramData.Bins.Length];//GreenC
  private static int[] CL3 = new int[HistogramData.Bins.Length];
  private static int[] GreenD = new int[HistogramData.Bins.Length];

  public static ChannelsHistogramPeaks BinData(ReadOnlySpan<ProcessedBead> inputBeadSpan, bool fromFile = false)
  {
    var MaxValue = HistogramData.UPPERLIMIT;

    //bool failed = false;
    foreach (var processedBead in inputBeadSpan)
    {
      DataProcessor.FillBinArray(GreenA, processedBead.greenssc);
      DataProcessor.FillBinArray(GreenB, processedBead.greenB);
      DataProcessor.FillBinArray(GreenC, processedBead.greenC);
      DataProcessor.FillBinArray(RedSSC, processedBead.redssc);
      DataProcessor.FillBinArray(CL1,    processedBead.cl1);
      DataProcessor.FillBinArray(CL2,    processedBead.cl2);
      DataProcessor.FillBinArray(CL3,    processedBead.redA);
      DataProcessor.FillBinArray(GreenD, processedBead.greenD);
    }
    //if (failed)
    //  System.Windows.MessageBox.Show("An error occured during Well File Read");
    var peakA = DataProcessor.GetSignalPeak(GreenA);
    var peakB = DataProcessor.GetSignalPeak(GreenB);
    var peakC = DataProcessor.GetSignalPeak(GreenC);
    var peakD = DataProcessor.GetSignalPeak(RedSSC);
    var peakE = DataProcessor.GetSignalPeak(CL1);
    var peakF = DataProcessor.GetSignalPeak(CL2);
    var peakG = DataProcessor.GetSignalPeak(CL3);
    var peakI = DataProcessor.GetSignalPeak(GreenD);

    var result = new ChannelsHistogramPeaks
    (
      peakA,
      peakB,
      peakC,
      peakD,
      peakE,
      peakF,
      peakG,
      peakI
      );
    ClearCounterArrays();
    return result;
  }

  private static void ClearCounterArrays()
  {
    Array.Clear(GreenA, 0, GreenA.Length);
    Array.Clear(GreenB, 0, GreenB.Length);
    Array.Clear(GreenC, 0, GreenC.Length);
    Array.Clear(RedSSC, 0, RedSSC.Length);
    Array.Clear(CL1, 0, CL1.Length);
    Array.Clear(CL2, 0, CL2.Length);
    Array.Clear(CL3, 0, CL3.Length);
    Array.Clear(GreenD, 0, GreenD.Length);
  }
}