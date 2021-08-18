using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ComponentsViewModel
  {
    protected ComponentsViewModel()
    {
    }

    public static ComponentsViewModel Create()
    {
      return ViewModelSource.Create(() => new ComponentsViewModel());
    }
  }
}