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
  /// Interaction logic for MotorsView.xaml
  /// </summary>
  public partial class MotorsView : UserControl
  {
    public static MotorsView Instance { get; private set; }
    public MotorsView()
    {
      InitializeComponent();
      Instance = this;
#if DEBUG
      Console.Error.WriteLine("#17 MotorsView Loaded");
#endif
    }
  }
}
