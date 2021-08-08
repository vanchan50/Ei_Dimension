using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class DashboardViewModel
  {
    protected DashboardViewModel()
    {
        
    }

    public static DashboardViewModel Create()
    {
      return ViewModelSource.Create(() => new DashboardViewModel());
    }
  }
}