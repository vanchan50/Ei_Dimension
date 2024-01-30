using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.ObjectModel;

namespace Ei_Dimension.Models;

[POCOViewModel]
public class ScatterData
{
  public virtual ObservableCollection<HistogramData> DisplayedForwardSsc { get; set; }
  public virtual ObservableCollection<HistogramData> DisplayedVioletSsc { get; set; }
  public virtual ObservableCollection<HistogramData> DisplayedRedSsc { get; set; }
  public virtual ObservableCollection<HistogramData> DisplayedGreenSsc { get; set; }
  public virtual ObservableCollection<HistogramData> DisplayedReporter { get; set; }
  public static ObservableCollection<HistogramData> CurrentForwardSsc { get; set; } = new();
  public static ObservableCollection<HistogramData> CurrentVioletSsc { get; set; } = new();
  public static ObservableCollection<HistogramData> CurrentRedSsc { get; set; } = new();
  public static ObservableCollection<HistogramData> CurrentGreenSsc { get; set; } = new();
  public static ObservableCollection<HistogramData> CurrentReporter { get; set; } = new();
  public static ObservableCollection<HistogramData> BackingForwardSsc { get; set; } = new();
  public static ObservableCollection<HistogramData> BackingVioletSsc { get; set; } = new();
  public static ObservableCollection<HistogramData> BackingRedSsc { get; set; } = new();
  public static ObservableCollection<HistogramData> BackingGreenSsc { get; set; } = new();
  public static ObservableCollection<HistogramData> BackingReporter { get; set; } = new();
  public static int[] bReporter = new int[HistogramData.Bins.Length];
  public static int[] bFsc = new int[HistogramData.Bins.Length];
  public static int[] bRed = new int[HistogramData.Bins.Length];
  public static int[] bGreen = new int[HistogramData.Bins.Length];
  public static int[] bViolet = new int[HistogramData.Bins.Length];
  public static int[] cReporter = new int[HistogramData.Bins.Length];
  public static int[] cFsc = new int[HistogramData.Bins.Length];
  public static int[] cRed = new int[HistogramData.Bins.Length];
  public static int[] cGreen = new int[HistogramData.Bins.Length];
  public static int[] cViolet = new int[HistogramData.Bins.Length];

  protected ScatterData()
  {
    for (var i = 0; i < HistogramData.Bins.Length; i++)
    {
      CurrentForwardSsc.Add(new HistogramData(0, HistogramData.Bins[i]));
      CurrentVioletSsc.Add(new HistogramData(0, HistogramData.Bins[i]));
      CurrentRedSsc.Add(new HistogramData(0, HistogramData.Bins[i]));
      CurrentGreenSsc.Add(new HistogramData(0, HistogramData.Bins[i]));
      CurrentReporter.Add(new HistogramData(0, HistogramData.Bins[i]));

      BackingForwardSsc.Add(new HistogramData(0, HistogramData.Bins[i]));
      BackingVioletSsc.Add(new HistogramData(0, HistogramData.Bins[i]));
      BackingRedSsc.Add(new HistogramData(0, HistogramData.Bins[i]));
      BackingGreenSsc.Add(new HistogramData(0, HistogramData.Bins[i]));
      BackingReporter.Add(new HistogramData(0, HistogramData.Bins[i]));
    }

    DisplayedForwardSsc = CurrentForwardSsc;
    DisplayedVioletSsc = CurrentVioletSsc;
    DisplayedRedSsc = CurrentRedSsc;
    DisplayedGreenSsc = CurrentGreenSsc;
    DisplayedReporter = CurrentReporter;
  }

  public static ScatterData Create()
  {
    ViewModels.ScatterChartViewModel.Instance.ScttrData = ViewModelSource.Create(() => new ScatterData());
    return ViewModels.ScatterChartViewModel.Instance.ScttrData;
  }

  public void DisplayCurrent(bool current = true)
  {
    if (current)
    {
      DisplayedForwardSsc = CurrentForwardSsc;
      DisplayedVioletSsc = CurrentVioletSsc;
      DisplayedRedSsc = CurrentRedSsc;
      DisplayedGreenSsc = CurrentGreenSsc;
      DisplayedReporter = CurrentReporter;
    }
    else
    {
      DisplayedForwardSsc = BackingForwardSsc;
      DisplayedVioletSsc = BackingVioletSsc;
      DisplayedRedSsc = BackingRedSsc;
      DisplayedGreenSsc = BackingGreenSsc;
      DisplayedReporter = BackingReporter;
    }
  }

  public void ClearData(bool current = true)
  {
    if (current)
    {
      for (var i = 0; i < CurrentReporter.Count; i++)
      {
        CurrentForwardSsc[i].Value = 0;
        CurrentVioletSsc[i].Value = 0;
        CurrentRedSsc[i].Value = 0;
        CurrentGreenSsc[i].Value = 0;
        CurrentReporter[i].Value = 0;
      }
    }
    else
    {
      for (var i = 0; i < BackingReporter.Count; i++)
      {
        BackingForwardSsc[i].Value = 0;
        BackingVioletSsc[i].Value = 0;
        BackingRedSsc[i].Value = 0;
        BackingGreenSsc[i].Value = 0;
        BackingReporter[i].Value = 0;
      }
    }
  }

  private void ClearCounterArrays(bool current = true)
  {
    if (current)
    {
      Array.Clear(cReporter, 0, cReporter.Length);
      Array.Clear(cFsc, 0, cFsc.Length);
      Array.Clear(cRed, 0, cRed.Length);
      Array.Clear(cGreen, 0, cGreen.Length);
      Array.Clear(cViolet, 0, cViolet.Length);
    }
    else
    {
      Array.Clear(bReporter, 0, bReporter.Length);
      Array.Clear(bFsc, 0, bFsc.Length);
      Array.Clear(bRed, 0, bRed.Length);
      Array.Clear(bGreen, 0, bGreen.Length);
      Array.Clear(bViolet, 0, bViolet.Length);
    }
  }

  public void FillCurrentData(bool fromFile = false)
  {
    if (fromFile)
    {
      for (var i = 0; i < BackingReporter.Count; i++)
      {
        BackingReporter[i].Value += bReporter[i];
        BackingForwardSsc[i].Value += bFsc[i];
        BackingRedSsc[i].Value += bRed[i];
        BackingGreenSsc[i].Value += bGreen[i];
        BackingVioletSsc[i].Value += bViolet[i];
      }
    }
    else
    {
      for (var i = 0; i < CurrentReporter.Count; i++)
      {
        CurrentReporter[i].Value += cReporter[i];
        CurrentForwardSsc[i].Value += cFsc[i];
        CurrentRedSsc[i].Value += cRed[i];
        CurrentGreenSsc[i].Value += cGreen[i];
        CurrentVioletSsc[i].Value += cViolet[i];
      }
    }
    ClearCounterArrays(!fromFile);
  }
}