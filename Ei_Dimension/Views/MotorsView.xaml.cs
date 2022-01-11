using System;
using System.Windows.Controls;

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
