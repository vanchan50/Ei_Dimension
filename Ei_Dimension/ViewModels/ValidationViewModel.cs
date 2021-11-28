using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
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

    public void ConfirmValidation()
    {
      App.Device.SaveCalVals(new MicroCy.MapCalParameters
      {
        TempCl0 = -1,
        TempCl1 = -1,
        TempCl2 = -1,
        TempCl3 = -1,
        TempRedSsc = -1,
        TempGreenSsc = -1,
        TempVioletSsc = -1,
        TempRpMaj = -1,
        TempRpMin = -1,
        TempFsc = -1,
        MinSSC = -1,
        MaxSSC = -1,
        Caldate = null,
        Valdate = System.DateTime.Now.ToString("dd.MM.yyyy")
      });
      ValidDateBox[0] = App.Device.ActiveMap.valtime;
    }
  }
}