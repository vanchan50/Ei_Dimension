using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using System;
using DevExpress.Mvvm.POCO;
namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ResultsViewModel
  {
    protected ResultsViewModel()
    {
        
    }

    public static ResultsViewModel Create()
    {
      return ViewModelSource.Create(() => new ResultsViewModel());
    }
  }
}