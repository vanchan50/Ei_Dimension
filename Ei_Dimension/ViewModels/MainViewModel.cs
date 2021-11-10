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
    public virtual System.Windows.Visibility NumpadVisible { get; set; }
    public virtual ObservableCollection<string> EventCountField { get; set; }
    public virtual ObservableCollection<string> EventCountCurrent { get; set; }
    public virtual ObservableCollection<string> EventCountLocal { get; set; }
    public virtual System.Windows.Visibility EventCountVisible { get; set; }
    public virtual System.Windows.Visibility StartButtonsVisible { get; set; }
    public virtual System.Windows.Visibility ServiceVisibility { get; set; }
    public int ServiceVisibilityCheck { get; set; }

    public static MainViewModel Instance { get; private set; }
    private INavigationService NavigationService => this.GetService<INavigationService>();

    protected MainViewModel()
    {
      App.NumpadShow = (this.GetType().GetProperty(nameof(NumpadVisible)), this);
      NumpadVisible = System.Windows.Visibility.Hidden;
      EventCountVisible = System.Windows.Visibility.Visible;
      StartButtonsVisible = System.Windows.Visibility.Visible;
      ServiceVisibility = System.Windows.Visibility.Hidden;
      EventCountCurrent = new ObservableCollection<string> { "0" };
      EventCountLocal = new ObservableCollection<string> { "0" };
      EventCountField = EventCountCurrent;
      Instance = this;
      ServiceVisibilityCheck = 0;
    }

    public static MainViewModel Create()
    {
      return ViewModelSource.Create(() => new MainViewModel());
    }

    public void NavigateExperiment()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      EventCountVisible = System.Windows.Visibility.Visible;
      StartButtonsVisible = System.Windows.Visibility.Visible;
      NavigationService.Navigate("ExperimentView", null, this);
      App.Device.InitSTab("readertab");
    }

    public void NavigateResults()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      EventCountVisible = System.Windows.Visibility.Visible;
      StartButtonsVisible = System.Windows.Visibility.Visible;
      NavigationService.Navigate("ResultsView", null, this);
    }

    public void NavigateMaintenance()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      StartButtonsVisible = System.Windows.Visibility.Hidden;
      EventCountVisible = System.Windows.Visibility.Hidden;
      NavigationService.Navigate("MaintenanceView", null, this);
    }

    public void NavigateSettings()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      StartButtonsVisible = System.Windows.Visibility.Hidden;
      EventCountVisible = System.Windows.Visibility.Hidden;
      NavigationService.Navigate("ServiceView", null, this);
    }

    public void NumpadToggleButon()
    {
      if (NumpadVisible == System.Windows.Visibility.Visible)
      {
        NumpadVisible = System.Windows.Visibility.Hidden;
      }
      else
      {
        NumpadVisible = System.Windows.Visibility.Visible;
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
  }
}