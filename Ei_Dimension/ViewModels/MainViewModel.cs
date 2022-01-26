using DevExpress.Mvvm;
using System;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class MainViewModel
  {
    public static string AppVersion { get; } = "Application Version: 1.0.1";
#if DEBUG
    public ObservableCollection<string> TotalBeadsInFirmware { get; set; } = new ObservableCollection<string> { "0" };
#endif
    public virtual ObservableCollection<bool> MainSelectorState { get; set; }
    public virtual System.Windows.Visibility NumpadVisible { get; set; }
    public virtual System.Windows.Visibility KeyboardVisible { get; set; }
    public virtual ObservableCollection<string> EventCountField { get; set; }
    public virtual ObservableCollection<string> EventCountCurrent { get; set; }
    public virtual ObservableCollection<string> EventCountLocal { get; set; }
    public virtual System.Windows.Visibility EventCountVisible { get; set; }
    public virtual System.Windows.Visibility StartButtonsVisible { get; set; }
    public virtual System.Windows.Visibility ServiceVisibility { get; set; }
    public int ServiceVisibilityCheck { get; set; }
    public bool TouchControlsEnabled { get; set; }

    public static MainViewModel Instance { get; private set; }
    private INavigationService NavigationService => this.GetService<INavigationService>();

    protected MainViewModel()
    {
      MainSelectorState = new ObservableCollection<bool> { false, false, false, false, false };
      App.NumpadShow = (this.GetType().GetProperty(nameof(NumpadVisible)), this);
      App.KeyboardShow = (this.GetType().GetProperty(nameof(KeyboardVisible)), this);
      NumpadVisible = System.Windows.Visibility.Hidden;
      KeyboardVisible = System.Windows.Visibility.Hidden;
      EventCountVisible = System.Windows.Visibility.Visible;
      StartButtonsVisible = System.Windows.Visibility.Visible;
      ServiceVisibility = System.Windows.Visibility.Hidden;
      EventCountCurrent = new ObservableCollection<string> { "0" };
      EventCountLocal = new ObservableCollection<string> { "0" };
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
      HideHint();
      if(VerificationViewModel.Instance != null)
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
      EventCountVisible = System.Windows.Visibility.Visible;
      StartButtonsVisible = System.Windows.Visibility.Visible;
      NavigationService.Navigate("ExperimentView", null, this);
      App.Device.InitSTab("readertab");
    }

    private void NavigateResults()
    {
      EventCountVisible = System.Windows.Visibility.Visible;
      StartButtonsVisible = System.Windows.Visibility.Visible;
      NavigationService.Navigate("ResultsView", null, this);
    }

    private void NavigateMaintenance()
    {
      StartButtonsVisible = System.Windows.Visibility.Hidden;
      EventCountVisible = System.Windows.Visibility.Hidden;
      NavigationService.Navigate("MaintenanceView", null, this);
    }

    private void NavigateSettings()
    {
      StartButtonsVisible = System.Windows.Visibility.Hidden;
      EventCountVisible = System.Windows.Visibility.Hidden;
      NavigationService.Navigate("ServiceView", null, this);
    }

    public void NumpadToggleButton(System.Windows.Controls.TextBox tb)
    {
      HideHint();
      if (TouchControlsEnabled)
      {
        tb.CaretBrush = System.Windows.Media.Brushes.Transparent;
        var p = tb.PointToScreen(MainWindow.Instance.wndw.PointFromScreen(new System.Windows.Point(0, 0)));
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

        MainWindow.Instance.Npd.Margin = new System.Windows.Thickness(p.X - shiftX, p.Y + shiftY, 0, 0);
        NumpadVisible = System.Windows.Visibility.Visible;
      }
      else
      {
        tb.CaretBrush = (System.Windows.Media.Brush)App.Current.Resources["AppTextColor"];
        tb.SelectAll();
      }
    }

    public void KeyboardToggle(System.Windows.Controls.TextBox tb)
    {
      HideHint();
      if (TouchControlsEnabled)
      {
        tb.CaretBrush = System.Windows.Media.Brushes.Transparent;
        var p = tb.PointToScreen(MainWindow.Instance.wndw.PointFromScreen(new System.Windows.Point(0, 0)));
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

        MainWindow.Instance.Kbd.Margin = new System.Windows.Thickness(p.X - shiftX, p.Y + shiftY, 0, 0);
        KeyboardVisible = System.Windows.Visibility.Visible;
      }
      else
      {
        tb.CaretBrush = (System.Windows.Media.Brush)App.Current.Resources["AppTextColor"];
        tb.SelectAll();
      }
    }

    public void HintToggle(string text, System.Windows.Controls.TextBox tb)
    {
      var p = tb.PointToScreen(MainWindow.Instance.wndw.PointFromScreen(new System.Windows.Point(0, 0)));
      double shiftX = 0;
      double shiftY = -30;

      HintViewModel.Instance.Width = tb.ActualWidth;
      MainWindow.Instance.Hint.Margin = new System.Windows.Thickness(p.X - shiftX, p.Y + shiftY, 0, 0);
      HintViewModel.Instance.Text[0] = text;
      HintViewModel.Instance.HintVisible = System.Windows.Visibility.Visible;
    }

    public void HideHint()
    {
      HintViewModel.Instance.Text[0] = null;
      HintViewModel.Instance.HintVisible = System.Windows.Visibility.Hidden;
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
      if (ServiceVisibility == System.Windows.Visibility.Hidden && ServiceVisibilityCheck > 2)
      {
        ServiceVisibility = System.Windows.Visibility.Visible;
        ServiceVisibilityCheck = 0;
      }
      else if (ServiceVisibility == System.Windows.Visibility.Visible && ServiceVisibilityCheck > 2)
      {
        ServiceVisibility = System.Windows.Visibility.Hidden;
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
  }
}