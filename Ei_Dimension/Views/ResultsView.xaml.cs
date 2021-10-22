using System;
using System.Collections.Generic;
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
      //var t = ChC.Diagram.Series[6];
      //var Point = new DevExpress.Xpf.Charts.SeriesPoint(100, 100);
      //Point.Brush = Brushes.Green;
      //t.Points.Add(Point);
      //var Point2 = new DevExpress.Xpf.Charts.SeriesPoint(1000, 1000);
      //Point2.Brush = Brushes.Red;
      //t.Points.Add(Point2);
    }
    public void AddXYPoint(double x, double y, SolidColorBrush brush)
    {
      var Point = new DevExpress.Xpf.Charts.SeriesPoint(x, y);
      Point.Brush = brush;
      ChC.Diagram.Series[1].Points.Add(Point);
    }
    public void ChangePointColor(int index, SolidColorBrush brush)
    {
      ChC.Diagram.Series[1].Points[index].Brush = brush;
    }
    public void ClearPoints()
    {
      ChC.Diagram.Series[1].Points.Clear();
    }
  }
}
