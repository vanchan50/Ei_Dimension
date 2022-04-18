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
    public virtual ObservableCollection<string> ActionButtonText { get; set; }
    public virtual ObservableCollection<string> Text { get; set; }
    public virtual System.Windows.Visibility NotificationVisible { get; set; }
    public virtual ObservableCollection<System.Windows.Visibility> ButtonVisible { get; set; }
    public virtual System.Windows.Media.Brush Background { get; set; }
    public virtual double FontSize { get; set; }
    public Action Action1 { set { _action1 = value; } }
    public Action Action2 { set { _action2 = value; } }
    public static System.Windows.Media.Brush DefaultBackground { get; } = (System.Windows.Media.Brush)App.Current.Resources["RibbonBackgroundActive"];
    private Action _action1;
    private Action _action2;
    protected NotificationViewModel()
    {
      Text = new ObservableCollection<string>{ null };
      ActionButtonText = new ObservableCollection<string> { null, null };
      ButtonVisible = new ObservableCollection<System.Windows.Visibility> { System.Windows.Visibility.Hidden, System.Windows.Visibility.Hidden, System.Windows.Visibility.Visible };
      NotificationVisible = System.Windows.Visibility.Hidden;
      Background = DefaultBackground;
      Instance = this;
    }

    public static NotificationViewModel Create()
    {
      return ViewModelSource.Create(() => new NotificationViewModel());
    }

    public void Close()
    {
      Text[0] = null;
      _action1 = null;
      _action2 = null;
      NotificationVisible = System.Windows.Visibility.Hidden;
      Background = DefaultBackground;
      ButtonVisible[0] = System.Windows.Visibility.Hidden;
      ButtonVisible[1] = System.Windows.Visibility.Hidden;
      ButtonVisible[2] = System.Windows.Visibility.Visible;
    }

    public void PerformAction1()
    {
      _action1?.Invoke();
      Close();
    }

    public void PerformAction2()
    {
      _action2?.Invoke();
      Close();
    }
  }
}