using System;
using System.Threading;
using DIOS.Core;
using Ei_Dimension.ViewModels;

namespace Ei_Dimension
{
  internal class TextBoxHandler
  {
    private static readonly string[] SyncElements = { "SHEATH", "SAMPLE_A", "SAMPLE_B", "FLASH", "END_WELL", "VALVES", "X_MOTOR",
      "Y_MOTOR", "Z_MOTOR", "PROXIMITY", "PRESSURE", "WASHING", "FAULT", "ALIGN MOTOR", "MAIN VALVE", "SINGLE STEP" };

    private static readonly object ConditionVar = new object();

    public void ParameterUpdateEventHandler(object sender, ParameterUpdateArgs parameter)
    {
      Action update = null;
      switch (parameter.Type)
      {
        case DeviceParameterType.SiPMTempCoeff:
          update = () => ChannelOffsetViewModel.Instance.SiPMTempCoeff[0] = parameter.FloatParameter.ToString();
          break;
        case DeviceParameterType.IdexPosition:
          update = () => ComponentsViewModel.Instance.IdexTextBoxInputs[0] = parameter.Parameter.ToString("X2");
          break;
        case DeviceParameterType.TotalBeadsInFirmware:
          update = () =>
          {
            MainViewModel.Instance.TotalBeadsInFirmware[0] = parameter.FloatParameter.ToString();
            Console.WriteLine($"[Report] FW:SW {MainViewModel.Instance.TotalBeadsInFirmware[0]} : {MainViewModel.Instance.EventCountCurrent[0]}");
          };
          break;
        case DeviceParameterType.CalibrationMargin:
          update = () => ChannelOffsetViewModel.Instance.CalibrationMargin[0] = parameter.FloatParameter.ToString();
          break;
        case DeviceParameterType.ValveCuvetDrain:
          update = () => ComponentsViewModel.Instance.ValvesStates[2] = parameter.Parameter == 1;
          break;
        case DeviceParameterType.ValveFan1:
          update = () => ComponentsViewModel.Instance.ValvesStates[3] = parameter.Parameter == 1;
          break;
        case DeviceParameterType.ValveFan2:  //sample A valve cb LEGACY, use 0x18 to switch with IDEX
          update = () => ComponentsViewModel.Instance.ValvesStates[0] = parameter.Parameter == 1;
          break;
        case DeviceParameterType.SyringePosition:
          switch (parameter.Parameter)
          {
            case 0:
              update = () => ComponentsViewModel.Instance.GetPositionTextBoxInputs[0] = parameter.FloatParameter.ToString();
              break;
            case 1:
              update = () => ComponentsViewModel.Instance.GetPositionTextBoxInputs[1] = parameter.FloatParameter.ToString();
              break;
            case 2:
              update = () => ComponentsViewModel.Instance.GetPositionTextBoxInputs[2] = parameter.FloatParameter.ToString();
              break;
          }
          break;
        case DeviceParameterType.IsSyringePositionActive:
          update = () => ComponentsViewModel.Instance.GetPositionToggleButtonStateBool[0] = parameter.Parameter == 1;
          break;
        case DeviceParameterType.IsPollStepActive:
          update = () => MotorsViewModel.Instance.PollStepActive[0] = parameter.Parameter == 1;
          break;
        case DeviceParameterType.IsInputSelectorAtPickup:
          update = () =>
          {
            if (parameter.Parameter != ComponentsViewModel.Instance.IInputSelectorState)
            {
              var temp = ComponentsViewModel.Instance.InputSelectorState[0];
              ComponentsViewModel.Instance.InputSelectorState[0] = ComponentsViewModel.Instance.InputSelectorState[1];
              ComponentsViewModel.Instance.InputSelectorState[1] = temp;
              ComponentsViewModel.Instance.IInputSelectorState = parameter.Parameter;
            }
          };
          break;
        case DeviceParameterType.IsCalibrationFailed:
          if (parameter.Parameter == 0)
          {
            CalibrationViewModel.Instance.CalJustFailed = false;
            _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
              CalibrationViewModel.Instance.CalibrationSuccess();
            }));
          }
          break;
        case DeviceParameterType.Pressure:
          update = () =>
          {
            var source = ComponentsViewModel.Instance;
            double dd = parameter.FloatParameter;
            if (dd > source.MaxPressure)
              source.MaxPressure = dd;
            var maxPressure = source.MaxPressure;
            source.ActualPressure = dd;

            if (!Settings.Default.PressureUnitsPSI)
            {
              dd *= ComponentsViewModel.TOKILOPASCALCOEFFICIENT;
              maxPressure *= ComponentsViewModel.TOKILOPASCALCOEFFICIENT;
            }

            source.PressureMon[0] = dd.ToString("f3");
            source.PressureMon[1] = maxPressure.ToString("f3");
          };
          break;
        case DeviceParameterType.DNRCoefficient:
          update = () => CalibrationViewModel.Instance.DNRContents[0] = parameter.FloatParameter.ToString();
          break;
        case DeviceParameterType.ChannelBias30C:  //make param a channel, and FloatParam an actual value.
          var type = (Channel)parameter.Parameter;
          var pos = 0;
          switch (type)
          {
            case Channel.GreenA:
              pos = 0;
              break;
            case Channel.GreenB:
              pos = 1;
              break;
            case Channel.GreenC:
              pos = 2;
              break;
            case Channel.RedA:
              pos = 3;
              break;
            case Channel.RedB:
              pos = 4;
              break;
            case Channel.RedC:
              pos = 5;
              break;
            case Channel.RedD:
              pos = 6;
              break;
            case Channel.VioletA:
              pos = 7;
              break;
            case Channel.VioletB:
              pos = 8;
              break;
            case Channel.ForwardScatter:
              pos = 9;
              break;
          }
          update = () => ChannelsViewModel.Instance.Bias30Parameters[pos] = parameter.FloatParameter.ToString("N0");
          break;
        case DeviceParameterType.SyringeSpeedSheath:
          var type2 = (SyringeSpeed)parameter.Parameter;
          var pos2 = 0;
          switch (type2)
          {
            case SyringeSpeed.Normal:
              pos2 = 0;
              break;
            case SyringeSpeed.HiSpeed:
              pos2 = 1;
              break;
            case SyringeSpeed.HiSensitivity:
              pos2 = 2;
              break;
            case SyringeSpeed.Flush:
              pos2 = 3;
              break;
            case SyringeSpeed.Pickup:
              pos2 = 4;
              break;
            case SyringeSpeed.MaxSpeed:
              pos2 = 5;
              break;
          }
          update = () => SyringeSpeedsViewModel.Instance.SheathSyringeParameters[pos2] = parameter.FloatParameter.ToString("N0");
          break;
        case DeviceParameterType.SyringSpeedSample:
          var type3 = (SyringeSpeed)parameter.Parameter;
          var pos3 = 0;
          switch (type3)
          {
            case SyringeSpeed.Normal:
              pos3 = 0;
              break;
            case SyringeSpeed.HiSpeed:
              pos3 = 1;
              break;
            case SyringeSpeed.HiSensitivity:
              pos3 = 2;
              break;
            case SyringeSpeed.Flush:
              pos3 = 3;
              break;
            case SyringeSpeed.Pickup:
              pos3 = 4;
              break;
            case SyringeSpeed.MaxSpeed:
              pos3 = 5;
              break;
          }
          update = () => SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[pos3] = parameter.FloatParameter.ToString("N0");
          break;
        case DeviceParameterType.MotorX:
          var type4 = (MotorParameterType)parameter.Parameter;
          var pos4 = 0;
          string format4 = "N0";
          switch (type4)
          {
            case MotorParameterType.Slope:
              pos4 = 2;
              break;
            case MotorParameterType.StartSpeed:
              pos4 = 3;
              break;
            case MotorParameterType.RunSpeed:
              pos4 = 4;
              break;
            case MotorParameterType.CurrentStep:
              pos4 = 5;
              format4 = "N3";
              break;
            case MotorParameterType.CurrentLimit:
              pos4 = 7;
              break;
          }
          update = () => MotorsViewModel.Instance.ParametersX[pos4] = parameter.FloatParameter.ToString(format4);
          break;
        case DeviceParameterType.MotorY:
          var type5 = (MotorParameterType)parameter.Parameter;
          var pos5 = 0;
          string format5 = "N0";
          switch (type5)
          {
            case MotorParameterType.Slope:
              pos5 = 2;
              break;
            case MotorParameterType.StartSpeed:
              pos5 = 3;
              break;
            case MotorParameterType.RunSpeed:
              pos5 = 4;
              break;
            case MotorParameterType.CurrentStep:
              pos5 = 5;
              format5 = "N3";
              break;
            case MotorParameterType.CurrentLimit:
              pos5 = 7;
              break;
          }
          update = () => MotorsViewModel.Instance.ParametersY[pos5] = parameter.FloatParameter.ToString(format5);
          break;
        case DeviceParameterType.MotorZ:
          var type6 = (MotorParameterType)parameter.Parameter;
          var pos6 = 0;
          string format6 = "N0";
          switch (type6)
          {
            case MotorParameterType.Slope:
              pos6 = 2;
              break;
            case MotorParameterType.StartSpeed:
              pos6 = 3;
              break;
            case MotorParameterType.RunSpeed:
              pos6 = 4;
              break;
            case MotorParameterType.CurrentStep:
              pos6 = 5;
              format6 = "N3";
              break;
            case MotorParameterType.CurrentLimit:
              pos6 = 7;
              break;
          }

          if (pos6 == 5)
          {
            update = () =>
            {
              lock (PlateCustomizationViewModel.Instance.ZStepIsUpdatedLock)
              {
                MotorsViewModel.Instance.ParametersZ[pos6] = parameter.FloatParameter.ToString(format6);
                Monitor.PulseAll(PlateCustomizationViewModel.Instance.ZStepIsUpdatedLock);
              }
            };
            break;
          }
          update = () => MotorsViewModel.Instance.ParametersZ[pos6] = parameter.FloatParameter.ToString(format6);
          break;
        case DeviceParameterType.MotorStepsX:
          var type7 = (MotorStepsX)parameter.Parameter;
          var pos7 = 0;
          switch (type7)
          {
            case MotorStepsX.Plate96C1:
              pos7 = 0;
              break;
            case MotorStepsX.Plate96C12:
              pos7 = 1;
              break;
            case MotorStepsX.Plate384C1:
              pos7 = 2;
              break;
            case MotorStepsX.Plate384C24:
              pos7 = 3;
              break;
            case MotorStepsX.Tube:
              pos7 = 4;
              break;
          }
          update = () => MotorsViewModel.Instance.StepsParametersX[pos7] = parameter.FloatParameter.ToString();
          break;
        case DeviceParameterType.MotorStepsY:
          var type8 = (MotorStepsY)parameter.Parameter;
          var pos8 = 0;
          switch (type8)
          {
            case MotorStepsY.Plate96RowA:
              pos8 = 0;
              break;
            case MotorStepsY.Plate96RowH:
              pos8 = 1;
              break;
            case MotorStepsY.Plate384RowA:
              pos8 = 2;
              break;
            case MotorStepsY.Plate384RowP:
              pos8 = 3;
              break;
            case MotorStepsY.Tube:
              pos8 = 4;
              break;
          }
          update = () => MotorsViewModel.Instance.StepsParametersY[pos8] = parameter.FloatParameter.ToString();
          break;
        case DeviceParameterType.MotorStepsZ:
          var type9 = (MotorStepsZ)parameter.Parameter;
          switch (type9)
          {
            case MotorStepsZ.A1:
              update = () =>
              {
                MotorsViewModel.Instance.StepsParametersZ[0] = parameter.FloatParameter.ToString();
                PlateCustomizationViewModel.Instance.DefaultPlate.A1 = parameter.FloatParameter;
                PlateCustomizationViewModel.Instance.UpdateDefault();
              };
              break;
            case MotorStepsZ.A12:
              update = () =>
              {
                MotorsViewModel.Instance.StepsParametersZ[1] = parameter.FloatParameter.ToString();
                PlateCustomizationViewModel.Instance.DefaultPlate.A12 = parameter.FloatParameter;
                PlateCustomizationViewModel.Instance.UpdateDefault();
              };
              break;
            case MotorStepsZ.H1:
              update = () =>
              {
                MotorsViewModel.Instance.StepsParametersZ[2] = parameter.FloatParameter.ToString();
                PlateCustomizationViewModel.Instance.DefaultPlate.H1 = parameter.FloatParameter;
                PlateCustomizationViewModel.Instance.UpdateDefault();
              };
              break;
            case MotorStepsZ.H12:
              update = () =>
              {
                MotorsViewModel.Instance.StepsParametersZ[3] = parameter.FloatParameter.ToString();
                PlateCustomizationViewModel.Instance.DefaultPlate.H12 = parameter.FloatParameter;
                PlateCustomizationViewModel.Instance.UpdateDefault();
              };
              break;
            case MotorStepsZ.Tube:
              update = () => MotorsViewModel.Instance.StepsParametersZ[4] = parameter.FloatParameter.ToString();
              break;
          }
          break;
        case DeviceParameterType.ChannelTemperature:
          var type10 = (Channel)parameter.Parameter;
          var pos10 = 0;
          switch (type10)
          {
            case Channel.GreenA:
              pos10 = 0;
              break;
            case Channel.GreenB:
              pos10 = 1;
              break;
            case Channel.GreenC:
              pos10 = 2;
              break;
            case Channel.RedA:
              pos10 = 3;
              break;
            case Channel.RedB:
              pos10 = 4;
              break;
            case Channel.RedC:
              pos10 = 5;
              break;
            case Channel.RedD:
              pos10 = 6;
              break;
            case Channel.VioletA:
              pos10 = 7;
              break;
            case Channel.VioletB:
              pos10 = 8;
              break;
            case Channel.ForwardScatter:
              pos10 = 9;
              break;
          }
          update = () => ChannelsViewModel.Instance.TempParameters[pos10] = (parameter.FloatParameter / 10.0).ToString("N1");
          break;
        case DeviceParameterType.ChannelCompensationBias:
          var type11 = (Channel)parameter.Parameter;
          var pos11 = 0;
          switch (type11)
          {
            case Channel.GreenA:
              pos11 = 0;
              break;
            case Channel.GreenB:
              pos11 = 1;
              break;
            case Channel.GreenC:
              pos11 = 2;
              break;
            case Channel.RedA:
              pos11 = 3;
              break;
            case Channel.RedB:
              pos11 = 4;
              break;
            case Channel.RedC:
              pos11 = 5;
              break;
            case Channel.RedD:
              pos11 = 6;
              break;
            case Channel.VioletA:
              pos11 = 7;
              break;
            case Channel.VioletB:
              pos11 = 8;
              break;
            case Channel.ForwardScatter:
              pos11 = 9;
              break;
          }
          update = () => ChannelsViewModel.Instance.TcompBiasParameters[pos11] = parameter.FloatParameter.ToString("N0");
          break;
        case DeviceParameterType.ChannelOffset:
          var type12 = (Channel)parameter.Parameter;
          var pos12 = 0;
          object[] sliders =
          {
            ChannelOffsetViewModel.Instance.SliderValue1,
            ChannelOffsetViewModel.Instance.SliderValue2,
            ChannelOffsetViewModel.Instance.SliderValue3
          };
          switch (type12)
          {
            case Channel.GreenA:
              pos12 = 0;
              break;
            case Channel.GreenB:
              pos12 = 1;
              break;
            case Channel.GreenC:
              pos12 = 2;
              break;
            case Channel.RedA:
              pos12 = 3;
              break;
            case Channel.RedB:
              pos12 = 4;
              break;
            case Channel.RedC:
              pos12 = 5;
              break;
            case Channel.RedD:
              pos12 = 6;
              break;
            case Channel.VioletA:
              pos12 = 7;
              break;
            case Channel.VioletB:
              pos12 = 8;
              break;
            case Channel.ForwardScatter:
              pos12 = 9;
              break;
          }

          if (pos12 < 3)
          {
            update = () =>
            {
              ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[pos12] = parameter.FloatParameter.ToString("N0");
              ChannelOffsetViewModel.Instance.OverrideSliderChange = true;
              sliders[pos12] = (double)(parameter.FloatParameter);
            };
            break;
          }
          update = () => ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[pos12] = parameter.FloatParameter.ToString("N0");
          break;
        case DeviceParameterType.Volume:
          var type13 = (VolumeType)parameter.Parameter;
          var pos13 = 0;
          switch (type13)
          {
            case VolumeType.Sample:
              pos13 = 0;
              break;
            case VolumeType.Wash:
              pos13 = 1;
              break;
            case VolumeType.Agitate:
              pos13 = 2;
              break;
          }
          update = () => DashboardViewModel.Instance.Volumes[pos13] = parameter.FloatParameter.ToString("N0");
          break;
        case DeviceParameterType.GreenAVoltage://put param * 0.0008 to device library. send here the value only
          update = () => ChannelOffsetViewModel.Instance.GreenAVoltage[0] = (parameter.FloatParameter * 0.0008).ToString("N3");
          break;
        case DeviceParameterType.IsLaserActive: //remake a bit. decision should be in the lib. pass param and fparam, which is 0 or 2. compare as "fparam > 1" for true
          var type14 = (LaserType)parameter.Parameter;
          var pos14 = 0;
          switch (type14)
          {
            case LaserType.Red:
              pos14 = 0;
              break;
            case LaserType.Green:
              pos14 = 1;
              break;
            case LaserType.Violet:
              pos14 = 2;
              break;
          }
          ComponentsViewModel.Instance.LasersActive[pos14] = parameter.FloatParameter > 1;
          break;
        case DeviceParameterType.LaserPower: //remake a bit. calculation should be in the lib. pass param and fparam, which is 0 or 2. compare as "fparam > 1" for true
          var type15 = (LaserType)parameter.Parameter;
          var val = (parameter.FloatParameter / 4096.0 / 0.04 * 3.3);
          switch (type15)
          {
            case LaserType.Red:
              update = () => ComponentsViewModel.Instance.LaserRedPowerValue[0] = val.ToString("N1") + " mw";
              break;
            case LaserType.Green:
              update = () => ComponentsViewModel.Instance.LaserGreenPowerValue[0] = val.ToString("N1") + " mw";
              break;
            case LaserType.Violet:
              update = () => ComponentsViewModel.Instance.LaserVioletPowerValue[0] = val.ToString("N1") + " mw";
              break;
          }
          break;
        case DeviceParameterType.IsSynchronizationPending:
          update = () =>
          {
            var list = MainButtonsViewModel.Instance.ActiveList;
            for (var i = 0; i < 16; i++)
            {
              if ((parameter.Parameter & (1 << i)) != 0)
              {
                if (!list.Contains(SyncElements[i]))
                  list.Add(SyncElements[i]);
              }
              else if (list.Contains(SyncElements[i]))
                list.Remove(SyncElements[i]);
            }
          };
          break;
        case DeviceParameterType.SheathFlow:  //change command to parameter dependency in the lib. send unspecified in case cmd !=1 or !=2, so it doesnt break
          if ((SheathFlowErrorType)parameter.Parameter == SheathFlowErrorType.SheathEmpty)
          {
            App.Device.MainCommand("Set Property", code: 0xcb, parameter: 0x1000);
            App.Device.MainCommand("Sheath"); //halt 
            App.Device.MainCommand("Set Property", code: 0xc1, parameter: 1);  //switch to recovery command buffer #1

            void Act()
            {
              App.Device.MainCommand("Sheath Empty Prime");
              App.Device.MainCommand("Set Property", code: 0xcb); //clear sync token to allow recovery to run
              lock (ConditionVar)
              {
                Monitor.Pulse(ConditionVar);
              }
            }
            App.Current.Dispatcher.Invoke(() =>
            {
              var msg1 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Sheath_Empty),
                Language.TranslationSource.Instance.CurrentCulture);
              var msg2 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Refill_Sheath_ToContinue),
                Language.TranslationSource.Instance.CurrentCulture);
              Notification.Show($"{msg1}\n{msg2}", Act, "OK");
            });
            lock (ConditionVar)
            {
              Monitor.Wait(ConditionVar);
            }
          }
          else if ((SheathFlowErrorType)parameter.Parameter == SheathFlowErrorType.PressureOverload)
          {
            if (parameter.FloatParameter > App.Device.MaxPressure)
            {
              void Act()
              {
                Environment.Exit(0);
                lock (ConditionVar)
                {
                  Monitor.Pulse(ConditionVar);
                }
              }
              App.Current.Dispatcher.Invoke(() =>
              {
                var msg1 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Pressure_Overload),
                  Language.TranslationSource.Instance.CurrentCulture);
                var msg2 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_CheckForWasteLineObstructions),
                  Language.TranslationSource.Instance.CurrentCulture);
                var msg3 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Button_Power_Off_Sys),
                  Language.TranslationSource.Instance.CurrentCulture);
                Notification.Show($"{msg1}\n{msg2}", Act,
                    msg3);
              });
              lock (ConditionVar)
              {
                Monitor.Wait(ConditionVar);
              }
            }
          }
          break;
        case DeviceParameterType.SampleSyringeStatus: //make command=>parameter in the lib. syringe A if floatparam < 1. check the Parameter thing. reshape to send > 0
          string ws;
          if (parameter.Parameter > 0)
          {
            if (parameter.FloatParameter < 1)  //sample syringe A
            {
              ws = "Sample syringe A Error " + parameter.Parameter.ToString();
            }
            else ws = "Sample syringe B Error " + parameter.Parameter.ToString();

            void Act()
            {
              //App.Device.Publisher.PublishEverything();

              //Environment.Exit(0);
              lock (ConditionVar)
              {
                Monitor.Pulse(ConditionVar);
              }
            }
            App.Current.Dispatcher.Invoke(() =>
            {
              var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Power_Off_Sys),
                Language.TranslationSource.Instance.CurrentCulture);
              Notification.Show(ws + $"\n{msg}", Act, "OK");
            });
            lock (ConditionVar)
            {
              Monitor.Wait(ConditionVar);
            }
          }
          break;
        case DeviceParameterType.NextWellWarning:
          if (!ComponentsViewModel.Instance.SuppressWarnings && App.Device.IsMeasurementGoing)
          {
            update = () =>
            {
              var currentWell = App.Device.WellController.CurrentWell;
              if (currentWell == null)
                return;
              PlatePictogramViewModel.Instance.PlatePictogram.ChangeState(currentWell.RowIdx, currentWell.ColIdx,
                warning: Models.WellWarningState.YellowWarning);
              if (!App.Device.WellController.IsLastWell) //aspirating next
                App._nextWellWarning = true;
            };
          }
          break;
        case DeviceParameterType.BubbleDetectorFault: //pay attention to swapped ==0 and >0 states
          if (parameter.Parameter > 0)
          {
            var msg1 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Bubble_Detector_Fault),
              Language.TranslationSource.Instance.CurrentCulture);
            var msg2 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Press_OK_ToContinue),
              Language.TranslationSource.Instance.CurrentCulture);
            Notification.Show($"{msg1}\n{msg2}");
            Console.WriteLine("Bubble Detector Fault");
          }
          break;
      }
      if (update != null)
        App.Current.Dispatcher.Invoke(update);
    }

    public static void Update()
    {
      while (App.Device.Commands.TryDequeue(out var exe))
      {
        Action update = null;
        switch (exe.Code)
        {
          case 0x02:
            update = () => ChannelOffsetViewModel.Instance.SiPMTempCoeff[0] = exe.FParameter.ToString();
            break;
          case 0x04:
            update = () => ComponentsViewModel.Instance.IdexTextBoxInputs[0] = exe.Parameter.ToString("X2");
            break;
          case 0x06:
            update = () =>
            {
              MainViewModel.Instance.TotalBeadsInFirmware[0] = exe.FParameter.ToString();
              Console.WriteLine($"[Report] FW:SW {MainViewModel.Instance.TotalBeadsInFirmware[0]} : {MainViewModel.Instance.EventCountCurrent[0]}");
            };
            break;
          case 0x08:
            update = () => ChannelOffsetViewModel.Instance.CalibrationMargin[0] = exe.FParameter.ToString();
            break;
          case 0x10:  //cuvet drain cb
            update = () => ComponentsViewModel.Instance.ValvesStates[2] = exe.Parameter == 1;
            break;
          case 0x11:  //Fan
            update = () => ComponentsViewModel.Instance.ValvesStates[3] = exe.Parameter == 1;
            break;
          case 0x12:  //sample A valve cb LEGACY, use 0x18 to switch with IDEX
            update = () => ComponentsViewModel.Instance.ValvesStates[0] = exe.Parameter == 1;
            break;
          //case 0x13:  //sample b LEGACY
          //  ComponentsViewModel.Instance.ValvesStates[1] = exe.Parameter == 1;
          //  break;
          case 0x14:
            switch (exe.Parameter)
            {
              case 0:
                update = () => ComponentsViewModel.Instance.GetPositionTextBoxInputs[0] = exe.FParameter.ToString();
                break;
              case 1:
                update = () => ComponentsViewModel.Instance.GetPositionTextBoxInputs[1] = exe.FParameter.ToString();
                break;
              case 2:
                update = () => ComponentsViewModel.Instance.GetPositionTextBoxInputs[2] = exe.FParameter.ToString();
                break;
            }
            break;
          case 0x15:
            update = () => ComponentsViewModel.Instance.GetPositionToggleButtonStateBool[0] = exe.Parameter == 1;
            break;
          case 0x16:
            update = () => MotorsViewModel.Instance.PollStepActive[0] = exe.Parameter == 1;
            break;
          case 0x18:
            update = () =>
            {
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
            };
            break;
          case 0x1b:
            if (exe.Parameter == 0)
            {
              CalibrationViewModel.Instance.CalJustFailed = false;
              _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
              {
                CalibrationViewModel.Instance.CalibrationSuccess();
              }));
            }
            break;
          case 0x22:  //pressure
            update = () =>
            {
              var source = ComponentsViewModel.Instance;
              double dd = exe.FParameter;
              if (dd > source.MaxPressure)
                source.MaxPressure = dd;
              var maxPressure = source.MaxPressure;
              source.ActualPressure = dd;

              if (!Settings.Default.PressureUnitsPSI)
              {
                dd *= ComponentsViewModel.TOKILOPASCALCOEFFICIENT;
                maxPressure *= ComponentsViewModel.TOKILOPASCALCOEFFICIENT;
              }

              source.PressureMon[0] = dd.ToString("f3");
              source.PressureMon[1] = maxPressure.ToString("f3");
            };
            break;
          case 0x20:
            update = () => CalibrationViewModel.Instance.DNRContents[0] = exe.FParameter.ToString();
            break;
          case 0x24:
            update = () => ChannelsViewModel.Instance.Bias30Parameters[9] = exe.Parameter.ToString();
            break;
          case 0x25:
            update = () => ChannelsViewModel.Instance.Bias30Parameters[7] = exe.Parameter.ToString();
            break;
          case 0x26:
            update = () => ChannelsViewModel.Instance.Bias30Parameters[8] = exe.Parameter.ToString();
            break;
          case 0x28:
            update = () => ChannelsViewModel.Instance.Bias30Parameters[0] = exe.Parameter.ToString();
            break;
          case 0x29:
            update = () => ChannelsViewModel.Instance.Bias30Parameters[1] = exe.Parameter.ToString();
            break;
          case 0x2a:
            update = () => ChannelsViewModel.Instance.Bias30Parameters[2] = exe.Parameter.ToString();
            break;
          case 0x2c:
            update = () => ChannelsViewModel.Instance.Bias30Parameters[3] = exe.Parameter.ToString();
            break;
          case 0x2d:
            update = () => ChannelsViewModel.Instance.Bias30Parameters[4] = exe.Parameter.ToString();
            break;
          case 0x2e:
            update = () => ChannelsViewModel.Instance.Bias30Parameters[5] = exe.Parameter.ToString();
            break;
          case 0x2f:
            update = () => ChannelsViewModel.Instance.Bias30Parameters[6] = exe.Parameter.ToString();
            break;
          case 0x30:
            update = () => SyringeSpeedsViewModel.Instance.SheathSyringeParameters[0] = exe.Parameter.ToString();
            break;
          case 0x31:
            update = () => SyringeSpeedsViewModel.Instance.SheathSyringeParameters[1] = exe.Parameter.ToString();
            break;
          case 0x32:
            update = () => SyringeSpeedsViewModel.Instance.SheathSyringeParameters[2] = exe.Parameter.ToString();
            break;
          case 0x33:
            update = () => SyringeSpeedsViewModel.Instance.SheathSyringeParameters[3] = exe.Parameter.ToString();
            break;
          case 0x34:
            update = () => SyringeSpeedsViewModel.Instance.SheathSyringeParameters[4] = exe.Parameter.ToString();
            break;
          case 0x35:
            update = () => SyringeSpeedsViewModel.Instance.SheathSyringeParameters[5] = exe.Parameter.ToString();
            break;
          case 0x38:
            update = () => SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[0] = exe.Parameter.ToString();
            break;
          case 0x39:
            update = () => SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[1] = exe.Parameter.ToString();
            break;
          case 0x3a:
            update = () => SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[2] = exe.Parameter.ToString();
            break;
          case 0x3b:
            update = () => SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[3] = exe.Parameter.ToString();
            break;
          case 0x3c:
            update = () => SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[4] = exe.Parameter.ToString();
            break;
          case 0x3d:
            update = () => SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[5] = exe.Parameter.ToString();
            break;
          case 0x41:
            update = () => MotorsViewModel.Instance.ParametersZ[3] = exe.Parameter.ToString();
            break;
          case 0x42:
            update = () => MotorsViewModel.Instance.ParametersZ[4] = exe.Parameter.ToString();
            break;
          case 0x43:
            update = () => MotorsViewModel.Instance.ParametersZ[2] = exe.Parameter.ToString();
            break;
          case 0x44:
            update = () =>
            {
              lock (PlateCustomizationViewModel.Instance.ZStepIsUpdatedLock)
              {
                MotorsViewModel.Instance.ParametersZ[5] = exe.FParameter.ToString();
                Monitor.PulseAll(PlateCustomizationViewModel.Instance.ZStepIsUpdatedLock);
              }
            };
            break;
          case 0x46:
            update = () => MotorsViewModel.Instance.StepsParametersZ[4] = exe.FParameter.ToString();
            break;
          case 0x48:
            update = () =>
            {
              MotorsViewModel.Instance.StepsParametersZ[0] = exe.FParameter.ToString();
              PlateCustomizationViewModel.Instance.DefaultPlate.A1 = exe.FParameter;
              PlateCustomizationViewModel.Instance.UpdateDefault();
            };
            break;
          case 0x4a:
            update = () =>
            {
              MotorsViewModel.Instance.StepsParametersZ[1] = exe.FParameter.ToString();
              PlateCustomizationViewModel.Instance.DefaultPlate.A12 = exe.FParameter;
              PlateCustomizationViewModel.Instance.UpdateDefault();
            };
            break;
          case 0x4c:
            update = () =>
            {
              MotorsViewModel.Instance.StepsParametersZ[2] = exe.FParameter.ToString();
              PlateCustomizationViewModel.Instance.DefaultPlate.H1 = exe.FParameter;
              PlateCustomizationViewModel.Instance.UpdateDefault();
            };
            break;
          case 0x4e:
            update = () =>
            {
              MotorsViewModel.Instance.StepsParametersZ[3] = exe.FParameter.ToString();
              PlateCustomizationViewModel.Instance.DefaultPlate.H12 = exe.FParameter;
              PlateCustomizationViewModel.Instance.UpdateDefault();
            };
            break;
          case 0x51:
            update = () => MotorsViewModel.Instance.ParametersX[3] = exe.Parameter.ToString();
            break;
          case 0x52:
            update = () => MotorsViewModel.Instance.ParametersX[4] = exe.Parameter.ToString();
            break;
          case 0x53:
            update = () => MotorsViewModel.Instance.ParametersX[2] = exe.Parameter.ToString();
            break;
          case 0x54:
            update = () => MotorsViewModel.Instance.ParametersX[5] = exe.FParameter.ToString();
            break;
          case 0x56:
            update = () => MotorsViewModel.Instance.StepsParametersX[4] = exe.FParameter.ToString();
            break;
          case 0x58:
            update = () => MotorsViewModel.Instance.StepsParametersX[0] = exe.FParameter.ToString();
            break;
          case 0x5a:
            update = () => MotorsViewModel.Instance.StepsParametersX[1] = exe.FParameter.ToString();
            break;
          case 0x5c:
            update = () => MotorsViewModel.Instance.StepsParametersX[2] = exe.FParameter.ToString();
            break;
          case 0x5e:
            update = () => MotorsViewModel.Instance.StepsParametersX[3] = exe.FParameter.ToString();
            break;
          case 0x61:
            update = () => MotorsViewModel.Instance.ParametersY[3] = exe.Parameter.ToString();
            break;
          case 0x62:
            update = () => MotorsViewModel.Instance.ParametersY[4] = exe.Parameter.ToString();
            break;
          case 0x63:
            update = () => MotorsViewModel.Instance.ParametersY[2] = exe.Parameter.ToString();
            break;
          case 0x64:
            update = () => MotorsViewModel.Instance.ParametersY[5] = exe.FParameter.ToString();
            break;
          case 0x66:
            update = () => MotorsViewModel.Instance.StepsParametersY[4] = exe.FParameter.ToString();
            break;
          case 0x68:
            update = () => MotorsViewModel.Instance.StepsParametersY[0] = exe.FParameter.ToString();
            break;
          case 0x6a:
            update = () => MotorsViewModel.Instance.StepsParametersY[1] = exe.FParameter.ToString();
            break;
          case 0x6c:
            update = () => MotorsViewModel.Instance.StepsParametersY[2] = exe.FParameter.ToString();
            break;
          case 0x6e:
            update = () => MotorsViewModel.Instance.StepsParametersY[3] = exe.FParameter.ToString();
            break;
          case 0x80:
            update = () => ChannelsViewModel.Instance.TempParameters[7] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0x81:
            update = () => ChannelsViewModel.Instance.TempParameters[8] = (exe.Parameter / 10.0).ToString("N1");
            break;
          //case 0x82:
          //  update = () => ChannelOffsetViewModel.Instance.ChannelsBaseline[7] = exe.Parameter.ToString();
          //  break;
          //case 0x83:
          //  update = () => ChannelOffsetViewModel.Instance.ChannelsBaseline[8] = exe.Parameter.ToString();
          //  break;
          case 0x84:
            update = () => ChannelsViewModel.Instance.TempParameters[9] = (exe.Parameter / 10.0).ToString("N1");
            break;
          //case 0x85:
          //  update = () => ChannelOffsetViewModel.Instance.ChannelsBaseline[9] = exe.Parameter.ToString();
          //  break;
          case 0x8b:
            //update = () => CalibrationViewModel.Instance.ClassificationTargetsContents[0] = exe.Parameter.ToString();
            break;
          case 0x8c:
            //update = () => CalibrationViewModel.Instance.ClassificationTargetsContents[1] = exe.Parameter.ToString();
            break;
          case 0x8d:
            //update = () => CalibrationViewModel.Instance.ClassificationTargetsContents[2] = exe.Parameter.ToString();
            break;
          case 0x90:
            update = () => MotorsViewModel.Instance.ParametersX[7] = exe.Parameter.ToString();
            break;
          case 0x91:
            update = () => MotorsViewModel.Instance.ParametersY[7] = exe.Parameter.ToString();
            break;
          case 0x92:
            update = () => MotorsViewModel.Instance.ParametersZ[7] = exe.Parameter.ToString();
            break;
          case 0x93:
            update = () => ChannelsViewModel.Instance.TcompBiasParameters[9] = exe.Parameter.ToString();
            break;
          case 0x94:
            update = () => ChannelsViewModel.Instance.TcompBiasParameters[8] = exe.Parameter.ToString();
            break;
          case 0x95:
            update = () => ChannelsViewModel.Instance.TcompBiasParameters[7] = exe.Parameter.ToString();
            break;
          case 0x96:
            update = () => ChannelsViewModel.Instance.TcompBiasParameters[6] = exe.Parameter.ToString();
            break;
          case 0x98:
            update = () => ChannelsViewModel.Instance.TcompBiasParameters[4] = exe.Parameter.ToString();
            break;
          case 0x99:
            update = () => ChannelsViewModel.Instance.TcompBiasParameters[3] = exe.Parameter.ToString();
            break;
          case 0x9a:
            update = () => ChannelsViewModel.Instance.TcompBiasParameters[1] = exe.Parameter.ToString();
            break;
          case 0x9b:
            update = () => ChannelsViewModel.Instance.TcompBiasParameters[2] = exe.Parameter.ToString();
            break;
          case 0x9c:
            update = () => ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[8] = exe.Parameter.ToString();
            break;
          case 0x9d:
            update = () => ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[7] = exe.Parameter.ToString();
            break;
          case 0x9e:
            update = () => ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[9] = exe.Parameter.ToString();
            break;
          case 0x9f:
            update = () => ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[6] = exe.Parameter.ToString();
            break;
          case 0xa0:
            update = () =>
            {
              ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[0] = exe.Parameter.ToString();
              ChannelOffsetViewModel.Instance.OverrideSliderChange = true;
              ChannelOffsetViewModel.Instance.SliderValue1 = (double)(exe.Parameter);
            };
            break;
          case 0xa1:
            update = () => ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[5] = exe.Parameter.ToString();
            break;
          case 0xa2:
            update = () => ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[4] = exe.Parameter.ToString();
            break;
          case 0xa3:
            update = () => ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[3] = exe.Parameter.ToString();
            break;
          case 0xa4:
            update = () =>
            {
              ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[1] = exe.Parameter.ToString();
              ChannelOffsetViewModel.Instance.OverrideSliderChange = true;
              ChannelOffsetViewModel.Instance.SliderValue2 = (double)(exe.Parameter);
            };
            break;
          case 0xa5:
            update = () =>
            {
              ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[2] = exe.Parameter.ToString();
              ChannelOffsetViewModel.Instance.OverrideSliderChange = true;
              ChannelOffsetViewModel.Instance.SliderValue3 = (double)(exe.Parameter);
            };
            break;
          case 0xa6:
            update = () => ChannelsViewModel.Instance.TcompBiasParameters[0] = exe.Parameter.ToString();
            break;
          case 0xa7:
            update = () => ChannelsViewModel.Instance.TcompBiasParameters[5] = exe.Parameter.ToString();
            break;
          case 0xac:
            update = () => DashboardViewModel.Instance.Volumes[1] = exe.Parameter.ToString();
            break;
          case 0xaf:
            update = () => DashboardViewModel.Instance.Volumes[0] = exe.Parameter.ToString();
            break;
          case 0xb0:
            update = () => ChannelsViewModel.Instance.TempParameters[0] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0xb1:
            update = () => ChannelsViewModel.Instance.TempParameters[1] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0xb2:
            update = () => ChannelsViewModel.Instance.TempParameters[2] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0xb3:
            update = () => ChannelsViewModel.Instance.TempParameters[3] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0xb4:
            update = () => ChannelsViewModel.Instance.TempParameters[4] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0xb5:
            update = () => ChannelsViewModel.Instance.TempParameters[5] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0xb6:
            update = () => ChannelsViewModel.Instance.TempParameters[6] = (exe.Parameter / 10.0).ToString("N1");
            break;
          case 0xb8:
            update = () =>
            {
              ChannelOffsetViewModel.Instance.GreenAVoltage[0] = (exe.Parameter * 0.0008).ToString();
            };
            break;
          /*
          case 0xb9:
            update = () =>
            {
              ChannelOffsetViewModel.Instance.ChannelsBaseline[1] = exe.Parameter.ToString();
            };
            break;
          case 0xba:
            update = () =>
            {
              ChannelOffsetViewModel.Instance.ChannelsBaseline[2] = exe.Parameter.ToString();
            };
            break;
          case 0xbb:
            update = () =>
            {
              ChannelOffsetViewModel.Instance.ChannelsBaseline[3] = exe.Parameter.ToString();
            };
            break;
          case 0xbc:
            update = () =>
            {
              ChannelOffsetViewModel.Instance.ChannelsBaseline[4] = exe.Parameter.ToString();
            };
            break;
          case 0xbd:
            update = () =>
            {
              ChannelOffsetViewModel.Instance.ChannelsBaseline[5] = exe.Parameter.ToString();
            };
            break;
          case 0xbe:
            update = () =>
            {
              ChannelOffsetViewModel.Instance.ChannelsBaseline[6] = exe.Parameter.ToString();
            };
            break;
          */
          case 0xc0:
            update = () =>
            {
              ComponentsViewModel.Instance.LasersActive[0] = (exe.Parameter & 0x01) == 1;
              ComponentsViewModel.Instance.LasersActive[1] = (exe.Parameter & 0x02) == 2;
              ComponentsViewModel.Instance.LasersActive[2] = (exe.Parameter & 0x04) == 4;
            };
            break;
          case 0xc4:
            update = () => DashboardViewModel.Instance.Volumes[2] = exe.FParameter.ToString();
            break;
          case 0xc7:
            update = () =>
            {
              var g = (float)(exe.Parameter / 4096.0 / 0.040 * 3.3);
              ComponentsViewModel.Instance.LaserVioletPowerValue[0] = g.ToString("N1") + " mw";
            };
            break;
          case 0xc8:
            update = () =>
            {
              var g = (float)(exe.Parameter / 4096.0 / 0.04 * 3.3);
              ComponentsViewModel.Instance.LaserGreenPowerValue[0] = g.ToString("N1") + " mw";
            };
            break;
          case 0xc9:
            update = () =>
            {
              var g = (float)(exe.Parameter / 4096.0 / 0.04 * 3.3);
              ComponentsViewModel.Instance.LaserRedPowerValue[0] = g.ToString("N1") + " mw";
            };
            break;
          case 0xcc:  //sync pending
            update = () =>
            {
              var list = MainButtonsViewModel.Instance.ActiveList;
              for (var i = 0; i < 16; i++)
              {
                if ((exe.Parameter & (1 << i)) != 0)
                {
                  if (!list.Contains(SyncElements[i]))
                    list.Add(SyncElements[i]);
                }
                else if (list.Contains(SyncElements[i]))
                  list.Remove(SyncElements[i]);
              }
            };
            break;
          case 0xf1:
            if (exe.Command == 1)  //sheath empty
            {
              App.Device.MainCommand("Set Property", code: 0xcb, parameter: 0x1000);
              App.Device.MainCommand("Sheath"); //halt 
              App.Device.MainCommand("Set Property", code: 0xc1, parameter: 1);  //switch to recovery command buffer #1

              void Act()
              {
                App.Device.MainCommand("Sheath Empty Prime");
                App.Device.MainCommand("Set Property", code: 0xcb); //clear sync token to allow recovery to run
                lock (ConditionVar)
                {
                  Monitor.Pulse(ConditionVar);
                }
              }
              App.Current.Dispatcher.Invoke(() =>
              {
                var msg1 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Sheath_Empty),
                  Language.TranslationSource.Instance.CurrentCulture);
                var msg2 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Refill_Sheath_ToContinue),
                  Language.TranslationSource.Instance.CurrentCulture);
                Notification.Show($"{msg1}\n{msg2}", Act, "OK");
              });
              lock (ConditionVar)
              {
                Monitor.Wait(ConditionVar);
              }
            }
            else if (exe.Command == 2) //pressure overload
            {
              if (exe.FParameter > App.Device.MaxPressure)
              {
                void Act()
                {
                  Environment.Exit(0);
                  lock (ConditionVar)
                  {
                    Monitor.Pulse(ConditionVar);
                  }
                }
                App.Current.Dispatcher.Invoke(() =>
                {
                  var msg1 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Pressure_Overload),
                    Language.TranslationSource.Instance.CurrentCulture);
                  var msg2 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_CheckForWasteLineObstructions),
                    Language.TranslationSource.Instance.CurrentCulture);
                  var msg3 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Button_Power_Off_Sys),
                    Language.TranslationSource.Instance.CurrentCulture);
                  Notification.Show($"{msg1}\n{msg2}", Act,
                      msg3);
                });
                lock (ConditionVar)
                {
                  Monitor.Wait(ConditionVar);
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
              
              void Act()
              {
                //App.Device.Publisher.PublishEverything();

                //Environment.Exit(0);
                lock (ConditionVar)
                {
                  Monitor.Pulse(ConditionVar);
                }
              }
              App.Current.Dispatcher.Invoke(() =>
              {
                var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Power_Off_Sys),
                  Language.TranslationSource.Instance.CurrentCulture);
                Notification.Show(ws + $"\n{msg}", Act, "OK");
              });
              lock (ConditionVar)
              {
                Monitor.Wait(ConditionVar);
              }
            }
            break;
          case 0xbf:
            //CalibrationViewModel.Instance.AttenuationBox[0] = exe.Parameter.ToString();
            break;
          case 0xf3:
            if (!ComponentsViewModel.Instance.SuppressWarnings && App.Device.IsMeasurementGoing)
            {
              update = () =>
              {
                var currentWell = App.Device.WellController.CurrentWell;
                if (currentWell == null)
                  return;
                PlatePictogramViewModel.Instance.PlatePictogram.ChangeState(currentWell.RowIdx, currentWell.ColIdx,
                  warning: Models.WellWarningState.YellowWarning);
                if (!App.Device.WellController.IsLastWell) //aspirating next
                  App._nextWellWarning = true;
              };
            }
            break;
          case 0xf4:
            if (exe.Command == 0x00)
            {
              var msg1 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Bubble_Detector_Fault),
                Language.TranslationSource.Instance.CurrentCulture);
              var msg2 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Press_OK_ToContinue),
                Language.TranslationSource.Instance.CurrentCulture);
              Notification.Show($"{msg1}\n{msg2}");
              Console.WriteLine("Bubble Detector Fault");
            }
            break;
        }
        if (update != null)
          App.Current.Dispatcher.Invoke(update);
      }
      App.Current.Dispatcher.Invoke(UpdatePressureMonitor);
    }

    public static void UpdateEventCounter()
    {
      App.Current.Dispatcher.Invoke(() => MainViewModel.Instance.EventCountCurrent[0] = App.Device.BeadCount.ToString());
    }

    private static void UpdatePressureMonitor()
    {
      if (ComponentsViewModel.Instance.PressureMonToggleButtonState)
        App.Device.MainCommand("Get FProperty", code: 0x22);
    }
  }
}