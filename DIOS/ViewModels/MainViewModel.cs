﻿using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class MainViewModel
  {
    public static string AppVersion { get; set; } = $"Application Version: {App.DiosApp.BUILD}\n   Firmware Version: ";
    public ObservableCollection<string> TotalBeadsInFirmware { get; set; } = new ObservableCollection<string> { "0" };
    public virtual ObservableCollection<bool> MainSelectorState { get; set; }
    public virtual Visibility NumpadVisible { get; set; }
    public virtual Visibility KeyboardVisible { get; set; }
    public virtual ObservableCollection<string> EventCountField { get; set; }
    public virtual ObservableCollection<string> EventCountCurrent { get; set; }
    public virtual ObservableCollection<string> EventCountLocal { get; set; }
    public virtual ObservableCollection<string> NormalizationMarkerText { get; set; }
    public virtual ObservableCollection<string> ScalingMarkerText { get; set; }
    public virtual Brush NormalizationMarkerColor { get; set; }
    public virtual Brush ScalingMarkerColor { get; set; }
    public virtual Visibility EventCountVisible { get; set; }
    public virtual Visibility StartButtonsVisible { get; set; }
    public virtual Visibility ServiceVisibility { get; set; }
    public int ServiceVisibilityCheck { get; set; }
    public bool TouchControlsEnabled { get; set; }
    public virtual int BeadConcentrationMonitorValue { get; protected set; } = 130;
    public virtual Brush BeadConcentrationMonitorColor { get; protected set; } = _RegularBrush;

    private static Brush _activeBrush = Brushes.Green;
    private static Brush _inactiveBrush = Brushes.Gray;
    private static Brush _RegularBrush = (Brush)App.Current.Resources["MenuButtonBackgroundActive"];
    private static Brush _WarningBrush = Brushes.DarkRed;

    public static MainViewModel Instance { get; private set; }
    private INavigationService NavigationService => this.GetService<INavigationService>();

    protected MainViewModel()
    {
      MainSelectorState = new ObservableCollection<bool> { false, false, false, false, false };
      App.NumpadShow = (this.GetType().GetProperty(nameof(NumpadVisible)), this);
      App.KeyboardShow = (this.GetType().GetProperty(nameof(KeyboardVisible)), this);
      NumpadVisible = Visibility.Hidden;
      KeyboardVisible = Visibility.Hidden;
      EventCountVisible = Visibility.Visible;
      StartButtonsVisible = Visibility.Visible;
      ServiceVisibility = Visibility.Hidden;
      EventCountCurrent = new ObservableCollection<string> { "0" };
      EventCountLocal = new ObservableCollection<string> { "0" };
      NormalizationMarkerText = new ObservableCollection<string> { "Normalization is ON" };
      ScalingMarkerText = new ObservableCollection<string> { "Scaling is 1.0" };
      NormalizationMarkerColor = _activeBrush;
      ScalingMarkerColor = _inactiveBrush;
      EventCountField = EventCountCurrent;
      Instance = this;
      ServiceVisibilityCheck = 0;
      TouchControlsEnabled = Settings.Default.TouchMode;
    }

    public static MainViewModel Create()
    {
      return ViewModelSource.Create(() => new MainViewModel());
    }

    public void NavigationSelector(byte num)
    {
      if (MainSelectorState[num])
        return;
      MainSelectorState[0] = false;
      MainSelectorState[1] = false;
      MainSelectorState[2] = false;
      MainSelectorState[3] = false;
      MainSelectorState[4] = false;
      MainSelectorState[num] = true;
      App.HideNumpad();
      App.HideKeyboard();
      HintHide();
      if (VerificationViewModel.Instance != null)
        VerificationViewModel.Instance.isActivePage = false;
      switch (num)
      {
        case 0:
          NavigateExperiment();
          break;
        case 1:
          ResultsViewModel.Instance.ShowResults();
          NavigateResults();
          break;
        case 2:
          ResultsViewModel.Instance.ShowAnalysis();
          NavigateResults();
          break;
        case 3:
          NavigateMaintenance();
          break;
        case 4:
          NavigateSettings();
          break;
      }
    }

    private void NavigateExperiment()
    {
      EventCountVisible = Visibility.Visible;
      StartButtonsVisible = Visibility.Visible;
      NavigationService.Navigate("ExperimentView", null, this);
      App.InitSTab("readertab");
    }

    private void NavigateResults()
    {
      EventCountVisible = Visibility.Visible;
      StartButtonsVisible = Visibility.Visible;
      NavigationService.Navigate("ResultsView", null, this);
    }

    private void NavigateMaintenance()
    {
      StartButtonsVisible = Visibility.Hidden;
      EventCountVisible = Visibility.Hidden;
      NavigationService.Navigate("MaintenanceView", null, this);
    }

    private void NavigateSettings()
    {
      StartButtonsVisible = Visibility.Hidden;
      EventCountVisible = Visibility.Hidden;
      NavigationService.Navigate("ServiceView", null, this);
    }

    public void SetScalingMarker(float scale)
    {
      if (scale >= 0.99999 && scale <= 1.00001)
      {
        ScalingMarkerText[0] = "Scaling is 1.0";
        ScalingMarkerColor = _inactiveBrush;
        return;
      }
      ScalingMarkerText[0] = $"Scaling is {scale:F2}";
      ScalingMarkerColor = _activeBrush;
    }

    public void SetNormalizationMarker(bool state)
    {
      if (state)
      {
        NormalizationMarkerText[0] = "Normalization ON";
        NormalizationMarkerColor = _activeBrush;
        return;
      }
      NormalizationMarkerText[0] = "Normalization OFF";
      NormalizationMarkerColor = _inactiveBrush;
    }

    public void SetBeadConcentrationMonitorValue(int value)
    {
      Debug.Assert(value >= 0 && value <= 255);

      BeadConcentrationMonitorValue = value;
      if (value >= 250)
      {
        BeadConcentrationMonitorColor = _WarningBrush;
        return;
      }
      BeadConcentrationMonitorColor = _RegularBrush;
    }

    public void NumpadToggleButton(System.Windows.Controls.TextBox tb)
    {
      HintHide();
      if (TouchControlsEnabled)
      {
        tb.CaretBrush = Brushes.Transparent;
        var p = tb.PointToScreen(MainWindow.Instance.wndw.PointFromScreen(new Point(0, 0)));
        double shiftX;
        double shiftY;
        double NpdHeight = 390;
        if (p.X > 100)
          shiftX = 100;
        else if (p.X > 50)
          shiftX = 50;
        else
          shiftX = 0;

        if (MainWindow.Instance.wndw.Height - p.Y > NpdHeight + tb.Height + 5)
          shiftY = tb.Height + 5;
        else
          shiftY = -NpdHeight - 5;

        MainWindow.Instance.Npd.Margin = new Thickness(p.X - shiftX, p.Y + shiftY, 0, 0);
        NumpadVisible = Visibility.Visible;
      }
      else
      {
        tb.CaretBrush = (Brush)App.Current.Resources["AppTextColor"];
        tb.SelectAll();
      }
    }

    public void KeyboardToggle(System.Windows.Controls.TextBox tb)
    {
      HintHide();
      if (TouchControlsEnabled)
      {
        tb.CaretBrush = Brushes.Transparent;
        var p = tb.PointToScreen(MainWindow.Instance.wndw.PointFromScreen(new Point(0, 0)));
        double shiftX;
        double shiftY;
        double KbdHeight = 460;
        if (p.X > 300)
          shiftX = 300;
        else if (p.X > 250)
          shiftX = 250;
        else if (p.X > 200)
          shiftX = 200;
        else if (p.X > 150)
          shiftX = 150;
        else if (p.X > 100)
          shiftX = 100;
        else if (p.X > 50)
          shiftX = 50;
        else
          shiftX = 0;

        if (MainWindow.Instance.wndw.Height - p.Y > KbdHeight + tb.Height + 5)
          shiftY = tb.Height + 5;
        else
          shiftY = -KbdHeight - 5;

        MainWindow.Instance.Kbd.Margin = new Thickness(p.X - shiftX, p.Y + shiftY, 0, 0);
        KeyboardVisible = Visibility.Visible;
      }
      else
      {
        tb.CaretBrush = (Brush)App.Current.Resources["AppTextColor"];
        tb.SelectAll();
      }
    }

    public void HintShow(string text, System.Windows.Controls.TextBox tb)
    {
      var p = tb.PointToScreen(MainWindow.Instance.wndw.PointFromScreen(new Point(0, 0)));
      double shiftX = 0;
      double shiftY = -30;

      if (tb.ActualWidth < 200)
      {
        HintViewModel.Instance.Width = tb.ActualWidth;
      }
      else
      {
        HintViewModel.Instance.Width = 200;
      }
      MainWindow.Instance.Hint.Margin = new Thickness(p.X - shiftX, p.Y + shiftY, 0, 0);
      HintViewModel.Instance.Text[0] = text;
      HintViewModel.Instance.HintVisible = Visibility.Visible;
    }

    public void HintHide()
    {
      HintViewModel.Instance.Text[0] = null;
      HintViewModel.Instance.HintVisible = Visibility.Hidden;
    }

    public void InitChildren()
    {
      NavigateExperiment();
      NavigateResults();
      NavigateMaintenance();
      NavigateSettings();
    }

    public void LogoClick()
    {
      ServiceVisibilityCheck++;
      if (ServiceVisibility == Visibility.Hidden && ServiceVisibilityCheck > 2)
      {
        ServiceVisibility = Visibility.Visible;
        ServiceVisibilityCheck = 0;
      }
      else if (ServiceVisibility == Visibility.Visible && ServiceVisibilityCheck > 2)
      {
        ServiceVisibility = Visibility.Hidden;
        ServiceVisibilityCheck = 0;
      }
    }

    public void KeyDown(System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Return)
      {
        App.HideNumpad();
      }
    }

    public void Minimize()
    {
      App.Current.MainWindow.WindowState = WindowState.Minimized;
    }
  }
}