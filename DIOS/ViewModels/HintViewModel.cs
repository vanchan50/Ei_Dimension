using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels;

[POCOViewModel]
public class HintViewModel
{
  public static HintViewModel Instance { get; private set; }
  public virtual ObservableCollection<string> Text { get; set; }
  public virtual System.Windows.Visibility HintVisible { get; set; }
  public virtual double Width { get; set; }
  protected HintViewModel()
  {
    Text = new ObservableCollection<string> {""};
    Width = 200;
    HintVisible = System.Windows.Visibility.Hidden;
    Instance = this;
  }

  public static HintViewModel Create()
  {
    return ViewModelSource.Create(() => new HintViewModel());
  }
}