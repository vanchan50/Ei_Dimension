using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.IO;
using DIOS.Core;
using DIOS.Core.HardwareIntercom;
using Ei_Dimension.Models;

namespace Ei_Dimension.ViewModels;

[POCOViewModel]
public class PlateCustomizationViewModel
{
  public virtual Visibility CustomizationVisible { get; set; } = Visibility.Hidden;
  public virtual Visibility WaitShieldVisibility { get; set; } = Visibility.Hidden;
  public virtual bool WaitIndicatorVisibility { get; set; } = false;
  public virtual ObservableCollection<string> DACCurrentLimit { get; set; } = new ObservableCollection<string>{"32000"};
  public virtual ObservableCollection<string> ZStep { get; set; } = new ObservableCollection<string> { "5" };
  public virtual ObservableCollection<string> InitZMotorPosition { get; set; } = new ObservableCollection<string> { "450" };
  public virtual ObservableCollection<string> FinalZMotorPosition { get; set; } = new ObservableCollection<string> { "600" };
  public object ZStepIsUpdatedLock { get; } = new object();

  public virtual ObservableCollection<string> NameList { get; set; } = new ObservableCollection<string>();
  public string SelectedItem { get; set; }
  private string _plateName;
  public virtual ObservableCollection<string> PlateSaveName { get; set; } = new ObservableCollection<string> { "DefaultPlateType" };
  public virtual ObservableCollection<string> FreshlyMeasuredValues { get; set; } = new ObservableCollection<string> { "", "", "", "" };
  public virtual ObservableCollection<string> CurrentValues { get; set; } = new ObservableCollection<string> { "", "", "", "" };
  public virtual ObservableCollection<string> SelectedValues { get; set; } = new ObservableCollection<string> { "", "", "", "" };
  public virtual Visibility DeleteButtonIsVisible { get; set; } = Visibility.Hidden;

  private static List<char> _invalidChars = new List<char>();
  public static PlateCustomizationViewModel Instance { get; private set; }
  public string CurrentPlateName { get; private set; }

  private int _tuningIsRunning;
  private PlateDepths _freshlyMeasuredPlateDepths;
  public PlateDepths DefaultPlate { get; } = new PlateDepths{ A1 = 1, A12 = 2, H1 = 3, H12 = 4 };
  public const string DEFAULTNAME = "Default";
  public const string PLATETYPEFILEEXTENSION = ".dplt";
  private readonly Device _device;
  protected PlateCustomizationViewModel()
  {
    var PlateTypeList = Directory.GetFiles($"{App.DiosApp.RootDirectory}\\Config", $"*{PLATETYPEFILEEXTENSION}");
    foreach (var plateType in PlateTypeList)
    {
      NameList.Add(Path.GetFileNameWithoutExtension(plateType));
    }
    NameList.Add(DEFAULTNAME);

    Instance = this;

    _invalidChars.AddRange(Path.GetInvalidPathChars());
    _invalidChars.AddRange(Path.GetInvalidFileNameChars());

    CurrentPlateName = DEFAULTNAME;
    UpdateDefault();
    _device = App.DiosApp.Device;
  }

  public static PlateCustomizationViewModel Create()
  {
    return ViewModelSource.Create(() => new PlateCustomizationViewModel());
  }

  public void Selected(SelectionChangedEventArgs e)
  {
    if (e.AddedItems.Count == 0)
      return;

    _plateName = e.AddedItems[0].ToString();
    SelectedItem = App.DiosApp.RootDirectory + @"\Config\" + e.AddedItems[0].ToString() + PLATETYPEFILEEXTENSION;
    PlateSaveName[0] = _plateName;

    if (_plateName == DEFAULTNAME)
    {
      UpdateSelected(DefaultPlate);
      DeleteButtonIsVisible = Visibility.Hidden;
      return;
    }

    try
    {
      using (TextReader reader = new StreamReader(SelectedItem))
      {
        var fileContents = reader.ReadToEnd();
        var newPD = JsonConvert.DeserializeObject<PlateDepths>(fileContents);
        UpdateSelected(newPD);
      }
    }
    catch
    {
    }
    DeleteButtonIsVisible = Visibility.Visible;
  }

  public void SavePlate()
  {
    UserInputHandler.InputSanityCheck();

    if (_freshlyMeasuredPlateDepths == null)
    {
      Notification.ShowLocalizedError(nameof(Language.Resources.Notification_NoPlateMeasured));
      return;
    }

    var path = App.DiosApp.RootDirectory + @"\Config\" + PlateSaveName[0] + PLATETYPEFILEEXTENSION;
    foreach (var c in _invalidChars)
    {
      if (PlateSaveName[0].Contains(c.ToString()))
      {
        Notification.ShowLocalizedError(nameof(Language.Resources.Notification_Invalid_File_Name));
        return;
      }
    }

    if (PlateSaveName[0] == DEFAULTNAME)
    {
      Notification.ShowLocalizedError(nameof(Language.Resources.Notification_Invalid_File_Name));
      return;
    }

    if (File.Exists(path))
    {
      void Overwrite()
      {
        SavingProcedure(path);
        DeleteButtonIsVisible = Visibility.Hidden;
      }

      if (Language.TranslationSource.Instance.CurrentCulture.TextInfo.CultureName == "zh-CN")
      {
        Notification.Show($"名称为 \"{PlateSaveName[0]}\" 的孔板已存在",
          Overwrite, "覆盖", null, "取消");
      }
      else
      {
        Notification.Show($"A plate with name \"{PlateSaveName[0]}\" already exists",
          Overwrite, "Overwrite", null, "Cancel");
      }
      return;
    }
    SavingProcedure(path);
    DeleteButtonIsVisible = Visibility.Hidden;
  }

  public void LoadPlate()
  {
    UserInputHandler.InputSanityCheck();
    PlateDepths newPD = null;
    if (Path.GetFileNameWithoutExtension(SelectedItem) == DEFAULTNAME)
    {
      newPD = DefaultPlate;
    }
    else
    {
      try
      {
        using (TextReader reader = new StreamReader(SelectedItem))
        {
          var fileContents = reader.ReadToEnd();
          newPD = JsonConvert.DeserializeObject<PlateDepths>(fileContents);
        }
      }
      catch
      {
        Notification.ShowLocalizedError(nameof(Language.Resources.Notification_No_Plate_Selected));
      }
    }

    if (newPD != null)
    {
      //apply
      _device.Hardware.SetParameter(DeviceParameterType.MotorStepsZTemporary, MotorStepsZ.A1,  newPD.A1);
      _device.Hardware.SetParameter(DeviceParameterType.MotorStepsZTemporary, MotorStepsZ.A12, newPD.A12);
      _device.Hardware.SetParameter(DeviceParameterType.MotorStepsZTemporary, MotorStepsZ.H1,  newPD.H1);
      _device.Hardware.SetParameter(DeviceParameterType.MotorStepsZTemporary, MotorStepsZ.H12, newPD.H12);
      UpdateCurrent(newPD);
      CurrentPlateName = Path.GetFileNameWithoutExtension(SelectedItem);
      UnselectAll();
    }
  }

  public void DeletePlate()
  {
    UserInputHandler.InputSanityCheck();
    if (SelectedItem != null && File.Exists(SelectedItem))
    {
      void Delete()
      {
        File.Delete(SelectedItem);
        NameList.Remove(_plateName);
        DeleteButtonIsVisible = Visibility.Hidden;
      }

      //if (Language.TranslationSource.Instance.CurrentCulture.TextInfo.CultureName == "zh-CN")
      //{
      //  Notification.Show($"您要删除 \"{Path.GetFileNameWithoutExtension(SelectedItem)}\" 模板吗？",
      //    Delete, "删除", null, "取消");
      //}
      //else
      //{
      Notification.Show($"Do you want to delete \"{Path.GetFileNameWithoutExtension(SelectedItem)}\" plate?",
        Delete, "Delete", null, "Cancel");
      //}
    }
    UpdateSelected(null);
    DeleteButtonIsVisible = Visibility.Hidden;
  }

  public void TunePlate()
  {
    if (Interlocked.CompareExchange(ref _tuningIsRunning, 1, 0) == 1)
      return;

    if (_device.IsMeasurementGoing)
    {
      Notification.Show("Please wait for the measurement to complete");
      _tuningIsRunning = 0;
      return;
    }

    if (!IsTableSize96())
    {
      Notification.Show("Valid only for plate size 96");
      _tuningIsRunning = 0;
      return;
    }

    Task.Run(() =>
    {
      if (IsPlateEjected())
      {
        _tuningIsRunning = 0;
        return;
      }

      ShowShield();
      var a1 = ProbeTuningProcedure(new Well(0, 0));
      var a12 = ProbeTuningProcedure(new Well(0, 11));
      var h1 = ProbeTuningProcedure(new Well (7, 0));
      var h12 = ProbeTuningProcedure(new Well (7, 11));
      HideShield();

      if (a1 < 0 || a12 < 0 || h1 < 0 || h12 < 0)
      {
        Notification.Show("Wrong Parameters set");
        _tuningIsRunning = 0;
        return;
      }
      var freshPD = new PlateDepths
      {
        A1 = a1,
        A12 = a12,
        H1 = h1,
        H12 = h12
      };
      UpdateFreshMeasurement(freshPD);

      _tuningIsRunning = 0;
    });
  }

  public void HideView()
  {
    UserInputHandler.InputSanityCheck();
    App.HideKeyboard();
    CustomizationVisible = Visibility.Hidden;
    UnselectAll();
  }

  public void ShowView()
  {
    CustomizationVisible = Visibility.Visible;
    DeleteButtonIsVisible = Visibility.Hidden;
  }

  private float ProbeTuningProcedure(Well well)
  {
    if (!ushort.TryParse(DACCurrentLimit[0], out var decreaseCurrentTo))
      return -1;
    if (!ushort.TryParse(InitZMotorPosition[0], out var motorZInitHeight))
      return -1;
    if (!ushort.TryParse(FinalZMotorPosition[0], out var motorZFinalHeight))
      return -1;

    _device.Hardware.MovePlateToWell(well);

    ChangeDACCurrent(decreaseCurrentTo);
    _device.Hardware.DescendProbe(motorZInitHeight);

    var resultingHeight = GetResultingProbeHeight();
    ChangeDACCurrent(0);


    _device.Hardware.AscendProbe(motorZFinalHeight);

    return resultingHeight;
  }

  /// <summary>
  /// </summary>
  /// <param name="value">0 means "max current"</param>
  private void ChangeDACCurrent(ushort value)
  {
    _device.Hardware.SetParameter(DeviceParameterType.MotorZ, MotorParameterType.CurrentLimit, value);
    _device.Hardware.SendCommand(DeviceCommandType.RefreshDAC);
  }

  private float GetResultingProbeHeight()
  {
    lock (ZStepIsUpdatedLock)
    {
      _device.Hardware.RequestParameter(DeviceParameterType.MotorZ, MotorParameterType.CurrentStep);
      Monitor.Wait(ZStepIsUpdatedLock);
    }
    Thread.Sleep(500);
    var currentStep = float.Parse(MotorsViewModel.Instance.ParametersZ[5]);
    var epsilonZ = float.Parse(ZStep[0]);
#if DEBUG
    App.Logger.Log($"ACTUAL PROBE HEIGHT: {currentStep},\t REPORTED HEIGHT: {currentStep - epsilonZ}");
#endif
    return currentStep - epsilonZ;
  }

  private void ShowShield()
  {
    WaitShieldVisibility = Visibility.Visible;
    WaitIndicatorVisibility = true;
  }

  private void HideShield()
  {
    WaitShieldVisibility = Visibility.Hidden;
    WaitIndicatorVisibility = false;
  }

  private bool IsTableSize96()
  {
    return WellsSelectViewModel.Instance.CurrentTableSize == 96;
  }

  private bool IsPlateEjected()
  {
    if (!_device.IsPlateEjected)
      return false;
      
    var requirementsLock = new object();
    bool requirementsPassed = false;
    void LoadPlate()
    {
      MainButtonsViewModel.Instance.LoadButtonClick();
      requirementsPassed = true;
      lock (requirementsLock)
      {
        Monitor.PulseAll(requirementsLock);
      }
    }
    void Cancel()
    {
      requirementsPassed = false;
      lock (requirementsLock)
      {
        Monitor.PulseAll(requirementsLock);
      }
    }
    _ = App.Current.Dispatcher.BeginInvoke(() =>
    {
      Notification.Show("Please Load Plate",
        LoadPlate, "Load Plate",
        Cancel, "Cancel");
    });
    lock (requirementsLock)
    {
      Monitor.Wait(requirementsLock);
    }

    if (!requirementsPassed)
    {
      return true;
    }

    return false;
  }

  private void SavingProcedure(string path)
  {
    try
    {
      using (var stream = new StreamWriter(path, append: false))
      {
        var contents = JsonConvert.SerializeObject(_freshlyMeasuredPlateDepths);
        stream.Write(contents);
      }
    }
    catch
    {
      Notification.ShowLocalized(nameof(Language.Resources.Notification_Plate_Save_Problem));
    }

    if (!NameList.Contains(PlateSaveName[0]))
    {
      NameList.Add(PlateSaveName[0]);
    }

    UnselectAll();
  }

  private void UpdateFreshMeasurement(PlateDepths pd)
  {
    _freshlyMeasuredPlateDepths = pd;
    if (pd != null)
    {
      FreshlyMeasuredValues[0] = _freshlyMeasuredPlateDepths.A1.ToString();
      FreshlyMeasuredValues[1] = _freshlyMeasuredPlateDepths.A12.ToString();
      FreshlyMeasuredValues[2] = _freshlyMeasuredPlateDepths.H1.ToString();
      FreshlyMeasuredValues[3] = _freshlyMeasuredPlateDepths.H12.ToString();
    }
    else
    {
      FreshlyMeasuredValues[0] = "";
      FreshlyMeasuredValues[1] = "";
      FreshlyMeasuredValues[2] = "";
      FreshlyMeasuredValues[3] = "";
    }
  }

  private void UpdateSelected(PlateDepths pd)
  {
    if (pd != null)
    {
      SelectedValues[0] = pd.A1.ToString();
      SelectedValues[1] = pd.A12.ToString();
      SelectedValues[2] = pd.H1.ToString();
      SelectedValues[3] = pd.H12.ToString();
    }
    else
    {
      SelectedValues[0] = "";
      SelectedValues[1] = "";
      SelectedValues[2] = "";
      SelectedValues[3] = "";
    }
  }

  private void UpdateCurrent(PlateDepths pd)
  {
    if (pd != null)
    {
      CurrentValues[0] = pd.A1.ToString();
      CurrentValues[1] = pd.A12.ToString();
      CurrentValues[2] = pd.H1.ToString();
      CurrentValues[3] = pd.H12.ToString();
    }
    else
    {
      CurrentValues[0] = "";
      CurrentValues[1] = "";
      CurrentValues[2] = "";
      CurrentValues[3] = "";
    }
  }

  public void UpdateDefault()
  {
    if (CurrentPlateName == DEFAULTNAME)
      UpdateCurrent(DefaultPlate);
  }

  public void FocusedBox(int num)
  {
    var Stackpanel = Views.PlateCustomizationView.Instance.SP.Children;
    switch (num)
    {
      case 0:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(DACCurrentLimit)), this, 0, (TextBox)Stackpanel[0]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[0]);
        break;
      case 1:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ZStep)), this, 0, (TextBox)Stackpanel[1]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[1]);
        break;
      case 2:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(InitZMotorPosition)), this, 0, (TextBox)Stackpanel[2]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[2]);
        break;
      case 3:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(FinalZMotorPosition)), this, 0, (TextBox)Stackpanel[3]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Stackpanel[3]);
        break;
      case 4:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(PlateSaveName)), this, 0, Views.PlateCustomizationView.Instance.nameBox);
        MainViewModel.Instance.KeyboardToggle(Views.PlateCustomizationView.Instance.nameBox);
        break;
    }
  }

  public void TextChanged(TextChangedEventArgs e)
  {
    UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
  }

  private void UnselectAll()
  {
    UpdateSelected(null);
    Views.PlateCustomizationView.Instance.list.UnselectAll();
  }
}