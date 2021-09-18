using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows;
using Ei_Dimension.Models;
using System.Collections.Generic;
using System.Windows.Controls;
using System;
using System.Linq;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ExperimentViewModel
  {
    public virtual Visibility WellSelectVisible { get; set; }
    public static ExperimentViewModel Instance { get; private set; }
    private INavigationService NavigationService => this.GetService<INavigationService>();

    protected ExperimentViewModel()
    {
      WellSelectVisible = Settings.Default.SystemControl == 0 ? Visibility.Visible : Visibility.Hidden;
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
      NavigationService.Navigate("DashboardView", null, this);
    }

    public void NavigateWellsSelect(int num)
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      NavigationService.Navigate("WellsSelectView", null, this);
      if (WellsSelectViewModel.Instance != null)
        WellsSelectViewModel.Instance.ChangeWellTableSize(num);
    }
  }
}