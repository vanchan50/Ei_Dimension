using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ComponentsViewModel
  {
    public virtual ObservableCollection<string> InputSelectorState { get; set; }
    protected ComponentsViewModel()
    {
      InputSelectorState = new ObservableCollection<string>();
      InputSelectorState.Add("To Pickup");
      InputSelectorState.Add("To Cuvet");
    }

    public static ComponentsViewModel Create()
    {
      return ViewModelSource.Create(() => new ComponentsViewModel());
    }

    public void InputSelectorSwapButtonClick()
    {
      string temp = InputSelectorState[0];
      InputSelectorState[0] = InputSelectorState[1];
      InputSelectorState[1] = temp;
    }
  }
}