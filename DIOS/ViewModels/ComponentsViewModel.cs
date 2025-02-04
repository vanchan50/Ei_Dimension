using System;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using DIOS.Core.HardwareIntercom;

namespace Ei_Dimension.ViewModels;

[POCOViewModel]
public class ComponentsViewModel
{
  public virtual ObservableCollection<string> InputSelectorState { get; set; }
  public int IInputSelectorState { get; set; } = 0;

  public virtual ObservableCollection<bool> ValvesStates { get; set; } = new(){ false, false, false, false };

  public virtual ObservableCollection<bool> LasersActive { get; set; } = new(){ true, true };
  public virtual ObservableCollection<string> LaserRedPowerValue { get; set; } = new(){ "" };
  public virtual ObservableCollection<string> LaserGreenPowerValue { get; set; } = new(){ "" };

  public virtual ObservableCollection<DropDownButtonContents> SyringeControlItems { get; set; }
  public virtual string SelectedSheathContent { get; set; }
  public virtual string SelectedSampleAContent { get; set; }
  public virtual string SelectedSampleBContent { get; set; }
  public virtual ObservableCollection<string> SyringeControlSheathValue  { get; set; } = new(){ "" };
  public virtual ObservableCollection<string> SyringeControlSampleAValue { get; set; } = new(){ "" };
  public virtual ObservableCollection<string> SyringeControlSampleBValue { get; set; } = new(){ "" };
  public virtual string GetPositionToggleButtonState { get; set; }
  public virtual ObservableCollection<bool> GetPositionToggleButtonStateBool { get; set; } = new(){ false };
  public virtual ObservableCollection<string> GetPositionTextBoxInputs { get; set; } = new(){ "", "", "" };

  public virtual bool SamplingActive { get; set; } = false;
  public virtual bool SingleStepDebugActive { get; set; } = false;

  public virtual ObservableCollection<string> MaxPressureBox { get; set; } = new(){ "" };
  public virtual ObservableCollection<string> StatisticsCutoffBox { get; set; }

  public virtual bool PressureMonToggleButtonState { get; set; }
  public virtual bool PressureUnitToggleButtonState { get; set; }
  public virtual ObservableCollection<string> PressureUnit { get; set; } = new(){ "" };
  public virtual ObservableCollection<string> PressureMon { get; set; } = new(){ "", "" };
  public double ActualPressure { get; set; }
  public double MaxPressure { get; set; } //TextBoxHandler line 114
  public const double TOKILOPASCALCOEFFICIENT = 6.89476;

  public static ComponentsViewModel Instance { get; private set; }
  public virtual bool SuppressWarnings { get; set; }
  public virtual bool ContinuousModeOn { get; set; }
  public virtual string SelectedChConfigContent { get; set; }
  public virtual ObservableCollection<DropDownButtonContents> ChConfigItems { get; set; }
  public ChannelConfiguration SelectedChConfigIndex { get; set; } = ChannelConfiguration.Standard;

  public byte[] SyringeControlStates { get; } = { 0, 0, 0 };
  private ushort _activeLasers = 7;
  private readonly string _loaderPath;
  private const string BOOTLOADEREXEPATH = "DIOS_FW_Loader.exe";

  protected ComponentsViewModel()
  {
    InputSelectorState = new ObservableCollection<string>
    {
      Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Components_To_Cuvet),
        Language.TranslationSource.Instance.CurrentCulture),
      Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Components_To_Pickup),
        Language.TranslationSource.Instance.CurrentCulture)
    };
    var RM = Language.Resources.ResourceManager;
    var curCulture = Language.TranslationSource.Instance.CurrentCulture;
    SyringeControlItems = new ObservableCollection<DropDownButtonContents>
    {
      new(Language.TranslationSource.Instance[nameof(Language.Resources.Dropdown_Halt)], this),
      new(Language.TranslationSource.Instance[nameof(Language.Resources.Dropdown_Move_Absolute)], this),
      new(Language.TranslationSource.Instance[nameof(Language.Resources.Dropdown_Pickup)], this),
      new(Language.TranslationSource.Instance[nameof(Language.Resources.Dropdown_Pre_inject)], this),
      new(Language.TranslationSource.Instance[nameof(Language.Resources.Dropdown_Speed)], this),
      new(Language.TranslationSource.Instance[nameof(Language.Resources.Dropdown_Initialize)], this),
      new(Language.TranslationSource.Instance[nameof(Language.Resources.Dropdown_Boot)], this),
      new(Language.TranslationSource.Instance[nameof(Language.Resources.Dropdown_Valve_Left)], this),
      new(Language.TranslationSource.Instance[nameof(Language.Resources.Dropdown_Valve_Right)], this),
      new(Language.TranslationSource.Instance[nameof(Language.Resources.Dropdown_Micro_step)], this),
      new(Language.TranslationSource.Instance[nameof(Language.Resources.Dropdown_Speed_Preset)], this),
      new(Language.TranslationSource.Instance[nameof(Language.Resources.Dropdown_Pos)], this)
    };
    DropDownButtonContents.ResetIndex();
    SelectedSheathContent = SyringeControlItems[0].Content;
    SelectedSampleAContent = SyringeControlItems[0].Content;
    SelectedSampleBContent = SyringeControlItems[0].Content;
      
    GetPositionToggleButtonState = Language.TranslationSource.Instance[nameof(Language.Resources.OFF)];
      
    StatisticsCutoffBox = new ObservableCollection<string> { (100 * Settings.Default.StatisticsTailDiscardPercentage).ToString() };
    SuppressWarnings = Settings.Default.SuppressWarnings;

    PressureUnitToggleButtonState = Settings.Default.PressureUnitsPSI;

    ChConfigItems = new ObservableCollection<DropDownButtonContents>
    {
      new(RM.GetString(nameof(Language.Resources.Dropdown_Standard), curCulture), this),
      new(RM.GetString(nameof(Language.Resources.Dropdown_StandardSpectraplex), curCulture), this),
      new(RM.GetString(nameof(Language.Resources.Dropdown_FM3D), curCulture), this),
      new(RM.GetString(nameof(Language.Resources.Dropdown_StandardPlusFSC), curCulture), this),
      new(RM.GetString(nameof(Language.Resources.Dropdown_OEMA), curCulture), this, 5),//intentionally skipping index = 4
      new(RM.GetString(nameof(Language.Resources.Dropdown_OEMSpectraplex), curCulture), this, 6)
    };
    SelectedChConfigContent = ChConfigItems[(int)SelectedChConfigIndex].Content;
    DropDownButtonContents.ResetIndex();

    Instance = this;

    _loaderPath = $"{AppDomain.CurrentDomain.BaseDirectory}{BOOTLOADEREXEPATH}";
  }

  public static ComponentsViewModel Create()
  {
    return ViewModelSource.Create(() => new ComponentsViewModel());
  }

  public void InputSelectorSwapButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    var temp = InputSelectorState[0];
    InputSelectorState[0] = InputSelectorState[1];
    InputSelectorState[1] = temp;
    IInputSelectorState = IInputSelectorState == 0 ? 1 : 0;
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.IsInputSelectorAtPickup, IInputSelectorState);
  }

  public void ValvesButtonClick(int num)
  {
    //ValveCuvetDrain,//3
    //ValveFan1,//4
    //ValveFan2//1
    UserInputHandler.InputSanityCheck();
    int param = 0;
    DeviceParameterType parameter = DeviceParameterType.ValveCuvetDrain;
    switch (num)
    {
      case 1:
        ValvesStates[0] = !ValvesStates[0];
        param = ValvesStates[0] ? 1 : 0;
        parameter = DeviceParameterType.WashPump;
        break;
      case 2: //not used
        ValvesStates[1] = !ValvesStates[1];
        param = ValvesStates[1] ? 1 : 0;
        break;
      case 3:
        ValvesStates[2] = !ValvesStates[2];
        param = ValvesStates[2] ? 1 : 0;
        parameter = DeviceParameterType.ValveCuvetDrain;
        break;
      case 4:
        ValvesStates[3] = !ValvesStates[3];
        param = ValvesStates[3] ? 1 : 0;
        parameter = DeviceParameterType.ValveFan1;
        break;
    }
    App.DiosApp.Device.Hardware.SetParameter(parameter, param);
  }

  public void PressureMonToggleButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    PressureMonToggleButtonState = !PressureMonToggleButtonState;
    MaxPressure = 0;
  }

  public void PressureUnitButtonClick()
  {
    Settings.Default.PressureUnitsPSI = !Settings.Default.PressureUnitsPSI;
    Settings.Default.Save();
    PressureUnitToggleButtonState = Settings.Default.PressureUnitsPSI;

    var RM = Language.Resources.ResourceManager;
    var curCulture = Language.TranslationSource.Instance.CurrentCulture;
    if (Settings.Default.PressureUnitsPSI)
    {
      PressureUnit[0] = RM.GetString(nameof(Language.Resources.Pressure_Units_PSI),
        curCulture);
      PressureMon[0] = ActualPressure.ToString("F3");
      PressureMon[1] = MaxPressure.ToString("F3");
      MaxPressureBox[0] = (double.Parse(MaxPressureBox[0]) / TOKILOPASCALCOEFFICIENT).ToString("F3");
    }
    else
    {
      PressureUnit[0] = RM.GetString(nameof(Language.Resources.Pressure_Units_kPa),
        curCulture);

      var actual = ActualPressure * TOKILOPASCALCOEFFICIENT;
      var maxPressure = MaxPressure * TOKILOPASCALCOEFFICIENT;

      PressureMon[0] = actual.ToString("f3");
      PressureMon[1] = maxPressure.ToString("f3");
      MaxPressureBox[0] = (double.Parse(MaxPressureBox[0]) * TOKILOPASCALCOEFFICIENT).ToString("F3");
    }
  }

  public void LasersButtonClick(int num)
  {
    UserInputHandler.InputSanityCheck();
    switch (num)
    {
      case 1:
        LasersActive[0] = !LasersActive[0];
        if (LasersActive[0])
          _activeLasers += 1;
        else
          _activeLasers -= 1;
        break;
      case 2:
        LasersActive[1] = !LasersActive[1];
        if (LasersActive[1])
          _activeLasers += 2;
        else
          _activeLasers -= 2;
        break;
    }
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.IsLaserActive, _activeLasers);
  }

  public void SheathRunButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    if (SyringeControlSheathValue[0] == "")
      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.PumpSheath, (SyringeControlState)SyringeControlStates[0], 0);
    else if (ushort.TryParse(SyringeControlSheathValue[0], out ushort usRes))
      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.PumpSheath, (SyringeControlState)SyringeControlStates[0], usRes);
  }

  public void SampleARunButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    if (SyringeControlSampleAValue[0] == "")
      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.PumpSampleA, (SyringeControlState)SyringeControlStates[1], 0);
    else if (ushort.TryParse(SyringeControlSampleAValue[0], out ushort usRes))
      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.PumpSampleA, (SyringeControlState)SyringeControlStates[1], usRes);
  }

  public void SampleBRunButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    if (SyringeControlSampleBValue[0] == "")
      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.PumpSampleB, (SyringeControlState)SyringeControlStates[2], 0);
    else if (ushort.TryParse(SyringeControlSampleBValue[0], out ushort usRes))
      App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.PumpSampleB, (SyringeControlState)SyringeControlStates[2], usRes);
  }

  public void GetPositionToggleButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    var RM = Language.Resources.ResourceManager;
    var curCulture = Language.TranslationSource.Instance.CurrentCulture;
    ushort state = 0;
    if (GetPositionToggleButtonState == RM.GetString(nameof(Language.Resources.OFF), curCulture))
    {
      GetPositionToggleButtonState = RM.GetString(nameof(Language.Resources.ON), curCulture);
      GetPositionToggleButtonStateBool[0] = true;
      state = 1;
    }
    else if (GetPositionToggleButtonState == RM.GetString(nameof(Language.Resources.ON), curCulture))
    {
      GetPositionToggleButtonState = RM.GetString(nameof(Language.Resources.OFF), curCulture);
      GetPositionToggleButtonStateBool[0] = false;
      state = 0;
    }
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.IsSyringePositionActive, state);
  }

  public void GetPositionButtonsClick(int num)
  {
    UserInputHandler.InputSanityCheck();
    SyringePosition pos = SyringePosition.Sheath;
    switch (num)
    {
      case 1:
        pos = SyringePosition.Sheath;
        break;
      case 2:
        pos = SyringePosition.SampleA;
        break;
      case 3:
        pos = SyringePosition.SampleB;
        break;
    }
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.SyringePosition, pos);
  }

  public void SamplingToggleButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    SamplingActive = !SamplingActive;
    if(SamplingActive)
      App.DiosApp.Device.Hardware.SendCommand(DeviceCommandType.StartSampling);
    else
      App.DiosApp.Device.Hardware.SendCommand(DeviceCommandType.EndSampling);
  }

  public void SingleStepDebugToggleButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    SingleStepDebugActive = !SingleStepDebugActive;
    var param = SingleStepDebugActive ? 1 : 0;
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.IsSingleStepDebugActive, param);
  }

  public void FlushButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    App.DiosApp.Device.Hardware.SendCommand(DeviceCommandType.FlushCommandQueue);
  }

  public void ClearButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.SystemActivityStatus);
  }

  public void UpdateFirmwareButtonClick()
  {
    Action Save = () =>
    {
      App.DiosApp.Device.Hardware.SendCommand(DeviceCommandType.UpdateFirmware);
      System.Threading.Thread.Sleep(1500);
      App.Current.Shutdown();

      Process.Start(_loaderPath);
    };
    var msg1 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Firmware_Update_Request),
      Language.TranslationSource.Instance.CurrentCulture);
    var update = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Button_Update),
      Language.TranslationSource.Instance.CurrentCulture);
    var cancel = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Cancel),
      Language.TranslationSource.Instance.CurrentCulture);
    Notification.Show(msg1,
      Save, update,
      null, cancel, fontSize:38);
  }

  public async Task StartupButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    await App.DiosApp.Device.Hardware.SendScriptAsync(DeviceScript.Startup);
  }

  public void SuppressWarningsClick()
  {
    SuppressWarnings = !SuppressWarnings;
    Settings.Default.SuppressWarnings = SuppressWarnings;
    Settings.Default.Save();
  }

  public void ContinuousModeToggle()
  {
    App.DiosApp.RunPlateContinuously = ContinuousModeOn;
  }

  public void DirectMemoryAccessClick()
  {
    DirectMemoryAccessViewModel.Instance.ShowView();
  }

  public void FocusedBox(int num)
  {
    switch (num)
    {
      case 2:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(MaxPressureBox)), this, 0, Views.ComponentsView.Instance.TB2);
        MainViewModel.Instance.NumpadToggleButton(Views.ComponentsView.Instance.TB2);
        break;
      case 5:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SyringeControlSheathValue)), this, 0, Views.ComponentsView.Instance.TB5);
        MainViewModel.Instance.NumpadToggleButton(Views.ComponentsView.Instance.TB5);
        break;
      case 6:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SyringeControlSampleAValue)), this, 0, Views.ComponentsView.Instance.TB6);
        MainViewModel.Instance.NumpadToggleButton(Views.ComponentsView.Instance.TB6);
        break;
      case 7:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(SyringeControlSampleBValue)), this, 0, Views.ComponentsView.Instance.TB7);
        MainViewModel.Instance.NumpadToggleButton(Views.ComponentsView.Instance.TB7);
        break;
      case 8:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(StatisticsCutoffBox)), this, 0, Views.ComponentsView.Instance.CutoffTB);
        MainViewModel.Instance.NumpadToggleButton(Views.ComponentsView.Instance.CutoffTB);
        break;
    }
  }

  public void TextChanged(TextChangedEventArgs e)
  {
    UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
  }

  public class DropDownButtonContents : Core.ObservableObject
  {
    public string Content
    {
      get => _content;
      set {
        _content = value;
        OnPropertyChanged();
      }
    }

    public byte Index { get; }
    private static byte _nextIndex = 0;
    private string _content;
    private static ComponentsViewModel _vm;
    public DropDownButtonContents(string content, ComponentsViewModel vm = null)
    {
      if (_vm == null)
      {
        _vm = vm;
      }
      Content = content;
      Index = _nextIndex++;
    }

    public DropDownButtonContents(string content, ComponentsViewModel vm, byte index)
    {
      if (_vm == null)
      {
        _vm = vm;
      }
      Content = content;
      Index = index;
    }

    public void Click(int num)
    {
      switch (num)
      {
        case 1:
          _vm.SelectedSheathContent = Content;
          _vm.SyringeControlStates[0] = Index;
          break;
        case 2:
          _vm.SelectedSampleAContent = Content;
          _vm.SyringeControlStates[1] = Index;
          break;
        case 3:
          _vm.SelectedSampleBContent = Content;
          _vm.SyringeControlStates[2] = Index;
          break;
        case 4:
          _vm.SelectedChConfigContent = Content;
          _vm.SelectedChConfigIndex = (ChannelConfiguration)Index;
          App.SetChannelConfig((ChannelConfiguration)Index);
          break;
      }
    }

    public static void ResetIndex()
    {
      _nextIndex = 0;
    }
  }
}