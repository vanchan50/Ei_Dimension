using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class SelRegionsViewModel
  {
    protected SelRegionsViewModel()
    {
    }

    public static SelRegionsViewModel Create()
    {
      return ViewModelSource.Create(() => new SelRegionsViewModel());
    }

    public void AddActiveRegion(byte num)
    {
      App.MapRegions.AddActiveRegion(num);
    }
  }
}