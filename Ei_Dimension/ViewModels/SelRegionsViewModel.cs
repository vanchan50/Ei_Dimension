using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class SelRegionsViewModel
  {
    public static SelRegionsViewModel Instance { get; private set; }
    protected SelRegionsViewModel()
    {
      Instance = this;
    }

    public static SelRegionsViewModel Create()
    {
      return ViewModelSource.Create(() => new SelRegionsViewModel());
    }

    public void AddActiveRegion(byte num)
    {
      //App.MapRegions.AddActiveRegion();
    }

    public void AllSelectClick()
    {
      for (var i = 0; i < App.MapRegions.ActiveRegions.Count; i++)
      {
        if(!App.MapRegions.ActiveRegions[i])
          App.MapRegions.AddActiveRegion(i);
      }
    }

    public void ResetClick()
    {
      App.MapRegions.FillRegions();
    }
  }
}