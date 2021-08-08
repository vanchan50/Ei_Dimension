using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ExperimentViewModel
  {
    protected ExperimentViewModel()
    {

    }

    public static ExperimentViewModel Create()
    {
      return ViewModelSource.Create(() => new ExperimentViewModel());
    }
  }
}