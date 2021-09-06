using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using System;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Charts;
using Ei_Dimension.Models;
using Ei_Dimension.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

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
    public virtual ObservableCollection<int> SeriesCL0 { get; set; }
    public virtual ObservableCollection<int> SeriesCL1 { get; set; }
    public virtual ObservableCollection<int> SeriesCL2 { get; set; }
    public virtual ObservableCollection<int> SeriesCL3 { get; set; }
    public virtual ObservableCollection<ResultFile> AvailableResults { get; set; }
    public virtual ObservableCollection<ResultFile> SelectedItem { get; set; }
    public virtual string HeatmapAxisXTitle { get; set; }
    public virtual string HeatmapAxisYTitle { get; set; }
    public virtual ObservableCollection<HeatMapData> HeatLevel1 { get; set; }
    public virtual ObservableCollection<HeatMapData> HeatLevel2 { get; set; }
    public virtual ObservableCollection<HeatMapData> HeatLevel3 { get; set; }
    public virtual ObservableCollection<HeatMapData> HeatLevel4 { get; set; }
    public virtual ObservableCollection<HeatMapData> HeatLevel5 { get; set; }
    public virtual ObservableCollection<bool> ScatterSelectorState { get; set; }
    public virtual ObservableCollection<bool> XAxisSelectorState { get; set; }
    public virtual ObservableCollection<bool> YAxisSelectorState { get; set; }

    private List<MicroCy.BeadInfoStruct> _beadStructsList;
    private ObservableCollection<HeatMapData> _heatMapSeriesXY;
    private List<SortedDictionary<int, int>> _histoDicts;
    private (int, int) _heatmapAxisXY;  //holds selected values for heatmap data
    private string _savedFilesLocation = @"C:\Users\Admin\Desktop\WorkC#\SampleData";

    public static ResultsViewModel Instance { get; private set; }

    protected ResultsViewModel()
    {
      GetAvailableResults();
      SelectedItem = new ObservableCollection<ResultFile>();
      _heatMapSeriesXY = new ObservableCollection<HeatMapData>();
      HeatmapAxisXTitle = "CL1";
      HeatmapAxisYTitle = "CL2";
      _heatmapAxisXY = (1, 2);

      HeatLevel1 = new ObservableCollection<HeatMapData>(); // Min     outside of bin
      HeatLevel2 = new ObservableCollection<HeatMapData>(); //         70%
      HeatLevel3 = new ObservableCollection<HeatMapData>(); // Medium  50%
      HeatLevel4 = new ObservableCollection<HeatMapData>(); //         30% of binwidth
      HeatLevel5 = new ObservableCollection<HeatMapData>(); // Max     bin center

      ScatterSelectorState = new ObservableCollection<bool> { false, false, false, false };
      byte temp = Settings.Default.ScatterGraphSelector;
      if (temp >= 8)
      {
        ScatterSelectorState[3] = true;
        temp -= 8;
      }
      if (temp >= 4)
      {
        ScatterSelectorState[2] = true;
        temp -= 4;
      }
      if (temp >= 2)
      {
        ScatterSelectorState[1] = true;
        temp -= 2;
      }
      if (temp >= 1)
      {
        ScatterSelectorState[0] = true;
      }

      XAxisSelectorState = new ObservableCollection<bool> { false, false, false, false };
      XAxisSelectorState[Settings.Default.XAxisG] = true;
      YAxisSelectorState = new ObservableCollection<bool> { false, false, false, false };
      YAxisSelectorState[Settings.Default.YAxisG] = true;

      Instance = this;
    }

    public static ResultsViewModel Create()
    {
      return ViewModelSource.Create(() => new ResultsViewModel());
    }

    public void GetAvailableResults()
    {
      if (AvailableResults == null)
      {
        AvailableResults = new ObservableCollection<ResultFile>();
      }
      else
      {
        AvailableResults.Clear();
      }
      try
      {
        string[] files = Directory.GetFiles(_savedFilesLocation, "*.csv");
        foreach (var f in files)
        {
          AvailableResults.Add(new ResultFile(f));
        }
      }
      catch
      {

      }
    }

    public async Task ParseBeadInfoAsync(string path)
    {
      if(_beadStructsList == null)
      {
        _beadStructsList = new List<MicroCy.BeadInfoStruct>();
      }
      else
      {
        _beadStructsList.Clear();
      }
      List<string> contents = await DataProcessor.GetDataFromFileAsync(path);

      for (var i = 0; i < contents.Count; i++)
      {
        _beadStructsList.Add(await Task.Run(() => {
          MicroCy.BeadInfoStruct bs;
          return bs = DataProcessor.ParseRow(contents[i]); }));
      }
    }

    [AsyncCommand(UseCommandManager = false)]
    public async void FillAllDataAsync()
    {
      if (_histoDicts != null)
      {
        foreach (var d in _histoDicts)
        {
          d.Clear();
        }
        _histoDicts.Clear();
      }
      await ParseBeadInfoAsync(SelectedItem[0].Path);
      _histoDicts = DataProcessor.MakeDictionariesFromData(_beadStructsList);
      Task<ObservableCollection<HistogramData>> ForwardTask = new Task<ObservableCollection<HistogramData>>(() => { return AddScatterItem(0); });
      ForwardTask.Start();
      Task<ObservableCollection<HistogramData>> VioletTask = new Task<ObservableCollection<HistogramData>>(() => { return AddScatterItem(1); });
      VioletTask.Start();
      Task<ObservableCollection<HistogramData>> RedTask = new Task<ObservableCollection<HistogramData>>(() => { return AddScatterItem(2); });
      RedTask.Start();
      Task<ObservableCollection<HistogramData>> GreenTask = new Task<ObservableCollection<HistogramData>>(() => { return AddScatterItem(3); });
      GreenTask.Start();
      Task<ObservableCollection<HistogramData>> ReporterTask = new Task<ObservableCollection<HistogramData>>(() => { return AddScatterItem(4); });
      ReporterTask.Start();
      Task<(ObservableCollection<int>, ObservableCollection<int>, ObservableCollection<int>, ObservableCollection<int>)> MapTask =
        new Task<(ObservableCollection<int>, ObservableCollection<int>, ObservableCollection<int>, ObservableCollection<int>)>(AddMapItem);
      MapTask.Start();

      ForwardSsc = await ForwardTask;
      VioletSsc = await VioletTask;
      RedSsc = await RedTask;
      GreenSsc = await GreenTask;
      Reporter = await ReporterTask;
      (SeriesCL0, SeriesCL1, SeriesCL2, SeriesCL3) = await MapTask;
      UpdateHeatmap();
    }

    public void ChangeScatterLegend(int num)
    {
      ScatterSelectorState[num] = !ScatterSelectorState[num];
      byte res = 0;
      res += ScatterSelectorState[0] ? (byte)1 : (byte)0;
      res += ScatterSelectorState[1] ? (byte)2 : (byte)0;
      res += ScatterSelectorState[2] ? (byte)4 : (byte)0;
      res += ScatterSelectorState[3] ? (byte)8 : (byte)0;
      Settings.Default.ScatterGraphSelector = res;
      Settings.Default.Save();
    }

    public void ChangeHeatmapX(byte num)
    {
      XAxisSelectorState[0] = false;
      XAxisSelectorState[1] = false;
      XAxisSelectorState[2] = false;
      XAxisSelectorState[3] = false;
      switch (num)
      {
        case 0:
          _heatmapAxisXY.Item1 = 0;
          HeatmapAxisXTitle = "CL0";
          break;
        case 1:
          _heatmapAxisXY.Item1 = 1;
          HeatmapAxisXTitle = "CL1";
          break;
        case 2:
          _heatmapAxisXY.Item1 = 2;
          HeatmapAxisXTitle = "CL2";
          break;
        case 3:
          _heatmapAxisXY.Item1 = 3;
          HeatmapAxisXTitle = "CL3";
          break;
      }
      XAxisSelectorState[num] = true;
      App.SetHeatmapX(num);
      UpdateHeatmap();
    }

    public void ChangeHeatmapY(byte num)
    {
      YAxisSelectorState[0] = false;
      YAxisSelectorState[1] = false;
      YAxisSelectorState[2] = false;
      YAxisSelectorState[3] = false;
      switch (num)
      {
        case 0:
          _heatmapAxisXY.Item2 = 0;
          HeatmapAxisYTitle = "CL0";
          break;
        case 1:
          _heatmapAxisXY.Item2 = 1;
          HeatmapAxisYTitle = "CL1";
          break;
        case 2:
          _heatmapAxisXY.Item2 = 2;
          HeatmapAxisYTitle = "CL2";
          break;
        case 3:
          _heatmapAxisXY.Item2 = 3;
          HeatmapAxisYTitle = "CL3";
          break;
      }
      YAxisSelectorState[num] = true;
      App.SetHeatmapY(num);
      UpdateHeatmap();
    }

    public void UpdateHeatmap()
    {
      _heatMapSeriesXY.Clear();
      ObservableCollection<int> x = new ObservableCollection<int>();
      ObservableCollection<int> y = new ObservableCollection<int>();
      switch (_heatmapAxisXY.Item1)
      {
        case 0:
          x = SeriesCL0;
          break;
        case 1:
          x = SeriesCL1;
          break;
        case 2:
          x = SeriesCL2;
          break;
        case 3:
          x = SeriesCL3;
          break;
      }
      switch (_heatmapAxisXY.Item2)
      {
        case 0:
          y = SeriesCL0;
          break;
        case 1:
          y = SeriesCL1;
          break;
        case 2:
          y = SeriesCL2;
          break;
        case 3:
          y = SeriesCL3;
          break;
      }
      int i = 0;
      if (x != null)
      {
        while (i < x.Count)
        {
          _heatMapSeriesXY.Add(new HeatMapData(x[i], y[i]));
          i++;
        }
        _ = MakeHeatmapAsync();
      }
    }

    private ObservableCollection<HistogramData> AddScatterItem(int itemIndex)
    {
      ObservableCollection<HistogramData> col = new ObservableCollection<HistogramData>();
      foreach (var x in _histoDicts[itemIndex])
      {
        col.Add(new HistogramData(x.Value, x.Key));
      }
      return col;
    }

    private (ObservableCollection<int>, ObservableCollection<int>, ObservableCollection<int>, ObservableCollection<int>) AddMapItem()
    {
      ObservableCollection<int> CL0 = new ObservableCollection<int>();
      ObservableCollection<int> CL1 = new ObservableCollection<int>();
      ObservableCollection<int> CL2 = new ObservableCollection<int>();
      ObservableCollection<int> CL3 = new ObservableCollection<int>();
      foreach (var bs in _beadStructsList)
      {
        CL0.Add((int)bs.cl0);
        CL1.Add((int)bs.cl1);
        CL2.Add((int)bs.cl2);
        CL3.Add((int)bs.cl3);
      }
      return (CL0, CL1, CL2, CL3);
    }

    public async Task MakeHeatmapAsync()  //should return several collections of diff color series
    {
      int xDict = _heatmapAxisXY.Item1 + 5; // 5 comes from DataProcessor.MakeDictionariesFromData
      int yDict = _heatmapAxisXY.Item2 + 5; // 5 comes from DataProcessor.MakeDictionariesFromData
      Task<List<HistogramData>> t1 = new Task<List<HistogramData>>(()=> { return DataProcessor.LinearizeDictionary(_histoDicts[xDict]);});
      t1.Start();
      Task<List<HistogramData>> t2 = new Task<List<HistogramData>>(() => { return DataProcessor.LinearizeDictionary(_histoDicts[yDict]); });
      t2.Start();
      List<HistogramData> XAxisHist;
      List<HistogramData> YAxisHist;

      //identify peaks
      var XAxisPeaks = DataProcessor.IdentifyPeaks(XAxisHist = await t1);
      var YAxisPeaks = DataProcessor.IdentifyPeaks(YAxisHist = await t2);

      HeatLevel1.Clear();
      HeatLevel2.Clear();
      HeatLevel3.Clear();
      HeatLevel4.Clear();
      HeatLevel5.Clear();

      double[] HeatLevels = {0.15, 0.25, 0.35, 0.5};

      //assign heat level to points based on binning
      for (var i = 0; i < _heatMapSeriesXY.Count - 1; i++)
      {
        var pointX = _heatMapSeriesXY[i].X;
        var pointY = _heatMapSeriesXY[i].Y;
        foreach (var peak in XAxisPeaks)
        { 
          //already assigned value
          if(_heatMapSeriesXY[i].IntensityX != 0)
          {
            break;
          }
          // identify which peak it corresponds to
          if (pointX < peak.Item3)
          {
            _heatMapSeriesXY[i].IntensityX = DataProcessor.AssignIntensity(pointX, peak, HeatLevels);
          }
        }

        foreach (var peak in YAxisPeaks)
        {
          //already assigned value
          if (_heatMapSeriesXY[i].IntensityY != 0)
          {
            break;
          }
          // identify which peak it corresponds to
          if (pointY < peak.Item3)
          {
            _heatMapSeriesXY[i].IntensityY = DataProcessor.AssignIntensity(pointY, peak, HeatLevels);
          }
        }
      }

      //sort points into HeatLevelX arrays
      foreach (var point in _heatMapSeriesXY)
      {
        int colorValue = point.IntensityX * point.IntensityY;

        if(colorValue >= 10)
        {
          HeatLevel5.Add(point);
          continue;
        }
        if (colorValue >= 5)
        {
          HeatLevel4.Add(point);
          continue;
        }
        if (colorValue >= 3)
        {
          HeatLevel3.Add(point);
          continue;
        }
        if (colorValue >= 2)
        {
          HeatLevel2.Add(point);
          continue;
        }
        HeatLevel1.Add(point);
      }

      //add nearbyneighbor analysis for better result
    }
  }
}