using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using System;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Charts;
using Ei_Dimension.Models;
using System.Collections.Generic;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ResultsViewModel
  {
    public virtual SampleData1 SampleForward {get;set; }
    public virtual SampleData1 SampleViolet { get; set; }
    public virtual SampleData1 SampleGreen { get; set; }
    public virtual SampleData1 SampleRed { get; set; }
    public virtual ObservableCollection<float> Reporter { get; set; }
    public virtual ObservableCollection<HistogramData> GreenC { get; set; }
    private List<BeadInfoStruct> BeadStructs { get; set; }

    protected ResultsViewModel()
    {
      SampleViolet = new SampleData1();
      SampleGreen = new SampleData1();
      SampleRed = new SampleData1();

      BeadStructs = new List<BeadInfoStruct>();

      Reporter = new ObservableCollection<float>();
      GreenC = new ObservableCollection<HistogramData>();

    //  ParseBeadInfo(@"C:\Users\Admin\Desktop\WorkC#\SampleData\Mon Run 2AA3_0.csv");

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

    public void AddReporterItem()
    {
      ParseBeadInfo(@"C:\Users\Admin\Desktop\WorkC#\SampleData\Mon Run 2AA3_0.csv");

      SortedDictionary<int, int> dict = new SortedDictionary<int, int>(); //value,bin
      int key;
      foreach (var bs in BeadStructs)
      {
        key = (int)bs.greenC;
        if (dict.ContainsKey(key))
        {
          dict[key]++;
        }
        else
        {
          dict.Add(key, 1);
        }
      }

      foreach (var x in dict)
      {
        GreenC.Add(new HistogramData(x.Value, x.Key));
      }
    }

    public void ParseBeadInfo(string path)
    {
      string contents;
      using (System.IO.FileStream fin = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
      using (System.IO.StreamReader sr = new System.IO.StreamReader(fin))
      {
        sr.ReadLine();
        contents = sr.ReadToEnd();
      }

      while (contents.Length > 0)
      {
        BeadInfoStruct bs = new BeadInfoStruct();
        ParseRow(ref bs, ref contents);
        BeadStructs.Add(bs);
      }

    }
    private void ParseRow(ref BeadInfoStruct struc, ref string data )
    {
      struc.Header = uint.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.EventTime = uint.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.fsc_bg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.vssc_bg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.cl0_bg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.cl1_bg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.cl2_bg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.cl3_bg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.rssc_bg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.gssc_bg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.greenB_bg = ushort.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.greenC_bg = ushort.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.greenB = ushort.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.greenC = ushort.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.l_offset_rg = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.l_offset_gv = byte.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.region = ushort.Parse(data.Substring(0, data.IndexOf(',')));
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.fsc = float.Parse(data.Substring(0, data.IndexOf(',')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.violetssc = float.Parse(data.Substring(0, data.IndexOf(',')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.cl0 = float.Parse(data.Substring(0, data.IndexOf(',')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.redssc = float.Parse(data.Substring(0, data.IndexOf(',')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.cl1 = float.Parse(data.Substring(0, data.IndexOf(',')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.cl2 = float.Parse(data.Substring(0, data.IndexOf(',')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.cl3 = float.Parse(data.Substring(0, data.IndexOf(',')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.greenssc = float.Parse(data.Substring(0, data.IndexOf(',')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf(',') + 1);
      struc.reporter = float.Parse(data.Substring(0, data.IndexOf('\r')), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
      data = data.Remove(0, data.IndexOf('\r') + 1);
    }
  }
}