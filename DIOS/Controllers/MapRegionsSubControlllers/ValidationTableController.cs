using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ei_Dimension.ViewModels;

namespace Ei_Dimension.Controllers;

public partial class MapRegionsController
{
  private class ValidationTableController
  {
    private MapRegionsController _parent;
    private StackPanel _validationNum;

    public ValidationTableController(MapRegionsController parent, StackPanel Validat_Num)
    {
      _parent = parent;
      _validationNum = Validat_Num;
    }

    public void Clear()
    {
      foreach (UIElement UIEl in _validationNum.Children)
      {
        BindingOperations.ClearAllBindings(UIEl);
        ((TextBox)UIEl).GotFocus -= ValidationRegionsTbGotFocus;
      }
      _validationNum.Children.Clear();
      _lastValidationRegionsBox = null;

      _lastValidationReporterBox = null;
    }

    public void AddRegionsTextBox(string propertyPath)
    {
      var tb = new TextBox();
      tb.Style = _regionsActiveStyle;
      if (_lastValidationRegionsBox != null)
        _lastValidationRegionsBox.Margin = _regionsTbAlignment;
      _lastValidationRegionsBox = tb;
      tb.Margin = _lastRegionsTbAlignment;
      tb.Name = $"_{_tbCounter}";
      tb.IsReadOnly = true;
      SetupBinding(tb, _parent, propertyPath, BindingMode.OneTime);
      tb.GotFocus += ValidationRegionsTbGotFocus;
      _validationNum.Children.Add(tb);
    }

    public void ChangeTbColor(int index, bool active)
    {
      var tb = _validationNum.Children[index] as TextBox;
      tb.Tag = active ? "Activated" : "Deactivated";
    }

    private void ValidationRegionsTbGotFocus(object sender, RoutedEventArgs e)
    {
      if (!((TextBox)e.Source).IsKeyboardFocusWithin)
        return;
      var regionNumber = int.Parse(((TextBox)e.Source).Text);
      //int Index = int.Parse(((TextBox)e.Source).Name.Trim('_'));

      var region = App.DiosApp.MapController.ActiveMap.Regions[regionNumber];
      //
      VerificationParametersViewModel.Instance.InstallRegionVerificationData(region);
      //
    }
  }
}