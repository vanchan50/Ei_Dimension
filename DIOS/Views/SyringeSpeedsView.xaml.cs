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
  /// Interaction logic for SyringeSpeedsView.xaml
  /// </summary>
  public partial class SyringeSpeedsView : UserControl
  {
    public static SyringeSpeedsView Instance { get; private set; }
    public SyringeSpeedsView()
    {
      InitializeComponent();
      Instance = this;
#if DEBUG
      Console.Error.WriteLine("#21 SyringeSpeedsView Loaded");
#endif
      StartupFinalizer.Run();
    }
  }
}
