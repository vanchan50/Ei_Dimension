using System;
using System.Collections.Generic;
using System.Linq;
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
  /// Interaction logic for SelRegionsView.xaml
  /// </summary>
  public partial class SelRegionsView : UserControl
  {
    public static SelRegionsView Instance { get; private set; }
    public SelRegionsView()
    {
      InitializeComponent();
      Instance = this;
#if DEBUG
      Console.Error.WriteLine("#12 SelRegionsView Loaded");
#endif
    }
  }
}
