using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class MaintenanceViewModel
  {
    private INavigationService NavigationService { get { return this.GetService<INavigationService>(); } }

    protected MaintenanceViewModel()
    {
        
    }

    public static MaintenanceViewModel Create()
    {
      return ViewModelSource.Create(() => new MaintenanceViewModel());
    }

    public void NavigateCalibration()
    {
      NavigationService.Navigate("CalibrationView", null, this);
    }

    public void NavigateChannels()
    {
      NavigationService.Navigate("ChannelsView", null, this);
    }
  }
}