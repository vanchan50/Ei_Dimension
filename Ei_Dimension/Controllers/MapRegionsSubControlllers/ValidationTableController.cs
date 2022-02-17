using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ei_Dimension.Models;

namespace Ei_Dimension.Controllers
{
  public partial class MapRegionsController
  {
    private class ValidationTableController
    {
      private MapRegionsController _parent;
      private StackPanel _validationNum;
      private StackPanel _validationReporterBorder;

      public ValidationTableController(MapRegionsController parent, StackPanel Validat_Num, StackPanel Validat_Reporter)
      {
        _parent = parent;
        _validationNum = Validat_Num;
        _validationReporterBorder = Validat_Reporter;
      }

      public void AddValidationRegion(int regionNum)
      {
        var index = GetMapRegionIndex(regionNum);
        if (!ActiveVerificationRegionNums.Contains(regionNum))
        {
          ActiveVerificationRegionNums.Add(regionNum);
          ShiftValidationTextBox(index - 1, true);
        }
        else
        {
          ShiftValidationTextBox(index - 1, false);
          ActiveVerificationRegionNums.Remove(regionNum);
        }
        App.UnfocusUIElement();
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

        foreach (UIElement UIEl in _validationReporterBorder.Children)
        {
          BindingOperations.ClearAllBindings(UIEl);
          ((TextBox)UIEl).GotFocus -= ValidationReporterTbGotFocus;
          ((TextBox)UIEl).TextChanged -= ValidationReporterTextChanged;
        }
        _validationReporterBorder.Children.Clear();
        _lastValidationReporterBox = null;
      }

      public void AddRegionsTextBox(string propertyPath)
      {
        var tb = new TextBox();
        tb.Style = _regionsStyle;
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

      public void AddReporterTextBox(string propertyPath)
      {
        var tb = new TextBox();
        tb.Style = _regionsNamesStyle;
        if (_lastValidationReporterBox != null)
          _lastValidationReporterBox.Margin = _NameBoxAlignment;
        _lastValidationReporterBox = tb;
        tb.Margin = _lastNameBoxAlignment;
        tb.Name = $"__{_tbCounter}";
        tb.Visibility = Visibility.Hidden;
        SetupBinding(tb, _parent, propertyPath, BindingMode.TwoWay);
        tb.GotFocus += ValidationReporterTbGotFocus;
        tb.TextChanged += ValidationReporterTextChanged;
        _validationReporterBorder.Children.Add(tb);
      }

      private void ValidationRegionsTbGotFocus(object sender, RoutedEventArgs e)
      {
        if (!((TextBox) e.Source).IsKeyboardFocusWithin)
          return;
        var regionNumber = int.Parse(((TextBox)e.Source).Text);
        int Index = int.Parse(((TextBox)e.Source).Name.Trim('_'));
        AddValidationRegion(regionNumber);
      }

      private void ValidationReporterTbGotFocus(object sender, RoutedEventArgs e)
      {
        if (!((TextBox) e.Source).IsKeyboardFocusWithin)
          return;
        var tb = (TextBox)sender;
        var property = typeof(MapRegionData).GetProperty(nameof(MapRegionData.TargetReporterValue));
        SetUserInputTextBox(property, tb);
      }
        
      private static void ValidationReporterTextChanged(object sender, TextChangedEventArgs e)
      {
        //Allows SanityCheck when input is from keyboard
        UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
      }

      private void ShiftValidationTextBox(int index, bool right)
      {
        var tb = (TextBox)_validationNum.Children[index];
        var shift = tb.Margin;
        shift.Left = right ? 140 : 10;
        tb.Margin = shift;

        var ReporterTb = (TextBox)_validationReporterBorder.Children[index];
        ReporterTb.Visibility = right ? Visibility.Visible : Visibility.Hidden;
      }
    }
  }
}
