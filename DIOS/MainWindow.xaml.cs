using DevExpress.Xpf.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Ei_Dimension;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : ThemedWindow
{
  public static MainWindow Instance { get; private set; }
  public MainWindow()
  {
    InitializeComponent();
    Instance = this;
#if DEBUG
    Binding bind = new Binding();
    bind.Source = ViewModels.MainViewModel.Instance;
    bind.Path = new PropertyPath($"{nameof(ViewModels.MainViewModel.Instance.TotalBeadsInFirmware)}[0]");
    bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
    _ = BindingOperations.SetBinding(EventCounterTooltip, TextBlock.TextProperty, bind);

    //TextBlock BoardV = new TextBlock();
    //BoardV.HorizontalAlignment = HorizontalAlignment.Left;
    //BoardV.VerticalAlignment = VerticalAlignment.Top;
    //BoardV.Margin = new Thickness(10, 5, 0, 0);
    //BoardV.Text = "TEST BoardVersion";
    //BoardV.FontSize = 20;
    //BoardV.Foreground = Brushes.Black;
    //BoardV.Opacity = 0;
    //BoardV.MouseDown += BoardV_MouseDown;
    //var t = wndw.LogicalChildren;
    //t.MoveNext();
    //((Grid)t.Current).Children.Add(BoardV);
#else
      EventCounterParent.IsHitTestVisible = false;
#endif
  }
}