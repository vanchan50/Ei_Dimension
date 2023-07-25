using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using DIOS.Core.HardwareIntercom;
using System.Windows;

namespace Ei_Dimension.ViewModels;

[POCOViewModel]
public class MotorsViewModel
{
  public virtual ObservableCollection<bool> PollStepActive { get; set; } = new ObservableCollection<bool> { false };
  public virtual ObservableCollection<DropDownButtonContents> WellRowButtonItems { get; set; }
  public virtual ObservableCollection<DropDownButtonContents> WellColumnButtonItems { get; set; }
  public virtual string SelectedWellRow { get; set; }
  public virtual string SelectedWellColumn { get; set; }
  public byte SelectedWellRowIndex { get; set; }
  public byte SelectedWellColumnIndex { get; set; }
  public virtual ObservableCollection<string> ParametersX { get; set; }
  public virtual ObservableCollection<string> ParametersY { get; set; }
  public virtual ObservableCollection<string> ParametersZ { get; set; }
  public virtual ObservableCollection<string> StepsParametersX { get; set; }
  public virtual ObservableCollection<string> StepsParametersY { get; set; }
  public virtual ObservableCollection<string> StepsParametersZ { get; set; }
  public virtual ObservableCollection<bool> WellSelectionButtonsChecked { get; set; }
  public virtual ObservableCollection<string> TraySteps { get; set; } = new ObservableCollection<string> { "" };
  public virtual ObservableCollection<bool> WashStationActive { get; set; }
  public virtual Visibility WashStationVisibility { get; set; } = Visibility.Collapsed;
  public virtual ObservableCollection<string> WashStationXCenterCoordinate { get; set; } = new ObservableCollection<string> { "" };

  public static MotorsViewModel Instance { get; private set; }

  private int _amountOfWells;

  protected MotorsViewModel()
  {
    ParametersX = new ObservableCollection<string>();
    ParametersY = new ObservableCollection<string>();
    ParametersZ = new ObservableCollection<string>();
    StepsParametersX = new ObservableCollection<string>();
    StepsParametersY = new ObservableCollection<string>();
    StepsParametersZ = new ObservableCollection<string>();
    for (var i = 0; i < 8; i++)
    {
      ParametersX.Add("");
      ParametersY.Add("");
      ParametersZ.Add("");
    }
    ParametersX[6] = Settings.Default.StepsPerRevX.ToString();
    ParametersY[6] = Settings.Default.StepsPerRevY.ToString();
    ParametersZ[6] = Settings.Default.StepsPerRevZ.ToString();
    for (var i = 0; i < 5; i++)
    {
      StepsParametersX.Add("");
      StepsParametersY.Add("");
      StepsParametersZ.Add("");
    }
    ParametersX[1] = "Left";
    ParametersY[1] = "Front";
    ParametersZ[1] = "Up";
    _amountOfWells = 96;
    WellSelectionButtonsChecked = new ObservableCollection<bool> { true, false, false };
    SelectedWellRow = "A";
    SelectedWellColumn = "1";
    WellRowButtonItems = new ObservableCollection<DropDownButtonContents> { new DropDownButtonContents("A", this) };
    for (var i = 1; i < 8; i++)
    {
      WellRowButtonItems.Add(new DropDownButtonContents(Convert.ToChar('A' + i).ToString()));
    }
    DropDownButtonContents.ResetIndex();

    WellColumnButtonItems = new ObservableCollection<DropDownButtonContents>();
    for (var i = 1; i < 13; i++)
    {
      WellColumnButtonItems.Add(new DropDownButtonContents(i.ToString()));
    }
    DropDownButtonContents.ResetIndex();
    SelectedWellRowIndex = 0;
    SelectedWellColumnIndex = 0;

    WashStationActive = new ObservableCollection<bool> { Settings.Default.UseWashStation == 1 };
    WashStationVisibility = Settings.Default.UseWashStation == 1 ? Visibility.Visible : Visibility.Collapsed;
    Instance = this;
  }

  public static MotorsViewModel Create()
  {
    return ViewModelSource.Create(() => new MotorsViewModel());
  }

  public void ChangeAmountOfWells(int num)
  {
    UserInputHandler.InputSanityCheck();
    if (_amountOfWells == num)
      return;
    _amountOfWells = num;
    WellRowButtonItems.Clear();
    WellColumnButtonItems.Clear();
    SelectedWellRow = "A";
    SelectedWellColumn = "1";
    SelectedWellRowIndex = 0;
    SelectedWellColumnIndex = 0;
    //switch only changes dropdown contents
    WellSelectionButtonsChecked[0] = false;
    WellSelectionButtonsChecked[1] = false;
    WellSelectionButtonsChecked[2] = false;
    switch (num)
    {
      case 96:
        WellSelectionButtonsChecked[0] = true;
        for (var i = 0; i < 8; i++)
        {
          WellRowButtonItems.Add(new DropDownButtonContents(Convert.ToChar('A' + i).ToString()));
        }
        DropDownButtonContents.ResetIndex();
        for (var i = 1; i < 13; i++)
        {
          WellColumnButtonItems.Add(new DropDownButtonContents(i.ToString()));
        }
        DropDownButtonContents.ResetIndex();
        break;
      case 384:
        WellSelectionButtonsChecked[1] = true;
        for (var i = 0; i < 16; i++)
        {
          WellRowButtonItems.Add(new DropDownButtonContents(Convert.ToChar('A' + i).ToString()));
        }
        DropDownButtonContents.ResetIndex();
        for (var i = 1; i < 25; i++)
        {
          WellColumnButtonItems.Add(new DropDownButtonContents(i.ToString()));
        }
        DropDownButtonContents.ResetIndex();
        break;
      case 1:
        WellSelectionButtonsChecked[2] = true;
        WellRowButtonItems.Add(new DropDownButtonContents("A"));
        DropDownButtonContents.ResetIndex();
        WellColumnButtonItems.Add(new DropDownButtonContents("1"));
        DropDownButtonContents.ResetIndex();
        break;
    }

    var param = num == 96 ? 0 : 1;
    if (num == 1)
      param = 2;
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.PlateType, param);
  }

  public void RunMotorButtonClick(string s)
  {
    UserInputHandler.InputSanityCheck();
    float fRes;
    switch (s)
    {
      case "x":
        if (float.TryParse(ParametersX[0], out fRes))
        {
          var direction = ParametersX[1] == "Left" ? MotorDirection.Left : MotorDirection.Right;
          App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorMoveX, direction, fRes);
        }
        break;
      case "y":
        if (float.TryParse(ParametersY[0], out fRes))
        {
          var direction = ParametersY[1] == "Back" ? MotorDirection.Back : MotorDirection.Front;
          App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorMoveY, direction, fRes);
        }
        break;
      case "z":
        if (float.TryParse(ParametersZ[0], out fRes))
        {
          var direction = ParametersZ[1] == "Up" ? MotorDirection.Up : MotorDirection.Down;
          App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorMoveZ, direction, fRes);
        }
        break;
    }
  }

  public void HaltMotorButtonClick(string s)
  {
    UserInputHandler.InputSanityCheck();
    switch (s)
    {
      case "x":
        App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorMoveX, MotorDirection.Halt);
        break;
      case "y":
        App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorMoveY, MotorDirection.Halt);
        break;
      case "z":
        App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.MotorMoveZ, MotorDirection.Halt);
        break;
    }
  }

  public void GoToWellButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.WellRowIndex, SelectedWellRowIndex);
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.WellColumnIndex, SelectedWellColumnIndex);
    App.DiosApp.Device.Hardware.SendCommand(DeviceCommandType.PositionWellPlate);
  }

  public void PollStepToggleButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    PollStepActive[0] = !PollStepActive[0];
    var param = PollStepActive[0] ? 1 : 0;
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.PollStepActivity, param);
  }

  public void PollStepSelector(string s)
  {
    UserInputHandler.InputSanityCheck();
    switch (s)
    {
      case "Left":
        ParametersX[1] = s;
        break;
      case "Right":
        ParametersX[1] = s;
        break;
      case "Back":
        ParametersY[1] = s;
        break;
      case "Front":
        ParametersY[1] = s;
        break;
      case "Up":
        ParametersZ[1] = s;
        break;
      case "Down":
        ParametersZ[1] = s;
        break;
    }
  }

  public void WashStationToggleButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    WashStationActive[0] = !WashStationActive[0];
    Settings.Default.UseWashStation = WashStationActive[0] ? 1 : 0;
    Settings.Default.Save();
    WashStationVisibility = WashStationActive[0] ? Visibility.Visible : Visibility.Collapsed;
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.UseWashStation, Settings.Default.UseWashStation);
  }

  public void DropPress()
  {
    UserInputHandler.InputSanityCheck();
  }

  public void FocusedBox(int num)
  {
    switch (num)
    {
      case 0:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersX)), this, 0, Views.MotorsView.Instance.xStep);
        MainViewModel.Instance.NumpadToggleButton(Views.MotorsView.Instance.xStep);
        break;
      case 1:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersX)), this, 2, (TextBox)Views.MotorsView.Instance.xStepsSP.Children[0]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.xStepsSP.Children[0]);
        break;
      case 2:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersX)), this, 3, (TextBox)Views.MotorsView.Instance.xStepsSP.Children[1]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.xStepsSP.Children[1]);
        break;
      case 3:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersX)), this, 4, (TextBox)Views.MotorsView.Instance.xStepsSP.Children[2]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.xStepsSP.Children[2]);
        break;
      case 5:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersX)), this, 6, (TextBox)Views.MotorsView.Instance.xStepsSP.Children[4]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.xStepsSP.Children[4]);
        break;
      case 6:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersY)), this, 0, Views.MotorsView.Instance.yStep);
        MainViewModel.Instance.NumpadToggleButton(Views.MotorsView.Instance.yStep);
        break;
      case 7:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersY)), this, 2, (TextBox)Views.MotorsView.Instance.yStepsSP.Children[0]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.yStepsSP.Children[0]);
        break;
      case 8:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersY)), this, 3, (TextBox)Views.MotorsView.Instance.yStepsSP.Children[1]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.yStepsSP.Children[1]);
        break;
      case 9:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersY)), this, 4, (TextBox)Views.MotorsView.Instance.yStepsSP.Children[2]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.yStepsSP.Children[2]);
        break;
      case 11:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersY)), this, 6, (TextBox)Views.MotorsView.Instance.yStepsSP.Children[4]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.yStepsSP.Children[4]);
        break;
      case 12:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersZ)), this, 0, Views.MotorsView.Instance.zStep);
        MainViewModel.Instance.NumpadToggleButton(Views.MotorsView.Instance.zStep);
        break;
      case 13:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersZ)), this, 2, (TextBox)Views.MotorsView.Instance.zStepsSP.Children[0]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.zStepsSP.Children[0]);
        break;
      case 14:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersZ)), this, 3, (TextBox)Views.MotorsView.Instance.zStepsSP.Children[1]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.zStepsSP.Children[1]);
        break;
      case 15:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersZ)), this, 4, (TextBox)Views.MotorsView.Instance.zStepsSP.Children[2]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.zStepsSP.Children[2]);
        break;
      case 17:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersZ)), this, 6, (TextBox)Views.MotorsView.Instance.zStepsSP.Children[4]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.zStepsSP.Children[4]);
        break;
      case 18:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(StepsParametersX)), this, 0, (TextBox)Views.MotorsView.Instance.xSP.Children[0]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.xSP.Children[0]);
        break;
      case 19:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(StepsParametersX)), this, 1, (TextBox)Views.MotorsView.Instance.xSP.Children[3]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.xSP.Children[3]);
        break;
      case 20:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(StepsParametersX)), this, 2, (TextBox)Views.MotorsView.Instance.xSP.Children[2]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.xSP.Children[2]);
        break;
      case 21:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(StepsParametersX)), this, 3, (TextBox)Views.MotorsView.Instance.xSP.Children[1]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.xSP.Children[1]);
        break;
      case 22:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(StepsParametersX)), this, 4, (TextBox)Views.MotorsView.Instance.xSP.Children[4]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.xSP.Children[4]);
        break;
      case 23:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(StepsParametersY)), this, 0, (TextBox)Views.MotorsView.Instance.ySP.Children[0]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.ySP.Children[0]);
        break;
      case 24:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(StepsParametersY)), this, 1, (TextBox)Views.MotorsView.Instance.ySP.Children[3]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.ySP.Children[3]);
        break;
      case 25:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(StepsParametersY)), this, 2, (TextBox)Views.MotorsView.Instance.ySP.Children[2]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.ySP.Children[2]);
        break;
      case 26:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(StepsParametersY)), this, 3, (TextBox)Views.MotorsView.Instance.ySP.Children[1]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.ySP.Children[1]);
        break;
      case 27:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(StepsParametersY)), this, 4, (TextBox)Views.MotorsView.Instance.ySP.Children[4]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.ySP.Children[4]);
        break;
      case 28:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(StepsParametersZ)), this, 0, (TextBox)Views.MotorsView.Instance.zSP.Children[0]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.zSP.Children[0]);
        break;
      case 29:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(StepsParametersZ)), this, 1, (TextBox)Views.MotorsView.Instance.zSP.Children[1]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.zSP.Children[1]);
        break;
      case 30:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(StepsParametersZ)), this, 2, (TextBox)Views.MotorsView.Instance.zSP.Children[2]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.zSP.Children[2]);
        break;
      case 31:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(StepsParametersZ)), this, 3, (TextBox)Views.MotorsView.Instance.zSP.Children[3]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.zSP.Children[3]);
        break;
      case 32:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(StepsParametersZ)), this, 4, (TextBox)Views.MotorsView.Instance.zSP.Children[4]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.zSP.Children[4]);
        break;
      case 33:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersX)), this, 7, (TextBox)Views.MotorsView.Instance.xStepsSP.Children[5]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.xStepsSP.Children[5]);
        break;
      case 34:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersY)), this, 7, (TextBox)Views.MotorsView.Instance.yStepsSP.Children[5]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.yStepsSP.Children[5]);
        break;
      case 35:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ParametersZ)), this, 7, (TextBox)Views.MotorsView.Instance.zStepsSP.Children[5]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.zStepsSP.Children[5]);
        break;
      case 36:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(TraySteps)), this, 0, (TextBox)Views.MotorsView.Instance.ySP.Children[5]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.ySP.Children[5]);
        break;
      case 37:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(WashStationXCenterCoordinate)), this, 0, (TextBox)Views.MotorsView.Instance.xSP.Children[5]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.MotorsView.Instance.xSP.Children[5]);
        break;
    }
  }

  public void TextChanged(TextChangedEventArgs e)
  {
    UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
  }

  public class DropDownButtonContents
  {
    public string Content { get; set; }
    public byte Index { get; set; }
    private static byte _nextIndex = 0;
    private static MotorsViewModel _vm;
    public DropDownButtonContents(string content, MotorsViewModel vm = null)
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
          _vm.SelectedWellRow = Content;
          App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.WellRowIndex, Index);
          _vm.SelectedWellRowIndex = Index;
          break;
        case 2:
          _vm.SelectedWellColumn = Content;
          App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.WellColumnIndex, Index);
          _vm.SelectedWellColumnIndex = Index;
          break;
      }
    }

    public static void ResetIndex()
    {
      _nextIndex = 0;
    }
  }
}