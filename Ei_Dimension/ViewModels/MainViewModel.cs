using DevExpress.Mvvm;
using System;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class MainViewModel
  {
    public virtual System.Windows.Visibility NumpadVisible { get; set; }
    private INavigationService NavigationService => this.GetService<INavigationService>();

    protected MainViewModel()
    {
      App.NumpadShow = (this.GetType().GetProperty(nameof(NumpadVisible)), this);
      NumpadVisible = System.Windows.Visibility.Hidden;
    }

    public static MainViewModel Create()
    {
      return ViewModelSource.Create(() => new MainViewModel());
    }

    public void NavigateDashboard()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      NavigationService.Navigate("DashboardView", null, this);
    }

    public void NavigateExperiment()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      NavigationService.Navigate("ExperimentView", null, this);
      App.Device.InitSTab("readertab");
    }

    public void NavigateResults()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      NavigationService.Navigate("ResultsView", null, this);
    }

    public void NavigateDataAnalysis()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      NavigationService.Navigate("DataAnalysisView", null, this);
    }

    public void NavigateMaintenance()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      NavigationService.Navigate("MaintenanceView", null, this);
    }

    public void NavigateSettings()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
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

  }
}