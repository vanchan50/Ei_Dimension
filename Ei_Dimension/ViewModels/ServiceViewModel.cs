using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ServiceViewModel
  {
    private INavigationService NavigationService { get { return this.GetService<INavigationService>(); } }
    protected ServiceViewModel()
    {
    }

    public static ServiceViewModel Create()
    {
      return ViewModelSource.Create(() => new ServiceViewModel());
    }

    public void NavigateMotors()
    {
      NavigationService.Navigate("MotorsView", null, this);
    }

    public void NavigateComponents()
    {
      NavigationService.Navigate("ComponentsView", null, this);
    }

    public void NavigateAlignment()
    {
      NavigationService.Navigate("AlignmentView", null, this);
    }

    public void NavigateChannelOffset()
    {
      NavigationService.Navigate("ChannelOffsetView", null, this);
    }

    public void NavigateSyringeSpeeds()
    {
      NavigationService.Navigate("SyringeSpeedsView", null, this);
    }
  }
}