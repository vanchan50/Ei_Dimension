using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class DataAnalysisViewModel
  {
    protected DataAnalysisViewModel()
    {
    }

    public static DataAnalysisViewModel Create()
    {
      return ViewModelSource.Create(() => new DataAnalysisViewModel());
    }
  }
}