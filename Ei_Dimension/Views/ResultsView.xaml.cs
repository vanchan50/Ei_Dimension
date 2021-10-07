using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ei_Dimension.Views
{
  /// <summary>
  /// Interaction logic for ResultsView.xaml
  /// </summary>
  public partial class ResultsView : UserControl
  {
    public static ResultsView Instance;
    private static Style _regionsNamesStyle = (Style)App.Current.Resources.MergedDictionaries[5]["InputFieldStyle"];
    private static Thickness _TbAlignment = new Thickness(0, 5, 0, 0);
    public ResultsView()
    {
      InitializeComponent();
      Instance = this;
    }
    public void AddRegionToTable(string name, BindingBase bind)
    {
      //TODO: use hashtable for bookkeeping sp name and index in Table.Children? no need to search for sp by name then
      StackPanel sp = new StackPanel();
      sp.Name = name;
      sp.VerticalAlignment = VerticalAlignment.Top;
      sp.HorizontalAlignment = HorizontalAlignment.Left;
      sp.Width = 140;
      sp.Height = 129+5+5+5+5;
      sp.Margin = new Thickness(10, 5, 0, 0);
      sp.Background = Brushes.Red;

      int index = int.Parse(name.Trim('_'));
      sp.Children.Add(MakeNameTextBox(bind));
      sp.Children.Add(MakeCounterTextBox(index));
      sp.Children.Add(MakeMeanTextBox(index));
      Table.Children.Add(sp);
    }

    public void RemoveRegionFromTable(string name)
    {
      var sp = (StackPanel)Table.Children[GetSPIndexByName(name, Table)];
      for(var i = sp.Children.Count -1; i > -1; i--)
      {
        BindingOperations.ClearAllBindings(sp.Children[i]);
        sp.Children.RemoveAt(i);
      }
      Table.Children.Remove(sp);
    }
    private TextBox MakeNameTextBox(BindingBase bind)
    {
      var tb = new TextBox();
      tb.VerticalAlignment = VerticalAlignment.Top;
      tb.Style = _regionsNamesStyle;
      tb.IsReadOnly = true;
      tb.Margin = _TbAlignment;
      BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
      return tb;
    }

    private TextBox MakeCounterTextBox(int index)
    {
      var tb = new TextBox();
      tb.Style = _regionsNamesStyle;
      tb.IsReadOnly = true;
      tb.Margin = _TbAlignment;
      Binding bind = new Binding();
      bind.Source = ViewModels.ResultsViewModel.Instance;
      bind.Path = new PropertyPath($"ActiveRegionsCount[{index}]");
      bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
      return tb;
    }

    private TextBox MakeMeanTextBox(int index)
    {
      var tb = new TextBox();
      tb.Style = _regionsNamesStyle;
      tb.IsReadOnly = true;
      tb.Margin = _TbAlignment;
      Binding bind = new Binding();
      bind.Source = ViewModels.ResultsViewModel.Instance;
      bind.Path = new PropertyPath($"ActiveRegionsMean[{index}]");
      bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);
      return tb;
    }
    
    private int GetSPIndexByName(string name, Panel UIEl)
    {
      for(var i = 0; i < UIEl.Children.Count; i++)
      {
        if (((StackPanel)UIEl.Children[i]).Name == name)
          return i;
      }
      return -1;
    }
  }
}
