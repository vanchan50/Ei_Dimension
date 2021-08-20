using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class MaintenanceViewModel
  {
    public virtual bool LEDsToggleButtonState { get; set; }
    public virtual object LEDSliderValue { get; set; }
    public virtual string SanitizeSecondsContent { get; set; }


    private INavigationService NavigationService { get { return this.GetService<INavigationService>(); } }

    protected MaintenanceViewModel()
    {
      LEDSliderValue = 0;
      LEDsToggleButtonState = false;
    }

    public static MaintenanceViewModel Create()
    {
      return ViewModelSource.Create(() => new MaintenanceViewModel());
    }

    public void LEDsButtonClick()
    {
      LEDsToggleButtonState = !LEDsToggleButtonState;
    }

    public void LEDSliderValueChanged()
    {
      SanitizeSecondsContent = LEDSliderValue.ToString();
    }

    public void UVCSanitizeClick()
    {

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