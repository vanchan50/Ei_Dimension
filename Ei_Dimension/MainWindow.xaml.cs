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
      if (Program.SpecializedVer == CompanyID.China)
      {
        CompanyLogo.Source = new BitmapImage(new Uri(@"/Ei_Dimension;component/Icons/Emission_LogoCh.png", UriKind.Relative));
        CompanyLogo.Margin = new Thickness(1730, 1030, 5, 0);
        InstrumentLogo.Source = new BitmapImage(new Uri(@"/Ei_Dimension;component/Icons/dimension flow analyzer logoCh.png", UriKind.Relative));
      }
#if DEBUG
      Binding bind = new Binding();
      bind.Source = ViewModels.MainViewModel.Instance;
      bind.Path = new PropertyPath("TotalBeadsInFirmware[0]");
      bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      _ = BindingOperations.SetBinding(EventCounter, TextBlock.TextProperty, bind);
#else
      EventCounterParent.IsHitTestVisible = false;
#endif
    }
  }
}
