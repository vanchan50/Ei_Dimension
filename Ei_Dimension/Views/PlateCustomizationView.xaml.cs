using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
