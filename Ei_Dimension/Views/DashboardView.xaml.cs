using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ei_Dimension.Views
{
  /// <summary>
  /// Interaction logic for DashboardView.xaml
  /// </summary>
  public partial class DashboardView : UserControl
  {
    private static uint _tbCounter = 0;
    private static uint _nameTbCounter = 0;
    private static Thickness _regionsTbAlignment = new Thickness(10, 10, 0, 0);
    private static Thickness _lastRegionsTbAlignment = new Thickness(10, 10, 0, 10);
    private static Thickness _NameBoxAlignment = new Thickness(0, 10, 0, 0);
    private static Thickness _lastNameBoxAlignment = new Thickness(0, 10, 0, 10);
    private static TextBox _lastRegionsBox;
    private static TextBox _lastRegionsNameBox;
    public static DashboardView Instance;

    public DashboardView()
    {
      InitializeComponent();
      Instance = this;
    }

    public void AddTextboxes(string BindingProperty1, string BindingProperty2)
    {
      AddRegionsTextBox(BindingProperty1);
      AddRegionsNamesTextBox(BindingProperty2);
    }

    public void ClearTextBoxes()
    {
      foreach (UIElement UIEl in RegionsBorder.Children)
      {
        BindingOperations.ClearAllBindings(UIEl);
        ((TextBox)UIEl).GotFocus -= RegionsTbGotFocus;
      }
      RegionsBorder.Children.Clear();
      _tbCounter = 0;
      _lastRegionsBox = null;
      foreach (UIElement UIEl in RegionsNamesBorder.Children)
      {
        BindingOperations.ClearAllBindings(UIEl);
        ((TextBox)UIEl).GotFocus -= RegionsNamesTbGotFocus;
      }
      RegionsNamesBorder.Children.Clear();
      _nameTbCounter = 0;
      _lastRegionsNameBox = null;
    }

    public void ShiftTextBox(bool right)
    {
      var tb = (TextBox)RegionsBorder.Children[(int)ViewModels.DashboardViewModel.Instance.SelectedRegionTextboxName];
      var shift = tb.Margin;
      shift.Left = right ? 140 : 10;
      tb.Margin = shift;

      var NameTb = (TextBox)RegionsNamesBorder.Children[(int)ViewModels.DashboardViewModel.Instance.SelectedRegionTextboxName];
      NameTb.Visibility = right ? Visibility.Visible : Visibility.Hidden;
    }

    private void AddRegionsTextBox(string propertyPath)
    {
      var tb = new TextBox();
      tb.Style = (Style)App.Current.Resources.MergedDictionaries[6]["RegionFieldStyle"];
      if (_lastRegionsBox != null)
        _lastRegionsBox.Margin = _regionsTbAlignment;
      _lastRegionsBox = tb;
      tb.Margin = _lastRegionsTbAlignment;
      tb.Name = $"_{_tbCounter++}";
      tb.IsReadOnly = true;
      Binding bind = new Binding();
      bind.Source = ViewModels.DashboardViewModel.Instance;
      bind.Path = new PropertyPath(propertyPath);
      bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
      tb.GotFocus += RegionsTbGotFocus;
      RegionsBorder.Children.Add(tb);
    }

    private void AddRegionsNamesTextBox(string propertyPath)
    {
      var tb = new TextBox();
      tb.Style = (Style)App.Current.Resources.MergedDictionaries[5]["InputFieldStyle"];
      if (_lastRegionsNameBox != null)
        _lastRegionsNameBox.Margin = _NameBoxAlignment;
      _lastRegionsNameBox = tb;
      tb.Margin = _lastNameBoxAlignment;
      tb.Name = $"__{_nameTbCounter++}";
      tb.Visibility = Visibility.Hidden;
      Binding bind = new Binding();
      bind.Source = ViewModels.DashboardViewModel.Instance;
      bind.Path = new PropertyPath(propertyPath);
      bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
      tb.GotFocus += RegionsNamesTbGotFocus;
      RegionsNamesBorder.Children.Add(tb);
    }

    private void RegionsTbGotFocus(object sender, RoutedEventArgs e)
    {
      ViewModels.DashboardViewModel.Instance.SelectedRegionTextboxName = int.Parse(((TextBox)e.Source).Name.Trim('_'));
      ViewModels.DashboardViewModel.Instance.SelectedRegionCache = uint.Parse(((TextBox)e.Source).Text);
    }

    private void RegionsNamesTbGotFocus(object sender, RoutedEventArgs e)
    {
      var DashVM = ViewModels.DashboardViewModel.Instance;
      App.SelectedTextBox = (DashVM.GetType()
        .GetProperty(nameof(DashVM.RegionsNamesList)),
        DashVM, int.Parse(((TextBox)e.Source).Name.Trim('_')));
    }
  }
}
