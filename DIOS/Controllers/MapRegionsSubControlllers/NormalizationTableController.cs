using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ei_Dimension.Models;

namespace Ei_Dimension.Controllers;

public partial class MapRegionsController
{
  private class NormalizationTableController
  {
    private MapRegionsController _parent;
    private StackPanel _normalizationNum;
    private StackPanel _normalizationMFIBorder;

    public NormalizationTableController(MapRegionsController parent, StackPanel Normaliz_Num, StackPanel Normaliz_MFI)
    {
      _parent = parent;
      _normalizationNum = Normaliz_Num;
      _normalizationMFIBorder = Normaliz_MFI;
    }

    public void Clear()
    {
      foreach (UIElement UIEl in _normalizationNum.Children)
      {
        BindingOperations.ClearAllBindings(UIEl);
      }
      _normalizationNum.Children.Clear();
      _lastValidationRegionsBox = null;

      foreach (UIElement UIEl in _normalizationMFIBorder.Children)
      {
        BindingOperations.ClearAllBindings(UIEl);
        ((TextBox)UIEl).GotFocus -= NormalizationMFITbGotFocus;
        ((TextBox)UIEl).TextChanged -= NormalizationMFITextChanged;
      }
      _normalizationMFIBorder.Children.Clear();
      _lastValidationReporterBox = null;
    }

    public void AddRegionsTextBox(string propertyPath)
    {
      var tb = new TextBox();
      tb.Style = _regionsNamesStyle;
      if (_lastValidationRegionsBox != null)
        _lastValidationRegionsBox.Margin = _regionsTbAlignment;
      _lastValidationRegionsBox = tb;
      tb.Margin = _lastRegionsTbAlignment;
      tb.Name = $"_{_tbCounter}";
      tb.IsReadOnly = true;
      SetupBinding(tb, _parent, propertyPath, BindingMode.OneTime);
      _normalizationNum.Children.Add(tb);
    }

    public void AddMFITextBox(string propertyPath)
    {
      var tb = new TextBox();
      tb.Style = _regionsNamesStyle;
      if (_lastValidationReporterBox != null)
        _lastValidationReporterBox.Margin = _NameBoxAlignment;
      _lastValidationReporterBox = tb;
      tb.Margin = _lastNameBoxAlignment;
      tb.Name = $"__{_tbCounter}";
      SetupBinding(tb, _parent, propertyPath, BindingMode.TwoWay);
      tb.GotFocus += NormalizationMFITbGotFocus;
      tb.TextChanged += NormalizationMFITextChanged;
      _normalizationMFIBorder.Children.Add(tb);
    }

    private void NormalizationMFITbGotFocus(object sender, RoutedEventArgs e)
    {
      if (!((TextBox)e.Source).IsKeyboardFocusWithin)
        return;
      var tb = (TextBox)sender;
      var property = typeof(MapRegionData).GetProperty(nameof(MapRegionData.MFIValue));
      SetUserInputTextBox(property, tb);
    }

    private static void NormalizationMFITextChanged(object sender, TextChangedEventArgs e)
    {
      //Allows SanityCheck when input is from keyboard
      UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }
  }
}