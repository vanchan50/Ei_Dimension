using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Map;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class DashboardViewModel
  {
    public virtual ObservableCollection<string> PressureMon { get; set; }
    public virtual bool PressureMonToggleButtonState { get; set; }
    public double MaxPressure { get; set; }
    public double MinPressure { get; set; }
    public virtual ObservableCollection<string> ActiveList { get; set; }

    public static DashboardViewModel Instance { get; private set; }

    protected DashboardViewModel()
    {
      PressureMonToggleButtonState = false;
      PressureMon = new ObservableCollection<string> {"","",""};
      ActiveList = new ObservableCollection<string>();
      Instance = this;
    }

    public static DashboardViewModel Create()
    {
      return ViewModelSource.Create(() => new DashboardViewModel());
    }

    public void PressureMonToggleButtonClick()
    {
      PressureMonToggleButtonState = !PressureMonToggleButtonState;
      MaxPressure = 0;
      MinPressure = 9999999999;
    }
  }
}