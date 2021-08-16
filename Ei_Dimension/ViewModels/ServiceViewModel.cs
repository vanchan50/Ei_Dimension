using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ServiceViewModel
  {
    protected ServiceViewModel()
    {
    }

    public static ServiceViewModel Create()
    {
      return ViewModelSource.Create(() => new ServiceViewModel());
    }
  }
}