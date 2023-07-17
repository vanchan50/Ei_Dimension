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
  /// Interaction logic for AlignmentView.xaml
  /// </summary>
  public partial class AlignmentView : UserControl
  {
    public static AlignmentView Instance { get; private set; }
    public AlignmentView()
    {
      InitializeComponent();
      Instance = this;
#if DEBUG
      System.Console.Error.WriteLine("#19 AlignmentView Loaded");
#endif
    }
  }
}
