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
    public virtual ObservableCollection<HistogramData<int, int>> ForwardSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> VioletSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> RedSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> GreenSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> Reporter { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CL0 { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CL1 { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CL2 { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CL3 { get; set; }
    public virtual ObservableCollection<HeatMapData> Map { get; set; }
    public static ResultsViewModel Instance { get; private set; }

    protected ResultsViewModel()
    {
      Instance = this;
      ForwardSsc = new ObservableCollection<HistogramData<int, int>>();
      VioletSsc = new ObservableCollection<HistogramData<int, int>>();
      RedSsc = new ObservableCollection<HistogramData<int, int>>();
      GreenSsc = new ObservableCollection<HistogramData<int, int>>();
      Reporter = new ObservableCollection<HistogramData<int, int>>();
      CL0 = new ObservableCollection<HistogramData<int, int>>();
      CL1 = new ObservableCollection<HistogramData<int, int>>();
      CL2 = new ObservableCollection<HistogramData<int, int>>();
      CL3 = new ObservableCollection<HistogramData<int, int>>();

      var bins = Core.DataProcessor.GenerateLogSpace(1, 1000000, 384);
      for (var i = 0; i < bins.Length; i++)
      {
        ForwardSsc.Add(new HistogramData<int, int>(0, bins[i]));
        VioletSsc.Add(new HistogramData<int, int>(0, bins[i]));
        RedSsc.Add(new HistogramData<int, int>(0, bins[i]));
        GreenSsc.Add(new HistogramData<int, int>(0, bins[i]));
        Reporter.Add(new HistogramData<int, int>(0, bins[i]));
        CL0.Add(new HistogramData<int, int>(0, bins[i]));
        CL1.Add(new HistogramData<int, int>(0, bins[i]));
        CL2.Add(new HistogramData<int, int>(0, bins[i]));
        CL3.Add(new HistogramData<int, int>(0, bins[i]));
      }

      Map = new ObservableCollection<HeatMapData>();
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