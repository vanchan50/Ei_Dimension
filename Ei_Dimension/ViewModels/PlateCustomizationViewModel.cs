using System.Windows;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class PlateCustomizationViewModel
  {
    public virtual Visibility CustomizationVisible { get; set; } = Visibility.Hidden;
    public static PlateCustomizationViewModel Instance { get; private set; }

    protected PlateCustomizationViewModel()
    {
      Instance = this;
    }

    public static PlateCustomizationViewModel Create()
    {
      return ViewModelSource.Create(() => new PlateCustomizationViewModel());
    }

    public void HideView()
    {
      UserInputHandler.InputSanityCheck();
      CustomizationVisible = Visibility.Hidden;
    }
  }
}
