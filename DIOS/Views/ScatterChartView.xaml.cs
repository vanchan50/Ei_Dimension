using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ei_Dimension.Views;

/// <summary>
/// Interaction logic for ScatterChartView.xaml
/// </summary>
public partial class ScatterChartView : UserControl
{
  public static ScatterChartView Instance;
  public ScatterChartView()
  {
    InitializeComponent();
    Instance = this;
  }

  public void Print(int resoultionDpi = 800)
  {
    App.Export(ScatterPlot, resoultionDpi);
    printSC.Visibility = Visibility.Hidden;
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