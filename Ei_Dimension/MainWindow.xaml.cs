using DevExpress.Xpf.Core;
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
using DIOS.Core;

namespace Ei_Dimension
{
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
      bind.Path = new PropertyPath("TotalBeadsInFirmware[0]");
      bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      _ = BindingOperations.SetBinding(EventCounterTooltip, TextBlock.TextProperty, bind);

      TextBlock BoardV = new TextBlock();
      BoardV.HorizontalAlignment = HorizontalAlignment.Left;
      BoardV.VerticalAlignment = VerticalAlignment.Top;
      BoardV.Margin = new Thickness(10, 5, 0, 0);
      BoardV.Text = "TEST BoardVersion";
      BoardV.FontSize = 20;
      BoardV.Foreground = Brushes.Black;
      BoardV.Opacity = 0;
      BoardV.MouseDown += BoardV_MouseDown;
      BoardV.IsMouseDirectlyOverChanged += BoardVOnIsMouseDirectlyOverChanged;
      var t = wndw.LogicalChildren;
      t.MoveNext();
      ((Grid)t.Current).Children.Add(BoardV);
#else
      EventCounterParent.IsHitTestVisible = false;
#endif
    }
    
#if DEBUG
    private void BoardVOnIsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      ((TextBlock)sender).Opacity = ((TextBlock)sender).Opacity == 0 ? 1 : 0;
    }

    private void BoardV_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (Views.ChannelOffsetView.Instance.AvgBgSP.Width == 0)
      {
        ViewModels.ChannelOffsetViewModel.Instance.OldBoardOffsetsVisible = Visibility.Hidden;
        Views.ChannelOffsetView.Instance.SlidersSP.Visibility = Visibility.Visible;
        //Views.ChannelOffsetView.Instance.BaselineSP.Width = 180;
        Views.ChannelOffsetView.Instance.AvgBgSP.Width = 180;
        ((TextBlock) sender).Text = "TEST BoardVersion = 3";
        App.Device.DEBUGSetBoardVersion(3);
      }
      else
      {
        ViewModels.ChannelOffsetViewModel.Instance.OldBoardOffsetsVisible = Visibility.Visible;
        Views.ChannelOffsetView.Instance.SlidersSP.Visibility = Visibility.Hidden;
        //Views.ChannelOffsetView.Instance.BaselineSP.Width = 0;
        Views.ChannelOffsetView.Instance.AvgBgSP.Width = 0;
        ((TextBlock) sender).Text = "TEST BoardVersion = 1";
        App.Device.DEBUGSetBoardVersion(1);
      }
    }
#endif
  }
}
