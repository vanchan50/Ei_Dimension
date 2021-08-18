using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class SyringeSpeedsViewModel
  {
    protected SyringeSpeedsViewModel()
    {
    }

    public static SyringeSpeedsViewModel Create()
    {
      return ViewModelSource.Create(() => new SyringeSpeedsViewModel());
    }
  }
}