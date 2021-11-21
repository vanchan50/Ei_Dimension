using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using Newtonsoft.Json;
using System.IO;
using Ei_Dimension.Models;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class TemplateSelectViewModel
  {
    public virtual ObservableCollection<string> NameList { get; set; }
    public string SelectedItem;
    public virtual ObservableCollection<string> TemplateSaveName { get; set; }
    public string _templateName;
    public static TemplateSelectViewModel Instance { get; private set; }
    public virtual Visibility DeleteVisible { get; set; }
    protected TemplateSelectViewModel()
    {
      TemplateSaveName = new ObservableCollection<string> { "TemplateName" };
      var TemplateList = Directory.GetFiles(App.Device.RootDirectory + @"\Config", "*.dtml");
      NameList = new ObservableCollection<string>();
      foreach (var template in TemplateList)
      {
        NameList.Add(Path.GetFileNameWithoutExtension(template));
      }
      DeleteVisible = Visibility.Hidden;
      Instance = this;
    }

    public static TemplateSelectViewModel Create()
    {
      return ViewModelSource.Create(() => new TemplateSelectViewModel());
    }

    public void LoadTemplate()
    {
      AcquisitionTemplate newTemplate = null;
      try
      {
        using (TextReader reader = new StreamReader(SelectedItem))
        {
          var fileContents = reader.ReadToEnd();
          newTemplate = JsonConvert.DeserializeObject<AcquisitionTemplate>(fileContents);
        }
      }
      catch { MessageBox.Show("No Template selected"); }
      if (newTemplate != null)
      {
        try
        {
          int iRes;
          var DashVM = DashboardViewModel.Instance;
          DashVM.SpeedItems[newTemplate.Speed].Click(1);
          DashVM.ClassiMapItems[App.GetMapIndex(newTemplate.Map)].Click(2);
          DashVM.ChConfigItems[newTemplate.ChConfig].Click(3);
          DashVM.OrderItems[newTemplate.Order].Click(4);
          DashVM.SysControlItems[newTemplate.SysControl].Click(5);
          DashVM.EndReadItems[newTemplate.EndRead].Click(6);
          DashVM.EndRead[0] = newTemplate.MinPerRegion.ToString();
          if (int.TryParse(DashVM.EndRead[0], out iRes))
          {
            App.Device.MinPerRegion = iRes;
            Settings.Default.MinPerRegion = iRes;
          }
          else
            MessageBox.Show("Error loading Template");
          DashVM.EndRead[1] = newTemplate.TotalEvents.ToString();
          if (int.TryParse(DashVM.EndRead[1], out iRes))
          {
            App.Device.BeadsToCapture = iRes;
            Settings.Default.BeadsToCapture = iRes;
          }
          else
            MessageBox.Show("Error loading Template");
          DashVM.Volumes[0] = newTemplate.SampleVolume.ToString();
          if (int.TryParse(DashVM.Volumes[0], out iRes))
          {
            App.Device.MainCommand("Set Property", code: 0xaf, parameter: (ushort)iRes);
          }
          else
            MessageBox.Show("Error loading Template");
          DashVM.Volumes[1] = newTemplate.WashVolume.ToString();
          if (int.TryParse(DashVM.Volumes[1], out iRes))
          {
            App.Device.MainCommand("Set Property", code: 0xac, parameter: (ushort)iRes);
          }
          else
            MessageBox.Show("Error loading Template");
          DashVM.Volumes[2] = newTemplate.AgitateVolume.ToString();
          if (int.TryParse(DashVM.Volumes[2], out iRes))
          {
            App.Device.MainCommand("Set Property", code: 0xc4, parameter: (ushort)iRes);
          }
          else
            MessageBox.Show("Error loading Template");
          uint chkBox = newTemplate.FileSaveCheckboxes;
          for (var i = FileSaveViewModel.Instance.Checkboxes.Count - 1 - 1; i > -1; i--)// -1 to not store system log
          {
            uint pow = (uint)Math.Pow(2, i);
            if (chkBox >= pow)
            {
              FileSaveViewModel.Instance.CheckedBox(i);
              chkBox -= pow;
            }
            else
              FileSaveViewModel.Instance.UncheckedBox(i); ;
          }
          for (var i = 0; i < App.MapRegions.ActiveRegions.Count; i++)
          {
            if (newTemplate.ActiveRegions[i])
            {
              App.MapRegions.AddActiveRegion(i);
            }
            App.MapRegions.RegionsNamesList[i] = newTemplate.RegionsNamesList[i];
          }
          WellsSelectViewModel.Instance.ChangeWellTableSize(newTemplate.TableSize);
          if (newTemplate.TableSize == 96)
            Views.ExperimentView.Instance.DbWell96.IsChecked = true;
          else if (newTemplate.TableSize == 384)
            Views.ExperimentView.Instance.DbWell384.IsChecked = true;
          var size = newTemplate.TableSize == 96 ? 12 : 24;
          var Wells = WellsSelectViewModel.Instance.CurrentTableSize == 96 ?
            WellsSelectViewModel.Instance.Table96Wells : WellsSelectViewModel.Instance.Table384Wells;
          if (newTemplate.TableSize > 1)
          {
            var j = 0;
            foreach (var row in Wells)
            {
              for (var i = 0; i < size; i++)
              {
                row.SetType(i, newTemplate.SelectedWells[j][i]);
              }
              j++;
            }
          }
          else
            Views.ExperimentView.Instance.DbTube.IsChecked = true;
        }
        catch { }
        if (_templateName == null)
        {
          _templateName = SelectedItem.Substring(SelectedItem.LastIndexOf("\\") + 1, SelectedItem.Length - SelectedItem.LastIndexOf("\\") - 6);
        }
        ExperimentViewModel.Instance.CurrentTemplateName = _templateName;
        Settings.Default.LastTemplate = SelectedItem;
        Settings.Default.Save();
        ExperimentViewModel.Instance.NavigateDashboard();
      }
    }

    public void SaveTemplate()
    {
      var path = App.Device.RootDirectory + @"\Config\" + TemplateSaveName[0] + ".dtml";
      if (File.Exists(path))
      {
        MessageBox.Show($"A template with name {TemplateSaveName[0]} already exists");
        return;
      }
      try
      {
        using (var stream = new StreamWriter(path))
        {
          var temp = new AcquisitionTemplate();
          var DashVM = DashboardViewModel.Instance;
          temp.Name = TemplateSaveName[0];
          temp.SysControl = DashVM.SelectedSystemControlIndex;
          temp.ChConfig = DashVM.SelectedChConfigIndex;
          temp.Order = DashVM.SelectedOrderIndex;
          temp.Speed = DashVM.SelectedSpeedIndex;
          temp.Map = DashVM.SelectedClassiMapContent;
          temp.EndRead = DashVM.SelectedEndReadIndex;
          temp.SampleVolume = uint.Parse(DashVM.Volumes[0]);
          temp.WashVolume = uint.Parse(DashVM.Volumes[1]);
          temp.AgitateVolume = uint.Parse(DashVM.Volumes[2]);
          temp.MinPerRegion = uint.Parse(DashVM.EndRead[0]);
          temp.TotalEvents = uint.Parse(DashVM.EndRead[1]);
          uint checkboxes = 0;
          int currVal = 0;
          for (var i = 0; i < FileSaveViewModel.Instance.Checkboxes.Count - 1; i++)  // -1 to not store system log
          {
            currVal = (int)Math.Pow(2, i) * (FileSaveViewModel.Instance.Checkboxes[i] ? 1 : 0);
            checkboxes += (uint)currVal;
          }
          temp.FileSaveCheckboxes = checkboxes;
          temp.ActiveRegions.AddRange(App.MapRegions.ActiveRegions);
          temp.RegionsNamesList.AddRange(App.MapRegions.RegionsNamesList);
          temp.TableSize = WellsSelectViewModel.Instance.CurrentTableSize;
          if (temp.TableSize != 1)
          {
            var tempSelWells = WellsSelectViewModel.Instance.CurrentTableSize == 96 ?
              WellsSelectViewModel.Instance.Table96Wells : WellsSelectViewModel.Instance.Table384Wells;
            foreach (var wellRow in tempSelWells)
            {
              var list = new List<WellType>();
              foreach (var type in wellRow.Types)
              {
                list.Add(type);
              }
              temp.SelectedWells.Add(list);
            }
          }

          var contents = JsonConvert.SerializeObject(temp);
          stream.Write(contents);
        }
      }
      catch {
        MessageBox.Show("There was a problem saving the Template"); }
      NameList.Add(TemplateSaveName[0]);
      DeleteVisible = Visibility.Hidden;
    }

    public void DeleteTemplate()
    {
      if (SelectedItem != null && File.Exists(SelectedItem))
      {
        File.Delete(SelectedItem);
        NameList.Remove(_templateName);
        if (ExperimentViewModel.Instance.CurrentTemplateName == _templateName)
          ExperimentViewModel.Instance.CurrentTemplateName = "None";
        DeleteVisible = Visibility.Hidden;
      }
    }

    public void Selected(SelectionChangedEventArgs e)
    {
      if (e.AddedItems.Count == 0)
        return;
      _templateName = e.AddedItems[0].ToString();
      SelectedItem = App.Device.RootDirectory + @"\Config\" + e.AddedItems[0].ToString() + ".dtml";
      DeleteVisible = Visibility.Visible;
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(TemplateSaveName)), this, 0);
          MainViewModel.Instance.KeyboardToggle(Views.TemplateSelectView.Instance.TmplBox);
          break;
      }
    }
  }
}