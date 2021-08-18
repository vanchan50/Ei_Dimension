using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class SyringeSpeedsViewModel
  {
    public virtual string[] SheathSyringeParameters { get; set; }
    public virtual string[] SamplesSyringeParameters { get; set; }

    protected SyringeSpeedsViewModel()
    {
      SheathSyringeParameters = new string[6];
      SamplesSyringeParameters = new string[6];
    }

    public static SyringeSpeedsViewModel Create()
    {
      return ViewModelSource.Create(() => new SyringeSpeedsViewModel());
    }
  }
}