using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.XtraSpreadsheet.Model;
using DIOS.Core;
using TextChangedEventArgs = System.Windows.Controls.TextChangedEventArgs;

namespace Ei_Dimension.ViewModels
{
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
    public static PlateCustomizationViewModel Instance { get; private set; }

    private static int _tuningIsRunning;
    protected PlateCustomizationViewModel()
    {
      Instance = this;
    }

    public static PlateCustomizationViewModel Create()
    {
      return ViewModelSource.Create(() => new PlateCustomizationViewModel());
    }

    public void HideView()
    {
      UserInputHandler.InputSanityCheck();
      CustomizationVisible = Visibility.Hidden;
    }

    public void TunePlate()
    {
      if (Interlocked.CompareExchange(ref _tuningIsRunning, 1, 0) == 1)
        return;
      Task.Run(() =>
      {
        if (!IsTableSize96())
        {
          Notification.Show("Valid only for plate size 96");
          _tuningIsRunning = 0;
          return;
        }

        if (IsPlateEjected())
        {
          _tuningIsRunning = 0;
          return;
        }

        ShowShield();
        var a1 = ProbeTuningProcedure(new Well { RowIdx = 0, ColIdx = 0 });
        var a12 = ProbeTuningProcedure(new Well { RowIdx = 0, ColIdx = 11 });
        var h1 = ProbeTuningProcedure(new Well { RowIdx = 7, ColIdx = 0 });
        var h12 = ProbeTuningProcedure(new Well { RowIdx = 7, ColIdx = 11 });
        HideShield();
        _tuningIsRunning = 0;
        Notification.Show($"The results are A1:{a1}, A12:{a12}, H1:{h1}, H12:{h12}");
      });
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
      }
    }

    public void TextChanged(TextChangedEventArgs e)
    {
      UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
    }

    private float ProbeTuningProcedure(Well well)
    {
      var decreaseCurrentTo = ushort.Parse(DACCurrentLimit[0]);
      ChangeDACCurrent(decreaseCurrentTo);

      MovePlateToWell(well);

      var motorZInitHeight = ushort.Parse(InitZMotorPosition[0]);
      MoveProbe(motorZInitHeight);

      var resultingHeight = GetResultingProbeHeight();

      ChangeDACCurrent(0);

      var motorZFinalHeight = ushort.Parse(FinalZMotorPosition[0]);
      MoveProbe(motorZFinalHeight);

      return resultingHeight;
    }

    /// <summary>
    /// </summary>
    /// <param name="value">0 means "max current"</param>
    private void ChangeDACCurrent(ushort value)
    {
      App.Device.MainCommand("Set Property", code: 0x92, parameter: value);
      App.Device.MainCommand("RefreshDac");
    }

    private void MovePlateToWell(Well well)
    {
      App.Device.MainCommand("Set Property", code: 0xad, parameter: well.RowIdx);
      App.Device.MainCommand("Set Property", code: 0xae, parameter: well.ColIdx);
      App.Device.MainCommand("Position Well Plate");
      lock (App.Device.SystemActivityNotBusyNotificationLock)
      {
        Monitor.Wait(App.Device.SystemActivityNotBusyNotificationLock);
      }
    }

    private void MoveProbe(ushort height)
    {
      App.Device.MainCommand("MotorZ", parameter: height);
      lock (App.Device.SystemActivityNotBusyNotificationLock)
      {
        Monitor.Wait(App.Device.SystemActivityNotBusyNotificationLock);
      }
    }

    private float GetResultingProbeHeight()
    {
      App.Device.MainCommand("Get Property", code: 0x44);
      lock (ZStepIsUpdatedLock)
      {
        Monitor.Wait(ZStepIsUpdatedLock);
      }
      var currentStep = float.Parse(MotorsViewModel.Instance.ParametersZ[5]);
      var epsilonZ = float.Parse(ZStep[0]);
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
      if (!App.Device.IsPlateEjected)
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
      _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        Notification.Show("Please Load Plate",
          LoadPlate, "Load Plate",
          Cancel, "Cancel");
      }));
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
  }
}
