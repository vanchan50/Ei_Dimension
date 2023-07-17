﻿using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class FileSaveViewModel
  {
    public virtual ObservableCollection<string> BaseFileName { get; set; }
    public virtual ObservableCollection<string> OutFolder { get; set; }
    public virtual ObservableCollection<bool> Checkboxes { get; set; }
    public static FileSaveViewModel Instance { get; private set; }
    private IFolderBrowserDialogService FolderBrowserDialogService => this.GetService<IFolderBrowserDialogService>();

    protected FileSaveViewModel()
    {
      BaseFileName = new ObservableCollection<string> { Settings.Default.SaveFileName };
      OutFolder = new ObservableCollection<string> { Settings.Default.LastOutFolder };
      Checkboxes = new ObservableCollection<bool>
      {
        Settings.Default.Everyevent,
        Settings.Default.RMeans,
        Settings.Default.PlateReport,
        Settings.Default.OnlyClassifed,
        false,
        false,
        Settings.Default.LegacyPlateReport
      };
      Instance = this;
    }
    
    public static FileSaveViewModel Create()
    {
      return ViewModelSource.Create(() => new FileSaveViewModel());
    }

    public void CheckedBox(int num)
    {
      UserInputHandler.InputSanityCheck();
      Checkboxes[num] = true;
      CheckBox(num);
    }

    public void UncheckedBox(int num)
    {
      UserInputHandler.InputSanityCheck();
      Checkboxes[num] = false;
      CheckBox(num);
    }

    public void SelectOutFolder(bool defaultDir = false)
    {
      UserInputHandler.InputSanityCheck();
      if (defaultDir)
      {
        App.DiosApp.Publisher.Outdir = Settings.Default.LastOutFolder = App.DiosApp.RootDirectory.FullName;
        Settings.Default.Save();
        OutFolder[0] = Settings.Default.LastOutFolder;
        return;
      }
      FolderBrowserDialogService.StartPath = App.DiosApp.Publisher.Outdir;
      if (FolderBrowserDialogService.ShowDialog())
      {
        App.DiosApp.Publisher.Outdir = Settings.Default.LastOutFolder = FolderBrowserDialogService.ResultPath;
        Settings.Default.Save();
        OutFolder[0] = Settings.Default.LastOutFolder;
      }
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(BaseFileName)), this, 0, Views.FileSaveView.Instance.FNBox);
          MainViewModel.Instance.KeyboardToggle(Views.FileSaveView.Instance.FNBox);
          break;
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }

    private void CheckBox(int num)
    {
      switch (num)
      {
        case 0:
          App.DiosApp.Publisher.IsBeadEventPublishingActive = Checkboxes[num];
          Settings.Default.Everyevent = Checkboxes[num];
          break;
        case 1:
          App.DiosApp.Publisher.IsResultsPublishingActive = Checkboxes[num];
          Settings.Default.RMeans = Checkboxes[num];
          break;
        case 2:
          App.DiosApp.Publisher.IsPlateReportPublishingActive = Checkboxes[num];
          Settings.Default.PlateReport = Checkboxes[num];
          break;
        case 3:
          App.DiosApp.Publisher.IsOnlyClassifiedBeadsPublishingActive = Checkboxes[num];
          Settings.Default.OnlyClassifed = Checkboxes[num];
          break;
        case 4:
          App.DiosApp.Publisher.IncludeReg0InPlateSummary = Checkboxes[num];
          break;
        case 5:
          break;
        case 6:
          App.DiosApp.Publisher.IsLegacyPlateReportPublishingActive = Checkboxes[num];
          Settings.Default.LegacyPlateReport = Checkboxes[num];
          break;
      }
      Settings.Default.Save();
    }
  }
}