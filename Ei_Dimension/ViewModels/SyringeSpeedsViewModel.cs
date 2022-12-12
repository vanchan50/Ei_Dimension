using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using DIOS.Core;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class SyringeSpeedsViewModel
  {
    public virtual ObservableCollection<string> SheathSyringeParameters { get; set; } = new ObservableCollection<string>();
    public virtual ObservableCollection<string> SamplesSyringeParameters { get; set; } = new ObservableCollection<string>();
    public virtual ObservableCollection<bool> SingleSyringeMode { get; set; } = new ObservableCollection<bool>{ false };
    public static SyringeSpeedsViewModel Instance { get; private set; }

    protected SyringeSpeedsViewModel()
    {
      for (var i = 0; i < 6; i++)
      {
        SheathSyringeParameters.Add("");
        SamplesSyringeParameters.Add("");
      }
      Instance = this;
    }

    public static SyringeSpeedsViewModel Create()
    {
      return ViewModelSource.Create(() => new SyringeSpeedsViewModel());
    }

    public void SingleSyringeModeChecked(bool state)
    { 
      UserInputHandler.InputSanityCheck();
      if (state)
        App.Device.SetHardwareParameter(DeviceParameterType.SampleSyringeType, SampleSyringeType.Double);
      else
        App.Device.SetHardwareParameter(DeviceParameterType.SampleSyringeType, SampleSyringeType.Single);
      SingleSyringeMode[0] = state;
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 0, (TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[0]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[0]);
          break;
        case 1:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 1, (TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[1]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[1]);
          break;
        case 2:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 2, (TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[2]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[2]);
          break;
        case 3:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 3, (TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[3]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[3]);
          break;
        case 4:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 4, (TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[4]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[4]);
          break;
        case 5:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SheathSyringeParameters)), this, 5, (TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[5]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.sheathSP.Children[5]);
          break;
        case 6:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 0, (TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[0]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[0]);
          break;
        case 7:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 1, (TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[1]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[1]);
          break;
        case 8:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 2, (TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[2]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[2]);
          break;
        case 9:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 3, (TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[3]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[3]);
          break;
        case 10:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 4, (TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[4]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[4]);
          break;
        case 11:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SamplesSyringeParameters)), this, 5, (TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[5]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.SyringeSpeedsView.Instance.samplesSP.Children[5]);
          break;
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }
  }
}