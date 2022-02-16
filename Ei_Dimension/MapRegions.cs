using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ei_Dimension.Models;
using Ei_Dimension.ViewModels;

namespace Ei_Dimension
{
  public class MapRegions
  {
    //all existing region numbers in a string format
    public ObservableCollection<MapRegionData> RegionsList { get; } = new ObservableCollection<MapRegionData>();
    //All the selected Active regions. Passed to the MicroCy.StartingProcedure()
    public HashSet<int> ActiveRegionNums { get; } = new HashSet<int>();
    public HashSet<int> VerificationRegionNums { get; } = new HashSet<int>();
    //storage for mean and count of all existing regions. For current reading and backing file select
    public ObservableCollection<string> CurrentActiveRegionsCount { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> CurrentActiveRegionsMean { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> BackingActiveRegionsCount { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> BackingActiveRegionsMean { get; } = new ObservableCollection<string>();
    //pointers to storages of mean and count
    //public only for binding purpose
    public ObservableCollection<string> DisplayedActiveRegionsCount { get; private set; }
    public ObservableCollection<string> DisplayedActiveRegionsMean { get; private set; }
    //exceptional NUllregion-case
    public bool IsNullRegionActive { get { return _nullTextboxActive; } }

    private StackPanel _regionsBorder;
    private StackPanel _regionsNamesBorder;
    private bool _firstLoadflag;
    //Results
    private static uint _tbCounter = 1;
    private static uint _nameTbCounter = 1;
    private static readonly Thickness _regionsTbAlignment = new Thickness(10, 10, 0, 0);
    private static readonly Thickness _lastRegionsTbAlignment = new Thickness(10, 10, 0, 10);
    private static readonly Thickness _NameBoxAlignment = new Thickness(0, 10, 0, 0);
    private static readonly Thickness _lastNameBoxAlignment = new Thickness(0, 10, 0, 10);
    private static readonly Style _regionsStyle = (Style)App.Current.Resources.MergedDictionaries[6]["RegionFieldStyle"];
    private static readonly Style _regionsNamesStyle = (Style)App.Current.Resources.MergedDictionaries[5]["InputFieldStyle"];
    private static TextBox _lastRegionsBox;
    private static TextBox _lastRegionsNameBox;
    private static TextBox _lastValidationRegionsBox;
    private static TextBox _lastValidationReporterBox;
    private ListBox _resultsTable;
    private static readonly Thickness _TbAlignment = new Thickness(5, 5, 5, 0);
    //DB
    private StackPanel _dashboardNum;
    private StackPanel _dashboardName;
    //Validation
    private StackPanel _validationNum;
    private StackPanel _validationReporterBorder;

    private bool _nullTextboxActive;
    private readonly SortedDictionary<int, int> _mapRegionNumberIndexDictionary = new SortedDictionary<int, int>();

    public MapRegions(StackPanel RegionsBorder, StackPanel RegionsNamesBorder, ListBox Table, StackPanel Db_Num, StackPanel Db_Name, StackPanel Validat_Num,
      StackPanel Validat_Reporter)
    {
      _regionsBorder = RegionsBorder;
      _regionsNamesBorder = RegionsNamesBorder;
      _resultsTable = Table;
      _dashboardNum = Db_Num;
      _dashboardName = Db_Name;
      _validationNum = Validat_Num;
      _validationReporterBorder = Validat_Reporter;
      DisplayCurrentActiveRegionsBeadStats();
      FillRegions();
    }

    public void DisplayCurrentActiveRegionsBeadStats(bool current = true)
    {
      if (current)
      {
        DisplayedActiveRegionsCount = CurrentActiveRegionsCount;
        DisplayedActiveRegionsMean = CurrentActiveRegionsMean;
        return;
      }
      DisplayedActiveRegionsCount = BackingActiveRegionsCount;
      DisplayedActiveRegionsMean = BackingActiveRegionsMean;
    }

    public void FillRegions(bool loadByPage = false)
    {
      if (_firstLoadflag && loadByPage)
        return;
      _firstLoadflag = true;
      ClearResultsTextBoxes();
      ClearTextBoxes();
      ClearDashboardTextBoxes();
      ClearValidationTextBoxes();
      RegionsList.Clear();
      ActiveRegionNums.Clear();
      VerificationRegionNums.Clear();
      CurrentActiveRegionsCount.Clear();
      CurrentActiveRegionsMean.Clear();
      BackingActiveRegionsCount.Clear();
      BackingActiveRegionsMean.Clear();
      _mapRegionNumberIndexDictionary.Clear();
      
      RegionsList.Add(new MapRegionData(0));
      _mapRegionNumberIndexDictionary.Add(0,0);
      RegionsList[0].Name[0] = "UNCLSSFD";
      CurrentActiveRegionsCount.Add("0");
      CurrentActiveRegionsMean.Add("0");
      BackingActiveRegionsCount.Add("0");
      BackingActiveRegionsMean.Add("0");
      var i = 1;
      foreach (var region in App.Device.MapCtroller.ActiveMap.regions)
      {
        RegionsList.Add(new MapRegionData(region.Number));
        _mapRegionNumberIndexDictionary.Add(region.Number, i);
        CurrentActiveRegionsCount.Add("0");
        CurrentActiveRegionsMean.Add("0");
        BackingActiveRegionsCount.Add("0");
        BackingActiveRegionsMean.Add("0");
        AddTextboxes(i);
        i++;
      }
    }

    public int GetMapRegionIndex(int regionNum)
    {
      if (_mapRegionNumberIndexDictionary.TryGetValue(regionNum, out var ret))
        return ret;
      return -1;
    }

    public void AddActiveRegion(int regionNum, bool callFromCode = false)
    {
      var index = GetMapRegionIndex(regionNum);
      if (!ActiveRegionNums.Contains(regionNum))
      {
        ActiveRegionNums.Add(regionNum);
        ShiftTextBox(index - 1, true);  // -1 accounts for inexistent region 0 box
      }
      else
      {
        ShiftTextBox(index - 1, false);
        ActiveRegionNums.Remove(regionNum);
      }

      if (callFromCode)
        return;
      App.UnfocusUIElement();
    }

    public void AddValidationRegion(int regionNum)
    {
      var index = GetMapRegionIndex(regionNum);
      if (!VerificationRegionNums.Contains(regionNum))
      {
        VerificationRegionNums.Add(regionNum);
        ShiftValidationTextBox(index - 1, true);
      }
      else
      {
        ShiftValidationTextBox(index - 1, false);
        VerificationRegionNums.Remove(regionNum);
      }
      App.UnfocusUIElement();
    }

    public void ShowNullTextBoxes()
    {
      if (!_nullTextboxActive)
      {
        Binding bind = new Binding();
        bind.Source = this;
        bind.Path = new PropertyPath("RegionsList[0].Name[0]");
        bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
        AddRegionToTable("__0", bind);
        _nullTextboxActive = true;
      }
    }

    public void RemoveNullTextBoxes()
    {
      if (_nullTextboxActive)
      {
        RemoveRegionFromTable("__0");
        _nullTextboxActive = false;
      }
    }

    public void ResetCurrentActiveRegionsDisplayedStats()
    {
      for (var i = 0; i < CurrentActiveRegionsCount.Count; i++)
      {
        CurrentActiveRegionsCount[i] = "0";
        CurrentActiveRegionsMean[i] = "0";
      }
    }

    private void AddTextboxes(int regionNum)
    {
      var i = regionNum.ToString();
      AddRegionsTextBox($"RegionsList[{i}].NumberString");
      AddRegionsNamesTextBox($"RegionsList[{i}].Name[0]");
      AddValidationRegionsTextBox($"RegionsList[{i}].NumberString");
      AddValidationReporterTextBox($"RegionsList[{i}].TargetReporterValue[0]");
      _tbCounter++;
    }

    private void ClearTextBoxes()
    {
      foreach (UIElement UIEl in _regionsBorder.Children)
      {
        BindingOperations.ClearAllBindings(UIEl);
        ((TextBox)UIEl).GotFocus -= RegionsTbGotFocus;
      }
      _regionsBorder.Children.Clear();
      _tbCounter = 1;
      _lastRegionsBox = null;
      foreach (UIElement UIEl in _regionsNamesBorder.Children)
      {
        BindingOperations.ClearAllBindings(UIEl);
        ((TextBox)UIEl).GotFocus -= RegionsNamesTbGotFocus;
      }
      _regionsNamesBorder.Children.Clear();
      _nameTbCounter = 1;
      _lastRegionsNameBox = null;
    }

    private void ClearDashboardTextBoxes()
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
    }

    private void ClearValidationTextBoxes()
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

    private void ShiftTextBox(int index, bool right)
    {
      var tb = (TextBox)_regionsBorder.Children[index];
      var shift = tb.Margin;
      shift.Left = right ? 140 : 10;
      tb.Margin = shift;

      var NameTb = (TextBox)_regionsNamesBorder.Children[index];
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
      RemoveNullTextBoxes();
    }

    private void AddRegionsTextBox(string propertyPath)
    {
      var tb = new TextBox();
      tb.Style = _regionsStyle;
      if (_lastRegionsBox != null)
        _lastRegionsBox.Margin = _regionsTbAlignment;
      _lastRegionsBox = tb;
      tb.Margin = _lastRegionsTbAlignment;
      tb.Name = $"_{_tbCounter}";
      tb.IsReadOnly = true;
      SetupBinding(tb, this, propertyPath, BindingMode.OneTime);
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
      SetupBinding(tb, this, propertyPath, BindingMode.TwoWay);
      tb.GotFocus += RegionsNamesTbGotFocus;
      _regionsNamesBorder.Children.Add(tb);
    }

    private void RegionsTbGotFocus(object sender, RoutedEventArgs e)
    {
      var regionNumber = int.Parse(((TextBox) e.Source).Text);
      AddActiveRegion(regionNumber);
    }

    private void RegionsNamesTbGotFocus(object sender, RoutedEventArgs e)
    {
      var tb = (TextBox)sender;
      var property = typeof(MapRegionData).GetProperty(nameof(MapRegionData.Name));
      SetUserInputTextBox(property, tb);
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
      _resultsTable.Items.Add(sp);
    }

    private void RemoveRegionFromTable(string name)
    {
      var index = GetSPIndexByName(name, _resultsTable);
      if (index < 0)
      {
        Console.Error.WriteLine("Tried to remove inexistent region from table");
        return;
      }
      var sp = (StackPanel)_resultsTable.Items[index];
      for (var i = sp.Children.Count - 1; i > -1; i--)
      {
        BindingOperations.ClearAllBindings(sp.Children[i]);
        sp.Children.RemoveAt(i);
      }
      _resultsTable.Items.Remove(sp);
    }

    private void ClearResultsTextBoxes()
    {
      for (var i = _resultsTable.Items.Count - 1; i > -1; i--)
      {
        var sp = (StackPanel)_resultsTable.Items[i];
        for (var j = sp.Children.Count - 1; j > -1; j--)
        {
          BindingOperations.ClearAllBindings(sp.Children[j]);
          sp.Children.RemoveAt(j);
        }
        _resultsTable.Items.RemoveAt(i);
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
      SetupBinding(tb, this, $"DisplayedActiveRegionsCount[{index}]", BindingMode.TwoWay);
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
      SetupBinding(tb, this, $"DisplayedActiveRegionsMean[{index}]", BindingMode.TwoWay);
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
      _dashboardNum.Children.Add(tb);
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
      _dashboardName.Children.Add(tb);
    }

    private void RemoveDbNumBox(string name)
    {
      var tb = (TextBox)_dashboardNum.Children[GetTBIndexByName(name, _dashboardNum)];
      BindingOperations.ClearAllBindings(tb);
      _dashboardNum.Children.Remove(tb);
    }
    private void RemoveDbNameBox(string name)
    {
      var tb = (TextBox)_dashboardName.Children[GetTBIndexByName(name, _dashboardName)];
      BindingOperations.ClearAllBindings(tb);
      _dashboardName.Children.Remove(tb);
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

    //ValidationView functionality

    private void AddValidationRegionsTextBox(string propertyPath)
    {
      var tb = new TextBox();
      tb.Style = _regionsStyle;
      if (_lastValidationRegionsBox != null)
        _lastValidationRegionsBox.Margin = _regionsTbAlignment;
      _lastValidationRegionsBox = tb;
      tb.Margin = _lastRegionsTbAlignment;
      tb.Name = $"_{_tbCounter}";
      tb.IsReadOnly = true;
      SetupBinding(tb, this, propertyPath, BindingMode.OneTime);
      tb.GotFocus += ValidationRegionsTbGotFocus;
      _validationNum.Children.Add(tb);
    }

    private void AddValidationReporterTextBox(string propertyPath)
    {
      var tb = new TextBox();
      tb.Style = _regionsNamesStyle;
      if (_lastValidationReporterBox != null)
        _lastValidationReporterBox.Margin = _NameBoxAlignment;
      _lastValidationReporterBox = tb;
      tb.Margin = _lastNameBoxAlignment;
      tb.Name = $"__{_tbCounter}";
      tb.Visibility = Visibility.Hidden;
      SetupBinding(tb, this, propertyPath, BindingMode.TwoWay);
      tb.GotFocus += ValidationReporterTbGotFocus;
      tb.TextChanged += ValidationReporterTextChanged;
      _validationReporterBorder.Children.Add(tb);
    }

    private void ValidationRegionsTbGotFocus(object sender, RoutedEventArgs e)
    {
      var regionNumber = int.Parse(((TextBox)e.Source).Text);
      int Index = int.Parse(((TextBox)e.Source).Name.Trim('_'));
      AddValidationRegion(regionNumber);
    }

    private void ValidationReporterTbGotFocus(object sender, RoutedEventArgs e)
    {
      var tb = (TextBox)sender;
      var property = typeof(MapRegionData).GetProperty(nameof(MapRegionData.TargetReporterValue));
      SetUserInputTextBox(property, tb);
    }
      
    private void ValidationReporterTextChanged(object sender, TextChangedEventArgs e)
    {
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

    private void SetUserInputTextBox(System.Reflection.PropertyInfo property, TextBox tb)
    {
      var index = int.Parse(tb.Name.Trim('_'));
      UserInputHandler.SelectedTextBox = (property,
        RegionsList[index], 0, tb);
      MainViewModel.Instance.KeyboardToggle(tb);
    }

    private void SetupBinding(TextBox tb, object source, string propertyPath, BindingMode mode)
    {
      Binding bind = new Binding();
      bind.Source = source;
      bind.Mode = mode;
      bind.Path = new PropertyPath(propertyPath);
      bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
    }
  }
}