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

namespace Ei_Dimension.Views;

/// <summary>
/// Interaction logic for MaintenanceView.xaml
/// </summary>
public partial class MaintenanceView : UserControl
{
  public static MaintenanceView Instance { get; private set; }
  public MaintenanceView()
  {
    InitializeComponent();
    Instance = this;
#if DEBUG
    Console.Error.WriteLine("#7 MaintenanceView Loaded");
#endif
  }
}