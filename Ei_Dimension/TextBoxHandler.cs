using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ei_Dimension.ViewModels;
using MicroCy;

namespace Ei_Dimension
{
  internal class TextBoxHandler
  {
    public static void Update()
    {
      float g = 0;
      while (App.Device.Commands.TryDequeue(out var exe))
      {
        switch (exe.Code)
        {
          case 0x01:
            App.Device.BoardVersion = exe.Parameter;
            if(App.Device.BoardVersion > 1)
              HideChannels();
            break;
          case 0x02:
            ChannelOffsetViewModel.Instance.SiPMTempCoeff[0] = exe.FParameter.ToString();
            break;
          case 0x04:
            ComponentsViewModel.Instance.IdexTextBoxInputs[0] = exe.Parameter.ToString("X2");
            break;
#if DEBUG
          case 0x06:
            MainViewModel.Instance.TotalBeadsInFirmware[0] = exe.FParameter.ToString();
            break;
#endif
          case 0x10:  //cuvet drain cb
            ComponentsViewModel.Instance.ValvesStates[2] = exe.Parameter == 1;
            break;
          case 0x11:  //Fan
            ComponentsViewModel.Instance.ValvesStates[3] = exe.Parameter == 1;
            break;
          case 0x12:  //sample A valve cb LEGACY, use 0x18 to switch with IDEX
            ComponentsViewModel.Instance.ValvesStates[0] = exe.Parameter == 1;
            break;
          //case 0x13:  //sample b LEGACY
          //  ComponentsViewModel.Instance.ValvesStates[1] = exe.Parameter == 1;
          //  break;
          case 0x14:
            switch (exe.Parameter)
            {
              case 0:
                ComponentsViewModel.Instance.GetPositionTextBoxInputs[0] = exe.FParameter.ToString();
                break;
              case 1:
                ComponentsViewModel.Instance.GetPositionTextBoxInputs[1] = exe.FParameter.ToString();
                break;
              case 2:
                ComponentsViewModel.Instance.GetPositionTextBoxInputs[2] = exe.FParameter.ToString();
                break;
            }
            break;
          case 0x15:
            ComponentsViewModel.Instance.GetPositionToggleButtonStateBool[0] = exe.Parameter == 1;
            break;
          case 0x16:
            MotorsViewModel.Instance.PollStepActive[0] = exe.Parameter == 1;
            break;
          case 0x18:
            if (exe.Parameter == 1)
            {
              if (ComponentsViewModel.Instance.IInputSelectorState == 0)
              {
                var temp = ComponentsViewModel.Instance.InputSelectorState[0];
                ComponentsViewModel.Instance.InputSelectorState[0] = ComponentsViewModel.Instance.InputSelectorState[1];
                ComponentsViewModel.Instance.InputSelectorState[1] = temp;
                ComponentsViewModel.Instance.IInputSelectorState = 1;
              }
            }
            else
            {
              if (ComponentsViewModel.Instance.IInputSelectorState == 1)
              {
                var temp = ComponentsViewModel.Instance.InputSelectorState[0];
                ComponentsViewModel.Instance.InputSelectorState[0] = ComponentsViewModel.Instance.InputSelectorState[1];
                ComponentsViewModel.Instance.InputSelectorState[1] = temp;
                ComponentsViewModel.Instance.IInputSelectorState = 0;
              }
            }
            break;
          //case 0x1a:
          //  ProxOncb.Checked = (exe.Parameter == 1);
          //  break;
          case 0x1b:
            if (exe.Parameter == 0)
            {
              App.Device.EndState = 1;
              CalibrationViewModel.Instance.CalJustFailed = false;
              _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
              {
                CalibrationViewModel.Instance.CalibrationSuccess();
              }));
            }
            break;
          case 0x20:
            CalibrationViewModel.Instance.DNRContents[0] = exe.FParameter.ToString();
            MicroCy.InstrumentParameters.Calibration.HDnrCoef = exe.FParameter;
            break;
          case 0x22:  //pressure
            double dd = exe.FParameter;
            DashboardViewModel.Instance.PressureMon[0] = dd.ToString("f3");
            if (dd < DashboardViewModel.Instance.MinPressure)
              DashboardViewModel.Instance.MinPressure = dd;
            if (dd > DashboardViewModel.Instance.MaxPressure)
              DashboardViewModel.Instance.MaxPressure = dd;
            DashboardViewModel.Instance.PressureMon[1] = DashboardViewModel.Instance.MaxPressure.ToString("f3");
            DashboardViewModel.Instance.PressureMon[2] = DashboardViewModel.Instance.MinPressure.ToString("f3");
            break;
          case 0x24:
            ChannelsViewModel.Instance.Bias30Parameters[9] = exe.Parameter.ToString();
            break;
          case 0x25:
            ChannelsViewModel.Instance.Bias30Parameters[7] = exe.Parameter.ToString();
            break;
          case 0x26:
            ChannelsViewModel.Instance.Bias30Parameters[8] = exe.Parameter.ToString();
            break;
          case 0x28:
            ChannelsViewModel.Instance.Bias30Parameters[0] = exe.Parameter.ToString();
            break;
          case 0x29:
            ChannelsViewModel.Instance.Bias30Parameters[1] = exe.Parameter.ToString();
            break;
          case 0x2a:
            ChannelsViewModel.Instance.Bias30Parameters[2] = exe.Parameter.ToString();
            break;
          case 0x2c:
            ChannelsViewModel.Instance.Bias30Parameters[3] = exe.Parameter.ToString();
            break;
          case 0x2d:
            ChannelsViewModel.Instance.Bias30Parameters[4] = exe.Parameter.ToString();
            break;
          case 0x2e:
            ChannelsViewModel.Instance.Bias30Parameters[5] = exe.Parameter.ToString();
            break;
          case 0x2f:
            ChannelsViewModel.Instance.Bias30Parameters[6] = exe.Parameter.ToString();
            break;
          case 0x30:
            SyringeSpeedsViewModel.Instance.SheathSyringeParameters[0] = exe.Parameter.ToString();
            break;
          case 0x31:
            SyringeSpeedsViewModel.Instance.SheathSyringeParameters[1] = exe.Parameter.ToString();
            break;
          case 0x32:
            SyringeSpeedsViewModel.Instance.SheathSyringeParameters[2] = exe.Parameter.ToString();
            break;
          case 0x33:
            SyringeSpeedsViewModel.Instance.SheathSyringeParameters[3] = exe.Parameter.ToString();
            break;
          case 0x34:
            SyringeSpeedsViewModel.Instance.SheathSyringeParameters[4] = exe.Parameter.ToString();
            break;
          case 0x35:
            SyringeSpeedsViewModel.Instance.SheathSyringeParameters[5] = exe.Parameter.ToString();
            break;
          case 0x38:
            SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[0] = exe.Parameter.ToString();
            break;
          case 0x39:
            SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[1] = exe.Parameter.ToString();
            break;
          case 0x3a:
            SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[2] = exe.Parameter.ToString();
            break;
          case 0x3b:
            SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[3] = exe.Parameter.ToString();
            break;
          case 0x3c:
            SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[4] = exe.Parameter.ToString();
            break;
          case 0x3d:
            SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[5] = exe.Parameter.ToString();
            break;
          case 0x41:
            MotorsViewModel.Instance.ParametersZ[3] = exe.Parameter.ToString();
            break;
          case 0x42:
            MotorsViewModel.Instance.ParametersZ[4] = exe.Parameter.ToString();
            break;
          case 0x43:
            MotorsViewModel.Instance.ParametersZ[2] = exe.Parameter.ToString();
            break;
          case 0x44:
            MotorsViewModel.Instance.ParametersZ[5] = exe.FParameter.ToString();
            break;
          case 0x46:
            MotorsViewModel.Instance.StepsParametersZ[4] = exe.FParameter.ToString();
            break;
          case 0x48:
            MotorsViewModel.Instance.StepsParametersZ[0] = exe.FParameter.ToString();
            break;
          case 0x4a:
            MotorsViewModel.Instance.StepsParametersZ[1] = exe.FParameter.ToString();
            break;
          case 0x4c:
            MotorsViewModel.Instance.StepsParametersZ[2] = exe.FParameter.ToString();
            break;
          case 0x4e:
            MotorsViewModel.Instance.StepsParametersZ[3] = exe.FParameter.ToString();
            break;
          case 0x51:
            MotorsViewModel.Instance.ParametersX[3] = exe.Parameter.ToString();
            break;
          case 0x52:
            MotorsViewModel.Instance.ParametersX[4] = exe.Parameter.ToString();
            break;
          case 0x53:
            MotorsViewModel.Instance.ParametersX[2] = exe.Parameter.ToString();
            break;
          case 0x54:
            MotorsViewModel.Instance.ParametersX[5] = exe.FParameter.ToString();
            break;
          case 0x56:
            MotorsViewModel.Instance.StepsParametersX[4] = exe.FParameter.ToString();
            break;
          case 0x58:
            MotorsViewModel.Instance.StepsParametersX[0] = exe.FParameter.ToString();
            break;
          case 0x5a:
            MotorsViewModel.Instance.StepsParametersX[1] = exe.FParameter.ToString();
            break;
          case 0x5c:
            MotorsViewModel.Instance.StepsParametersX[2] = exe.FParameter.ToString();
            break;
          case 0x5e:
            MotorsViewModel.Instance.StepsParametersX[3] = exe.FParameter.ToString();
            break;
          case 0x61:
            MotorsViewModel.Instance.ParametersY[3] = exe.Parameter.ToString();
            break;
          case 0x62:
            MotorsViewModel.Instance.ParametersY[4] = exe.Parameter.ToString();
            break;
          case 0x63:
            MotorsViewModel.Instance.ParametersY[2] = exe.Parameter.ToString();
            break;
          case 0x64:
            MotorsViewModel.Instance.ParametersY[5] = exe.FParameter.ToString();
            break;
          case 0x66:
            MotorsViewModel.Instance.StepsParametersY[4] = exe.FParameter.ToString();
            break;
          case 0x68:
            MotorsViewModel.Instance.StepsParametersY[0] = exe.FParameter.ToString();
            break;
          case 0x6a:
            MotorsViewModel.Instance.StepsParametersY[1] = exe.FParameter.ToString();
            break;
          case 0x6c:
            MotorsViewModel.Instance.StepsParametersY[2] = exe.FParameter.ToString();
            break;
          case 0x6e:
            MotorsViewModel.Instance.StepsParametersY[3] = exe.FParameter.ToString();
            break;
          case 0x80:
            ChannelsViewModel.Instance.TempParameters[7] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0x81:
            ChannelsViewModel.Instance.TempParameters[8] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0x82:
            ChannelsViewModel.Instance.BackgroundParameters[7] = exe.Parameter.ToString();
            break;
          case 0x83:
            ChannelsViewModel.Instance.BackgroundParameters[8] = exe.Parameter.ToString();
            break;
          case 0x84:
            ChannelsViewModel.Instance.TempParameters[9] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0x85:
            ChannelsViewModel.Instance.BackgroundParameters[9] = exe.Parameter.ToString();
            break;
          case 0x8b:
            //CalibrationViewModel.Instance.ClassificationTargetsContents[0] = exe.Parameter.ToString();
            break;
          case 0x8c:
            //CalibrationViewModel.Instance.ClassificationTargetsContents[1] = exe.Parameter.ToString();
            break;
          case 0x8d:
            //CalibrationViewModel.Instance.ClassificationTargetsContents[2] = exe.Parameter.ToString();
            break;
          case 0x90:
            MotorsViewModel.Instance.ParametersX[7] = exe.Parameter.ToString();
            break;
          case 0x91:
            MotorsViewModel.Instance.ParametersY[7] = exe.Parameter.ToString();
            break;
          case 0x92:
            MotorsViewModel.Instance.ParametersZ[7] = exe.Parameter.ToString();
            break;
          case 0x93:
            ChannelsViewModel.Instance.TcompBiasParameters[9] = exe.Parameter.ToString();
            break;
          case 0x94:
            ChannelsViewModel.Instance.TcompBiasParameters[8] = exe.Parameter.ToString();
            break;
          case 0x95:
            ChannelsViewModel.Instance.TcompBiasParameters[7] = exe.Parameter.ToString();
            break;
          case 0x96:
            ChannelsViewModel.Instance.TcompBiasParameters[6] = exe.Parameter.ToString();
            break;
          case 0x98:
            ChannelsViewModel.Instance.TcompBiasParameters[4] = exe.Parameter.ToString();
            break;
          case 0x99:
            ChannelsViewModel.Instance.TcompBiasParameters[3] = exe.Parameter.ToString();
            break;
          case 0x9a:
            ChannelsViewModel.Instance.TcompBiasParameters[1] = exe.Parameter.ToString();
            break;
          case 0x9b:
            ChannelsViewModel.Instance.TcompBiasParameters[2] = exe.Parameter.ToString();
            break;
          case 0x9c:
            ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[8] = exe.Parameter.ToString();
            break;
          case 0x9d:
            ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[7] = exe.Parameter.ToString();
            break;
          case 0x9e:
            ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[9] = exe.Parameter.ToString();
            break;
          case 0x9f:
            ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[6] = exe.Parameter.ToString();
            break;
          case 0xa0:
            ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[0] = exe.Parameter.ToString();
            break;
          case 0xa1:
            ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[5] = exe.Parameter.ToString();
            break;
          case 0xa2:
            ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[4] = exe.Parameter.ToString();
            break;
          case 0xa3:
            ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[3] = exe.Parameter.ToString();
            break;
          case 0xa4:
            ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[1] = exe.Parameter.ToString();
            break;
          case 0xa5:
            ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[2] = exe.Parameter.ToString();
            break;
          case 0xa6:
            ChannelsViewModel.Instance.TcompBiasParameters[0] = exe.Parameter.ToString();
            break;
          case 0xa7:
            ChannelsViewModel.Instance.TcompBiasParameters[5] = exe.Parameter.ToString();
            break;
          case 0xa8:
            DashboardViewModel.Instance.OrderItems[exe.Parameter].ForAppUpdater(4);
            break;
          case 0xa9:
            DashboardViewModel.Instance.ClassiMapItems[exe.Parameter].ForAppUpdater(2);
            break;
          case 0xaa:  //read speed
            DashboardViewModel.Instance.SpeedItems[exe.Parameter].ForAppUpdater(1);
            break;
          case 0xac:
            DashboardViewModel.Instance.Volumes[1] = exe.Parameter.ToString();
            break;
          case 0xad:  //TODO: remove?
            if (exe.Parameter > 15)
              exe.Parameter = 0;
            //MotorsViewModel.Instance.WellRowButtonItems[exe.Parameter].ForAppUpdater(1);
            App.Device.PlateRow = (byte)exe.Parameter;
            break;
          case 0xae:  //TODO: remove?
            if (exe.Parameter > 24)
              exe.Parameter = 0;
            //MotorsViewModel.Instance.WellColumnButtonItems[exe.Parameter].ForAppUpdater(2);
            App.Device.PlateCol = (byte)exe.Parameter;  //TODO: it doesn't accout for 96well; can go overboard and crash
            break;
          case 0xaf:
            DashboardViewModel.Instance.Volumes[0] = exe.Parameter.ToString();
            break;
          case 0xb0:
            ChannelsViewModel.Instance.TempParameters[0] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0xb1:
            ChannelsViewModel.Instance.TempParameters[1] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0xb2:
            ChannelsViewModel.Instance.TempParameters[2] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0xb3:
            ChannelsViewModel.Instance.TempParameters[3] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0xb4:
            ChannelsViewModel.Instance.TempParameters[4] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0xb5:
            ChannelsViewModel.Instance.TempParameters[5] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0xb6:
            ChannelsViewModel.Instance.TempParameters[6] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0xb8:
            ChannelsViewModel.Instance.BackgroundParameters[0] = exe.Parameter.ToString();
            break;
          case 0xb9:
            ChannelsViewModel.Instance.BackgroundParameters[1] = exe.Parameter.ToString();
            break;
          case 0xba:
            ChannelsViewModel.Instance.BackgroundParameters[2] = exe.Parameter.ToString();
            break;
          case 0xbb:
            ChannelsViewModel.Instance.BackgroundParameters[3] = exe.Parameter.ToString();
            break;
          case 0xbc:
            ChannelsViewModel.Instance.BackgroundParameters[4] = exe.Parameter.ToString();
            break;
          case 0xbd:
            ChannelsViewModel.Instance.BackgroundParameters[5] = exe.Parameter.ToString();
            break;
          case 0xbe:
            ChannelsViewModel.Instance.BackgroundParameters[6] = exe.Parameter.ToString();
            break;
          case 0xc0:
            ComponentsViewModel.Instance.LasersActive[0] = (exe.Parameter & 0x01) == 1;
            ComponentsViewModel.Instance.LasersActive[1] = (exe.Parameter & 0x02) == 2;
            ComponentsViewModel.Instance.LasersActive[2] = (exe.Parameter & 0x04) == 4;
            break;
          case 0xc4:
            DashboardViewModel.Instance.Volumes[2] = exe.FParameter.ToString();
            break;
          case 0xc7:
            g = (float)(exe.Parameter / 4096.0 / 0.040 * 3.3);
            ComponentsViewModel.Instance.LaserVioletPowerValue[0] = g.ToString("N1") + " mw";
            break;
          case 0xc8:
            g = (float)(exe.Parameter / 4096.0 / 0.04 * 3.3);
            ComponentsViewModel.Instance.LaserGreenPowerValue[0] = g.ToString("N1") + " mw";
            break;
          case 0xc9:
            g = (float)(exe.Parameter / 4096.0 / 0.04 * 3.3);
            ComponentsViewModel.Instance.LaserRedPowerValue[0] = g.ToString("N1") + " mw";
            break;
          case 0xca:  //TODO: remove?
            CalibrationViewModel.Instance.GatingItems[exe.Parameter].ForAppUpdater();
            break;
          case 0xcc:  //sync pending
            var list = MainButtonsViewModel.Instance.ActiveList;
            for (var i = 0; i < 16; i++)
            {
              if ((exe.Parameter & (1 << i)) != 0)
              {
                if (!list.Contains(App.Device.SyncElements[i]))
                  list.Add(App.Device.SyncElements[i]);
              }
              else if (list.Contains(App.Device.SyncElements[i]))
                list.Remove(App.Device.SyncElements[i]);
            }
            break;
          case 0xcd:
            //CalibrationViewModel.Instance.EventTriggerContents[0] = exe.Parameter.ToString();
            break;
          case 0xce:
            //CalibrationViewModel.Instance.EventTriggerContents[1] = exe.Parameter.ToString();
            break;
          case 0xcf:
            //CalibrationViewModel.Instance.EventTriggerContents[2] = exe.Parameter.ToString();
            break;
          case 0xf1:
            if (exe.Command == 1)  //sheath empty
            {
              App.Device.MainCommand("Set Property", code: 0xcb, parameter: 0x1000);
              App.Device.MainCommand("Sheath"); //halt 
              App.Device.MainCommand("Set Property", code: 0xc1, parameter: 1);  //switch to recovery command buffer #1
              if (MessageBox.Show("Sheath Empty\nRefill and press OK", "Operator Alert", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
              {
                App.Device.MainCommand("Sheath Empty Prime");
                App.Device.MainCommand("Set Property", code: 0xcb); //clear sync token to allow recovery to run
              }
            }
            else if (exe.Command == 2) //pressure overload
            {
              if (exe.FParameter > int.Parse(ComponentsViewModel.Instance.MaxPressureBox))
              {
                if (MessageBox.Show("Pressure Overload\nCheck for waste line obstructions\nPower Off System",
                  "Operator Alert", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                  Environment.Exit(0);
                }
              }
            }
            break;
          case 0xf2:
            string ws;
            if (exe.Command > 0x63)
            {
              if (exe.Parameter == 0x501)  //sample syringe A
              {
                ws = "Sample syringe A Error " + exe.Command.ToString();
              }
              else ws = "Sample syringe B Error " + exe.Command.ToString();

              if (MessageBox.Show(ws + "\nPower Off System", "Operator Alert", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
              {
                var tempres = new List<WellResults>(App.Device.WellResults.Count);
                for (var i = 0; i < App.Device.WellResults.Count; i++)
                {
                  var r = new WellResults();
                  r.RP1vals = new List<float>(App.Device.WellResults[i].RP1vals);
                  r.RP1bgnd = new List<float>(App.Device.WellResults[i].RP1bgnd);
                  r.regionNumber = App.Device.WellResults[i].regionNumber;
                  tempres.Add(r);
                }
                App.Device.WellsToRead = App.Device.CurrentWellIdx;
                App.Device.SaveBeadFile(tempres);
                Environment.Exit(0);
              }
            }
            break;
          //  case 0xf8:
          //    string tabnam = tabControl1.SelectedTab.Name;
          //    App.Device.InitSTab(tabnam);
          //    break;
          case 0xfd:
            if (App.Device.EndState == 0)
              App.Device.EndState = 1; //start the end of well state machine
            break;
          case 0xfe:
            if (App.Device.EndState == 0)
              App.Device.EndState = 1;
            break;
          case 0xbf:
            //CalibrationViewModel.Instance.AttenuationBox[0] = exe.Parameter.ToString();
            break;
          case 0xf3:
            if (!ComponentsViewModel.Instance.SuppressWarnings)
            {
              ResultsViewModel.Instance.PlatePictogram.ChangeState(App.Device.ReadingRow, App.Device.ReadingCol, warning: Models.WellWarningState.YellowWarning);
              if (App.Device.CurrentWellIdx < App.Device.WellsToRead) //aspirating next
                App._nextWellWarning = true;
            }
            break;
        }
      }
      UpdatePressureMonitor();
    }

    public static void UpdateEventCounter()
    {
      MainViewModel.Instance.EventCountCurrent[0] = App.Device.BeadCount.ToString();
    }

    private static void UpdatePressureMonitor()
    {
      if (DashboardViewModel.Instance.PressureMonToggleButtonState)
        App.Device.MainCommand("Get FProperty", code: 0x22);
    }

    private static void HideChannels()
    {
      ChannelOffsetViewModel.Instance.OldBoardOffsetsVisible = Visibility.Hidden;
      Views.ChannelOffsetView.Instance.cover.Visibility = Visibility.Visible;
    }
  }
}
