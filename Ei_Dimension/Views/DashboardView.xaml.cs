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
    private uint _tbCounter = 0;
    private Thickness _alignmentLeft = new Thickness(8, 8, 0, 0);
    private Thickness _alignmentRight = new Thickness(128, 8, 8, 0);
    public static DashboardView Instance;
    public DashboardView()
    {
      InitializeComponent();
      Instance = this;
    }

    public void AddTextBox(string propertyPath)
    {
      var tb = new TextBox();
      tb.Style = (Style)App.Current.Resources.MergedDictionaries[6]["RegionFieldStyle"];
      tb.Margin = _alignmentLeft;
      tb.Name = $"_{_tbCounter++}";
      tb.IsReadOnly = true;
      Binding bind = new Binding();
      bind.Source = ViewModels.DashboardViewModel.Instance;
      bind.Path = new PropertyPath(propertyPath);
      bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
      tb.TextChanged += Tb_TextChanged;
      tb.GotFocus += Tb_GotFocus;
      RegionsBorder.Children.Add(tb);
    }

    private void Tb_GotFocus(object sender, RoutedEventArgs e)
    {
      ViewModels.DashboardViewModel.Instance.SelectedRegionTextboxName = int.Parse(((TextBox)e.Source).Name.Trim('_'));
      ViewModels.DashboardViewModel.Instance.SelectedRegionCache = uint.Parse(((TextBox)e.Source).Text);
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
        ((TextBox)UIEl).GotFocus -= Tb_GotFocus;
      }
      RegionsBorder.Children.Clear();
      _tbCounter = 0;
    }

    public void ShiftTextBox(bool right)
    {
      var tb = (TextBox)RegionsBorder.Children[(int)ViewModels.DashboardViewModel.Instance.SelectedRegionTextboxName];
      tb.Margin = right ? _alignmentRight : _alignmentLeft;
    }
  }
}
