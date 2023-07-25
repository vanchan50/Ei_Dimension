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

namespace Ei_Dimension.Views;

/// <summary>
/// Interaction logic for NormalizationView.xaml
/// </summary>
public partial class NormalizationView : UserControl
{
  public static NormalizationView Instance { get; private set; }
  public NormalizationView()
  {
    InitializeComponent();
    Instance = this;
#if DEBUG
    Console.Error.WriteLine("# NormalizationView Loaded");
#endif
  }
}