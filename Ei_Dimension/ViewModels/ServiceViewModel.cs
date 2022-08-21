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
    private int _lastActiveTab = 0;

    protected ServiceViewModel()
    {
    }

    public static ServiceViewModel Create()
    {
      return ViewModelSource.Create(() => new ServiceViewModel());
    }

    public void NavigateTab()
    {
      switch (_lastActiveTab)
      {
        case 0:
          NavigateMotors();
          break;
        case 1:
          NavigateComponents();
          break;
        case 2:
          NavigateAlignment();
          break;
        case 3:
          NavigateChannelOffset();
          break;
        case 4:
          NavigateSyringeSpeeds();
          break;
      }
    }

    public void NavigateMotors()
    {
      App.HideNumpad();
      MainViewModel.Instance.HintHide();
      NavigationService.Navigate("MotorsView", null, this);
      App.InitSTab("motorstab");
      _lastActiveTab = 0;
    }

    public void NavigateComponents()
    {
      App.HideNumpad();
      MainViewModel.Instance.HintHide();
      NavigationService.Navigate("ComponentsView", null, this);
      App.InitSTab("componentstab");
      _lastActiveTab = 1;
    }

    public void NavigateAlignment()
    {
      App.HideNumpad();
      MainViewModel.Instance.HintHide();
      NavigationService.Navigate("AlignmentView", null, this);
      _lastActiveTab = 2;
    }

    public void NavigateChannelOffset()
    {
      App.HideNumpad();
      MainViewModel.Instance.HintHide();
      NavigationService.Navigate("ChannelOffsetView", null, this);
      App.InitSTab("channeltab");
      _lastActiveTab = 3;
    }

    public void NavigateSyringeSpeeds()
    {
      App.HideNumpad();
      MainViewModel.Instance.HintHide();
      NavigationService.Navigate("SyringeSpeedsView", null, this);
      App.InitSTab("calibtab");
      _lastActiveTab = 4;
    }

    public void InitChildren()
    {
      NavigateMotors();
      NavigateComponents();
      NavigateAlignment();
      NavigateChannelOffset();
      NavigateSyringeSpeeds();
      _lastActiveTab = 0;
    }

    public void SaveAllClick()
    {
      if(!IsSystemIdle())
        return;
      App.Device.MainCommand("SaveToFlash");
      Notification.Show("Flash saved");
    }

    public void RestoreClick()
    {
      if (!IsSystemIdle())
        return;
      App.Device.MainCommand("InitOpVars");
      Notification.Show("Flash Restored");
    }

    public void RestoreDefaultsClick()
    {
      if (!IsSystemIdle())
        return;
      App.Device.MainCommand("InitOpVars", cmd: 1);
      Notification.Show("Flash Restored to Factory Defaults");
    }

    private bool IsSystemIdle()
    {
      for (var i = 0; i < App.Device.SystemActivity.Length; i++)
      {
        if (App.Device.SystemActivity[i]) //if any activity is going on - dismiss
        {
          Notification.ShowError("Please wait, until the system is Idle\nand try again");
          return false;
        }
      }
      return true;
    }
  }
}