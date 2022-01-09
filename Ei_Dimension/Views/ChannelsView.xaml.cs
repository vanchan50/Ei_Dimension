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
  /// Interaction logic for ChannelsView.xaml
  /// </summary>
  public partial class ChannelsView : UserControl
  {
    public static ChannelsView Instance { get; private set; }
    public ChannelsView()
    {
      InitializeComponent();
      Instance = this;
#if DEBUG
      System.Console.Error.WriteLine("#16 ChannelsView Loaded");
#endif
    }
  }
}
