using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using DIOS.Core;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class MaintenanceViewModel
  {
    public virtual ObservableCollection<string> SanitizeSecondsContent { get; set; }
    public virtual ObservableCollection<bool> TouchModeEnabled { get; set; }

    public virtual ObservableCollection<DropDownButtonContents> LanguageItems { get; set; }
    public virtual string SelectedLanguage { get; set; }

    private INavigationService NavigationService => this.GetService<INavigationService>();
    private int _lastActiveTab = 0;

    public static MaintenanceViewModel Instance { get; private set; }

    protected MaintenanceViewModel()
    {
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
        App.Device.SetHardwareParameter(DeviceParameterType.UVCSanitize, res);
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
          NavigateNormalization();
          break;
        case 3:
          NavigateChannels();
          break;
      }
    }

    public void NavigateCalibration()
    {
      App.HideNumpad();
      MainViewModel.Instance.HintHide();
      if (VerificationViewModel.Instance != null)
        VerificationViewModel.Instance.isActivePage = false;
      NavigationService.Navigate("CalibrationView", null, this);
      App.InitSTab("calibtab");
      _lastActiveTab = 0;
    }

    public void NavigateVerification()
    {
      App.HideNumpad();
      MainViewModel.Instance.HintHide();
      if (VerificationViewModel.Instance != null)
        VerificationViewModel.Instance.isActivePage = true;
      NavigationService.Navigate("VerificationView", null, this);
      _lastActiveTab = 1;
    }

    public void NavigateNormalization()
    {
      App.HideNumpad();
      MainViewModel.Instance.HintHide();
      if (VerificationViewModel.Instance != null)
        VerificationViewModel.Instance.isActivePage = false;
      NavigationService.Navigate("NormalizationView", null, this);
      _lastActiveTab = 2;
    }

    public void NavigateChannels()
    {
      App.HideNumpad();
      MainViewModel.Instance.HintHide();
      if (VerificationViewModel.Instance != null)
        VerificationViewModel.Instance.isActivePage = false;
      NavigationService.Navigate("ChannelsView", null, this);
      App.InitSTab("channeltab");
      _lastActiveTab = 3;
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
      NavigateNormalization();
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