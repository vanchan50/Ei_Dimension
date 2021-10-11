using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ei_Dimension.Views
{
  /// <summary>
  /// Interaction logic for DashboardView.xaml
  /// </summary>
  public partial class DashboardView : UserControl
  {
    public static DashboardView Instance;

    public DashboardView()
    {
      InitializeComponent();
      Instance = this;
    }
  }
}