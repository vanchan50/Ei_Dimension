using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ValidationViewModel
  {
    protected ValidationViewModel()
    {

    }

    public static ValidationViewModel Create()
    {
      return ViewModelSource.Create(() => new ValidationViewModel());
    }

    public void AddValidationRegion(byte num)
    {
      App.MapRegions.AddValidationRegion(num);
    }

    public void StartValidationClick()
    {

    }
  }
}