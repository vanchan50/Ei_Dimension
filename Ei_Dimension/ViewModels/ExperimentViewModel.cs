using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Windows;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ExperimentViewModel
  {
    public virtual Visibility WellSelectVisible { get; set; }
    public static ExperimentViewModel Instance { get; private set; }
    public virtual string CurrentTemplateName { get; set; }
    private INavigationService NavigationService => this.GetService<INavigationService>();

    protected ExperimentViewModel()
    {
      WellSelectVisible = Settings.Default.SystemControl == 0 ? Visibility.Visible : Visibility.Hidden;
      CurrentTemplateName = "None";
      Instance = this;
    }

    public static ExperimentViewModel Create()
    {
      return ViewModelSource.Create(() => new ExperimentViewModel());
    }

    public void NavigateDashboard()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      MainViewModel.Instance.StartButtonsVisible = Visibility.Visible;
      NavigationService.Navigate("DashboardView", null, this);
    }

    public void NavigateSelRegions()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      MainViewModel.Instance.StartButtonsVisible = Visibility.Hidden;
      NavigationService.Navigate("SelRegionsView", null, this);
    }

    public void NavigateFileSave()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      App.Device.InitSTab("reportingtab");
      MainViewModel.Instance.StartButtonsVisible = Visibility.Hidden;
      NavigationService.Navigate("FileSaveView", null, this);
    }

    public void NavigateWellsSelect(int num)
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      if (num != 1)
      {
        MainViewModel.Instance.StartButtonsVisible = Visibility.Hidden;
        NavigationService.Navigate("WellsSelectView", null, this);
      }
      else
        NavigateDashboard();
      if (WellsSelectViewModel.Instance != null)
        WellsSelectViewModel.Instance.ChangeWellTableSize(num);
    }

    public void NavigateTemplateSelect()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      if(Views.TemplateSelectView.Instance != null)
      {
        Views.TemplateSelectView.Instance.list.SelectedItem = null;
        TemplateSelectViewModel.Instance.SelectedItem = null;
        TemplateSelectViewModel.Instance.DeleteVisible = Visibility.Hidden;
      }
      MainViewModel.Instance.StartButtonsVisible = Visibility.Hidden;
      NavigationService.Navigate("TemplateSelectView", null, this);
    }

    public void InitChildren()
    {
      NavigateWellsSelect(96);
      NavigateDashboard();
      NavigateFileSave();
      NavigateSelRegions();
    }
  }
}