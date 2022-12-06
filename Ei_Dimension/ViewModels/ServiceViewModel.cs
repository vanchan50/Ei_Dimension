using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Core;

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
      if(IsSystemBusy() || IsMeasurementGoing())
        return;
      App.Device.SendHardwareCommand(DeviceCommandType.FlashSave);
      Notification.ShowLocalizedError(nameof(Language.Resources.Notification_FlashSaved));
    }

    public void RestoreClick()
    {
      if (IsSystemBusy() || IsMeasurementGoing())
        return;
      App.Device.SendHardwareCommand(DeviceCommandType.FlashRestore);
      Notification.ShowLocalizedError(nameof(Language.Resources.Notification_FlashRestored));
    }

    public void RestoreDefaultsClick()
    {
      if (IsSystemBusy() || IsMeasurementGoing())
        return;
      App.Device.SendHardwareCommand(DeviceCommandType.FlashFactoryReset);
      Notification.Show("Flash Restored to Factory Defaults");
      Notification.ShowLocalizedError(nameof(Language.Resources.Notification_FlashRestoredToDefaults));
    }

    private bool IsSystemBusy()
    {
      for (var i = 0; i < App.Device.SystemActivity.Length; i++)
      {
        if (App.Device.SystemActivity[i]) //if any activity is going on - dismiss
        {
          Notification.ShowLocalizedError(nameof(Language.Resources.Notification_WaitSystemIdle));
          return true;
        }
      }
      return false;
    }

    private bool IsMeasurementGoing()
    {
      if (App.Device.IsMeasurementGoing) //if any activity is going on - dismiss
      {
        Notification.ShowError("Please wait, until the Measurement is complete\nand try again");
        return true;
      }
      return false;
    }
  }
}