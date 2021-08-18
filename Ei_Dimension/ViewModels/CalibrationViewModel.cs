using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class CalibrationViewModel
  {

    protected CalibrationViewModel()
    {
    }

    public static CalibrationViewModel Create()
    {
      return ViewModelSource.Create(() => new CalibrationViewModel());
    }
  }
}