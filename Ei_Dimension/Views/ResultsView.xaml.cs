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
      if (Program.SpecializedVer == CompanyID.China)
      {
        InstrumentLogo.Source = new BitmapImage(new Uri(@"/Ei_Dimension;component/Icons/dimension flow analyzer logoCh.png", UriKind.Relative));
      }
      Instance = this;
      ChC.AxisX.WholeRange.AutoSideMargins = false;
      ChC.AxisX.WholeRange.SetMinMaxValues(10, 35000);
      ChC.AxisY.WholeRange.AutoSideMargins = false;
      ChC.AxisY.WholeRange.SetMinMaxValues(10, 35000);
    }

    public void AddXYPoint(double x, double y, SolidColorBrush brush, bool LargeXY = false)
    {
      var point = new SeriesPoint(x, y);
      point.Brush = brush;
      var heatmap = LargeXY ? LargeHeatMap : HeatMap;
      heatmap.Points.Add(point);
    }

    public void ChangePointColor(int index, SolidColorBrush brush)
    {
      HeatMap.Points[index].Brush = brush;
    }

    public void ClearPoints()
    {
      HeatMap.Points.Clear();
      LargeHeatMap.Points.Clear();
    }

    public void PrintXY(int resoultionDpi = 800)
    {
      var options = new DevExpress.XtraPrinting.ImageExportOptions
      {
        TextRenderingMode = DevExpress.XtraPrinting.TextRenderingMode.SingleBitPerPixelGridFit,
        Resolution = resoultionDpi,
        Format = new System.Drawing.Imaging.ImageFormat(System.Drawing.Imaging.ImageFormat.Png.Guid)
      };
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

    public void PrintScatter(int resoultionDpi = 800)
    {
      var options = new DevExpress.XtraPrinting.ImageExportOptions
      {
        TextRenderingMode = DevExpress.XtraPrinting.TextRenderingMode.SingleBitPerPixelGridFit,
        Resolution = resoultionDpi,
        Format = new System.Drawing.Imaging.ImageFormat(System.Drawing.Imaging.ImageFormat.Png.Guid)
      };
      string date = DateTime.Now.ToString("dd.MM.yyyy.hhtt-mm-ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
      if (!Directory.Exists(App.Device.Outdir + "\\SavedImages"))
        Directory.CreateDirectory(App.Device.Outdir + "\\SavedImages");
      ScatterPlot.ExportToImage(App.Device.Outdir + @"\SavedImages\" + date + ".png", options);
      printSC.Visibility = Visibility.Hidden;
    }

    public void Print3D(int resoultionDpi = 800)
    {
      var options = new DevExpress.XtraPrinting.ImageExportOptions
      {
        TextRenderingMode = DevExpress.XtraPrinting.TextRenderingMode.SingleBitPerPixelGridFit,
        Resolution = resoultionDpi,
        Format = new System.Drawing.Imaging.ImageFormat(System.Drawing.Imaging.ImageFormat.Png.Guid)
      };
      string date = DateTime.Now.ToString("dd.MM.yyyy.hhtt-mm-ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
      if (!Directory.Exists(App.Device.Outdir + "\\SavedImages"))
        Directory.CreateDirectory(App.Device.Outdir + "\\SavedImages");
      AnalysisPlot.ExportToImage(App.Device.Outdir + @"\SavedImages\" + date + ".png", options);
      printAnalysis.Visibility = Visibility.Hidden;
    }

    public void ShowSmallXYPlot()
    {
      HeatMap.Visible = true;
      LargeHeatMap.Visible = false;
      XYPlot.Width = 660;
      XYPlot.Height = 412;
      WldMap.MarkerSize = 5;
      XYPlot.Margin = new Thickness(80, 444, 0, 0);
      printXY.Margin = new Thickness(175, 470, 0, 0);
    }

    public void ShowLargeXYPlot()
    {
      HeatMap.Visible = false;
      LargeHeatMap.Visible = true;
      XYPlot.Width = 1140;
      XYPlot.Height = 856;
      WldMap.MarkerSize = 7;
      XYPlot.Margin = new Thickness(80, 0, 0, 0);
      printXY.Margin = new Thickness(280, 26, 0, 0);
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

    private void Plot3D_MouseEnter(object sender, MouseEventArgs e)
    {
      printAnalysis.Visibility = Visibility.Visible;
    }

    private void Plot3D_MouseLeave(object sender, MouseEventArgs e)
    {
      printAnalysis.Visibility = Visibility.Hidden;
    }
  }
}