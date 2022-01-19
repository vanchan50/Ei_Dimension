using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Ei_Dimension.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using MicroCy;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class DashboardViewModel
  {
    public virtual ObservableCollection<string> EndRead { get; set; }
    public virtual ObservableCollection<string> Volumes { get; set; }
    public virtual ObservableCollection<string> WorkOrder { get; set; }
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
    public virtual ObservableCollection<string> PressureMon { get; set; }
    public virtual bool PressureMonToggleButtonState { get; set; }
    public double MaxPressure { get; set; }
    public double MinPressure { get; set; }
    public virtual bool CalValModeEnabled { get; set; }
    public virtual bool CalModeOn { get; set; }
    public virtual bool ValModeOn { get; set; }
    public virtual ObservableCollection<string> CaliDateBox { get; set; }
    public virtual ObservableCollection<string> ValidDateBox { get; set; }
    public static DashboardViewModel Instance { get; private set; }

    public byte SelectedSystemControlIndex { get; set; }
    public byte SelectedSpeedIndex { get; set; }
    public byte SelectedChConfigIndex { get; set; }
    public byte SelectedOrderIndex { get; set; }
    public byte SelectedEndReadIndex { get; set; }
    public virtual ObservableCollection<Visibility> EndReadVisibility { get; set; }
    public virtual Visibility WorkOrderVisibility { get; set; }
    public virtual Visibility VerificationWarningVisible { get; set; }
    

    private string _dbsampleVolumeTempHolder;
    private int _dbEndReadIndexTempHolder;

    protected DashboardViewModel()
    {
      EndRead = new ObservableCollection<string> { "100", "500" };

      Volumes = new ObservableCollection<string> { "0", "0", "0" };
      WorkOrder = new ObservableCollection<string> { "" };

      var RM = Language.Resources.ResourceManager;
      var curCulture = Language.TranslationSource.Instance.CurrentCulture;
      SpeedItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Normal), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Hi_Speed), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Hi_Sens), curCulture), this)
      };
      SelectedSpeedIndex = 0;
      SelectedSpeedContent = SpeedItems[SelectedSpeedIndex].Content;
      DropDownButtonContents.ResetIndex();

      ClassiMapItems = new ObservableCollection<DropDownButtonContents>();
      if (App.Device.MapCtroller.MapList.Count > 0)
      {
        foreach (var map in App.Device.MapCtroller.MapList)
        {
          ClassiMapItems.Add(new DropDownButtonContents(map.mapName, this));
        }
      }
      else
        ClassiMapItems.Add(new DropDownButtonContents("No map", this));
      SelectedClassiMapContent = App.Device.MapCtroller.ActiveMap.mapName;
      DropDownButtonContents.ResetIndex();

      ChConfigItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Standard), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Cells), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_FM3D), curCulture), this)
      };
      SelectedChConfigIndex = 0;
      SelectedChConfigContent = ChConfigItems[SelectedChConfigIndex].Content;
      DropDownButtonContents.ResetIndex();

      OrderItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Column), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Row), curCulture), this),
      };
      SelectedOrderIndex = 0; //Column
      SelectedOrderContent = OrderItems[SelectedOrderIndex].Content;
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
      SelectedEndReadIndex = MicroCyDevice.TerminationType;
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

      PressureMonToggleButtonState = false;
      PressureMon = new ObservableCollection<string> {"","",""};

      CalValModeEnabled = App.Device.MapCtroller.ActiveMap.validation ? true : false;
      CalModeOn = false;
      ValModeOn = false;
      CaliDateBox = new ObservableCollection<string> { App.Device.MapCtroller.ActiveMap.caltime };
      ValidDateBox = new ObservableCollection<string> { App.Device.MapCtroller.ActiveMap.valtime };
      _dbsampleVolumeTempHolder = null;
      _dbEndReadIndexTempHolder = 0;

      Instance = this;
    }

    public static DashboardViewModel Create()
    {
      return ViewModelSource.Create(() => new DashboardViewModel());
    }

    public void PressureMonToggleButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      PressureMonToggleButtonState = !PressureMonToggleButtonState;
      MaxPressure = 0;
      MinPressure = 9999999999;
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
      App.Device.MainCommand("Set Property", code: 0x19, parameter: 1); //bubble detect on
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
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }

    private Wells MakeWell(byte row, byte col)
    {
      _ = short.TryParse(Volumes[0], out var volRes);
      _ = short.TryParse(Volumes[1], out var washRes);
      _ = short.TryParse(Volumes[2], out var agitRes);
      
      return new MicroCy.Wells
      {
        rowIdx = row,
        colIdx = col,
        runSpeed = SelectedSpeedIndex,
        sampVol = volRes,
        washVol = washRes,
        agitateVol = agitRes,
        termType = MicroCyDevice.TerminationType,
        chanConfig = SelectedChConfigIndex,
        regTermCnt = MicroCyDevice.MinPerRegion,
        termCnt = MicroCyDevice.BeadsToCapture
      };
    }

    public void SetWellsInOrder()
    {
      MicroCyDevice.WellsInOrder.Clear();
      if (WellsSelectViewModel.Instance.CurrentTableSize > 1)
      {
        ObservableCollection<WellTableRow> plate = WellsSelectViewModel.Instance.CurrentTableSize == 96 ?
          WellsSelectViewModel.Instance.Table96Wells : WellsSelectViewModel.Instance.Table384Wells;
        if (SelectedSystemControlIndex == 0)  //manual control of plate //TODO: SystemControl can be removed from device fields maybe?
        {
          for (byte r = 0; r < plate.Count; r++)
          {
            for (byte c = 0; c < plate[r].Types.Count; c++)
            {
              if (plate[r].Types[c] != WellType.Empty)
                MicroCyDevice.WellsInOrder.Add(MakeWell(r, c));
            }
          }
          if (SelectedOrderIndex == 0)
          {
            //sort list by col/row
            MicroCyDevice.WellsInOrder = MicroCyDevice.WellsInOrder.OrderBy(x => x.colIdx).ThenBy(x => x.rowIdx).ToList();
          }
        }
        else  //Work Order control of plate
        {
          //fill wells from work order
          MicroCyDevice.WellsInOrder = MicroCyDevice.WorkOrder.woWells;
        }
      }
      else if (WellsSelectViewModel.Instance.CurrentTableSize == 1)  //tube
        MicroCyDevice.WellsInOrder.Add(MakeWell(0, 0));  //  a 1 record work order

      MicroCyDevice.WellsToRead = MicroCyDevice.WellsInOrder.Count - 1;  //make zero based like well index is
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
      UserInputHandler.InputSanityCheck();
      if (CalModeOn)
      {
        if (MicroCyDevice.Mode == OperationMode.Normal)
        {
          _dbsampleVolumeTempHolder = Volumes[0];
          SetFixedVolumeButtonClick(100);
          MicroCyDevice.Mode = OperationMode.Calibration;
          CalibrationViewModel.Instance.CalFailsInARow = 0;
          CalibrationViewModel.Instance.MakeCalMap();
          _dbEndReadIndexTempHolder = SelectedEndReadIndex;
          EndReadItems[2].Click(6);
          MainButtonsViewModel.Instance.Flavor[0] = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Maintenance_Calibration),
            Language.TranslationSource.Instance.CurrentCulture);
          MainWindow.Instance.wndw.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 191));
          App.LockMapSelection();
          App.Device.MainCommand("Set Property", code: 0x1b, parameter: 1);
          return;
        }
        CalModeOn = false;
        if (MicroCyDevice.Mode == OperationMode.Verification)
          Notification.Show("The instrument is in Verificationtion mode");
      }
      else
      {
        MicroCyDevice.Mode = OperationMode.Normal;
        EndReadItems[_dbEndReadIndexTempHolder].Click(6);
        Volumes[0] = _dbsampleVolumeTempHolder;
        App.Device.MainCommand("Set Property", code: 0xaf, parameter: ushort.Parse(_dbsampleVolumeTempHolder));
        MainButtonsViewModel.Instance.Flavor[0] = null;
        MainWindow.Instance.wndw.Background = (System.Windows.Media.SolidColorBrush)App.Current.Resources["AppBackground"];
        App.UnlockMapSelection();
        App.Device.MainCommand("Set Property", code: 0x1b, parameter: 0);
      }
    }

    public void ValModeToggle()
    {
      UserInputHandler.InputSanityCheck();
      if (ValModeOn)
      {
        if (MicroCyDevice.Mode == OperationMode.Normal && VerificationViewModel.Instance.ValMapInfoReady())
        {
          _dbsampleVolumeTempHolder = Volumes[0];
          SetFixedVolumeButtonClick(25);
          MicroCyDevice.Mode = OperationMode.Verification;
          MainButtonsViewModel.Instance.Flavor[0] = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Maintenance_Validation),
            Language.TranslationSource.Instance.CurrentCulture);
          MainWindow.Instance.wndw.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(97, 162, 135));
          ResultsViewModel.Instance.ValidationCoverVisible = Visibility.Visible;
          App.LockMapSelection();
          return;
        }
        ValModeOn = false;
        if (MicroCyDevice.Mode == OperationMode.Calibration)
          Notification.Show("The instrument is in Calibration mode");
      }
      else
      {
        MicroCyDevice.Mode = OperationMode.Normal;
        Volumes[0] = _dbsampleVolumeTempHolder;
        App.Device.MainCommand("Set Property", code: 0xaf, parameter: ushort.Parse(_dbsampleVolumeTempHolder));
        MainButtonsViewModel.Instance.Flavor[0] = null;
        MainWindow.Instance.wndw.Background = (System.Windows.Media.SolidColorBrush)App.Current.Resources["AppBackground"];
        ResultsViewModel.Instance.ValidationCoverVisible = Visibility.Hidden;
        App.UnlockMapSelection();
      }
    }

    public void DropPress()
    {
      UserInputHandler.InputSanityCheck();
    }

    private static void SetSystemControl(byte num)
    {
      MicroCyDevice.SystemControl = num;
      Settings.Default.SystemControl = num;
      Settings.Default.Save();
    }

    private static void SetTerminationType(byte num)
    {
      MicroCyDevice.TerminationType = num;
      Settings.Default.EndRead = num;
      Settings.Default.Save();
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
            _vm.SelectedSpeedIndex = Index;
            App.Device.MainCommand("Set Property", code: 0xaa, parameter: (ushort)Index);
            break;
          case 2:
            _vm.SelectedClassiMapContent = Content;
            App.SetActiveMap(Content);
            App.Device.MainCommand("Set Property", code: 0xa9, parameter: (ushort)Index);
            App.MapRegions.FillRegions();
            VerificationViewModel.Instance.LoadClick(fromCode: true);
            break;
          case 3:
            _vm.SelectedChConfigContent = Content;
            _vm.SelectedChConfigIndex = Index;
            App.Device.MainCommand("Set Property", code: 0xc2, parameter: (ushort)Index);
            break;
          case 4:
            _vm.SelectedOrderContent = Content;
            _vm.SelectedOrderIndex = Index;
            App.Device.MainCommand("Set Property", code: 0xa8, parameter: (ushort)Index);
            break;
          case 5:
            _vm.SelectedSysControlContent = Content;
            _vm.SelectedSystemControlIndex = Index;
            SetSystemControl(Index);
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
            SetTerminationType(Index);
            _vm.EndReadVisibilitySwitch();
            break;
        }
      }

      public void ForAppUpdater(int num)
      {
        switch (num)
        {
          case 1:
            _vm.SelectedSpeedContent = Content;
            _vm.SelectedSpeedIndex = Index;
            break;
          case 2:
            _vm.SelectedClassiMapContent = Content;
            break;
          case 3:
            _vm.SelectedChConfigContent = Content;
            _vm.SelectedChConfigIndex = Index;
            break;
          case 4:
            _vm.SelectedOrderContent = Content;
            _vm.SelectedOrderIndex = Index;
            break;
          case 5:
            _vm.SelectedSysControlContent = Content;
            _vm.SelectedSystemControlIndex = Index;
            SetSystemControl(Index);
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
            SetTerminationType(Index);
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