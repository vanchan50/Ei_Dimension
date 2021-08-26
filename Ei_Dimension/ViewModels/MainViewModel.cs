using DevExpress.Mvvm;
using System;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class MainViewModel 
  {
    public virtual bool NumpadShown { get; set; }
    private INavigationService NavigationService { get { return this.GetService<INavigationService>(); } }
    private IWindowService WindowService { get { return this.GetService<IWindowService>(); } }

    protected MainViewModel()
    {
      NumpadShown = false;
    }

    public static MainViewModel Create()
    {
      return ViewModelSource.Create(() => new MainViewModel());
    }

    public void NavigateDashboard()
    {
      HideNumpad();
      NavigationService.Navigate("DashboardView", null, this);
    }

    public void NavigateExperiment()
    {
      HideNumpad();
      NavigationService.Navigate("ExperimentView", null, this);
    }

    public void NavigateResults()
    {
      HideNumpad();
      NavigationService.Navigate("ResultsView", null, this);
    }

    public void NavigateDataAnalysis()
    {
      HideNumpad();
      NavigationService.Navigate("DataAnalysisView", null, this);
    }

    public void NavigateMaintenance()
    {
      HideNumpad();
      NavigationService.Navigate("MaintenanceView", null, this);
    }

    public void NavigateSettings()
    {
      HideNumpad();
      NavigationService.Navigate("ServiceView", null, this);
    }

    public void NumpadToggleButon()
    {
      if (NumpadShown)
      {
        HideNumpad();
        return;
      }
      else
      {
        WindowService.Show("NumpadView", null, this);
        NumpadShown = true;
      }
    }

    public void HideNumpad()
    {
      NumpadShown = false;
      WindowService.Hide();
    }
  }
}