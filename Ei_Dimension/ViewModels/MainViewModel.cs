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
    public static string AppVersion { get; } = "Application Version: 0.8.1";
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
      MainSelectorState = new ObservableCollection<bool> { false, false, false, false };
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
      MainSelectorState[num] = true;
      App.HideNumpad();
      App.HideKeyboard();
      switch (num)
      {
        case 0:
          NavigateExperiment();
          break;
        case 1:
          NavigateResults();
          break;
        case 2:
          NavigateMaintenance();
          break;
        case 3:
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
      if (TouchControlsEnabled)
      {
        tb.CaretBrush = System.Windows.Media.Brushes.Transparent;
        var p = tb.PointToScreen(MainWindow.Instance.wndw.PointFromScreen(new System.Windows.Point(0, 0)));
        double shiftX;
        double shiftY;
        double NpdHeight = 340;
        if (p.X > 100)
          shiftX = 100;
        else if (p.X > 50)
          shiftX = 50;
        else
          shiftX = 0;

        if (MainWindow.Instance.wndw.Height - p.Y > NpdHeight + tb.Height + 5)
          shiftY = tb.Height + 5;
        else
          shiftY = - NpdHeight - 5;

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
      if (TouchControlsEnabled)
      {
        tb.CaretBrush = System.Windows.Media.Brushes.Transparent;
        var p = tb.PointToScreen(MainWindow.Instance.wndw.PointFromScreen(new System.Windows.Point(0, 0)));
        double shiftX;
        double shiftY;
        double KbdHeight = 410;
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
        App.InputSanityCheck();
      }
    }
  }
}