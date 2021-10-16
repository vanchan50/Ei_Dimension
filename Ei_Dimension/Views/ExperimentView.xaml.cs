using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
  /// Interaction logic for ExperimentView.xaml
  /// </summary>
  public partial class ExperimentView : UserControl
  {
    public ExperimentView()
    {
      InitializeComponent();
    }

    private void DbTube_Click(object sender, RoutedEventArgs e)
    {
      DbButton.IsChecked = true;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      DbButton.IsChecked = true;
    }
  }
}
