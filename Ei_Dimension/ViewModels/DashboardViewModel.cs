using System.Collections.Generic;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Ei_Dimension.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using DIOS.Core;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class DashboardViewModel
  {
    public virtual ObservableCollection<string> EndRead { get; set; } = new ObservableCollection<string> { "100", "500" };
    public virtual ObservableCollection<string> Volumes { get; set; } = new ObservableCollection<string> { "0", "0", "0" };
    public virtual ObservableCollection<string> Repeats { get; set; } = new ObservableCollection<string> { "1", "1" };
    public virtual ObservableCollection<string> WorkOrder { get; set; } = new ObservableCollection<string> { "" };
    public virtual string SelectedSpeedContent { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> SpeedItems { get; set; }
    public virtual string SelectedClassiMapContent { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> ClassiMapItems { get; set; }
    public virtual string SelectedChConfigContent { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> ChConfigItems { get; set; }
    public virtual string SelectedOrderContent { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> OrderItems { get; set; }
    public virtual string SelectedSysControlContent { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> SysControlItems { get; set; }
    public virtual string SelectedEndReadContent { get; set; }
    public virtual ObservableCollection<DropDownButtonContents> EndReadItems { get; set; }
    public virtual bool CalValModeEnabled { get; set; }
    public virtual bool CalModeOn { get; set; }
    public virtual bool ValModeOn { get; set; }
    public virtual ObservableCollection<string> CaliDateBox { get; set; } = new ObservableCollection<string> { "" };
    public virtual ObservableCollection<string> ValidDateBox { get; set; } = new ObservableCollection<string> { "" };
    public static DashboardViewModel Instance { get; private set; }

    public byte SelectedSystemControlIndex { get; set; }
    public WellReadingSpeed SelectedSpeedIndex { get; set; } = WellReadingSpeed.Normal;
    public ChannelConfiguration SelectedChConfigIndex { get; set; } = ChannelConfiguration.Standard;
    public WellReadingOrder SelectedOrderIndex { get; set; } = WellReadingOrder.Column;
    public byte SelectedEndReadIndex { get; set; }
    public virtual ObservableCollection<Visibility> EndReadVisibility { get; set; }
    public virtual Visibility WorkOrderVisibility { get; set; }
    public virtual Visibility VerificationWarningVisible { get; set; }
    

    private string _dbsampleVolumeTempHolder;
    private int _dbEndReadIndexTempHolder;

    protected DashboardViewModel()
    {
      var RM = Language.Resources.ResourceManager;
      var curCulture = Language.TranslationSource.Instance.CurrentCulture;
      SpeedItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Normal), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Hi_Speed), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Hi_Sens), curCulture), this)
      };
      SelectedSpeedContent = SpeedItems[(int)SelectedSpeedIndex].Content;
      DropDownButtonContents.ResetIndex();

      ClassiMapItems = new ObservableCollection<DropDownButtonContents>();
      if (App.DiosApp.MapController.MapList.Count > 0)
      {
        foreach (var map in App.DiosApp.MapController.MapList)
        {
          ClassiMapItems.Add(new DropDownButtonContents(map.mapName, this));
        }
        SelectedClassiMapContent = App.DiosApp.MapController.ActiveMap.mapName;
      }
      else
      {
        ClassiMapItems.Add(new DropDownButtonContents("No map", this));
        SelectedClassiMapContent = "No map";
      }
      DropDownButtonContents.ResetIndex();

      ChConfigItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Standard), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Cells), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_FM3D), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_StandardPlusFSC), curCulture), this)
      };
      SelectedChConfigContent = ChConfigItems[(int)SelectedChConfigIndex].Content;
      DropDownButtonContents.ResetIndex();

      OrderItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Column), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Row), curCulture), this),
      };
      SelectedOrderContent = OrderItems[(int)SelectedOrderIndex].Content;
      DropDownButtonContents.ResetIndex();

      SysControlItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Experiment_Manual), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Experiment_Work_Order), curCulture), this),
        //new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Experiment_Work_Order_Plus_Bcode), curCulture), this),
      };
      SelectedSystemControlIndex = Settings.Default.SystemControl;
      SelectedSysControlContent = SysControlItems[SelectedSystemControlIndex].Content;
      DropDownButtonContents.ResetIndex();

      if(SelectedSystemControlIndex != 0)
        WorkOrderVisibility = Visibility.Visible;
      else
        WorkOrderVisibility = Visibility.Hidden;

      VerificationWarningVisible = Visibility.Hidden;

      EndReadItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Experiment_Min_Per_Reg), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Experiment_Total_Events), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Experiment_End_of_Sample), curCulture), this),
      };
      SelectedEndReadIndex = Settings.Default.EndRead;
      SelectedEndReadContent = EndReadItems[SelectedEndReadIndex].Content;
      EndReadVisibility = new ObservableCollection<Visibility>
      {
        Visibility.Hidden,
        Visibility.Hidden
      };
      EndReadVisibilitySwitch();
      DropDownButtonContents.ResetIndex();

      if (SelectedSystemControlIndex == 0)
        MainButtonsViewModel.Instance.StartButtonEnabled = true;
      else
        MainButtonsViewModel.Instance.StartButtonEnabled = false;

      CalValModeEnabled = App.DiosApp.MapController.ActiveMap.validation;
      _dbsampleVolumeTempHolder = null;
      _dbEndReadIndexTempHolder = 0;

      SetCalibrationDate(App.DiosApp.MapController.ActiveMap.caltime);
      SetValidationDate(App.DiosApp.MapController.ActiveMap.valtime);
      
      Instance = this;
    }

    public static DashboardViewModel Create()
    {
      return ViewModelSource.Create(() => new DashboardViewModel());
    }

    public void SetFixedVolumeButtonClick(ushort num)
    {
      UserInputHandler.InputSanityCheck();
      App.Device.MainCommand("Set Property", code: 0xaf, parameter: num);
      Volumes[0] = num.ToString();
    }

    public void FluidicsButtonClick(int i)
    {
      UserInputHandler.InputSanityCheck();
      App.Device.MainCommand("Set Property", code: 0x19, parameter: 1); //bubble detect on
      string cmd = "";
      switch (i)
      {
        case 0:
          cmd = "Prime";
          break;
        case 1:
          cmd = "Wash A";
          break;
        case 2:
          cmd = "Wash B";
          break;
      }
      App.Device.MainCommand(cmd);
      App.Device.MainCommand("Set Property", code: 0x19, parameter: 0); //bubble detect off
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(EndRead)), this, 0, Views.DashboardView.Instance.Endr0TB);
          MainViewModel.Instance.NumpadToggleButton(Views.DashboardView.Instance.Endr0TB);
          break; 
        case 1:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(EndRead)), this, 1, Views.DashboardView.Instance.Endr1TB);
          MainViewModel.Instance.NumpadToggleButton(Views.DashboardView.Instance.Endr1TB);
          break;
        case 2:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Volumes)), this, 0, Views.DashboardView.Instance.SampVTB);
          MainViewModel.Instance.NumpadToggleButton(Views.DashboardView.Instance.SampVTB);
          break;
        case 3:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Volumes)), this, 1, Views.DashboardView.Instance.WashVTB);
          MainViewModel.Instance.NumpadToggleButton(Views.DashboardView.Instance.WashVTB);
          break;
        case 4:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Volumes)), this, 2, Views.DashboardView.Instance.AgitVTB);
          MainViewModel.Instance.NumpadToggleButton(Views.DashboardView.Instance.AgitVTB);
          break;
        case 5:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(WorkOrder)), this, 0, Views.DashboardView.Instance.SysCTB);
          MainViewModel.Instance.NumpadToggleButton(Views.DashboardView.Instance.SysCTB);
          break;
        case 6:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Repeats)), this, 0, Views.DashboardView.Instance.WashRepTB);
          MainViewModel.Instance.NumpadToggleButton(Views.DashboardView.Instance.WashRepTB);
          break;
        case 7:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Repeats)), this, 1, Views.DashboardView.Instance.AgitateRepTB);
          MainViewModel.Instance.NumpadToggleButton(Views.DashboardView.Instance.AgitateRepTB);
          break;
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }

    private void EndReadVisibilitySwitch()
    {
      switch (SelectedEndReadIndex)
      {
        case 0:
          EndReadVisibility[0] = Visibility.Visible;
          EndReadVisibility[1] = Visibility.Hidden;
          break;
        case 1:
          EndReadVisibility[0] = Visibility.Hidden;
          EndReadVisibility[1] = Visibility.Visible;
          break;
        default:
          EndReadVisibility[0] = Visibility.Hidden;
          EndReadVisibility[1] = Visibility.Hidden;
          break;
      }
    }

    public void CalModeToggle()
    {
      void ReturnToNormal()
      {
        App.Device.Mode = OperationMode.Normal;
        EndReadItems[_dbEndReadIndexTempHolder].Click(6);
        Volumes[0] = _dbsampleVolumeTempHolder;
        App.Device.MainCommand("Set Property", code: 0xaf, parameter: ushort.Parse(_dbsampleVolumeTempHolder));
        MainButtonsViewModel.Instance.Flavor[0] = null;
        MainWindow.Instance.wndw.Background = (System.Windows.Media.SolidColorBrush)App.Current.Resources["AppBackground"];
        UnlockMapSelection();
        UnLockEndReadSelection();
        App.Device.MainCommand("Set Property", code: 0x1b, parameter: 0);
      }

      UserInputHandler.InputSanityCheck();
      if (CalModeOn)
      {
        if (App.Device.Mode == OperationMode.Normal)
        {
          _dbsampleVolumeTempHolder = Volumes[0];
          SetFixedVolumeButtonClick(100);
          App.Device.Mode = OperationMode.Calibration;
          CalibrationViewModel.Instance.CalFailsInARow = 0;
          CalibrationViewModel.Instance.MakeCalMap();
          _dbEndReadIndexTempHolder = SelectedEndReadIndex;
          EndReadItems[2].Click(6);
          MainButtonsViewModel.Instance.Flavor[0] = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Maintenance_Calibration),
            Language.TranslationSource.Instance.CurrentCulture);
          MainWindow.Instance.wndw.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 191));
          LockMapSelection();
          LockEndReadSelection();
          App.Device.MainCommand("Set Property", code: 0x1b, parameter: 1);
          return;
        }
        CalModeOn = false;
        if (App.Device.Mode == OperationMode.Verification)
        {
          var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Instrument_IsIn_ValMode),
            Language.TranslationSource.Instance.CurrentCulture);
          Notification.Show(msg);
          return;
        }
        ReturnToNormal();
      }
      else
      {
        ReturnToNormal();
      }
    }

    public void ValModeToggle()
    {
      void ReturnToNormal()
      {
        App.Device.Mode = OperationMode.Normal;
        Volumes[0] = _dbsampleVolumeTempHolder;
        App.Device.MainCommand("Set Property", code: 0xaf, parameter: ushort.Parse(_dbsampleVolumeTempHolder));
        MainButtonsViewModel.Instance.Flavor[0] = null;
        MainWindow.Instance.wndw.Background = (System.Windows.Media.SolidColorBrush)App.Current.Resources["AppBackground"];
        ResultsViewModel.Instance.ValidationCoverVisible = Visibility.Hidden;
        UnlockMapSelection();
      }

      UserInputHandler.InputSanityCheck();
      if (ValModeOn)
      {
        if (App.Device.Mode == OperationMode.Normal && VerificationViewModel.Instance.ValMapInfoReady())
        {
          _dbsampleVolumeTempHolder = Volumes[0];
          SetFixedVolumeButtonClick(25);
          App.Device.Mode = OperationMode.Verification;
          MainButtonsViewModel.Instance.Flavor[0] = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Maintenance_Validation),
            Language.TranslationSource.Instance.CurrentCulture);
          MainWindow.Instance.wndw.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(97, 162, 135));
          ResultsViewModel.Instance.ValidationCoverVisible = Visibility.Visible;
          LockMapSelection();
          return;
        }
        ValModeOn = false;
        if (App.Device.Mode == OperationMode.Calibration)
        {
          var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Instrument_IsIn_CalMode),
            Language.TranslationSource.Instance.CurrentCulture);
          Notification.Show(msg);
          return;
        }
        ReturnToNormal();
      }
      else
      {
        ReturnToNormal();
      }
    }

    private static void LockMapSelection()
    {
      Views.DashboardView.Instance.MapSelectr.IsEnabled = false;
      Views.CalibrationView.Instance.MapSelectr.IsEnabled = false;
      Views.VerificationView.Instance.MapSelectr.IsEnabled = false;
      Views.ChannelsView.Instance.MapSelectr.IsEnabled = false;
    }

    private static void UnlockMapSelection()
    {
      Views.DashboardView.Instance.MapSelectr.IsEnabled = true;
      Views.CalibrationView.Instance.MapSelectr.IsEnabled = true;
      Views.VerificationView.Instance.MapSelectr.IsEnabled = true;
      Views.ChannelsView.Instance.MapSelectr.IsEnabled = true;
    }

    private static void LockEndReadSelection()
    {
      Views.DashboardView.Instance.EndReadSelectr.IsEnabled = false;
    }

    private static void UnLockEndReadSelection()
    {
      Views.DashboardView.Instance.EndReadSelectr.IsEnabled = true;
    }

    public void DropPress()
    {
      UserInputHandler.InputSanityCheck();
    }

    public void SetCalibrationDate(string date)
    {
      if (date == null)
      {
        CaliDateBox[0] = null;
        return;
      }
      var year = date.Substring(date.LastIndexOf(".") + 1);
      var month = date.Substring(date.IndexOf(".") + 1,2);
      var day = date.Substring(0,2);
      CaliDateBox[0] = $"{year}.{month}.{day}";
    }

    public void SetValidationDate(string date)
    {
      if (date == null)
      {
        ValidDateBox[0] = null;
        return;
      }
      var year = date.Substring(date.LastIndexOf(".") + 1);
      var month = date.Substring(date.IndexOf(".") + 1, 2);
      var day = date.Substring(0, 2);
      ValidDateBox[0] = $"{year}.{month}.{day}";
    }

    public void OnMapChanged(CustomMap map)
    {
      if (map.validation)
      {
        CalValModeEnabled = true;
        SetCalibrationDate(map.caltime);
        SetValidationDate(map.valtime);
      }
      else
      {
        CalValModeEnabled = false;
        SetCalibrationDate(null);
        SetValidationDate(null);
      }
      bool Warning = map.IsVerificationExpired((VerificationExpirationTime)Settings.Default.VerificationWarningIndex);
      VerificationWarningVisible = Warning ? Visibility.Visible : Visibility.Hidden;
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
      private static DashboardViewModel _vm;
      public DropDownButtonContents(string content, DashboardViewModel vm = null)
      {
        if (_vm == null)
        {
          _vm = vm;
        }
        Content = content;
        Index = _nextIndex++;
      }

      public void Click(int num)
      {
        switch (num)
        {
          case 1:
            _vm.SelectedSpeedContent = Content;
            _vm.SelectedSpeedIndex = (WellReadingSpeed)Index;
            App.Device.SetHardwareParameter(DeviceParameterType.WellReadingSpeed, Index);
            break;
          case 2:
            _vm.SelectedClassiMapContent = Content;
            App.SetActiveMap(Content);
            App.MapRegions.FillRegions();
            VerificationViewModel.Instance.LoadClick(fromCode: true);
            NormalizationViewModel.Instance.Load();
            break;
          case 3:
            _vm.SelectedChConfigContent = Content;
            _vm.SelectedChConfigIndex = (ChannelConfiguration)Index;
            App.Device.SetHardwareParameter(DeviceParameterType.ChannelConfiguration, Index);
            break;
          case 4:
            _vm.SelectedOrderContent = Content;
            _vm.SelectedOrderIndex = (WellReadingOrder)Index;
            App.Device.SetHardwareParameter(DeviceParameterType.WellReadingOrder, Index);
            break;
          case 5:
            _vm.SelectedSysControlContent = Content;
            _vm.SelectedSystemControlIndex = Index;
            App.SetSystemControl(Index);
            if (Index != 0)
            {
              ExperimentViewModel.Instance.WellSelectVisible = Visibility.Hidden;
              MainButtonsViewModel.Instance.StartButtonEnabled = false;
              _vm.WorkOrderVisibility = Visibility.Visible;
            }
            else
            {
              ExperimentViewModel.Instance.WellSelectVisible = Visibility.Visible;
              MainButtonsViewModel.Instance.StartButtonEnabled = true;
              _vm.WorkOrderVisibility = Visibility.Hidden;
            }
            break;
          case 6:
            _vm.SelectedEndReadContent = Content;
            _vm.SelectedEndReadIndex = Index;
            App.SetTerminationType(Index);
            _vm.EndReadVisibilitySwitch();
            break;
        }
      }
      
      public static void ResetIndex()
      {
        _nextIndex = 0;
      }
    }
  }
}