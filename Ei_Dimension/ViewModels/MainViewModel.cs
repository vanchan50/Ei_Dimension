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
    public virtual ObservableCollection<string> EventCountGlobal { get; set; }
    public virtual ObservableCollection<string> EventCountLocal { get; set; }
    public virtual System.Windows.Visibility EventCountVisible { get; set; }
    public virtual System.Windows.Visibility StartButtonsVisible { get; set; }
    public virtual System.Windows.Visibility PlexResultsButtonVisible { get; set; }

    public static MainViewModel Instance { get; private set; }
    private INavigationService NavigationService => this.GetService<INavigationService>();

    protected MainViewModel()
    {
      App.NumpadShow = (this.GetType().GetProperty(nameof(NumpadVisible)), this);
      NumpadVisible = System.Windows.Visibility.Hidden;
      EventCountVisible = System.Windows.Visibility.Visible;
      StartButtonsVisible = System.Windows.Visibility.Visible;
      PlexResultsButtonVisible = System.Windows.Visibility.Hidden;
      EventCountGlobal = new ObservableCollection<string> { "0" };
      EventCountLocal = new ObservableCollection<string> { "0" };
      EventCountField = EventCountGlobal;
      Instance = this;
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
      PlexResultsButtonVisible = System.Windows.Visibility.Hidden;
      NavigationService.Navigate("ExperimentView", null, this);
      App.Device.InitSTab("readertab");
    }

    public void NavigateResults()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      EventCountVisible = System.Windows.Visibility.Visible;
      StartButtonsVisible = System.Windows.Visibility.Visible;
      PlexResultsButtonVisible = System.Windows.Visibility.Visible;
      NavigationService.Navigate("ResultsView", null, this);
    }

    public void NavigateDataAnalysis()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      StartButtonsVisible = System.Windows.Visibility.Hidden;
      EventCountVisible = System.Windows.Visibility.Hidden;
      PlexResultsButtonVisible = System.Windows.Visibility.Hidden;
      NavigationService.Navigate("DataAnalysisView", null, this);
    }

    public void NavigateMaintenance()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      StartButtonsVisible = System.Windows.Visibility.Hidden;
      EventCountVisible = System.Windows.Visibility.Hidden;
      PlexResultsButtonVisible = System.Windows.Visibility.Hidden;
      NavigationService.Navigate("MaintenanceView", null, this);
    }

    public void NavigateSettings()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      StartButtonsVisible = System.Windows.Visibility.Hidden;
      EventCountVisible = System.Windows.Visibility.Hidden;
      PlexResultsButtonVisible = System.Windows.Visibility.Hidden;
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

    public void NavigateStaticMap()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      NavigationService.Navigate("StaticMapView", null, this);
    }

    public void InitChildren()
    {
      NavigateExperiment();
      NavigateResults();
      NavigateDataAnalysis();
      NavigateMaintenance();
      NavigateSettings();
    }

    public void PlexResults()
    {
      if(ResultsViewModel.Instance.MultiPlexVisible == System.Windows.Visibility.Visible)
      {
        ResultsViewModel.Instance.MultiPlexVisible = System.Windows.Visibility.Hidden;
        ResultsViewModel.Instance.SinglePlexVisible = System.Windows.Visibility.Visible;
      }
      else
      {
        ResultsViewModel.Instance.MultiPlexVisible = System.Windows.Visibility.Visible;
        ResultsViewModel.Instance.SinglePlexVisible = System.Windows.Visibility.Hidden;
      }
    }
  }
}