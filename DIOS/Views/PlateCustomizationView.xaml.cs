using System.Windows.Controls;

namespace Ei_Dimension.Views
{
  /// <summary>
  /// Interaction logic for PlateCustomizationView.xaml
  /// </summary>
  public partial class PlateCustomizationView : UserControl
  {
    public static PlateCustomizationView Instance { get; private set; }
    public PlateCustomizationView()
    {
      InitializeComponent();
      Instance = this;
    }
  }
}
