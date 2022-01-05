using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using MicroCy;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class FileSaveViewModel
  {
    public virtual ObservableCollection<string> BaseFileName { get; set; }
    public virtual ObservableCollection<bool> Checkboxes { get; set; }
    public static FileSaveViewModel Instance { get; private set; }
    private IFolderBrowserDialogService FolderBrowserDialogService => this.GetService<IFolderBrowserDialogService>();

    protected FileSaveViewModel()
    {
      BaseFileName = new ObservableCollection<string> { Settings.Default.SaveFileName };
      Checkboxes = new ObservableCollection<bool>
      {
        Settings.Default.Everyevent,
        Settings.Default.RMeans,
        Settings.Default.PlateReport,
        Settings.Default.OnlyClassifed,
        false,
        false,
        true
      };
      Instance = this;
    }
    
    public static FileSaveViewModel Create()
    {
      return ViewModelSource.Create(() => new FileSaveViewModel());
    }

    public void CheckedBox(int num)
    {
      App.InputSanityCheck();
      Checkboxes[num] = true;
      CheckBox(num);
    }

    public void UncheckedBox(int num)
    {
      App.InputSanityCheck();
      Checkboxes[num] = false;
      CheckBox(num);
    }

    public void SelectOutFolder(bool defaultDir = false)
    {
      App.InputSanityCheck();
      if (defaultDir)
      {
        ResultReporter.Outdir = MicroCyDevice.RootDirectory.FullName;
        return;
      }
      FolderBrowserDialogService.StartPath = ResultReporter.Outdir;
      if (FolderBrowserDialogService.ShowDialog())
        ResultReporter.Outdir = FolderBrowserDialogService.ResultPath;
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(BaseFileName)), this, 0, Views.FileSaveView.Instance.FNBox);
          MainViewModel.Instance.KeyboardToggle(Views.FileSaveView.Instance.FNBox);
          break;
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      App.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }

    private void CheckBox(int num)
    {
      switch (num)
      {
        case 0:
          MicroCyDevice.Everyevent = Checkboxes[num];
          Settings.Default.Everyevent = Checkboxes[num];
          break;
        case 1:
          MicroCyDevice.RMeans = Checkboxes[num];
          Settings.Default.RMeans = Checkboxes[num];
          break;
        case 2:
          MicroCyDevice.PltRept = Checkboxes[num];
          Settings.Default.PlateReport = Checkboxes[num];
          break;
        case 3:
          MicroCyDevice.OnlyClassified = Checkboxes[num];
          Settings.Default.OnlyClassifed = Checkboxes[num];
          break;
        case 4:
          App.Device.Reg0stats = Checkboxes[num];
          break;
        case 5:
          break;
        case 6:
          if (Checkboxes[num])
            App.SetLogOutput();
          else
          {
            Console.Out.Close();
            StreamWriter sw = new StreamWriter(Console.OpenStandardOutput());
            sw.AutoFlush = true;
            Console.SetOut(sw);
          }
          break;
      }
      Settings.Default.Save();
    }
  }
}