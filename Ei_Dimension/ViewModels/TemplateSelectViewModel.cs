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
using System.Threading.Tasks;
using System.Windows.Controls;
using Ei_Dimension.Controllers;
using DIOS.Core;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class TemplateSelectViewModel
  {
    public virtual ObservableCollection<string> NameList { get; set; }
    public string SelectedItem;
    public virtual ObservableCollection<string> TemplateSaveName { get; set; }
    public virtual Visibility WaitIndicatorBorderVisibility { get; set; }
    public string _templateName;
    public static TemplateSelectViewModel Instance { get; private set; }
    public virtual Visibility DeleteVisible { get; set; }
    private static List<char> _invalidChars;
    protected TemplateSelectViewModel()
    {
      var defTemplateName = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.DefaultTemplateName),
        Language.TranslationSource.Instance.CurrentCulture);
      TemplateSaveName = new ObservableCollection<string> { defTemplateName };
      var TemplateList = Directory.GetFiles(Device.RootDirectory + @"\Config", "*.dtml");
      NameList = new ObservableCollection<string>();
      foreach (var template in TemplateList)
      {
        NameList.Add(Path.GetFileNameWithoutExtension(template));
      }
      DeleteVisible = Visibility.Hidden;
      _invalidChars = new List<char>();
      _invalidChars.AddRange(Path.GetInvalidPathChars());
      _invalidChars.AddRange(Path.GetInvalidFileNameChars());
      WaitIndicatorBorderVisibility = Visibility.Hidden;
      Instance = this;
    }

    public static TemplateSelectViewModel Create()
    {
      return ViewModelSource.Create(() => new TemplateSelectViewModel());
    }

    public void LoadTemplate()
    {
      UserInputHandler.InputSanityCheck();
      AcquisitionTemplate newTemplate = null;
      try
      {
        using (TextReader reader = new StreamReader(SelectedItem))
        {
          var fileContents = reader.ReadToEnd();
          newTemplate = JsonConvert.DeserializeObject<AcquisitionTemplate>(fileContents);
        }
      }
      catch
      {
        Notification.ShowLocalized(nameof(Language.Resources.Notification_No_Template_Selected));
      }
      if (newTemplate != null)
      {
        ShowWaitIndicator();
        Task.Run(() =>
        {
          App.Current.Dispatcher.Invoke((Action) (() =>
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

              var temp = newTemplate.MinPerRegion.ToString();
              if (int.TryParse(temp, out iRes))
              {
                App.Device.MinPerRegion = iRes;
                Settings.Default.MinPerRegion = iRes;
                DashVM.EndRead[0] = temp;
              }
              else
                throw new Exception();
              
              temp = newTemplate.TotalEvents.ToString();
              if (int.TryParse(temp, out iRes))
              {
                App.Device.BeadsToCapture = iRes;
                Settings.Default.BeadsToCapture = iRes;
                DashVM.EndRead[1] = temp;
              }
              else
                throw new Exception();
              
              temp = newTemplate.SampleVolume.ToString();
              if (int.TryParse(temp, out iRes))
              {
                App.Device.MainCommand("Set Property", code: 0xaf, parameter: (ushort) iRes);
                DashVM.Volumes[0] = temp;
              }
              else
                throw new Exception();
              
              temp = newTemplate.WashVolume.ToString();
              if (int.TryParse(temp, out iRes))
              {
                App.Device.MainCommand("Set Property", code: 0xac, parameter: (ushort) iRes);
                DashVM.Volumes[1] = temp;
              }
              else
                throw new Exception();
              
              
              temp = newTemplate.AgitateVolume.ToString();
              if (int.TryParse(temp, out iRes))
              {
                App.Device.MainCommand("Set Property", code: 0xc4, parameter: (ushort) iRes);
                DashVM.Volumes[2] = temp;
              }
              else
                throw new Exception();

              uint chkBox = newTemplate.FileSaveCheckboxes;
              for (var i = FileSaveViewModel.Instance.Checkboxes.Count - 1 - 1; i > -1; i--) // -1 to not store system log
              {
                uint pow = (uint) Math.Pow(2, i);
                if (chkBox >= pow)
                {
                  FileSaveViewModel.Instance.CheckedBox(i);
                  chkBox -= pow;
                }
                else
                  FileSaveViewModel.Instance.UncheckedBox(i);
              }

              for (var i = 0; i < MapRegionsController.RegionsList.Count - 1; i++)
              {
                MapRegionsController.RegionsList[i + 1].Name[0] = newTemplate.RegionsNamesList[i];
                if (newTemplate.ActiveRegions[i])
                {
                  App.MapRegions.AddActiveRegion(MapRegionsController.RegionsList[i+1].Number);
                }
              }

              if (newTemplate.TableSize != 0)
                WellsSelectViewModel.Instance.ChangeWellTableSize(newTemplate.TableSize);
              if (newTemplate.TableSize == 96)
                Views.ExperimentView.Instance.DbWell96.IsChecked = true;
              else if (newTemplate.TableSize == 384)
                Views.ExperimentView.Instance.DbWell384.IsChecked = true;
              else if (newTemplate.TableSize == 1)
                Views.ExperimentView.Instance.DbTube.IsChecked = true;
              if (newTemplate.TableSize == 96 || newTemplate.TableSize == 384)
              {
                var size = newTemplate.TableSize == 96 ? 12 : 24;
                var Wells = WellsSelectViewModel.Instance.CurrentTableSize == 96
                  ? WellsSelectViewModel.Instance.Table96Wells
                  : WellsSelectViewModel.Instance.Table384Wells;
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
            }
            catch
            {
              Notification.ShowLocalized(nameof(Language.Resources.Notification_Error_Loading_template));
              return;
            }
            finally
            {
              HideWaitIndicator();
            }
            if (_templateName == null)
            {
              _templateName = SelectedItem.Substring(SelectedItem.LastIndexOf("\\") + 1, SelectedItem.Length - SelectedItem.LastIndexOf("\\") - 6);
            }
            ExperimentViewModel.Instance.CurrentTemplateName = _templateName;
            Settings.Default.LastTemplate = SelectedItem;
            Settings.Default.Save();
            ExperimentViewModel.Instance.NavigateDashboard();
          }));
        });
      }
    }

    public void SaveTemplate()
    {
      UserInputHandler.InputSanityCheck();
      var path = Device.RootDirectory + @"\Config\" + TemplateSaveName[0] + ".dtml";
      foreach (var c in _invalidChars)
      {
        if (TemplateSaveName[0].Contains(c.ToString()))
        {
          Notification.ShowLocalized(nameof(Language.Resources.Notification_Invalid_File_Name));
          return;
        }
      }
      if (File.Exists(path))
      {
        void Overwrite()
        {
          SavingProcedure(path);
        }

        if (Language.TranslationSource.Instance.CurrentCulture.TextInfo.CultureName == "zh-CN")
        {
          Notification.Show($"名为 \"{TemplateSaveName[0]}\" 的模板已存在",
            Overwrite, "覆盖", null, "取消");
        }
        else
        {
          Notification.Show($"A template with name \"{TemplateSaveName[0]}\" already exists.",
            Overwrite, "Overwrite", null, "Cancel");
        }
        return;
      }
      SavingProcedure(path);
    }
    
    private void SavingProcedure(string path)
    {
      try
      {
        using (var stream = new StreamWriter(path, append: false))
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
          for (var i = 0; i < FileSaveViewModel.Instance.Checkboxes.Count - 1; i++)  // -1 to not store system log
          {
            int currVal = (int)Math.Pow(2, i) * (FileSaveViewModel.Instance.Checkboxes[i] ? 1 : 0);
            checkboxes += (uint)currVal;
          }
          temp.FileSaveCheckboxes = checkboxes;

          foreach (var region in MapRegionsController.RegionsList)
          {
            if(region.Number == 0)
              continue;
            var isActive = MapRegionsController.ActiveRegionNums.Contains(region.Number);
            temp.ActiveRegions.Add(isActive);
            temp.RegionsNamesList.Add(region.Name[0]);
          }

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
      catch
      {
        Notification.ShowLocalized(nameof(Language.Resources.Notification_Template_Save_Problem));
      }
      if (!NameList.Contains(TemplateSaveName[0]))
      {
        NameList.Add(TemplateSaveName[0]);
      }
      DeleteVisible = Visibility.Hidden;
      Views.TemplateSelectView.Instance.list.UnselectAll();
    }

    public void DeleteTemplate()
    {
      UserInputHandler.InputSanityCheck();
      if (SelectedItem != null && File.Exists(SelectedItem))
      {
        void Delete()
        {
          File.Delete(SelectedItem);
          NameList.Remove(_templateName);
          if (ExperimentViewModel.Instance.CurrentTemplateName == _templateName)
            ExperimentViewModel.Instance.CurrentTemplateName = "None";
          DeleteVisible = Visibility.Hidden;
        }

        if (Language.TranslationSource.Instance.CurrentCulture.TextInfo.CultureName == "zh-CN")
        {
          Notification.Show($"您要删除 \"{Path.GetFileNameWithoutExtension(SelectedItem)}\" 模板吗？",
            Delete, "删除", null, "取消");
        }
        else
        {
          Notification.Show($"Do you want to delete \"{Path.GetFileNameWithoutExtension(SelectedItem)}\" template?",
            Delete, "Delete", null, "Cancel");
        }
      }
    }

    public void Selected(SelectionChangedEventArgs e)
    {
      if (e.AddedItems.Count == 0)
        return;
      _templateName = e.AddedItems[0].ToString();
      SelectedItem = Device.RootDirectory + @"\Config\" + e.AddedItems[0].ToString() + ".dtml";
      TemplateSaveName[0] = _templateName;
      DeleteVisible = Visibility.Visible;
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(TemplateSaveName)), this, 0, Views.TemplateSelectView.Instance.TmplBox);
          MainViewModel.Instance.KeyboardToggle(Views.TemplateSelectView.Instance.TmplBox);
          break;
      }
    }

    private void ShowWaitIndicator()
    {
      WaitIndicatorBorderVisibility = Visibility.Visible;
    }

    private void HideWaitIndicator()
    {
      WaitIndicatorBorderVisibility = Visibility.Hidden;
    }
  }
}