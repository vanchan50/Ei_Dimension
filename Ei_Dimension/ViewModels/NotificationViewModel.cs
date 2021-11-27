using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class NotificationViewModel
  {
    public static NotificationViewModel Instance { get; private set; }
    public virtual ObservableCollection<string> Text { get; set; }
    public virtual System.Windows.Visibility NotificationVisible { get; set; }
    protected NotificationViewModel()
    {
      Text = new ObservableCollection<string>{null};
      NotificationVisible = System.Windows.Visibility.Hidden;
      Instance = this;
    }

    public static NotificationViewModel Create()
    {
      return ViewModelSource.Create(() => new NotificationViewModel());
    }

    public void Close()
    {
      Text[0] = null;
      NotificationVisible = System.Windows.Visibility.Hidden;
    }
  }
}