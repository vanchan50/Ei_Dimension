using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Ei_Dimension.Models;
using System.Collections.Generic;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class CalibrationViewModel
  {
    public virtual string SelectedGatingContent { get; set; }
    public byte SelectedGatingIndex { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> GatingItems { get; }
    public virtual ObservableCollection<bool> CalibrationSelectorState { get; set; }
    public virtual bool CalModeOn { get; set; }
    public virtual bool ValModeOn { get; set; }
    public virtual System.Windows.Visibility CalValModeVisible { get; set; }
    public virtual ObservableCollection<string> EventTriggerContents { get; set; }
    public virtual ObservableCollection<string> ClassificationTargetsContents { get; set; }
    public virtual ObservableCollection<string> CompensationPercentageContent { get; set; }
    public virtual ObservableCollection<string> DNRContents { get; set; }
    public virtual ObservableCollection<string> CurrentMapName { get; set; }
    public virtual ObservableCollection<string> AttenuationBox { get; set; }
    public virtual ObservableCollection<string> CaliDateBox { get; set; }
    public byte CalFailsInARow { get; set; }

    public static CalibrationViewModel Instance { get; private set; }
    private string _dbsampleVolumeTempHolder;
    private int _dbEndReadIndexTempHolder;


    protected CalibrationViewModel()
    {
      var RM = Language.Resources.ResourceManager;
      var curCulture = Language.TranslationSource.Instance.CurrentCulture;
      GatingItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_None), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Green_SSC), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Red_SSC), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Green_Red_SSC), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Rp_bg), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Green_Rp_bg), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Red_Rp_bg), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Green_Red_Rp_bg), curCulture), this)
      };
      SelectedGatingIndex = 0;
      SelectedGatingContent = GatingItems[SelectedGatingIndex].Content;
      EventTriggerContents = new ObservableCollection<string> {"", App.Device.ActiveMap.minmapssc.ToString(), App.Device.ActiveMap.maxmapssc.ToString()};

      ClassificationTargetsContents = new ObservableCollection<string> { "1", "1", "1", "1", "3500"};

      CalibrationSelectorState = new ObservableCollection<bool> { true, false, false };
      CalModeOn = false;
      ValModeOn = false;
      CalValModeVisible = App.Device.ActiveMap.validation ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;

      CompensationPercentageContent = new ObservableCollection<string> { MicroCy.InstrumentParameters.Calibration.Compensation.ToString() };
      DNRContents = new ObservableCollection<string> { "", MicroCy.InstrumentParameters.Calibration.HdnrTrans.ToString() };

      CurrentMapName = new ObservableCollection<string> { App.Device.ActiveMap.mapName };
      AttenuationBox = new ObservableCollection<string> { App.Device.ActiveMap.att.ToString() };

      CalFailsInARow = 0;
      CaliDateBox = new ObservableCollection<string> { App.Device.ActiveMap.caltime };

      _dbEndReadIndexTempHolder = 0;
      Instance = this;
    }

    public static CalibrationViewModel Create()
    {
      return ViewModelSource.Create(() => new CalibrationViewModel());
    }

    public void CalibrationSelector(byte num)
    {
      CalibrationSelectorState[0] = false;
      CalibrationSelectorState[1] = false;
      CalibrationSelectorState[2] = false;
      CalibrationSelectorState[num] = true;
      App.Device.MainCommand("Set Property", code: 0x1b, parameter: num);
    }

    public void ConfirmCalibration()
    {
      App.Device.SaveCalVals(new MicroCy.MapCalParameters
      {
        TempCl0 = -1,
        TempCl1 = -1,
        TempCl2 = -1,
        TempCl3 = -1,
        TempRedSsc = -1,
        TempGreenSsc = -1,
        TempVioletSsc = -1,
        TempRpMaj = -1,
        TempRpMin = -1,
        TempFsc = -1,
        MinSSC = -1,
        MaxSSC = -1,
        Caldate = System.DateTime.Now.ToString("dd.MM.yyyy"),
        Valdate = null
      });
      CaliDateBox[0] = App.Device.ActiveMap.caltime;
      CalModeOn = false;
      CalModeToggle();
      App.ShowLocalizedNotification(nameof(Language.Resources.Calibration_Success));
    }

    public void SaveCalibrationToMapClick()
    {
      App.Device.SaveCalVals(new MicroCy.MapCalParameters
      {
        TempCl0 = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[8]),
        TempCl1 = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[5]),
        TempCl2 = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[6]),
        TempCl3 = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[3]),
        TempRedSsc = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[4]),
        TempGreenSsc = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[0]),
        TempVioletSsc = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[7]),
        TempRpMaj = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[1]),
        TempRpMin = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[2]),
        TempFsc = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[9]),
        MinSSC = ushort.Parse(EventTriggerContents[1]),
        MaxSSC = ushort.Parse(EventTriggerContents[2]),
        Caldate = null,
        Valdate = null
      });
    }

    public void CalModeToggle()
    {
      if (CalModeOn)
      {
        if (App.Device.Mode == MicroCy.OperationMode.Normal)
        {
          _dbsampleVolumeTempHolder = DashboardViewModel.Instance.Volumes[0];
          DashboardViewModel.Instance.SetFixedVolumeButtonClick(100);
          App.Device.Mode = MicroCy.OperationMode.Calibration;
          CalFailsInARow = 0;
          MakeCalMap();
          _dbEndReadIndexTempHolder = DashboardViewModel.Instance.SelectedEndReadIndex;
          DashboardViewModel.Instance.EndReadItems[2].Click(6);
          MainButtonsViewModel.Instance.Flavor[0] = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Maintenance_Calibration),
            Language.TranslationSource.Instance.CurrentCulture);
          MainWindow.Instance.wndw.Background = System.Windows.Media.Brushes.LightGray;
          App.LockMapSelection();
          return;
        }
        CalModeOn = false;
      }
      else
      {
        App.Device.Mode = MicroCy.OperationMode.Normal;
        DashboardViewModel.Instance.EndReadItems[_dbEndReadIndexTempHolder].Click(6);
        DashboardViewModel.Instance.Volumes[0] = _dbsampleVolumeTempHolder;
        App.Device.MainCommand("Set Property", code: 0xaf, parameter: ushort.Parse(_dbsampleVolumeTempHolder));
        MainButtonsViewModel.Instance.Flavor[0] = null;
        MainWindow.Instance.wndw.Background = (System.Windows.Media.SolidColorBrush)App.Current.Resources["AppBackground"];
        App.UnlockMapSelection();
      }
    }

    public void ValModeToggle()
    {
      if (ValModeOn)
      {
        if (App.Device.Mode == MicroCy.OperationMode.Normal && ValMapInfoReady())
        {
          _dbsampleVolumeTempHolder = DashboardViewModel.Instance.Volumes[0];
          DashboardViewModel.Instance.SetFixedVolumeButtonClick(25);
          App.Device.Mode = MicroCy.OperationMode.Validation;
          MainButtonsViewModel.Instance.Flavor[0] = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Maintenance_Validation),
            Language.TranslationSource.Instance.CurrentCulture);
          MainWindow.Instance.wndw.Background = System.Windows.Media.Brushes.LightYellow;
          ResultsViewModel.Instance.ValidationCoverVisible = System.Windows.Visibility.Visible;
          App.LockMapSelection();
          return;
        }
        ValModeOn = false;
      }
      else
      {
        App.Device.Mode = MicroCy.OperationMode.Normal;
        DashboardViewModel.Instance.Volumes[0] = _dbsampleVolumeTempHolder;
        App.Device.MainCommand("Set Property", code: 0xaf, parameter: ushort.Parse(_dbsampleVolumeTempHolder));
        MainButtonsViewModel.Instance.Flavor[0] = null;
        MainWindow.Instance.wndw.Background = (System.Windows.Media.SolidColorBrush)App.Current.Resources["AppBackground"];
        ResultsViewModel.Instance.ValidationCoverVisible = System.Windows.Visibility.Hidden;
        App.UnlockMapSelection();
      }
    }

    public void MakeCalMap()
    {
      ResultsViewModel.Instance.CalibrationWorldMap = new List<HeatMapData>();
      int cl1Index = System.Array.BinarySearch(HeatMapData.bins, int.Parse(ClassificationTargetsContents[1]));
      if (cl1Index < 0)
        cl1Index = ~cl1Index;
      int cl2Index = System.Array.BinarySearch(HeatMapData.bins, int.Parse(ClassificationTargetsContents[2]));
      if (cl2Index < 0)
        cl2Index = ~cl2Index;
      for (var i = -5; i < 6; i++)
      {
        for (var j = -6; j < 7; j++)
        {
          if(System.Math.Pow(i, 2) + System.Math.Pow(j, 2) <= 16
            && cl1Index + i >= 0 && cl1Index + i < 256 && cl2Index + j >= 0 && cl2Index + j < 256)
            ResultsViewModel.Instance.CalibrationWorldMap.Add(
              new HeatMapData((int)HeatMapData.bins[cl1Index + i], (int)HeatMapData.bins[cl2Index + j]));
        }
      }
    }

    private bool ValMapInfoReady()
    {
      bool activeRegions = false;
      for (var i = 0; i < App.MapRegions.RegionsList.Count; i++)
      {
        if (App.MapRegions.ValidationRegions[i])
        {
          activeRegions = true;
          if(App.MapRegions.ValidationReporterList[i] == "" || App.MapRegions.ValidationCVList[i] == "")
          {
            return false;
          }
        }
      }
      if (!activeRegions)
        return false;
      return true;
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationPercentageContent)), this, 0);
          MainViewModel.Instance.NumpadToggleButton(Views.CalibrationView.Instance.TB0);
          break;
        case 1:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(EventTriggerContents)), this, 0);
          MainViewModel.Instance.NumpadToggleButton(Views.CalibrationView.Instance.TB1);
          break;
        case 2:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(EventTriggerContents)), this, 1);
          MainViewModel.Instance.NumpadToggleButton(Views.CalibrationView.Instance.TB2);
          break;
        case 3:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(EventTriggerContents)), this, 2);
          MainViewModel.Instance.NumpadToggleButton(Views.CalibrationView.Instance.TB3);
          break;
        case 4:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(DNRContents)), this, 0);
          MainViewModel.Instance.NumpadToggleButton(Views.CalibrationView.Instance.TB4);
          break;
        case 5:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(DNRContents)), this, 1);
          MainViewModel.Instance.NumpadToggleButton(Views.CalibrationView.Instance.TB5);
          break;
        case 6:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ClassificationTargetsContents)), this, 0);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.CalibrationView.Instance.targetsSP.Children[0]);
          break;
        case 7:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ClassificationTargetsContents)), this, 1);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.CalibrationView.Instance.targetsSP.Children[1]);
          break;
        case 8:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ClassificationTargetsContents)), this, 2);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.CalibrationView.Instance.targetsSP.Children[2]);
          break;
        case 9:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ClassificationTargetsContents)), this, 3);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.CalibrationView.Instance.targetsSP.Children[3]);
          break;
        case 10:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ClassificationTargetsContents)), this, 4);
          MainViewModel.Instance.NumpadToggleButton((TextBox)Views.CalibrationView.Instance.targetsSP.Children[4]);
          break;
        case 11:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(ClassificationTargetsContents)), this, 0);
          MainViewModel.Instance.NumpadToggleButton(Views.CalibrationView.Instance.TB10);
          break;
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      App.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }

    public class DropDownButtonContents : Core.ObservableObject
    {
      public string Content
      {
        get => _content;
        set
        {
          _content = value;
          OnPropertyChanged();
        }
      }
      public byte Index { get; set; }
      private static byte _nextIndex = 0;
      private string _content;
      private static CalibrationViewModel _vm;
      public DropDownButtonContents(string content, CalibrationViewModel vm = null)
      {
        if (_vm == null)
        {
          _vm = vm;
        }
        Content = content;
        Index = _nextIndex++;
      }

      public void Click()
      {
        _vm.SelectedGatingContent = Content;
        App.Device.MainCommand("Set Property", code: 0xca, parameter: (ushort)Index);
        App.Device.ScatterGate = Index;
      }

      public void ForAppUpdater()
      {
        _vm.SelectedGatingContent = Content;
        App.Device.ScatterGate = Index;
      }

      public static void ResetIndex()
      {
        _nextIndex = 0;
      }
    }
  }
}