using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class AlignmentViewModel
  {
    public virtual string[] LastCalibratedPosition { get; set; }
    public virtual string LaserAlignMotor { get; set; }
    public virtual string LaserAlignMotorSelectorState { get; set; }
    public virtual string AutoAlign { get; set; }
    public virtual string AutoAlignSelectorState { get; set; }

    protected AlignmentViewModel()
    {
      LastCalibratedPosition = new string[2];
      LaserAlignMotorSelectorState = "Left";
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
  }
}