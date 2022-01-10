using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public HashSet<int> ActiveRegionNums { get; } = new HashSet<int>();
    public HashSet<int> VerificationRegionNums { get; } = new HashSet<int>();
    public ObservableCollection<string> RegionsNamesList { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> VerificationReporterList { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> CurrentActiveRegionsCount { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> CurrentActiveRegionsMean { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> BackingActiveRegionsCount { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> BackingActiveRegionsMean { get; } = new ObservableCollection<string>();
    public virtual ObservableCollection<string> DisplayedActiveRegionsCount { get; set; }
    public virtual ObservableCollection<string> DisplayedActiveRegionsMean { get; set; }
    public List<bool> ActiveRegions { get; } = new List<bool>();
    public List<bool> VerificationRegions { get; } = new List<bool>();
    private StackPanel _regionsBorder;
    private StackPanel _regionsNamesBorder;
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
    private static TextBox _lastValidationRegionsBox;
    private static TextBox _lastValidationReporterBox;
    private ListBox _resultsTable;
    private static Thickness _TbAlignment = new Thickness(5, 5, 5, 0);
    //DB
    private StackPanel _dashboardNum;
    private StackPanel _dashboardName;
    //Validation
    private StackPanel _validationNum;
    private StackPanel _validationReporterBorder;

    protected MapRegions(StackPanel RegionsBorder, StackPanel RegionsNamesBorder, ListBox Table, StackPanel Db_Num, StackPanel Db_Name, StackPanel Validat_Num,
      StackPanel Validat_Reporter)
    {
      _regionsBorder = RegionsBorder;
      _regionsNamesBorder = RegionsNamesBorder;
      _resultsTable = Table;
      _dashboardNum = Db_Num;
      _dashboardName = Db_Name;
      _validationNum = Validat_Num;
      _validationReporterBorder = Validat_Reporter;
      DisplayedActiveRegionsCount = CurrentActiveRegionsCount;
      DisplayedActiveRegionsMean = CurrentActiveRegionsMean;
      FillRegions();
    }

    public static MapRegions Create(StackPanel RegionsBorder, StackPanel RegionsNamesBorder, ListBox Table, StackPanel Db_Num, StackPanel Db_Name, StackPanel Validat_Num,
      StackPanel Validat_Reporter)
    {
      return ViewModelSource.Create(() => new MapRegions(RegionsBorder, RegionsNamesBorder, Table, Db_Num, Db_Name, Validat_Num, Validat_Reporter));
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
      RegionsNamesList.Clear();
      ActiveRegions.Clear();
      VerificationRegions.Clear();
      VerificationReporterList.Clear();
      CurrentActiveRegionsCount.Clear();
      CurrentActiveRegionsMean.Clear();
      BackingActiveRegionsCount.Clear();
      BackingActiveRegionsMean.Clear();
      var i = 0;
      foreach (var region in App.Device.ActiveMap.regions)
      {
        RegionsList.Add(region.Number.ToString());
        RegionsNamesList.Add("");
        VerificationReporterList.Add("");
        ActiveRegions.Add(false);
        VerificationRegions.Add(false);
        CurrentActiveRegionsCount.Add("0");
        CurrentActiveRegionsMean.Add("0");
        BackingActiveRegionsCount.Add("0");
        BackingActiveRegionsMean.Add("0");
        AddTextboxes($"RegionsList[{i}]", $"RegionsNamesList[{i}]", $"VerificationReporterList[{i}]");
        i++;
      }
    }

    public void AddActiveRegion(int index, bool callFromCode = false)
    {
      if (!ActiveRegions[index])
      {
        ShiftTextBox(index, true);
        ActiveRegions[index] = true;
        if (RegionsNamesList[index] == "")
        {
          RegionsNamesList[index] = RegionsList[index].ToString();
        }
      }
      else
      {
        ActiveRegions[index] = false;
        ShiftTextBox(index, false);
      }

      if (callFromCode)
        return;
      App.UnfocusUIElement();
    }

    private void AddTextboxes(string RegionsNums, string RegionsNames, string ValidationReporter)
    {
      AddRegionsTextBox(RegionsNums);
      AddRegionsNamesTextBox(RegionsNames);
      AddValidationRegionsTextBox(RegionsNums);
      AddValidationReporterTextBox(ValidationReporter);
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
        ActiveRegionNums.Add(int.Parse(tb.Text.Trim('_')));
      }
      else
      {
        RemoveRegionFromTable(NameTb.Name);
        RemoveDbNumBox(tb.Name);
        RemoveDbNameBox(NameTb.Name);
        ActiveRegionNums.Remove(int.Parse(tb.Text.Trim('_')));
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
      tb.Name = $"_{_tbCounter}";
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
      var Index = int.Parse(((TextBox)e.Source).Name.Trim('_'));
      AddActiveRegion(Index);
    }

    private void RegionsNamesTbGotFocus(object sender, RoutedEventArgs e)
    {
      App.SelectedTextBox = (this.GetType()
        .GetProperty(nameof(RegionsNamesList)),
        this, int.Parse(((TextBox)e.Source).Name.Trim('_')), (TextBox)sender);
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
      _resultsTable.Items.Add(sp);
    }

    private void RemoveRegionFromTable(string name)
    {
      var sp = (StackPanel)_resultsTable.Items[GetSPIndexByName(name, _resultsTable)];
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
      Binding bind = new Binding();
      bind.Source = this;
      bind.Path = new PropertyPath(propertyPath);
      bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
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
      Binding bind = new Binding();
      bind.Source = this;
      bind.Path = new PropertyPath(propertyPath);
      bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
      tb.GotFocus += ValidationReporterTbGotFocus;
      tb.TextChanged += ValidationReporterTextChanged;
      _validationReporterBorder.Children.Add(tb);
    }

    private void ValidationRegionsTbGotFocus(object sender, RoutedEventArgs e)
    {
      int Index = int.Parse(((TextBox)e.Source).Name.Trim('_'));
      AddValidationRegion(Index);
    }

    private void ValidationReporterTbGotFocus(object sender, RoutedEventArgs e)
    {
      App.SelectedTextBox = (this.GetType()
        .GetProperty(nameof(VerificationReporterList)),
        this, int.Parse(((TextBox)e.Source).Name.Trim('_')), (TextBox)sender);
      MainViewModel.Instance.NumpadToggleButton((TextBox)e.Source);
    }
      
    private void ValidationReporterTextChanged(object sender, TextChangedEventArgs e)
    {
      App.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }

    public void AddValidationRegion(int index)
    {
      if (!VerificationRegions[index])
      {
        ShiftValidationTextBox(index, true);
        VerificationRegions[index] = true;
      }
      else
      {
        VerificationRegions[index] = false;
        ShiftValidationTextBox(index, false);
      }
      App.UnfocusUIElement();
    }

    private void ShiftValidationTextBox(int index, bool right)
    {
      var tb = (TextBox)_validationNum.Children[index];
      var shift = tb.Margin;
      shift.Left = right ? 140 : 10;
      tb.Margin = shift;

      var ReporterTb = (TextBox)_validationReporterBorder.Children[index];
      ReporterTb.Visibility = right ? Visibility.Visible : Visibility.Hidden;
      if(right)
        VerificationRegionNums.Add(int.Parse(tb.Text.Trim('_')));
      else
        VerificationRegionNums.Remove(int.Parse(tb.Text.Trim('_')));
    }

    public void ResetCurrentActiveRegionsDisplayedStats()
    {
      for (var i = 0; i < CurrentActiveRegionsCount.Count; i++)
      {
        CurrentActiveRegionsCount[i] = "0";
        CurrentActiveRegionsMean[i] = "0";
      }
    }

  }
}