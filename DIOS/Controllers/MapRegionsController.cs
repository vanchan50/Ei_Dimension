using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ei_Dimension.Models;
using Ei_Dimension.ViewModels;

namespace Ei_Dimension.Controllers;

public partial class MapRegionsController
{
  //all existing region numbers in a string format
  public static ObservableCollection<MapRegionData> RegionsList { get; } = new();
  //All the selected Active regions. Passed to the MicroCy.StartingProcedure()
  public static HashSet<int> ActiveRegionNums { get; } = new(101);
  public static HashSet<int> ActiveVerificationRegionNums { get; } = new(101);
  //exceptional NUllregion-case
  public bool IsNullRegionActive { get { return _resultsTableController.NullTextboxActive; } }

  private StackPanel _regionsBorder;
  private StackPanel _regionsNamesBorder;
  private bool _firstLoadflag;

  private static uint _tbCounter = 1;
  private static uint _nameTbCounter = 1;
  private static readonly Thickness _regionsTbAlignment = new(10, 10, 0, 0);
  private static readonly Thickness _lastRegionsTbAlignment = new(10, 10, 0, 10);
  private static readonly Thickness _NameBoxAlignment = new(0, 10, 0, 0);
  private static readonly Thickness _lastNameBoxAlignment = new(0, 10, 0, 10);
  private static readonly Style _regionsStyle = (Style)App.Current.Resources.MergedDictionaries[6]["RegionFieldStyle"];
  private static readonly Style _regionsNamesStyle = (Style)App.Current.Resources.MergedDictionaries[5]["InputFieldStyle"];
  private static TextBox _lastRegionsBox;
  private static TextBox _lastRegionsNameBox;
  private static TextBox _lastValidationRegionsBox;
  private static TextBox _lastValidationReporterBox;
  private static readonly Thickness _TbAlignment = new(5, 5, 5, 0);

  private ResultsTableController _resultsTableController;
  private ValidationTableController _validationTableController;
  private NormalizationTableController _normalizationTableController;
  private DashboardTableController _dashboardTableController;

  private static readonly SortedDictionary<int, int> _mapRegionNumberIndexDictionary = new();

  public MapRegionsController(StackPanel RegionsBorder, StackPanel RegionsNamesBorder, ListBox resultsTable,
    StackPanel Db_Num, StackPanel Db_Name, StackPanel Validat_Num, StackPanel Normaliz_Num, StackPanel Normaliz_MFI)
  {
    _regionsBorder = RegionsBorder;
    _regionsNamesBorder = RegionsNamesBorder;
    _resultsTableController = new ResultsTableController(this, resultsTable);
    _dashboardTableController = new DashboardTableController(Db_Num, Db_Name);
    _validationTableController = new ValidationTableController(this, Validat_Num);
    _normalizationTableController = new NormalizationTableController(this, Normaliz_Num, Normaliz_MFI);
    FillRegions();
  }

  /// <summary>
  /// </summary>
  /// <returns>false if there are no active regions or the only active region is Nullregion</returns>
  public static bool AreThereActiveRegions()
  {
    return !(ActiveRegionNums.Count == 0
             || (ActiveRegionNums.Count == 1 && ActiveRegionNums.Contains(0)));
  }

  public void FillRegions(bool loadByPage = false)
  {
    if (_firstLoadflag && loadByPage)
      return;
    _firstLoadflag = true;

    ClearAllTextboxes();
    RegionsList.Clear();
    ActiveRegionNums.Clear();
    ActiveVerificationRegionNums.Clear();
    ActiveRegionsStatsController.Instance.Clear();
    _mapRegionNumberIndexDictionary.Clear();
      
    RegionsList.Add(new MapRegionData(0));
    _mapRegionNumberIndexDictionary.Add(0,0);
    RegionsList[0].Name[0] = "UNCLSSFD";
    ActiveRegionsStatsController.Instance.AddNewEntry();
    var i = 1;
    foreach (var region in App.DiosApp.MapController.ActiveMap.regions)
    {
      RegionsList.Add(new MapRegionData(region.Number));
      _mapRegionNumberIndexDictionary.Add(region.Number, i);
      ActiveRegionsStatsController.Instance.AddNewEntry();
      AddTextboxes(i);
      i++;
    }
  }

  public static int GetMapRegionIndex(int regionNum)
  {
    if (_mapRegionNumberIndexDictionary.TryGetValue(regionNum, out var ret))
      return ret;
    return -1;
  }

  public void ResetRegions()
  {
    while (ActiveRegionNums.Count > 0)
    {
      var regionNum = ActiveRegionNums.First();
      if (regionNum == 0)
      {
        ActiveRegionNums.Remove(0);
        continue;
      }
      AddActiveRegion(regionNum, callFromCode: true);
    }
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

  public void ShowNullTextBoxes()
  {
    _resultsTableController.ShowNullTb();
    if(FileSaveViewModel.Instance.Checkboxes[4])
      ActiveRegionNums.Add(0);
  }

  public void RemoveNullTextBoxes()
  {
    _resultsTableController.RemoveNullTb();
    ActiveRegionNums.Remove(0);
  }

  private void AddTextboxes(int regionIndex)
  {
    var i = regionIndex.ToString();
    AddRegionsTextBox($"{nameof(RegionsList)}[{i}].{nameof(MapRegionData.NumberString)}");
    AddRegionsNamesTextBox($"{nameof(RegionsList)}[{i}].{nameof(MapRegionData.Name)}[0]");
    _validationTableController.AddRegionsTextBox($"{nameof(RegionsList)}[{i}].{nameof(MapRegionData.NumberString)}");
    _normalizationTableController.AddRegionsTextBox($"{nameof(RegionsList)}[{i}].{nameof(MapRegionData.NumberString)}");
    _normalizationTableController.AddMFITextBox($"{nameof(RegionsList)}[{i}].{nameof(MapRegionData.MFIValue)}[0]");
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
      _resultsTableController.AddRegion(NameTb.Name, nameBindToCopy);
      _dashboardTableController.AddDbNumBox(tb.Name, numBindToCopy);
      _dashboardTableController.AddDbNameBox(NameTb.Name, nameBindToCopy);
    }
    else
    {
      _resultsTableController.RemoveRegion(NameTb.Name);
      _dashboardTableController.RemoveDbNumBox(tb.Name);
      _dashboardTableController.RemoveDbNameBox(NameTb.Name);
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
    if (!((TextBox) e.Source).IsKeyboardFocusWithin)
      return;
    var regionNumber = int.Parse(((TextBox) e.Source).Text);
    AddActiveRegion(regionNumber);
  }

  private static void RegionsNamesTbGotFocus(object sender, RoutedEventArgs e)
  {
    if (!((TextBox) e.Source).IsKeyboardFocusWithin)
      return;
    var tb = (TextBox)sender;
    var property = typeof(MapRegionData).GetProperty(nameof(MapRegionData.Name));
    SetUserInputTextBox(property, tb);
  }

  private void ClearAllTextboxes()
  {
    _resultsTableController.Clear();
    ClearTextBoxes();
    _dashboardTableController.Clear();
    _validationTableController.Clear();
    _normalizationTableController.Clear();
  }

  private static void SetUserInputTextBox(System.Reflection.PropertyInfo property, TextBox tb)
  {
    var index = int.Parse(tb.Name.Trim('_'));
    UserInputHandler.SelectedTextBox = (property,
      RegionsList[index], 0, tb);
    MainViewModel.Instance.KeyboardToggle(tb);
  }

  private static void SetupBinding(TextBox tb, object source, string propertyPath, BindingMode mode)
  {
    Binding bind = new Binding();
    bind.Source = source;
    bind.Mode = mode;
    bind.Path = new PropertyPath(propertyPath);
    bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
    BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
  }
}