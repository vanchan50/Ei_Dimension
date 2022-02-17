using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ei_Dimension.Controllers
{
  public partial class MapRegionsController
  {
    private class ResultsTableController
    {
      //pointers to storages of mean and count
      public ObservableCollection<string> DisplayedActiveRegionsCount { get; private set; }
      public ObservableCollection<string> DisplayedActiveRegionsMean { get; private set; }
      
      public bool NullTextboxActive;
      private MapRegionsController _parent;
      private ListBox _table;
      private static readonly Dictionary<string, StackPanel> _stackPanelNameEntityDictionary = new Dictionary<string, StackPanel>();

      public ResultsTableController(MapRegionsController parent, ListBox resultsTable)
      {
        _parent = parent;
        _table = resultsTable;
      }

      public void DisplayCurrentActiveRegionsBeadStats(bool current)
      {
        if (current)
        {
          DisplayedActiveRegionsCount = _parent.CurrentActiveRegionsCount;
          DisplayedActiveRegionsMean = _parent.CurrentActiveRegionsMean;
          return;
        }
        DisplayedActiveRegionsCount = _parent.BackingActiveRegionsCount;
        DisplayedActiveRegionsMean = _parent.BackingActiveRegionsMean;
      }
      
      public void AddRegion(string name, BindingBase bind)
      {
        StackPanel sp = new StackPanel
        {
          Name = name,
          VerticalAlignment = VerticalAlignment.Top,
          HorizontalAlignment = HorizontalAlignment.Left,
          Orientation = Orientation.Horizontal,
          Width = 430,
          Height = 53,
          Margin = new Thickness(0, 0, 0, 0)
        };

        int index = int.Parse(name.Trim('_'));
        sp.Children.Add(MakeNameTextBox(bind));
        sp.Children.Add(MakeCounterTextBox(index));
        sp.Children.Add(MakeMeanTextBox(index));

        _table.Items.Add(sp);
        _stackPanelNameEntityDictionary.Add(sp.Name, sp);
      }

      public void RemoveRegion(string name)
      {
        var sp = GetSPByName(name);
        if (sp == null)
        {
          Console.Error.WriteLine("Tried to remove inexistent region from table");
          return;
        }
        for (var i = sp.Children.Count - 1; i > -1; i--)
        {
          BindingOperations.ClearAllBindings(sp.Children[i]);
          sp.Children.RemoveAt(i);
        }
        _table.Items.Remove(sp);
        _stackPanelNameEntityDictionary.Remove(sp.Name);
      }

      public void Clear()
      {
        for (var i = _table.Items.Count - 1; i > -1; i--)
        {
          var sp = (StackPanel)_table.Items[i];
          for (var j = sp.Children.Count - 1; j > -1; j--)
          {
            BindingOperations.ClearAllBindings(sp.Children[j]);
            sp.Children.RemoveAt(j);
          }
          _table.Items.RemoveAt(i);
        }
        _stackPanelNameEntityDictionary.Clear();
      }

      public void ShowNullTb()
      {
        if (!NullTextboxActive)
        {
          Binding bind = new Binding
          {
            Source = _parent,
            Path = new PropertyPath("RegionsList[0].Name[0]"),
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
          };
          AddRegion("__0", bind);
          NullTextboxActive = true;
        }
      }

      public void RemoveNullTb()
      {
        if (NullTextboxActive)
        {
          RemoveRegion("__0");
          NullTextboxActive = false;
        }
      }

      private static StackPanel GetSPByName(string name)
      {
        if (_stackPanelNameEntityDictionary.TryGetValue(name, out var sp))
          return sp;
        return null;
      }

      private TextBox MakeNameTextBox(BindingBase bind)
      {
        var tb = new TextBox
        {
          VerticalAlignment = VerticalAlignment.Top,
          HorizontalAlignment = HorizontalAlignment.Left,
          Width = 160,
          Style = _regionsNamesStyle,
          IsReadOnly = true,
          Margin = _TbAlignment
        };
        BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
        return tb;
      }

      private TextBox MakeCounterTextBox(int index)
      {
        var tb = new TextBox
        {
          VerticalAlignment = VerticalAlignment.Top,
          HorizontalAlignment = HorizontalAlignment.Left,
          Style = _regionsNamesStyle,
          Width = 100,
          IsReadOnly = true,
          Margin = _TbAlignment
        };
        SetupBinding(tb, this, $"DisplayedActiveRegionsCount[{index}]", BindingMode.OneWay);
        return tb;
      }

      private TextBox MakeMeanTextBox(int index)
      {
        var tb = new TextBox
        {
          VerticalAlignment = VerticalAlignment.Top,
          HorizontalAlignment = HorizontalAlignment.Left,
          Style = _regionsNamesStyle,
          Width = 140,
          IsReadOnly = true,
          Margin = _TbAlignment
        };
        SetupBinding(tb, this, $"DisplayedActiveRegionsMean[{index}]", BindingMode.OneWay);
        return tb;
      }
    }
  }
}
