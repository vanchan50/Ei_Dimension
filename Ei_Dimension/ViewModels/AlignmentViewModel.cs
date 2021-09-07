using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class AlignmentViewModel
  {
    public virtual ObservableCollection<string> LastCalibratedPosition { get; set; }
    public virtual ObservableCollection<string> LaserAlignMotor { get; set; }
    public virtual byte LaserAlignMotorSelectorState { get; set; }
    public virtual byte AutoAlignSelectorState { get; set; }

    protected AlignmentViewModel()
    {
      LastCalibratedPosition = new ObservableCollection<string> { "", "" };
      LaserAlignMotor = new ObservableCollection<string> { "" };
      LaserAlignMotorSelectorState = 1;
      AutoAlignSelectorState = 0;
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
      switch (num)
      {
        case 0:
          App.Device.MainCommand("Set Property", code: 0xc5);
          MaintenanceViewModel.Instance.LEDsEnabled = true;
          break;
        case 1:
          MaintenanceViewModel.Instance.LEDsToggleButtonState = true;
          MaintenanceViewModel.Instance.LEDsButtonClick();
          MaintenanceViewModel.Instance.LEDsEnabled = false;
          App.Device.MainCommand("Set Property", code: 0xc5, parameter: 1);
          break;
        case 2:
          MaintenanceViewModel.Instance.LEDsToggleButtonState = true;
          MaintenanceViewModel.Instance.LEDsButtonClick();
          MaintenanceViewModel.Instance.LEDsEnabled = false;
          App.Device.MainCommand("Set Property", code: 0xc5, parameter: 2);
          break;
        case 3:
          MaintenanceViewModel.Instance.LEDsToggleButtonState = true;
          MaintenanceViewModel.Instance.LEDsButtonClick();
          MaintenanceViewModel.Instance.LEDsEnabled = false;
          App.Device.MainCommand("Set Property", code: 0xc5, parameter: 3);
          break;
      }
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
          break;
        case 1:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(LastCalibratedPosition)), this, 1);
          break;
        case 2:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(LaserAlignMotor)), this, 0);
          break;
      }
    }
  }
}