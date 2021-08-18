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
    public virtual string AutoAlign { get; set; }

    protected AlignmentViewModel()
    {
      LastCalibratedPosition = new string[2];
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

    }

    public void RunLaserAlignMotorClick()
    {

    }

    public void HaltLaserAlignMotorClick()
    {

    }

    public void AutoAlignSelector(string s)
    {

    }

    public void RunAutoAlignClick()
    {

    }

    public void RunAlignSequenceClick()
    {

    }

    public void HaltAlignSequenceClick()
    {

    }

    public void GoToAlignSequenceClick()
    {

    }
  }
}