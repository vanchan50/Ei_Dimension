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
      App.ResetFocusedTextbox();
      App.HideNumpad();
      NavigationService.Navigate("MotorsView", null, this);
      App.Device.InitSTab("motorstab");
    }

    public void NavigateComponents()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      NavigationService.Navigate("ComponentsView", null, this);
      App.Device.InitSTab("componentstab");
    }

    public void NavigateAlignment()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      NavigationService.Navigate("AlignmentView", null, this);
    }

    public void NavigateChannelOffset()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      NavigationService.Navigate("ChannelOffsetView", null, this);
      App.Device.InitSTab("channeltab");
    }

    public void NavigateSyringeSpeeds()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      NavigationService.Navigate("SyringeSpeedsView", null, this);
      App.Device.InitSTab("calibtab");
    }
    public void InitChildren()
    {
      NavigateMotors();
      NavigateComponents();
      NavigateAlignment();
      NavigateChannelOffset();
      NavigateSyringeSpeeds();
    }
  }
}