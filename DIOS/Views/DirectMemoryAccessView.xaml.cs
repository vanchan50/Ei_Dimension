using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ei_Dimension.Views;

/// <summary>
/// Interaction logic for DirectMemoryAccessView.xaml
/// </summary>
public partial class DirectMemoryAccessView : UserControl
{
  public static DirectMemoryAccessView Instance { get; private set; }
  public DirectMemoryAccessView()
  {
    InitializeComponent();
    Instance = this;
  }

  public void BlockUI()
  {
    foreach (UIElement input in dataSP.Children)
    {
      (input as TextBox).Background = (Brush)App.Current.Resources["HaltButtonBackground"];
      (input as TextBox).IsHitTestVisible = false;
    }
    foreach (UIElement button in buttonsSP.Children)
    {
      (button as Button).IsEnabled = false;
    }
  }

  public void UnBlockUI()
  {
    foreach (UIElement input in dataSP.Children)
    {
      (input as TextBox).Background = (Brush)App.Current.Resources["AppBackground"];
      (input as TextBox).IsHitTestVisible = true;
    }
    foreach (UIElement button in buttonsSP.Children)
    {
      (button as Button).IsEnabled = true;
    }
  }
}