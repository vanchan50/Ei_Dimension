using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ComponentsViewModel
  {
    public virtual ObservableCollection<string> InputSelectorState { get; set; }

    public virtual bool[] ValvesStates { get; set; }

    public virtual bool LaserRedActive { get; set; }
    public virtual bool LaserGreenActive { get; set; }
    public virtual bool LaserVioletActive { get; set; }
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
    public virtual ObservableCollection<string> GetPositionTextBoxInputs { get; set; }

    public virtual bool SamplingActive { get; set; }
    public virtual bool SingleStepDebugActive { get; set; }

    public virtual ObservableCollection<string> IdexTextBoxInputs { get; set; }
    public virtual bool CWDirectionActive { get; set; }

    public static ComponentsViewModel Instance { get; private set; }

    protected ComponentsViewModel()
    {
      ValvesStates = new bool[4] { false,false,false,false };
      InputSelectorState = new ObservableCollection<string>();
      InputSelectorState.Add(Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Components_To_Pickup),
        Language.TranslationSource.Instance.CurrentCulture));
      InputSelectorState.Add(Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Components_To_Cuvet),
        Language.TranslationSource.Instance.CurrentCulture));


      LaserRedActive = false;
      LaserGreenActive = false;
      LaserVioletActive = false;
      LaserRedPowerValue = new ObservableCollection<string> {""};
      LaserGreenPowerValue = new ObservableCollection<string> {""};
      LaserVioletPowerValue = new ObservableCollection<string> {""};

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

      SyringeControlSheathValue = new ObservableCollection<string> {""};
      SyringeControlSampleAValue = new ObservableCollection<string> {""};
      SyringeControlSampleBValue = new ObservableCollection<string> {""};

      GetPositionToggleButtonState = RM.GetString(nameof(Language.Resources.OFF),
        Language.TranslationSource.Instance.CurrentCulture);

      GetPositionTextBoxInputs = new ObservableCollection<string> {"", "", ""};

      SamplingActive = false;
      SingleStepDebugActive = false;

      IdexTextBoxInputs = new ObservableCollection<string> { "", "" };
      CWDirectionActive = false;
      Instance = this;
    }

    public static ComponentsViewModel Create()
    {
      return ViewModelSource.Create(() => new ComponentsViewModel());
    }

    public void InputSelectorSwapButtonClick()
    {
      var temp = InputSelectorState[0];
      InputSelectorState[0] = InputSelectorState[1];
      InputSelectorState[1] = temp;
    }

    public void ValvesButtonClick(int num)
    {
      switch (num)
      {
        case 1:
          ValvesStates[0] = !ValvesStates[0];
          break;
        case 2:
          ValvesStates[1] = !ValvesStates[1];
          break;
        case 3:
          ValvesStates[2] = !ValvesStates[2];
          break;
        case 4:
          ValvesStates[3] = !ValvesStates[3];
          break;
      }
    }

    public void LasersButtonClick(int num)
    {
      switch (num)
      {
        case 1:
          LaserRedActive = !LaserRedActive;
          break;
        case 2:
          LaserGreenActive = !LaserGreenActive;
          break;
        case 3:
          LaserVioletActive = !LaserVioletActive;
          break;
      }
    }

    public void SheathRunButtonClick()
    {

    }

    public void SampleARunButtonClick()
    {

    }

    public void SampleBRunButtonClick()
    {

    }

    public void GetPositionToggleButtonClick()
    {
      var RM = Language.Resources.ResourceManager;
      var curCulture = Language.TranslationSource.Instance.CurrentCulture;
      if ( GetPositionToggleButtonState == RM.GetString(nameof(Language.Resources.OFF), curCulture))
      {
        GetPositionToggleButtonState = RM.GetString(nameof(Language.Resources.ON), curCulture);
      }
      else if( GetPositionToggleButtonState == RM.GetString(nameof(Language.Resources.ON), curCulture))
      {
        GetPositionToggleButtonState = RM.GetString(nameof(Language.Resources.OFF), curCulture);
      }
    }

    public void GetPositionButtonsClick(int num)
    {
      switch (num)
      {
        case 1:
          break;
        case 2:
          break;
        case 3:
          break;
      }
    }

    public void SamplingToggleButtonClick()
    {
      SamplingActive = !SamplingActive;
    }

    public void SingleStepDebugToggleButtonClick()
    {
      SingleStepDebugActive = !SingleStepDebugActive;
    }

    public void FlushButtonClick()
    {

    }

    public void StartupButtonClick()
    {
      App.Device.MainCommand("Startup");
    }

    public void MoveIdexButtonClick()
    {

    }

    public void CWDirectionToggleButtonClick()
    {
      CWDirectionActive = !CWDirectionActive;
    }

    public void IdexPositionButtonClick()
    {

    }

    public void FocusedBox(int num)
    {
      switch (num)
      {
        case 0:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(IdexTextBoxInputs)), this, 0);
          break;
        case 1:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(IdexTextBoxInputs)), this, 1);
          break;
        case 2:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(LaserRedPowerValue)), this, 0);
          break;
        case 3:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(LaserGreenPowerValue)), this, 0);
          break;
        case 4:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(LaserVioletPowerValue)), this, 0);
          break;
        case 5:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SyringeControlSheathValue)), this, 0);
          break;
        case 6:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SyringeControlSampleAValue)), this, 0);
          break;
        case 7:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(SyringeControlSampleBValue)), this, 0);
          break;
        case 8:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(GetPositionTextBoxInputs)), this, 0);
          break;
        case 9:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(GetPositionTextBoxInputs)), this, 1);
          break;
        case 10:
          App.SelectedTextBox = (this.GetType().GetProperty(nameof(GetPositionTextBoxInputs)), this, 2);
          break;
      }
    }

    public class DropDownButtonContents : Core.ObservableObject
    {
      public string Content
      {
        get => _content;
        set {
          _content = value;
          OnPropertyChanged();
        } }
      private string _content;
      private static ComponentsViewModel _vm;
      public DropDownButtonContents(string content, ComponentsViewModel vm = null)
      {
        if (_vm == null)
        {
          _vm = vm;
        }
        Content = content;
      }

      public void Click(int num)
      {
        switch (num)
        {
          case 1:
            _vm.SelectedSheathContent = Content;
            break;
          case 2:
            _vm.SelectedSampleAContent = Content;
            break;
          case 3:
            _vm.SelectedSampleBContent = Content;
            break;
        }
      }
    }
  }
}