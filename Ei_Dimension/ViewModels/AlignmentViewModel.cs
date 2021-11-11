using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class AlignmentViewModel
  {
    public virtual ObservableCollection<string> LastCalibratedPosition { get; set; }
    public virtual ObservableCollection<string> LaserAlignMotor { get; set; }
    public virtual byte LaserAlignMotorSelectorState { get; set; }
    public virtual byte AutoAlignSelectorState { get; set; }

    private MaintenanceViewModel _maintVM;

    protected AlignmentViewModel()
    {
      LastCalibratedPosition = new ObservableCollection<string> { "", "" };
      LaserAlignMotor = new ObservableCollection<string> { "" };
      LaserAlignMotorSelectorState = 1;
      AutoAlignSelectorState = 0;

      if(MaintenanceViewModel.Instance != null)
        _maintVM = MaintenanceViewModel.Instance;
    }

    public static AlignmentViewModel Create()
    {
      return ViewModelSource.Create(() => new AlignmentViewModel());
    }

    public void GoToLastCalibratedPositionClick()
    {

    }

    public void LaserAlignMotorSelector(byte num)
    {
      LaserAlignMotorSelectorState = num;
    }

    public void RunLaserAlignMotorClick()
    {
      if (float.TryParse(LaserAlignMotor[0], out float fRes))
      {
        App.Device.MainCommand("AlignMotor", cmd: LaserAlignMotorSelectorState, fparameter: fRes);
      }
    }

    public void HaltLaserAlignMotorClick()
    {
      App.Device.MainCommand("AlignMotor");
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

    public void RunAutoAlignClick()
    {

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

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(LastCalibratedPosition)), this, 0);
          MainViewModel.Instance.NumpadToggleButton(Views.AlignmentView.Instance.TB0);
          break;
        case 1:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(LastCalibratedPosition)), this, 1);
          MainViewModel.Instance.NumpadToggleButton(Views.AlignmentView.Instance.TB1);
          break;
        case 2:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(LaserAlignMotor)), this, 0);
          MainViewModel.Instance.NumpadToggleButton(Views.AlignmentView.Instance.TB2);
          break;
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      App.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
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
  }
}