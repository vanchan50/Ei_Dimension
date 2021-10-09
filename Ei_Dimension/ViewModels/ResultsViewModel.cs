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
    public virtual ObservableCollection<HistogramData<int,double>> ForwardSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, double>> VioletSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, double>> RedSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, double>> GreenSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, double>> Reporter { get; set; }
    public virtual ObservableCollection<string> ActiveRegionsCount { get; set; }
    public virtual ObservableCollection<string> ActiveRegionsMean { get; set; }
    public static ResultsViewModel Instance { get; private set; }

    protected ResultsViewModel()
    {
      Instance = this;
      ForwardSsc = new ObservableCollection<HistogramData<int, double>>();
      VioletSsc = new ObservableCollection<HistogramData<int, double>>();
      RedSsc = new ObservableCollection<HistogramData<int, double>>();
      GreenSsc = new ObservableCollection<HistogramData<int, double>>();
      Reporter = new ObservableCollection<HistogramData<int, double>>();
      var bins = Core.DataProcessor.GenerateLogSpace(1, 1000000, 384);
      for (var i = 0; i < bins.Length; i++)
      {
        ForwardSsc.Add(new HistogramData<int, double>(0, bins[i]));
        VioletSsc.Add(new HistogramData<int, double>(0, bins[i]));
        RedSsc.Add(new HistogramData<int, double>(0, bins[i]));
        GreenSsc.Add(new HistogramData<int, double>(0, bins[i]));
        Reporter.Add(new HistogramData<int, double>(0, bins[i]));
      }

      ActiveRegionsCount = new ObservableCollection<string>();
      ActiveRegionsMean = new ObservableCollection<string>();
    }

    public static ResultsViewModel Create()
    {
      return ViewModelSource.Create(() => new ResultsViewModel());
    }

    public void ClearGraphs()
    {
      for (var i = 0; i < Reporter.Count; i++)
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