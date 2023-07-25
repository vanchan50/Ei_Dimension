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
/// Interaction logic for ValidationView.xaml
/// </summary>
public partial class VerificationView : UserControl
{
  public static VerificationView Instance { get; private set; }
  public VerificationView()
  {
    InitializeComponent();
    Instance = this;
#if DEBUG
    Console.Error.WriteLine("#15 VerificationView Loaded");
#endif
  }
}