using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows;
using Ei_Dimension.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Windows.Controls;
using System;
using System.Linq;
using System.IO;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ExperimentViewModel
  {
    public virtual Visibility WellSelectVisible { get; set; }
    public static ExperimentViewModel Instance { get; private set; }
    public string DialogFilter { get; set; }
    public string DialogTitleLoad { get; set; }
    public string DialogTitleSave { get; set; }
    private string _templateName;
    private INavigationService NavigationService => this.GetService<INavigationService>();
    private IOpenFileDialogService OpenFileDialogService => this.GetService<IOpenFileDialogService>();
    private ISaveFileDialogService SaveFileDialogService => this.GetService<ISaveFileDialogService>();

    protected ExperimentViewModel()
    {
      WellSelectVisible = Settings.Default.SystemControl == 0 ? Visibility.Visible : Visibility.Hidden;

      var curCulture = Language.TranslationSource.Instance.CurrentCulture;
      var RM = Language.Resources.ResourceManager;
      DialogFilter = RM.GetString(nameof(Language.Resources.JSON_Files),
            curCulture) + "|*.json|" + RM.GetString(nameof(Language.Resources.All_Files),
            curCulture) + "|*.*";
      DialogTitleSave = RM.GetString(nameof(Language.Resources.Experiment_Save_Template_Dialog_Title),
            curCulture);
      DialogTitleLoad = RM.GetString(nameof(Language.Resources.Experiment_Load_Template_Dialog_Title),
            curCulture);
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