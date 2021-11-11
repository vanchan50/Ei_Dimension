using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Ei_Dimension.ViewModels;

namespace Ei_Dimension.Models
{
  [POCOViewModel]
  public class MapRegions
  {
    public ObservableCollection<string> RegionsList { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> RegionsNamesList { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> CurrentActiveRegionsCount { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> CurrentActiveRegionsMean { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> BackingActiveRegionsCount { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> BackingActiveRegionsMean { get; } = new ObservableCollection<string>();
    public virtual ObservableCollection<string> DisplayedActiveRegionsCount { get; set; }
    public virtual ObservableCollection<string> DisplayedActiveRegionsMean { get; set; }
    public List<bool> ActiveRegions { get; } = new List<bool>();
    private StackPanel _regionsBorder;
    private StackPanel _regionsNamesBorder;
    public int? SelectedRegionTextboxIndex { get; set; }
    private bool _firstLoadflag;
    //Results
    private static uint _tbCounter = 0;
    private static uint _nameTbCounter = 0;
    private static Thickness _regionsTbAlignment = new Thickness(10, 10, 0, 0);
    private static Thickness _lastRegionsTbAlignment = new Thickness(10, 10, 0, 10);
    private static Thickness _NameBoxAlignment = new Thickness(0, 10, 0, 0);
    private static Thickness _lastNameBoxAlignment = new Thickness(0, 10, 0, 10);
    private static Style _regionsStyle = (Style)App.Current.Resources.MergedDictionaries[6]["RegionFieldStyle"];
    private static Style _regionsNamesStyle = (Style)App.Current.Resources.MergedDictionaries[5]["InputFieldStyle"];
    private static TextBox _lastRegionsBox;
    private static TextBox _lastRegionsNameBox;
    private ListBox _table;
    private static Thickness _TbAlignment = new Thickness(5, 5, 5, 0);
    //DB
    private StackPanel _dbNum;
    private StackPanel _dbName;
    protected MapRegions(StackPanel RegionsBorder, StackPanel RegionsNamesBorder, ListBox Table, StackPanel Db_Num, StackPanel Db_Name)
    {
      _regionsBorder = RegionsBorder;
      _regionsNamesBorder = RegionsNamesBorder;
      _table = Table;
      _dbNum = Db_Num;
      _dbName = Db_Name;
      DisplayedActiveRegionsCount = CurrentActiveRegionsCount;
      DisplayedActiveRegionsMean = CurrentActiveRegionsMean;
      FillRegions();
    }
    public static MapRegions Create(StackPanel RegionsBorder, StackPanel RegionsNamesBorder, ListBox Table, StackPanel Db_Num, StackPanel Db_Name)
    {
      return ViewModelSource.Create(() => new MapRegions(RegionsBorder, RegionsNamesBorder, Table, Db_Num, Db_Name));
    }
    public void FillRegions(bool loadByPage = false)
    {
      if (_firstLoadflag && loadByPage)
        return;
      _firstLoadflag = true;
      ClearTable();
      ClearTextBoxes();
      RegionsList.Clear();
      RegionsNamesList.Clear();
      ActiveRegions.Clear();
      CurrentActiveRegionsCount.Clear();
      CurrentActiveRegionsMean.Clear();
      BackingActiveRegionsCount.Clear();
      BackingActiveRegionsMean.Clear();
      var i = 0;
      var r = 0;
      // regions should have been added in ascending order
      foreach (var point in App.Device.ActiveMap.classificationMap)
      {
        if(point.r > r)
        {
          r = point.r;
          RegionsList.Add(point.r.ToString());
          RegionsNamesList.Add("");
          ActiveRegions.Add(false);
          CurrentActiveRegionsCount.Add("0");
          CurrentActiveRegionsMean.Add("0");
          BackingActiveRegionsCount.Add("0");
          BackingActiveRegionsMean.Add("0");
          AddTextboxes($"RegionsList[{i}]", $"RegionsNamesList[{i}]");
          i++;
        }
      }
    }
    public void AddActiveRegion(byte num)
    {
      if (SelectedRegionTextboxIndex != null)
      {
        if (num == 1 && !ActiveRegions[(int)SelectedRegionTextboxIndex])
        {
          ShiftTextBox(true);
          ActiveRegions[(int)SelectedRegionTextboxIndex] = true;
        }
        else if (num == 0 && ActiveRegions[(int)SelectedRegionTextboxIndex])
        {
          ActiveRegions[(int)SelectedRegionTextboxIndex] = false;
          ShiftTextBox(false);
        }
        SelectedRegionTextboxIndex = null;
      }
    }
    private void AddTextboxes(string BindingProperty1, string BindingProperty2)
    {
      AddRegionsTextBox(BindingProperty1);
      AddRegionsNamesTextBox(BindingProperty2);
    }

    private void ClearTextBoxes()
    {
      foreach (UIElement UIEl in _regionsBorder.Children)
      {
        BindingOperations.ClearAllBindings(UIEl);
        ((TextBox)UIEl).GotFocus -= RegionsTbGotFocus;
      }
      _regionsBorder.Children.Clear();
      _tbCounter = 0;
      _lastRegionsBox = null;
      foreach (UIElement UIEl in _regionsNamesBorder.Children)
      {
        BindingOperations.ClearAllBindings(UIEl);
        ((TextBox)UIEl).GotFocus -= RegionsNamesTbGotFocus;
      }
      _regionsNamesBorder.Children.Clear();
      _nameTbCounter = 0;
      _lastRegionsNameBox = null;

      foreach (UIElement UIEl in _dbNum.Children)
      {
        BindingOperations.ClearAllBindings(UIEl);
      }
      _dbNum.Children.Clear();
      foreach (UIElement UIEl in _dbName.Children)
      {
        BindingOperations.ClearAllBindings(UIEl);
        ((TextBox)UIEl).GotFocus -= RegionsNamesTbGotFocus;
      }
      _dbName.Children.Clear();
    }

    private void ShiftTextBox(bool right)
    {
      var tb = (TextBox)_regionsBorder.Children[(int)SelectedRegionTextboxIndex];
      var shift = tb.Margin;
      shift.Left = right ? 140 : 10;
      tb.Margin = shift;

      var NameTb = (TextBox)_regionsNamesBorder.Children[(int)SelectedRegionTextboxIndex];
      NameTb.Visibility = right ? Visibility.Visible : Visibility.Hidden;

      if (right)
      {
        var numBindToCopy = BindingOperations.GetBindingBase(tb, TextBox.TextProperty);
        var nameBindToCopy = BindingOperations.GetBindingBase(NameTb, TextBox.TextProperty);
        AddRegionToTable(NameTb.Name, nameBindToCopy);
        AddDbNumBox(tb.Name, numBindToCopy);
        AddDbNameBox(NameTb.Name, nameBindToCopy);
      }
      else
      {
        RemoveRegionFromTable(NameTb.Name);
        RemoveDbNumBox(tb.Name);
        RemoveDbNameBox(NameTb.Name);
      }
    }

    private void AddRegionsTextBox(string propertyPath)
    {
      var tb = new TextBox();
      tb.Style = _regionsStyle;
      if (_lastRegionsBox != null)
        _lastRegionsBox.Margin = _regionsTbAlignment;
      _lastRegionsBox = tb;
      tb.Margin = _lastRegionsTbAlignment;
      tb.Name = $"_{_tbCounter++}";
      tb.IsReadOnly = true;
      Binding bind = new Binding();
      bind.Source = this;
      bind.Path = new PropertyPath(propertyPath);
      bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
      tb.GotFocus += RegionsTbGotFocus;
      _regionsBorder.Children.Add(tb);
    }

    private void AddRegionsNamesTextBox(string propertyPath)
    {
      var tb = new TextBox();
      tb.Style = _regionsNamesStyle;
      if (_lastRegionsNameBox != null)
        _lastRegionsNameBox.Margin = _NameBoxAlignment;
      _lastRegionsNameBox = tb;
      tb.Margin = _lastNameBoxAlignment;
      tb.Name = $"__{_nameTbCounter++}";
      tb.Visibility = Visibility.Hidden;
      Binding bind = new Binding();
      bind.Source = this;
      bind.Path = new PropertyPath(propertyPath);
      bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
      tb.GotFocus += RegionsNamesTbGotFocus;
      _regionsNamesBorder.Children.Add(tb);
    }

    private void RegionsTbGotFocus(object sender, RoutedEventArgs e)
    {
      SelectedRegionTextboxIndex = int.Parse(((TextBox)e.Source).Name.Trim('_'));
    }

    private void RegionsNamesTbGotFocus(object sender, RoutedEventArgs e)
    {
      App.SelectedTextBox = (this.GetType()
        .GetProperty(nameof(RegionsNamesList)),
        this, int.Parse(((TextBox)e.Source).Name.Trim('_')));
      MainViewModel.Instance.KeyboardToggle((TextBox)e.Source);
    }
    //Resultsview functionality
    private void AddRegionToTable(string name, BindingBase bind)
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
    }

    private void RemoveRegionFromTable(string name)
    {
      var sp = (StackPanel)_table.Items[GetSPIndexByName(name, _table)];
      for (var i = sp.Children.Count - 1; i > -1; i--)
      {
        BindingOperations.ClearAllBindings(sp.Children[i]);
        sp.Children.RemoveAt(i);
      }
      _table.Items.Remove(sp);
    }

    private void ClearTable()
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
      Binding bind = new Binding();
      bind.Source = this;
      bind.Path = new PropertyPath($"DisplayedActiveRegionsCount[{index}]");
      bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
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
      Binding bind = new Binding();
      bind.Source = this;
      bind.Path = new PropertyPath($"DisplayedActiveRegionsMean[{index}]");
      bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
      return tb;
    }

    private int GetSPIndexByName(string name, ListBox UIEl)
    {
      for (var i = 0; i < UIEl.Items.Count; i++)
      {
        if (((StackPanel)UIEl.Items[i]).Name == name)
          return i;
      }
      return -1;
    }
    //Dashboard functionality
    private void AddDbNumBox(string name, BindingBase bind)
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
      _dbNum.Children.Add(tb);
    }

    private void AddDbNameBox(string name, BindingBase bind)
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
      _dbName.Children.Add(tb);
    }

    private void RemoveDbNumBox(string name)
    {
      var tb = (TextBox)_dbNum.Children[GetTBIndexByName(name, _dbNum)];
      BindingOperations.ClearAllBindings(tb);
      _dbNum.Children.Remove(tb);
    }
    private void RemoveDbNameBox(string name)
    {
      var tb = (TextBox)_dbName.Children[GetTBIndexByName(name, _dbName)];
      BindingOperations.ClearAllBindings(tb);
      _dbName.Children.Remove(tb);
    }

    private int GetTBIndexByName(string name, StackPanel UIEl)
    {
      for (var i = 0; i < UIEl.Children.Count; i++)
      {
        if (((TextBox)UIEl.Children[i]).Name == name)
          return i;
      }
      return -1;
    }
  }
}
