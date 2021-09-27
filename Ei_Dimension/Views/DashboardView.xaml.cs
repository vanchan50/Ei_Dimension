using System;
using System.Collections.Generic;
using System.Reflection;
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
using DevExpress.Mvvm.UI;

namespace Ei_Dimension.Views
{
  /// <summary>
  /// Interaction logic for DashboardView.xaml
  /// </summary>
  public partial class DashboardView : UserControl
  {
    public static DashboardView Instance;
    public DashboardView()
    {
      InitializeComponent();
      Instance = this;
    }

    public void AddTextBox(string propertyPath)
    {
      var tb = new TextBox();
      tb.Style = (Style)App.Current.Resources.MergedDictionaries[5]["InputFieldStyle"];
      tb.Margin = new Thickness(8, 8, 8, 0);

      Binding bind = new Binding();
      bind.Source = ViewModels.DashboardViewModel.Instance;
      bind.Path = new PropertyPath(propertyPath);
      bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
      RegionsBorder.Children.Add(tb);
      tb.TextChanged += Tb_TextChanged;
    }

    private void Tb_TextChanged(object sender, TextChangedEventArgs e)
    {
      ViewModels.DashboardViewModel.Instance.RegionsRenamed = true;
    }

    public void ClearTextBoxes()
    {
      foreach(UIElement UIEl in RegionsBorder.Children)
      {
        BindingOperations.ClearAllBindings(UIEl);
        ((TextBox)UIEl).TextChanged -= Tb_TextChanged;
      }
      RegionsBorder.Children.Clear();
    }
  }
}
