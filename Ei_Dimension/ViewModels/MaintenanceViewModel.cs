using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class MaintenanceViewModel
  {
    public virtual bool LEDsToggleButtonState { get; set; }
    public virtual bool LEDsEnabled { get; set; }
    public virtual object LEDSliderValue { get; set; }
    public virtual ObservableCollection<string> SanitizeSecondsContent { get; set; }

    public virtual ObservableCollection<DropDownButtonContents> LanguageItems { get; set; }
    public virtual string SelectedLanguage { get; set; }

    private INavigationService NavigationService => this.GetService<INavigationService>();

    public static MaintenanceViewModel Instance { get; private set; }

    protected MaintenanceViewModel()
    {
      LEDSliderValue = 0;
      LEDsEnabled = true;
      LEDsToggleButtonState = false;
      SanitizeSecondsContent = new ObservableCollection<string> { "" };
      LanguageItems = new ObservableCollection<DropDownButtonContents>();
      foreach(var lang in Language.Supported.Languages)
      {
        LanguageItems.Add(new DropDownButtonContents(lang.Item1, lang.Item2, this));
      }
      SelectedLanguage = LanguageItems[0].Content;
      Instance = this;
    }

    public static MaintenanceViewModel Create()
    {
      return ViewModelSource.Create(() => new MaintenanceViewModel());
    }

    public void LEDsButtonClick()
    {
      if(LEDsEnabled)
        LEDsToggleButtonState = !LEDsToggleButtonState;
    }

    public void LEDSliderValueChanged()
    {
      if (LEDsEnabled)
      {

      }
    }

    public void UVCSanitizeClick()
    {
      if (int.TryParse(SanitizeSecondsContent[0], out int res))
        App.Device.MainCommand("Set Property", code: 0x1f, parameter: (ushort)res);
    }

    public void NavigateCalibration()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      NavigationService.Navigate("CalibrationView", null, this);
      App.Device.InitSTab("calibtab");
    }

    public void NavigateChannels()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      NavigationService.Navigate("ChannelsView", null, this);
      App.Device.InitSTab("channeltab");
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SanitizeSecondsContent)), this, 0);
          break;
      }
    }

    public class DropDownButtonContents
    {
      public string Content { get; set; }
      private string _locale;
      private static MaintenanceViewModel _vm;
      public DropDownButtonContents(string content, string locale, MaintenanceViewModel vm = null)
      {
        if (_vm == null)
        {
          _vm = vm;
        }
        Content = content;
        _locale = locale;
      }

      public void Click()
      {
        _vm.SelectedLanguage = Content;
        App.SetLanguage(_locale);
      }
    }
  }
}