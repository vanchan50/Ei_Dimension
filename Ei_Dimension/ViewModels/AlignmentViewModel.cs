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
    public virtual byte LaserAlignMotorSelectorState { get; set; }
    public virtual byte AutoAlignSelectorState { get; set; }

    private MaintenanceViewModel _maintVM;

    protected AlignmentViewModel()
    {
      LaserAlignMotorSelectorState = 1;
      AutoAlignSelectorState = 0;

      if(MaintenanceViewModel.Instance != null)
        _maintVM = MaintenanceViewModel.Instance;
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
  }
}