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
      NavigationService.Navigate("DashboardView", null, this);
    }

    public void NavigateFileSave()
    {
      App.ResetFocusedTextbox();
      App.HideNumpad();
      App.Device.InitSTab("reportingtab");
      NavigationService.Navigate("FileSaveView", null, this);
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
        var file = OpenFileDialogService.GetFullFileName();
        try
        {
          using (TextReader reader = new StreamReader(file))
          {
            var fileContents = reader.ReadToEnd();
            App.AcquisitionTemplateLoaded(JsonConvert.DeserializeObject<AcquisitionTemplate>(fileContents));
          }
        }
        catch { }
      }
    }

    public void SaveTemplate()
    {
      SaveFileDialogService.InitialDirectory = App.Device.RootDirectory.FullName + @"\Config";
      SaveFileDialogService.Filter = DialogFilter;
      SaveFileDialogService.FilterIndex = 1;
      SaveFileDialogService.Title = DialogTitleSave;
      SaveFileDialogService.DefaultExt = "json";
      //  SaveFileDialogService.DefaultFileName = DefaultFileName;
      if (SaveFileDialogService.ShowDialog())
      {
        try
        {
          using (var stream = new StreamWriter(SaveFileDialogService.OpenFile()))
          {
            var temp = new AcquisitionTemplate();
            temp.Name = SaveFileDialogService.File.Name;
            temp.SysControl = DashboardViewModel.Instance.SelectedSystemControlIndex;
            temp.ChConfig = DashboardViewModel.Instance.SelectedChConfigIndex;
            temp.Order = DashboardViewModel.Instance.SelectedOrderIndex;
            temp.Speed = DashboardViewModel.Instance.SelectedSpeedIndex;
            temp.Map = DashboardViewModel.Instance.SelectedClassiMapContent;
            temp.EndRead = DashboardViewModel.Instance.SelectedEndReadIndex;
            temp.SampleVolume = uint.Parse(DashboardViewModel.Instance.Volumes[0]);
            temp.WashVolume = uint.Parse(DashboardViewModel.Instance.Volumes[1]);
            temp.AgitateVolume = uint.Parse(DashboardViewModel.Instance.Volumes[2]);
            temp.MinPerRegion = uint.Parse(DashboardViewModel.Instance.EndRead[0]);
            temp.TotalEvents = uint.Parse(DashboardViewModel.Instance.EndRead[1]);

            var contents = JsonConvert.SerializeObject(temp);
            _ = stream.WriteAsync(contents);
          }
        }
        catch { }
      }
    }
  }
}