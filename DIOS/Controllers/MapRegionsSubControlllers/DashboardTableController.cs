using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ei_Dimension.Controllers
{
  public partial class MapRegionsController
  {
    private class DashboardTableController
    {
      private StackPanel _dashboardNum;
      private StackPanel _dashboardName;
      private static readonly Dictionary<string, TextBox> _textBoxNameEntityDictionary = new Dictionary<string, TextBox>();

      public DashboardTableController(StackPanel Db_Num, StackPanel Db_Name)
      {
        _dashboardNum = Db_Num;
        _dashboardName = Db_Name;
      }

      public void Clear()
      {
        foreach (UIElement UIEl in _dashboardNum.Children)
        {
          BindingOperations.ClearAllBindings(UIEl);
        }
        _dashboardNum.Children.Clear();
        foreach (UIElement UIEl in _dashboardName.Children)
        {
          BindingOperations.ClearAllBindings(UIEl);
          ((TextBox)UIEl).GotFocus -= RegionsNamesTbGotFocus;
        }
        _dashboardName.Children.Clear();
        _textBoxNameEntityDictionary.Clear();
      }

      public void AddDbNumBox(string name, BindingBase bind)
      {
        var tb = new TextBox
        {
          VerticalAlignment = VerticalAlignment.Top,
          Style = _regionsNamesStyle,
          IsReadOnly = true,
          Margin = _regionsTbAlignment,
          Name = name
        };
        BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
        _dashboardNum.Children.Add(tb);
        _textBoxNameEntityDictionary.Add(tb.Name, tb);
      }

      public void AddDbNameBox(string name, BindingBase bind)
      {
        var tb = new TextBox
        {
          VerticalAlignment = VerticalAlignment.Top,
          Style = _regionsNamesStyle,
          Margin = _regionsTbAlignment,
          Name = name
        };
        tb.GotFocus += RegionsNamesTbGotFocus;
        BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
        _dashboardName.Children.Add(tb);
        _textBoxNameEntityDictionary.Add(tb.Name, tb);
      }

      public void RemoveDbNumBox(string name)
      {
        var tb = GetTBByName(name);
        BindingOperations.ClearAllBindings(tb);
        _dashboardNum.Children.Remove(tb);
        _textBoxNameEntityDictionary.Remove(tb.Name);
      }

      public void RemoveDbNameBox(string name)
      {
        var tb = GetTBByName(name);
        BindingOperations.ClearAllBindings(tb);
        _dashboardName.Children.Remove(tb);
        _textBoxNameEntityDictionary.Remove(tb.Name);
      }
    
      private static TextBox GetTBByName(string name)
      {
        if (_textBoxNameEntityDictionary.TryGetValue(name, out var tb))
            return tb;
        return null;
      }
    }
  }
}
