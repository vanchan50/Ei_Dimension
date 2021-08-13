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
using System.Linq;

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
    public virtual ObservableCollection<HeatMapData> HeatMapSeriesXY { get; set; }
    public virtual string HeatmapAxisXTitle { get; set; }
    public virtual string HeatmapAxisYTitle { get; set; }

    private List<BeadInfoStruct> _beadStructsList;
    private List<SortedDictionary<int, int>> _histoDicts;
    private (int, int) _heatmapAxisXY;

    protected ResultsViewModel()
    {
      GetAvailableResults();
      SelectedItem = new ObservableCollection<ResultFile>();
      HeatMapSeriesXY = new ObservableCollection<HeatMapData>();
      HeatmapAxisXTitle = "CL1";
      HeatmapAxisYTitle = "CL2";
      _heatmapAxisXY = (1,2);
    }

    public static ResultsViewModel Create()
    {
      return ViewModelSource.Create(() => new ResultsViewModel());
    }

    public void GetAvailableResults()
    {
      if(AvailableResults == null)
      {
        AvailableResults = new ObservableCollection<ResultFile>();
      }
      AvailableResults.Clear();
      string[] files = Directory.GetFiles(@"C:\Users\Admin\Desktop\WorkC#\SampleData", "*.csv");
      foreach(var f in files)
      {
        AvailableResults.Add(new ResultFile(f));
      }
    }

    public void ParseBeadInfo(string path)
    {
      if(_beadStructsList == null)
      {
        _beadStructsList = new List<BeadInfoStruct>();
      }
      else
      {
        _beadStructsList.Clear();
      }
      string contents = GetDataFromFile(path);

      while (contents.Length > 0)
      {
        BeadInfoStruct bs = ParseRow(ref contents);
        _beadStructsList.Add(bs);
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
      await Task.Run(()=> { ParseBeadInfo(SelectedItem[0].Path); } );
      _histoDicts = MakeDictionariesFromData();
      Task<ObservableCollection<HistogramData>> ForwardTask = new Task<ObservableCollection<HistogramData>>(AddForwardItem);
      ForwardTask.Start();
      Task<ObservableCollection<HistogramData>> VioletTask = new Task<ObservableCollection<HistogramData>>(AddVioletItem);
      VioletTask.Start();
      Task<ObservableCollection<HistogramData>> RedTask = new Task<ObservableCollection<HistogramData>>(AddRedItem);
      RedTask.Start();
      Task<ObservableCollection<HistogramData>> GreenTask = new Task<ObservableCollection<HistogramData>>(AddGreenItem);
      GreenTask.Start();
      Task<ObservableCollection<HistogramData>> ReporterTask = new Task<ObservableCollection<HistogramData>>(AddReporterItem);
      ReporterTask.Start();
      Task<(ObservableCollection<int>, ObservableCollection<int>, ObservableCollection<int>, ObservableCollection<int>)> MapTask =
        new Task<(ObservableCollection<int>, ObservableCollection<int>, ObservableCollection<int>, ObservableCollection<int>)>(AddMapItem);
      MapTask.Start();
      await ForwardTask;
      ForwardSsc = ForwardTask.Result;
      await VioletTask;
      VioletSsc = VioletTask.Result;
      await RedTask;
      RedSsc = RedTask.Result;
      await GreenTask;
      GreenSsc = GreenTask.Result;
      await ReporterTask;
      Reporter = ReporterTask.Result;
      await MapTask;
      (SeriesCL0, SeriesCL1, SeriesCL2, SeriesCL3) = MapTask.Result;
      UpdateHeatmap();
    }

    public void ChangeHeatmapX(int index)
    {
      switch (index)
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
      UpdateHeatmap();
    }
    public void ChangeHeatmapY(int index)
    {
      switch (index)
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
      UpdateHeatmap();
    }
    public void UpdateHeatmap()
    {
      HeatMapSeriesXY.Clear();
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
      while (i < x.Count)
      {
        HeatMapSeriesXY.Add(new HeatMapData(x[i],y[i]));
        i++;
      }
    }

    private string GetDataFromFile(string path)
    {
      string str;
      using (System.IO.FileStream fin = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
      using (System.IO.StreamReader sr = new System.IO.StreamReader(fin))
      {
        sr.ReadLine();
        str = sr.ReadToEnd();
      }
      return str;
    }

    private BeadInfoStruct ParseRow(ref string data)
    {
      BeadInfoStruct Binfo;
      Binfo.Header = uint.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.EventTime = uint.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.fsc_bg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.vssc_bg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.cl0_bg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.cl1_bg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.cl2_bg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.cl3_bg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.rssc_bg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.gssc_bg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.greenB_bg = ushort.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.greenC_bg = ushort.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.greenB = ushort.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.greenC = ushort.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.l_offset_rg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.l_offset_gv = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.region = ushort.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.fsc = float.Parse(data.Substring(0, data.IndexOf(',')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.violetssc = float.Parse(data.Substring(0, data.IndexOf(',')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.cl0 = float.Parse(data.Substring(0, data.IndexOf(',')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.redssc = float.Parse(data.Substring(0, data.IndexOf(',')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.cl1 = float.Parse(data.Substring(0, data.IndexOf(',')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.cl2 = float.Parse(data.Substring(0, data.IndexOf(',')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.cl3 = float.Parse(data.Substring(0, data.IndexOf(',')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.greenssc = float.Parse(data.Substring(0, data.IndexOf(',')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf(',') + 1);
      Binfo.reporter = float.Parse(data.Substring(0, data.IndexOf('\r')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf('\r') + 1);
      return Binfo;
    }

    private List<SortedDictionary<int, int>> MakeDictionariesFromData()
    {
      SortedDictionary<int, int> dictForward = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictVioletssc = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictRedssc = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictGreenssc = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictReporter = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictCl0 = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictCl1 = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictCl2 = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictCl3 = new SortedDictionary<int, int>(); //value,bin
      int key;
      foreach (var bs in _beadStructsList)
      {
        key = (int)bs.reporter;
        if (dictReporter.ContainsKey(key))
        {
          dictReporter[key]++;
        }
        else
        {
          dictReporter.Add(key, 1);
        }
        
        key = (int)bs.fsc;
        if (dictForward.ContainsKey(key))
        {
          dictForward[key]++;
        }
        else
        {
          dictForward.Add(key, 1);
        }

        key = (int)bs.violetssc;
        if (dictVioletssc.ContainsKey(key))
        {
          dictVioletssc[key]++;
        }
        else
        {
          dictVioletssc.Add(key, 1);
        }

        key = (int)bs.redssc;
        if (dictRedssc.ContainsKey(key))
        {
          dictRedssc[key]++;
        }
        else
        {
          dictRedssc.Add(key, 1);
        }

        key = (int)bs.greenssc;
        if (dictGreenssc.ContainsKey(key))
        {
          dictGreenssc[key]++;
        }
        else
        {
          dictGreenssc.Add(key, 1);
        }

        //bin these into 50 point bins
        for(var i = 50; i < 10050; i += 50)
        {
          dictCl0.Add(i, 0);
          dictCl1.Add(i, 0);
          dictCl2.Add(i, 0);
          dictCl3.Add(i, 0);
        }

        for (var i = 50; i < 10050; i += 50)
        {
          if(bs.cl0 > i)
          {
            continue;
          }
          dictCl0[i]++;
        }

      //  key = (int)bs.cl0;
      //  if (dictCl0.ContainsKey(key))
      //  {
      //    dictCl0[key]++;
      //  }
      //  else
      //  {
      //    dictCl0.Add(key, 1);
      //  }

      //  key = (int)bs.cl1;
      //  if (dictCl1.ContainsKey(key))
      //  {
      //    dictCl1[key]++;
      //  }
      //  else
      //  {
      //    dictCl1.Add(key, 1);
      //  }
      //
      //  key = (int)bs.cl2;
      //  if (dictCl2.ContainsKey(key))
      //  {
      //    dictCl2[key]++;
      //  }
      //  else
      //  {
      //    dictCl2.Add(key, 1);
      //  }
      //
      //  key = (int)bs.cl3;
      //  if (dictCl3.ContainsKey(key))
      //  {
      //    dictCl3[key]++;
      //  }
      //  else
      //  {
      //    dictCl3.Add(key, 1);
      //  }
      }

      var lst = new List<SortedDictionary<int, int>>();
      lst.Add(dictForward);
      lst.Add(dictVioletssc);
      lst.Add(dictRedssc);
      lst.Add(dictGreenssc);
      lst.Add(dictReporter);
      lst.Add(dictCl0);
      lst.Add(dictCl1);
      lst.Add(dictCl2);
      lst.Add(dictCl3);
      return lst;
    }
    #region Ugly
    private ObservableCollection<HistogramData> AddForwardItem()
    {
      ObservableCollection<HistogramData> col = new ObservableCollection<HistogramData>();
      foreach (var x in _histoDicts[0])
      {
        col.Add(new HistogramData(x.Value, x.Key));
      }
      return col;
    }
    private ObservableCollection<HistogramData> AddVioletItem()
    {
      ObservableCollection<HistogramData> col = new ObservableCollection<HistogramData>();
      foreach (var x in _histoDicts[1])
      {
        col.Add(new HistogramData(x.Value, x.Key));
      }
      return col;
    }
    private ObservableCollection<HistogramData> AddRedItem()
    {
      ObservableCollection<HistogramData> col = new ObservableCollection<HistogramData>();
      foreach (var x in _histoDicts[2])
      {
        col.Add(new HistogramData(x.Value, x.Key));
      }
      return col;
    }
    private ObservableCollection<HistogramData> AddGreenItem()
    {
      ObservableCollection<HistogramData> col = new ObservableCollection<HistogramData>();
      foreach (var x in _histoDicts[3])
      {
        col.Add(new HistogramData(x.Value, x.Key));
      }
      return col;
    }
    private ObservableCollection<HistogramData> AddReporterItem()
    {
      ObservableCollection<HistogramData> col = new ObservableCollection<HistogramData>();
      foreach (var x in _histoDicts[4])
      {
        col.Add(new HistogramData(x.Value, x.Key));
      }
      return col;
    }
    //AddMapItem is different
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
    #endregion

    public void MakeHeatmap()  //should return several collections of diff color series
    {
      //_histoDicts[5];678;
      List<HistogramData> CL1Hist = new List<HistogramData>();
    //  foreach(var x in _histoDicts[6])
    //  {
    //    CL1Hist.Add(new HistogramData(x.Value, x.Key));
    //  }
      int max = _histoDicts[6].ElementAt(_histoDicts[6].Count - 1).Key; //to include zero points => average properly
      for (var i = 1; i <= max; i++)
      {
        int temp=0;
        _histoDicts[6].TryGetValue(i, out temp);
        CL1Hist.Add(new HistogramData(temp, i));
      }
      List<HistogramData> CL2Hist = new List<HistogramData>();
    //  foreach (var x in _histoDicts[7])
    //  {
    //    CL2Hist.Add(new HistogramData(x.Value, x.Key));
    //  }
      max = _histoDicts[7].ElementAt(_histoDicts[7].Count - 1).Key;
      for (var i = 1; i <= max; i++)
      {
        int temp = 0;
        _histoDicts[7].TryGetValue(i, out temp);
        CL2Hist.Add(new HistogramData(temp, i));
      }


      CL1Hist = HeatmapDataProcessor.MovAvgFilter(CL1Hist);
      CL2Hist = HeatmapDataProcessor.MovAvgFilter(CL2Hist);
      #region quicktest
    //  using (System.IO.FileStream str = new FileStream(@"C:\Users\Admin\Desktop\WorkC#\data3.csv", FileMode.Create))
    //  using (StreamWriter sw = new StreamWriter(str))
    //  {
    //    foreach (var x in CL1Hist)
    //    {
    //      sw.WriteLine($"{x.Argument},{x.Value}");
    //    }
    //  }
      using (System.IO.FileStream str = new FileStream(@"C:\Users\Admin\Desktop\WorkC#\data4.csv", FileMode.Create))
        using (StreamWriter sw = new StreamWriter(str))
        {
          foreach (var x in CL2Hist)
          {
            sw.WriteLine($"{x.Argument},{x.Value}");
          }
        }
      //mmaybe should search dictionaries
      #endregion
      //need 5 gradations (very hot, hot, medium, cool, cold)
      //need histogramdata for cl0-cl3
      //from histogramdata define heatregions [cold][cool][medium][hot][very hot][hot][medium][cool][cold] x3
      //attach a property to points based on region belonging
      //fill pointsources with necessary points
      //that's part1- result would be circular'ish

      //add nearbyneighbor analysis for better result
    }
  }
}