using DevExpress.Xpf.Charts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ei_Dimension.Views
{
  /// <summary>
  /// Interaction logic for ResultsView.xaml
  /// </summary>
  public partial class ResultsView : UserControl
  {
    public static ResultsView Instance;
    public ResultsView()
    {
      InitializeComponent();
      Instance = this;
      ChC.AxisX.WholeRange.AutoSideMargins = false;
      ChC.AxisX.WholeRange.SetMinMaxValues(10, 35000);
      ChC.AxisY.WholeRange.AutoSideMargins = false;
      ChC.AxisY.WholeRange.SetMinMaxValues(10, 35000);
    }
    public void AddXYPoint(double x, double y, SolidColorBrush brush)
    {
      var Point = new SeriesPoint(x, y);
      Point.Brush = brush;
      HeatMap.Points.Add(Point);
    }
    public void ChangePointColor(int index, SolidColorBrush brush)
    {
      HeatMap.Points[index].Brush = brush;
    }
    public void ClearPoints()
    {
      HeatMap.Points.Clear();
    }

    public void PrintXY(int Resoultion_dpi = 800)
    {
      var options = new DevExpress.XtraPrinting.ImageExportOptions();
      options.TextRenderingMode = DevExpress.XtraPrinting.TextRenderingMode.SingleBitPerPixelGridFit;
      options.Resolution = Resoultion_dpi; options.Format = new System.Drawing.Imaging.ImageFormat(System.Drawing.Imaging.ImageFormat.Png.Guid);
      string date = DateTime.Now.ToString("dd.MM.yyyy.hhtt-mm-ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
      ChC.AxisX.Title.Visible = true;
      ChC.AxisY.Title.Visible = true;
      if (!Directory.Exists(App.Device.Outdir + "\\SavedImages"))
        Directory.CreateDirectory(App.Device.Outdir + "\\SavedImages");
      XYPlot.ExportToImage(App.Device.Outdir + @"\SavedImages\" + date + ".png", options);
      ChC.AxisX.Title.Visible = false;
      ChC.AxisY.Title.Visible = false;
      printXY.Visibility = Visibility.Hidden;
    }
    public void PrintScatter(int Resoultion_dpi = 800)
    {
      var options = new DevExpress.XtraPrinting.ImageExportOptions();
      options.TextRenderingMode = DevExpress.XtraPrinting.TextRenderingMode.SingleBitPerPixelGridFit;
      options.Resolution = Resoultion_dpi; options.Format = new System.Drawing.Imaging.ImageFormat(System.Drawing.Imaging.ImageFormat.Png.Guid);
      string date = DateTime.Now.ToString("dd.MM.yyyy.hhtt-mm-ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
      if (!Directory.Exists(App.Device.Outdir + "\\SavedImages"))
        Directory.CreateDirectory(App.Device.Outdir + "\\SavedImages");
      ScatterPlot.ExportToImage(App.Device.Outdir + @"\SavedImages\" + date + ".png", options);
      printSC.Visibility = Visibility.Hidden;
    }

    private void XYPlot_MouseEnter(object sender, MouseEventArgs e)
    {
      printXY.Visibility = Visibility.Visible;
    }

    private void XYPlot_MouseLeave(object sender, MouseEventArgs e)
    {
      printXY.Visibility = Visibility.Hidden;
    }

    private void Grid_MouseEnter(object sender, MouseEventArgs e)
    {
      printSC.Visibility = Visibility.Visible;
    }

    private void Grid_MouseLeave(object sender, MouseEventArgs e)
    {
      printSC.Visibility = Visibility.Hidden;
    }
  }
}
