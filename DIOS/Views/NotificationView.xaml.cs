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
  /// Interaction logic for NotificationView.xaml
  /// </summary>
  public partial class NotificationView : UserControl
  {
    public static NotificationView Instance { get; private set; }
    public NotificationView()
    {
      InitializeComponent();
      Instance = this;
#if DEBUG
      Console.Error.WriteLine("#4 NotificationView Loaded");
#endif
    }
  }
}
