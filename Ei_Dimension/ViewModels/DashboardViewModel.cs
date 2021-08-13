using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Map;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class DashboardViewModel
  {
    public List<DataModel> Items { get; set; }
    protected DashboardViewModel()
    {
      Items = new List<DataModel>();
      Items.Add(new DataModel(0, 0));
      Items.Add(new DataModel(10, 10));
      Items.Add(new DataModel(20, 20));
      Items.Add(new DataModel(30, 30));

      ChoroplethColorizer colorizer = new ChoroplethColorizer();
      colorizer.RangeStops = new System.Windows.Media.DoubleCollection(new double[]{ 0.1, 0.2, 0.7, 1 });
      colorizer.Colors.Add(System.Windows.Media.Color.FromArgb(50, 128, 255, 0));
      colorizer.Colors.Add(System.Windows.Media.Color.FromArgb(255, 255, 255, 0));
      colorizer.Colors.Add(System.Windows.Media.Color.FromArgb(255, 234, 72, 58));
      colorizer.Colors.Add(System.Windows.Media.Color.FromArgb(255, 162, 36, 25));
      colorizer.ApproximateColors = true;

      HeatmapDataSourceAdapter adapter = new HeatmapDataSourceAdapter();
      adapter.DataSource = Items;

      HeatmapProvider provider = new HeatmapProvider();
      provider.PointSource = adapter;
      provider.Algorithm = new HeatmapDensityBasedAlgorithm { PointRadius = 8 };
      provider.Colorizer = colorizer;
    }

    public static DashboardViewModel Create()
    {
      return ViewModelSource.Create(() => new DashboardViewModel());
    }
  }

  public class DataModel
  {
    public double X { get; set; }
    public double Y { get; set; }
    public DataModel(double x, double y)
    {
      X = x;
      Y = y;
    }
  }

}