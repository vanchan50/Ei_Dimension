﻿using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
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

    public virtual ObservableCollection<DropDownButtonContents> LanguageItems { get; set; }
    public virtual string SelectedLanguage { get; set; }

    private INavigationService NavigationService => this.GetService<INavigationService>();

    public static MaintenanceViewModel Instance { get; private set; }

    protected MaintenanceViewModel()
    {
      LEDSliderValue = 1170;
      LEDsEnabled = true;
      LEDsToggleButtonState = false;
      SanitizeSecondsContent = new ObservableCollection<string> { "" };
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
      MainViewModel.Instance.TouchControlsEnabled = !MainViewModel.Instance.TouchControlsEnabled;
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
          MainViewModel.Instance.NumpadToggleButton(Views.MaintenanceView.Instance.SecsTB);
          break;
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      App.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }

    public void InitChildren()
    {
      NavigateCalibration();
      NavigateChannels();
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
        App.SetLanguage(Locale);
        Settings.Default.Language = Index;
        Settings.Default.Save();
      }
    }
  }
}