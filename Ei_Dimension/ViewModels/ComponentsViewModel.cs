using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ComponentsViewModel
  {
    public virtual ObservableCollection<string> InputSelectorState { get; set; }
    public int IInputSelectorState { get; set; }

    public virtual ObservableCollection<bool> ValvesStates { get; set; }

    public virtual ObservableCollection<bool> LasersActive { get; set; }
    public virtual ObservableCollection<string> LaserRedPowerValue { get; set; }
    public virtual ObservableCollection<string> LaserGreenPowerValue { get; set; }
    public virtual ObservableCollection<string> LaserVioletPowerValue { get; set; }

    public virtual ObservableCollection<DropDownButtonContents> SyringeControlItems { get; set; }
    public virtual string SelectedSheathContent { get; set; }
    public virtual string SelectedSampleAContent { get; set; }
    public virtual string SelectedSampleBContent { get; set; }
    public virtual ObservableCollection<string> SyringeControlSheathValue { get; set; }
    public virtual ObservableCollection<string> SyringeControlSampleAValue { get; set; }
    public virtual ObservableCollection<string> SyringeControlSampleBValue { get; set; }
    public virtual string GetPositionToggleButtonState { get; set; }
    public virtual ObservableCollection<bool> GetPositionToggleButtonStateBool { get; set; }
    public virtual ObservableCollection<string> GetPositionTextBoxInputs { get; set; }

    public virtual bool SamplingActive { get; set; }
    public virtual bool SingleStepDebugActive { get; set; }

    public virtual ObservableCollection<string> IdexTextBoxInputs { get; set; }
    public virtual bool CWDirectionActive { get; set; }

    public virtual string MaxPressureBox { get; set; }

    public static ComponentsViewModel Instance { get; private set; }
    public virtual bool SuppressWarnings { get; set; }

    private byte[] _syringeControlStates;
    private ushort _activeLasers;

    protected ComponentsViewModel()
    {
      ValvesStates = new ObservableCollection<bool> { false, false, false, false };
      InputSelectorState = new ObservableCollection<string>();
      InputSelectorState.Add(Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Components_To_Pickup),
        Language.TranslationSource.Instance.CurrentCulture));
      InputSelectorState.Add(Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Components_To_Cuvet),
        Language.TranslationSource.Instance.CurrentCulture));
      IInputSelectorState = 0;

      LasersActive = new ObservableCollection<bool> { true, true, true };
      LaserRedPowerValue = new ObservableCollection<string> {""};
      LaserGreenPowerValue = new ObservableCollection<string> {""};
      LaserVioletPowerValue = new ObservableCollection<string> {""};
      _activeLasers = 7;

      var RM = Language.Resources.ResourceManager;
      var curCulture = Language.TranslationSource.Instance.CurrentCulture;
      SyringeControlItems = new ObservableCollection<DropDownButtonContents>
      {
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Halt), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Move_Absolute), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Pickup), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Pre_inject), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Speed), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Initialize), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Boot), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Valve_Left), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Valve_Right), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Micro_step), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Speed_Preset), curCulture), this),
        new DropDownButtonContents(RM.GetString(nameof(Language.Resources.Dropdown_Pos), curCulture), this)
      };
      SelectedSheathContent = SyringeControlItems[0].Content;
      SelectedSampleAContent = SyringeControlItems[0].Content;
      SelectedSampleBContent = SyringeControlItems[0].Content;
      _syringeControlStates = new byte[]{ 0, 0, 0 };

      SyringeControlSheathValue = new ObservableCollection<string> {""};
      SyringeControlSampleAValue = new ObservableCollection<string> {""};
      SyringeControlSampleBValue = new ObservableCollection<string> {""};
      
      GetPositionToggleButtonState = RM.GetString(nameof(Language.Resources.OFF), Language.TranslationSource.Instance.CurrentCulture);
      GetPositionToggleButtonStateBool = new ObservableCollection<bool> { false };
      GetPositionTextBoxInputs = new ObservableCollection<string> {"", "", ""};

      SamplingActive = false;
      SingleStepDebugActive = false;

      IdexTextBoxInputs = new ObservableCollection<string> { "", "" };
      CWDirectionActive = false;
      MaxPressureBox = Settings.Default.MaxPressure.ToString();
      SuppressWarnings = Settings.Default.SuppressWarnings;

      Instance = this;
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
      App.Device.MainCommand("Set Property", code: 0x18, parameter: (ushort)IInputSelectorState);
    }

    public void ValvesButtonClick(int num)
    {
      UserInputHandler.InputSanityCheck();
      int param = 0;
      byte Code = 0x00;
      switch (num)
      {
        case 1:
          ValvesStates[0] = !ValvesStates[0];
          param = ValvesStates[0] ? 1 : 0;
          Code = 0x13;
          break;
        case 2:
          ValvesStates[1] = !ValvesStates[1];
          param = ValvesStates[1] ? 1 : 0;
          Code = 0x12;
          break;
        case 3:
          ValvesStates[2] = !ValvesStates[2];
          param = ValvesStates[2] ? 1 : 0;
          Code = 0x10;
          break;
        case 4:
          ValvesStates[3] = !ValvesStates[3];
          param = ValvesStates[3] ? 1 : 0;
          Code = 0x11;
          break;
      }
      App.Device.MainCommand("Set Property", code: Code, parameter: (ushort)param);
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
        case 3:
          LasersActive[2] = !LasersActive[2];
          if (LasersActive[2])
            _activeLasers += 4;
          else
            _activeLasers -= 4;
          break;
      }
      App.Device.MainCommand("Set Property", code: 0xc0, parameter: _activeLasers);
    }

    public void SheathRunButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      if (SyringeControlSheathValue[0] == "")
        App.Device.MainCommand("Sheath", cmd: _syringeControlStates[0]);
      else if (ushort.TryParse(SyringeControlSheathValue[0], out ushort usRes))
        App.Device.MainCommand("Sheath", cmd: _syringeControlStates[0], parameter: usRes);
    }

    public void SampleARunButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      if (SyringeControlSampleAValue[0] == "")
        App.Device.MainCommand("SampleA", cmd: _syringeControlStates[1]);
      else if (ushort.TryParse(SyringeControlSampleAValue[0], out ushort usRes))
        App.Device.MainCommand("SampleA", cmd: _syringeControlStates[1], parameter: usRes);
    }

    public void SampleBRunButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      if (SyringeControlSampleBValue[0] == "")
        App.Device.MainCommand("SampleB", cmd: _syringeControlStates[2]);
      else if (ushort.TryParse(SyringeControlSampleBValue[0], out ushort usRes))
        App.Device.MainCommand("SampleB", cmd: _syringeControlStates[2], parameter: usRes);
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
      App.Device.MainCommand("Set Property", code: 0x15, parameter: state);
    }

    public void GetPositionButtonsClick(int num)
    {
      UserInputHandler.InputSanityCheck();
      ushort param = 0;
      switch (num)
      {
        case 1:
          param = 0;
          break;
        case 2:
          param = 1;
          break;
        case 3:
          param = 2;
          break;
      }
      App.Device.MainCommand("Get FProperty", code: 0x14, parameter: param);
    }

    public void SamplingToggleButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      SamplingActive = !SamplingActive;
      if(SamplingActive)
        App.Device.MainCommand("Start Sampling");
      else
        App.Device.MainCommand("End Sampling");
    }

    public void SingleStepDebugToggleButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      SingleStepDebugActive = !SingleStepDebugActive;
      var param = SingleStepDebugActive ? 1 : 0;
      App.Device.MainCommand("Set Property", code: 0xf7, parameter: (ushort)param);
    }

    public void FlushButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      App.Device.MainCommand("FlushCmdQueue", cmd: 0x02);
    }

    public void ClearButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      App.Device.MainCommand("Set Property", code: 0xcc);
    }

    public void StartupButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      App.Device.MainCommand("Startup");
    }

    public void MoveIdexButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      App.Device.MainCommand("Idex");
    }

    public void CWDirectionToggleButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      CWDirectionActive = !CWDirectionActive;
      MicroCy.InstrumentParameters.Idex.Dir = CWDirectionActive ? 1 : 0;
    }

    public void IdexPositionButtonClick()
    {
      UserInputHandler.InputSanityCheck();
      App.Device.MainCommand("Get Property", code: 0x04);
    }

    public void SuppressWarningsClick()
    {
      SuppressWarnings = !SuppressWarnings;
      Settings.Default.SuppressWarnings = SuppressWarnings;
      Settings.Default.Save();
    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(IdexTextBoxInputs)), this, 0, Views.ComponentsView.Instance.TB0);
          MainViewModel.Instance.NumpadToggleButton(Views.ComponentsView.Instance.TB0);
          break;
        case 1:
          UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(IdexTextBoxInputs)), this, 1, Views.ComponentsView.Instance.TB1);
          MainViewModel.Instance.NumpadToggleButton(Views.ComponentsView.Instance.TB1);
          break;
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
      public byte Index { get; set; }
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

      public void Click(int num)
      {
        switch (num)
        {
          case 1:
            _vm.SelectedSheathContent = Content;
            _vm._syringeControlStates[0] = Index;
            break;
          case 2:
            _vm.SelectedSampleAContent = Content;
            _vm._syringeControlStates[1] = Index;
            break;
          case 3:
            _vm.SelectedSampleBContent = Content;
            _vm._syringeControlStates[2] = Index;
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