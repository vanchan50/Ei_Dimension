using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Ei_Dimension.Models;
using System;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ResultsViewModel
  {
    public virtual ObservableCollection<HistogramData> ForwardSsc { get; set; }
    public virtual ObservableCollection<HistogramData> VioletSsc { get; set; }
    public virtual ObservableCollection<HistogramData> RedSsc { get; set; }
    public virtual ObservableCollection<HistogramData> GreenSsc { get; set; }
    public virtual ObservableCollection<HistogramData> Reporter { get; set; }
    public static ResultsViewModel Instance { get; private set; }

    protected ResultsViewModel()
    {
      Instance = this;
      ForwardSsc = new ObservableCollection<HistogramData>();
      VioletSsc = new ObservableCollection<HistogramData>();
      RedSsc = new ObservableCollection<HistogramData>();
      GreenSsc = new ObservableCollection<HistogramData>();
      Reporter = new ObservableCollection<HistogramData>();
      //64 bins per decade. 1-9;10-99;100-999;1000-9999;10000-99999;
      double[] data = new double[384];
      for(var i = 1; i < 385; i++)
      {
        data[i-1] = Math.Log(i);
      }

      for(var i = 0; i < 256; i++)
      {
        int bin = (int)Math.Exp(i / 24.526);  //make const bin array in microcy. place bin[i] as xAxis
        ForwardSsc.Add(new HistogramData(0, i));
        VioletSsc.Add(new HistogramData(0, i));
        RedSsc.Add(new HistogramData(0, i));
        GreenSsc.Add(new HistogramData(0, i));
        Reporter.Add(new HistogramData(0, i));
      }
    }

    public static ResultsViewModel Create()
    {
      return ViewModelSource.Create(() => new ResultsViewModel());
    }

    public void ClearGraphs()
    {
      for (var i = 0; i < 256; i++)
      {
        ForwardSsc[i].Value = 0;
        VioletSsc[i].Value = 0;
        RedSsc[i].Value = 0;
        GreenSsc[i].Value = 0;
        Reporter[i].Value = 0;
      }
    }
  }
}