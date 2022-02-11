using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class AlignmentViewModel
  {
    public virtual byte LaserAlignMotorSelectorState { get; set; }
    public virtual byte AutoAlignSelectorState { get; set; }
    public virtual ObservableCollection<string> ValidationTolerances { get; set; }
    public static AlignmentViewModel Instance { get; private set; }

    private MaintenanceViewModel _maintVM;

    protected AlignmentViewModel()
    {
      LaserAlignMotorSelectorState = 1;
      AutoAlignSelectorState = 0;
      ValidationTolerances = new ObservableCollection<string>
      {
        Settings.Default.ValidatorToleranceReporter.ToString("0.000"),
        Settings.Default.ValidatorToleranceClassification.ToString("0.000"),
        Settings.Default.ValidatorToleranceMisclassification.ToString("0.000")
      };

      if(MaintenanceViewModel.Instance != null)
        _maintVM = MaintenanceViewModel.Instance;
      Instance  = this;
    }

    public static AlignmentViewModel Create()
    {
      return ViewModelSource.Create(() => new AlignmentViewModel());
    }

    public void AutoAlignSelector(byte num)
    {
      bool state = false;
      if(num == 0)
        state = true;
      LedsOn(state);
      App.Device.MainCommand("Set Property", code: 0xc5, parameter: (ushort)num);
      AutoAlignSelectorState = num;
    }

    public void ScanAlignSequenceClick()
    {
      App.Device.MainCommand("AlignMotor", cmd: 3);
    }

    public void FindPeakAlignSequenceClick()
    {
      App.Device.MainCommand("AlignMotor", cmd: 4);
    }

    public void GoToAlignSequenceClick()
    {
      App.Device.MainCommand("AlignMotor", cmd: 5);
    }

    private void LedsOn(bool on)
    {
      if (_maintVM == null && MaintenanceViewModel.Instance != null)
        _maintVM = MaintenanceViewModel.Instance;

      if (_maintVM != null)
      {
        if (on)
          _maintVM.LEDsEnabled = true;
        else
        {
          _maintVM.LEDsToggleButtonState = true;
          _maintVM.LEDsButtonClick();
          _maintVM.LEDsEnabled = false;
        }
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }

    public void FocusedBox(int num)
    {
      var Stackpanel = Views.AlignmentView.Instance.ToleranceSP.Children;
      switch (num)
      {
        case 0:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ValidationTolerances)), this, 0, (TextBox)Stackpanel[1]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[1]);
          break;
        case 1:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ValidationTolerances)), this, 1, (TextBox)Stackpanel[3]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[3]);
          break;
        case 2:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ValidationTolerances)), this, 2, (TextBox)Stackpanel[5]);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[5]);
          break;
      }
    }
  }
}