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
    public virtual string LaserRedPowerValue { get; set; }
    public virtual string LaserGreenPowerValue { get; set; }
    public virtual string LaserVioletPowerValue { get; set; }

    public virtual ObservableCollection<DropDownButtonContents> SyringeControlItems { get; set; }
    public virtual string SelectedSheathContent { get; set; }
    public virtual string SelectedSampleAContent { get; set; }
    public virtual string SelectedSampleBContent { get; set; }
    public virtual string SyringeControlSheathValue { get; set; }
    public virtual string SyringeControlSampleAValue { get; set; }
    public virtual string SyringeControlSampleBValue { get; set; }

    public virtual string GetPositionToggleButtonState { get; set; }
    public virtual string[] GetPositionTextBoxInputs { get; set; }

    public virtual bool SamplingActive { get; set; }
    public virtual bool SingleStepDebugActive { get; set; }

    public virtual string[] IdexTextBoxInputs { get; set; }
    public virtual bool CWDirectionActive { get; set; }

    protected ComponentsViewModel()
    {
      ValvesStates = new bool[4] { false,false,false,false };
      InputSelectorState = new ObservableCollection<string>();
      InputSelectorState.Add("To Pickup");
      InputSelectorState.Add("To Cuvet");

      LaserRedActive = false;
      LaserGreenActive = false;
      LaserVioletActive = false;

      SyringeControlItems = new ObservableCollection<DropDownButtonContents> { new DropDownButtonContents("Halt", this), new DropDownButtonContents("Move Absolute", this),
        new DropDownButtonContents("Pickup", this), new DropDownButtonContents("Pre Inject", this), new DropDownButtonContents("Speed", this),
        new DropDownButtonContents("Initialize", this), new DropDownButtonContents("Boot", this), new DropDownButtonContents("Valve Left", this),
        new DropDownButtonContents("Valve Right", this), new DropDownButtonContents("Micro Step", this), new DropDownButtonContents("Speed Preset", this),
        new DropDownButtonContents("Position", this)};
      SelectedSheathContent = SyringeControlItems[0].Content;
      SelectedSampleAContent = SyringeControlItems[0].Content;
      SelectedSampleBContent = SyringeControlItems[0].Content;

      GetPositionToggleButtonState = "OFF";
      GetPositionTextBoxInputs = new string[3];

      SamplingActive = false;
      SingleStepDebugActive = false;

      IdexTextBoxInputs = new string[2];
      CWDirectionActive = false;
    }

    public static ComponentsViewModel Create()
    {
      return ViewModelSource.Create(() => new ComponentsViewModel());
    }

    public void InputSelectorSwapButtonClick()
    {
      string temp = InputSelectorState[0];
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
      if( GetPositionToggleButtonState == "OFF")
      {
        GetPositionToggleButtonState = "ON";
      }
      else if(GetPositionToggleButtonState == "ON")
      {
        GetPositionToggleButtonState = "OFF";
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


    public class DropDownButtonContents
    {
      public string Content { get; set; }
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