using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class SettingsViewModel
  {
    protected SettingsViewModel()
    {
    }

    public static SettingsViewModel Create()
    {
      return ViewModelSource.Create(() => new SettingsViewModel());
    }
  }
}