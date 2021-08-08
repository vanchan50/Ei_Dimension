using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class MaintenanceViewModel
  {
    protected MaintenanceViewModel()
    {
        
    }

    public static MaintenanceViewModel Create()
    {
      return ViewModelSource.Create(() => new MaintenanceViewModel());
    }
  }
}