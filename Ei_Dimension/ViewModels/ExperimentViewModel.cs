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
    public string DialogFilter { get; set; }
    public string DialogTitleLoad { get; set; }
    public string DialogTitleSave { get; set; }
    private INavigationService NavigationService => this.GetService<INavigationService>();
    private IOpenFileDialogService OpenFileDialogService => this.GetService<IOpenFileDialogService>();
    private ISaveFileDialogService SaveFileDialogService => this.GetService<ISaveFileDialogService>();

    protected ExperimentViewModel()
    {
      WellSelectVisible = Settings.Default.SystemControl == 0 ? Visibility.Visible : Visibility.Hidden;

      var curCulture = Language.TranslationSource.Instance.CurrentCulture;
      var RM = Language.Resources.ResourceManager;
      DialogFilter = RM.GetString(nameof(Language.Resources.Text_Files),
            curCulture) + "|*.txt|" + RM.GetString(nameof(Language.Resources.All_Files),
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

    public void LoadTemplate()
    {
      OpenFileDialogService.InitialDirectory = App.Device.RootDirectory.FullName + @"\Config";
      OpenFileDialogService.Filter = DialogFilter;
      OpenFileDialogService.FilterIndex = 1;
      OpenFileDialogService.Title = DialogTitleLoad;
      if (OpenFileDialogService.ShowDialog())
      {
        var file = OpenFileDialogService.File;
      }

    }

    public void SaveTemplate()
    {
      SaveFileDialogService.InitialDirectory = App.Device.RootDirectory.FullName + @"\Config";
      SaveFileDialogService.Filter = DialogFilter;
      SaveFileDialogService.FilterIndex = 1;
      SaveFileDialogService.Title = DialogTitleSave;
      SaveFileDialogService.DefaultExt = "txt";
      //  SaveFileDialogService.DefaultFileName = DefaultFileName;
      if (SaveFileDialogService.ShowDialog())
      {
        using (var stream = new System.IO.StreamWriter(SaveFileDialogService.OpenFile()))
        {
          stream.Write("");
        }
      }
    }
  }
}