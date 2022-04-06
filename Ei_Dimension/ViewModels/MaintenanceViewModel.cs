using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class MaintenanceViewModel
  {
    public virtual bool LEDsToggleButtonState { get; set; }
    public virtual bool LEDsEnabled { get; set; }
    public virtual object LEDSliderValue { get; set; }
    public virtual ObservableCollection<string> SanitizeSecondsContent { get; set; }
    public virtual ObservableCollection<bool> TouchModeEnabled { get; set; }

    public virtual ObservableCollection<DropDownButtonContents> LanguageItems { get; set; }
    public virtual string SelectedLanguage { get; set; }

    private INavigationService NavigationService => this.GetService<INavigationService>();
    private int _lastActiveTab = 0;

    public static MaintenanceViewModel Instance { get; private set; }

    protected MaintenanceViewModel()
    {
      LEDSliderValue = 1170;
      LEDsEnabled = true;
      LEDsToggleButtonState = false;
      SanitizeSecondsContent = new ObservableCollection<string> { "" };
      TouchModeEnabled = new ObservableCollection<bool> { Settings.Default.TouchMode };
      LanguageItems = new ObservableCollection<DropDownButtonContents>();
      foreach(var lang in Language.Supported.Languages)
      {
        LanguageItems.Add(new DropDownButtonContents(lang.Item1, lang.Item2, this));
      }
      SelectedLanguage = LanguageItems[Settings.Default.Language].Content;
      Instance = this;
    }

    public static MaintenanceViewModel Create()
    {
      return ViewModelSource.Create(() => new MaintenanceViewModel());
    }

    public void LEDsButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      if (LEDsEnabled)
      {
        LEDsToggleButtonState = !LEDsToggleButtonState;
        var param = LEDsToggleButtonState ? 1 : 0;
        App.Device.MainCommand("Set Property", code: 0x17, parameter: (ushort)param);
        if (!LEDsToggleButtonState)
        {
          LEDSliderValue = 1170.0;
          App.Device.MainCommand("Set Property", code: 0x97, parameter: 1170);
        }
      }
    }

    public void LEDSliderValueChanged()
    {
      if (LEDsEnabled)
      {
        App.Device.MainCommand("Set Property", code: 0x97, parameter: (ushort)(double)LEDSliderValue);
        App.Device.MainCommand("RefreshDac");
      }
    }

    public void TouchModeToggle()
    {
      UserInputHandler.InputSanityCheck();
      MainViewModel.Instance.TouchControlsEnabled = !MainViewModel.Instance.TouchControlsEnabled;
      Settings.Default.TouchMode = TouchModeEnabled[0];
      Settings.Default.Save();
    }

    public void UVCSanitizeClick()
    {
      UserInputHandler.InputSanityCheck();
      if (int.TryParse(SanitizeSecondsContent[0], out int res))
        App.Device.MainCommand("Set Property", code: 0x1f, parameter: (ushort)res);
    }

    public void NavigateTab()
    {
      switch (_lastActiveTab)
      {
        case 0:
          NavigateCalibration();
          break;
        case 1:
          NavigateVerification();
          break;
        case 2:
          NavigateChannels();
          break;
      }
    }

    public void NavigateCalibration()
    {
      App.HideNumpad();
      MainViewModel.Instance.HideHint();
      if (VerificationViewModel.Instance != null)
        VerificationViewModel.Instance.isActivePage = false;
      NavigationService.Navigate("CalibrationView", null, this);
      App.InitSTab("calibtab");
      _lastActiveTab = 0;
    }

    public void NavigateVerification()
    {
      App.HideNumpad();
      MainViewModel.Instance.HideHint();
      if (VerificationViewModel.Instance != null)
        VerificationViewModel.Instance.isActivePage = true;
      NavigationService.Navigate("VerificationView", null, this);
      _lastActiveTab = 1;
    }

    public void NavigateChannels()
    {
      App.HideNumpad();
      MainViewModel.Instance.HideHint();
      if (VerificationViewModel.Instance != null)
        VerificationViewModel.Instance.isActivePage = false;
      NavigationService.Navigate("ChannelsView", null, this);
      App.InitSTab("channeltab");
      _lastActiveTab = 2;
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SanitizeSecondsContent)), this, 0, Views.MaintenanceView.Instance.SecsTB);
          MainViewModel.Instance.NumpadToggleButton(Views.MaintenanceView.Instance.SecsTB);
          break;
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }

    public void InitChildren()
    {
      NavigateCalibration();
      NavigateVerification();
      NavigateChannels();
      _lastActiveTab = 0;
    }

    public class DropDownButtonContents
    {
      public string Content { get; set; }
      public string Locale { get; }
      private static MaintenanceViewModel _vm;
      public byte Index { get; set; }
      private static byte _nextIndex = 0;
      public DropDownButtonContents(string content, string locale, MaintenanceViewModel vm = null)
      {
        if (_vm == null)
        {
          _vm = vm;
        }
        Content = content;
        Locale = locale;
        Index = _nextIndex++;
      }

      public void Click()
      {
        _vm.SelectedLanguage = Content;
        LanguageSwap.SetLanguage(Locale);
        Settings.Default.Language = Index;
        Settings.Default.Save();
      }
    }
  }
}