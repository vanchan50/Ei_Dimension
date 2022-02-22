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
    private INavigationService NavigationService => this.GetService<INavigationService>();

    protected ServiceViewModel()
    {
    }

    public static ServiceViewModel Create()
    {
      return ViewModelSource.Create(() => new ServiceViewModel());
    }

    public void NavigateMotors()
    {
      App.HideNumpad();
      MainViewModel.Instance.HideHint();
      NavigationService.Navigate("MotorsView", null, this);
      App.InitSTab("motorstab");
    }

    public void NavigateComponents()
    {
      App.HideNumpad();
      MainViewModel.Instance.HideHint();
      NavigationService.Navigate("ComponentsView", null, this);
      App.InitSTab("componentstab");
    }

    public void NavigateAlignment()
    {
      App.HideNumpad();
      MainViewModel.Instance.HideHint();
      NavigationService.Navigate("AlignmentView", null, this);
    }

    public void NavigateChannelOffset()
    {
      App.HideNumpad();
      MainViewModel.Instance.HideHint();
      NavigationService.Navigate("ChannelOffsetView", null, this);
      App.InitSTab("channeltab");
    }

    public void NavigateSyringeSpeeds()
    {
      App.HideNumpad();
      MainViewModel.Instance.HideHint();
      NavigationService.Navigate("SyringeSpeedsView", null, this);
      App.InitSTab("calibtab");
    }

    public void InitChildren()
    {
      NavigateMotors();
      NavigateComponents();
      NavigateAlignment();
      NavigateChannelOffset();
      NavigateSyringeSpeeds();
    }

    public void SaveAllClick()
    {
      App.Device.MainCommand("SaveToFlash");
    }

    public void RestoreClick()
    {
      App.Device.MainCommand("InitOpVars");
    }

    public void RestoreDefaultsClick()
    {
      App.Device.MainCommand("InitOpVars", cmd: 1);
    }
  }
}