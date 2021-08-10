using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using System;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Charts;
using Ei_Dimension.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ResultsViewModel
  {
    public virtual SampleData1 SampleForward {get;set; }
    public virtual SampleData1 SampleViolet { get; set; }
    public virtual SampleData1 SampleGreen { get; set; }
    public virtual SampleData1 SampleRed { get; set; }
    public virtual ObservableCollection<HistogramData> VioletSsc { get; set; }
    public virtual ObservableCollection<HistogramData> RedSsc { get; set; }
    public virtual ObservableCollection<HistogramData> GreenSsc { get; set; }
    public virtual ObservableCollection<HistogramData> Reporter { get; set; }
    public virtual ObservableCollection<HistogramData> GreenC { get; set; }
    private List<BeadInfoStruct> BeadStructs { get; set; }

    protected ResultsViewModel()
    {
      SampleViolet = new SampleData1();
      SampleGreen = new SampleData1();
      SampleRed = new SampleData1();

      BeadStructs = new List<BeadInfoStruct>();


      ParseBeadInfo(@"C:\Users\Admin\Desktop\WorkC#\SampleData\Mon Run 2AA3_0.csv");

    }

    public static ResultsViewModel Create()
    {
      return ViewModelSource.Create(() => new ResultsViewModel());
    }

    public void AddScatterItem()
    {
      var r = new Random();
      foreach(var sc in SampleViolet)
      {
        sc.Intensity = r.Next(1,100);
        sc.Wavelength = r.Next(400, 1000);
      }
      SampleViolet.Add(new Scatter(r.Next(400,1000),r.Next(1,100)));
      foreach (var sc in SampleGreen)
      {
        sc.Intensity = r.Next(1, 100);
        sc.Wavelength = r.Next(400, 1000);
      }
      SampleGreen.Add(new Scatter(r.Next(400, 1000), r.Next(1, 100)));
      foreach (var sc in SampleRed)
      {
        sc.Intensity = r.Next(1, 100);
        sc.Wavelength = r.Next(400, 1000);
      }
      SampleRed.Add(new Scatter(r.Next(400, 1000), r.Next(1, 100)));
    }

    public async void FillReporter()
    {
      Task<ObservableCollection<HistogramData>> t = new Task<ObservableCollection<HistogramData>>(AddReporterItem);
      t.Start();
      await t;
      Reporter = t.Result;
    }

    private ObservableCollection<HistogramData> AddReporterItem()
    {
      var lst = MakeDictionariesFromData();
      ObservableCollection<HistogramData> col = new ObservableCollection<HistogramData>();
      foreach (var x in lst[3])
      {
        col.Add(new HistogramData(x.Value, x.Key));
      }
      return col;
    }

    private List<SortedDictionary<int, int>> MakeDictionariesFromData()
    {
      SortedDictionary<int, int> dictVioletssc = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictRedssc = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictGreenssc = new SortedDictionary<int, int>(); //value,bin
      SortedDictionary<int, int> dictReporter = new SortedDictionary<int, int>(); //value,bin
      int key;
      foreach (var bs in BeadStructs)
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
      }

      var lst = new List<SortedDictionary<int, int>>();
      lst.Add(dictVioletssc);
      lst.Add(dictRedssc);
      lst.Add(dictGreenssc);
      lst.Add(dictReporter);
      return lst;
    }

    public void ParseBeadInfo(string path)
    {
      string contents = GetDataFromFile(path);

      while (contents.Length > 0)
      {
        BeadInfoStruct bs = ParseRow(ref contents);
        BeadStructs.Add(bs);
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

    private BeadInfoStruct ParseRow(ref string data )
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
  }
}