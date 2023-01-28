using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class DirectMemoryAccessViewModel
  {
    public virtual Visibility ViewVisible { get; set; } = Visibility.Hidden;
    public virtual ObservableCollection<string> HexCode { get; set; } = new ObservableCollection<string> { "1" };
    public virtual ObservableCollection<string> IntValue { get; set; } = new ObservableCollection<string> { "0" };
    public virtual ObservableCollection<string> FloatValue { get; set; } = new ObservableCollection<string> { "0" };
    public static DirectMemoryAccessViewModel Instance { get; private set; }

    protected DirectMemoryAccessViewModel()
    {
      Instance = this;
    }

    public static DirectMemoryAccessViewModel Create()
    {
      return ViewModelSource.Create(() => new DirectMemoryAccessViewModel());
    }

    public void HideView()
    {
      UserInputHandler.InputSanityCheck();
      App.HideKeyboard();
      ViewVisible = Visibility.Hidden;
    }

    public void ShowView()
    {
      ViewVisible = Visibility.Visible;
    }

    public void Direct(bool getset)
    {
      var code = byte.Parse(HexCode[0], NumberStyles.HexNumber, Language.TranslationSource.Instance.CurrentCulture);
      var iVal = ushort.Parse(IntValue[0]);
      var fVal = float.Parse(FloatValue[0]);
      if (getset)
      {
        App.DiosApp.Device.Hardware.DirectFlashAccess(getset, code);
        Views.DirectMemoryAccessView.Instance.BlockUI();
      }
      else //set
      {
        App.DiosApp.Device.Hardware.DirectFlashAccess(getset, code, iVal, fVal);
      }
    }

    public void FocusedBox(int num)
    {
      var Stackpanel = Views.DirectMemoryAccessView.Instance.dataSP.Children;
      switch (num)
      {
        case 0:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(HexCode)), this, 0, (TextBox)Stackpanel[0]);
          MainViewModel.Instance.KeyboardToggle((TextBox)Stackpanel[0]);
          break;
        case 1:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(IntValue)), this, 0, (TextBox)Stackpanel[1]);
          MainViewModel.Instance.KeyboardToggle((TextBox)Stackpanel[1]);
          break;
        case 2:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(FloatValue)), this, 0, (TextBox)Stackpanel[2]);
          MainViewModel.Instance.KeyboardToggle((TextBox)Stackpanel[2]);
          break;
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }
  }
}
