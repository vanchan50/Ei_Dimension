using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ValidationViewModel
  {
    public virtual ObservableCollection<string> ValidDateBox { get; set; }
    public static ValidationViewModel Instance { get; private set; }
    protected ValidationViewModel()
    {
      ValidDateBox = new ObservableCollection<string> { App.Device.ActiveMap.valtime };
      Instance = this;
    }

    public static ValidationViewModel Create()
    {
      return ViewModelSource.Create(() => new ValidationViewModel());
    }

    public void AddValidationRegion(byte num)
    {
      //App.MapRegions.AddValidationRegion(num);
    }

    public void ValidateMap()
    {

    }
  }
}