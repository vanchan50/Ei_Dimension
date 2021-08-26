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
    public virtual string LaserAlignMotorSelectorState { get; set; }
    public virtual ObservableCollection<string> AutoAlign { get; set; }
    public virtual string AutoAlignSelectorState { get; set; }

    protected AlignmentViewModel()
    {
      LastCalibratedPosition = new ObservableCollection<string> { "", "" };
      LaserAlignMotor = new ObservableCollection<string> { "" };
      LaserAlignMotorSelectorState = "Left";
      AutoAlign = new ObservableCollection<string> { "" };
      AutoAlignSelectorState = "Off";
    }

    public static AlignmentViewModel Create()
    {
      return ViewModelSource.Create(() => new AlignmentViewModel());
    }

    public void GoToLastCalibratedPositionClick()
    {

    }

    public void LaserAlignMotorSelector(string s)
    {
      LaserAlignMotorSelectorState = s;
    }

    public void RunLaserAlignMotorClick()
    {

    }

    public void HaltLaserAlignMotorClick()
    {

    }

    public void AutoAlignSelector(string s)
    {
      AutoAlignSelectorState = s;
    }

    public void RunAutoAlignClick()
    {

    }

    public void ScanAlignSequenceClick()
    {

    }

    public void FindPeakAlignSequenceClick()
    {

    }

    public void GoToAlignSequenceClick()
    {

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
        case 3:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(AutoAlign)), this, 0);
          break;
      }
    }
  }
}