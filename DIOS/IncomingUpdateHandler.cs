using System;
using System.Threading;
using System.Windows.Media;
using DIOS.Core;
using DIOS.Core.HardwareIntercom;
using Ei_Dimension.ViewModels;

namespace Ei_Dimension;

internal class IncomingUpdateHandler
{
  private static readonly string[] SyncElements = { "SHEATH", "SAMPLE_A", "SAMPLE_B", "FLASH", "END_WELL", "VALVES", "X_MOTOR",
    "Y_MOTOR", "Z_MOTOR", "WASH PUMP", "PRESSURE", "WASHING", "FAULT", "ALIGN MOTOR", "MAIN VALVE", "SINGLE STEP" };

  private readonly object _callingThreadLock = new();
  private int _externalRangeMultipliersObtained;
  private float[] _externalRangeMultipliersbuffer = {0, 0};
  private static readonly string FormatWithNoFloatingDedcimals = "F0";
  private static readonly string FormatWith1FloatingDedcimals = "F1";
  private static readonly string FormatWith3FloatingDecimals = "F3";

  public void ParameterUpdateEventHandler(object sender, ParameterUpdateEventArgs parameter)
  {
    if (parameter.Type != DeviceParameterType.BeadConcentration)
    {
      App.Logger.Log($"UI UPDATE: {parameter.ToString()}");
    }
    Action update = null;
    switch (parameter.Type)
    {
      case DeviceParameterType.SiPMTempCoeff:
        update = () => ChannelOffsetViewModel.Instance.SiPMTempCoeff[0] = parameter.FloatParameter.ToString();
        break;
      case DeviceParameterType.TotalBeadsInFirmware:
        update = () =>
        {
          MainViewModel.Instance.TotalBeadsInFirmware[0] = parameter.FloatParameter.ToString();
          App.Logger.Log($"[Report] FW:SW {MainViewModel.Instance.TotalBeadsInFirmware[0]} : {MainViewModel.Instance.EventCountCurrent[0]}");
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
      case DeviceParameterType.WashPump:  //sample A valve cb LEGACY, use 0x18 to switch with IDEX
        update = () => ComponentsViewModel.Instance.ValvesStates[0] = parameter.Parameter == 1;
        break;
      case DeviceParameterType.SyringePosition:
        switch ((SyringePosition)parameter.Parameter)
        {
          case SyringePosition.Sheath:
            update = () => ComponentsViewModel.Instance.GetPositionTextBoxInputs[0] = parameter.FloatParameter.ToString();
            break;
          case SyringePosition.SampleA:
            update = () => ComponentsViewModel.Instance.GetPositionTextBoxInputs[1] = parameter.FloatParameter.ToString();
            break;
          case SyringePosition.SampleB:
            update = () => ComponentsViewModel.Instance.GetPositionTextBoxInputs[2] = parameter.FloatParameter.ToString();
            break;
        }
        break;
      case DeviceParameterType.IsSyringePositionActive:
        update = () => ComponentsViewModel.Instance.GetPositionToggleButtonStateBool[0] = parameter.Parameter == 1;
        break;
      case DeviceParameterType.PollStepActivity:
        update = () => MotorsViewModel.Instance.PollStepActive[0] = parameter.Parameter == 1;
        break;
      case DeviceParameterType.IsInputSelectorAtPickup:
        update = () =>
        {
          if (parameter.Parameter <= 1 && parameter.Parameter != ComponentsViewModel.Instance.IInputSelectorState)
          {
            var temp = ComponentsViewModel.Instance.InputSelectorState[0];
            ComponentsViewModel.Instance.InputSelectorState[0] = ComponentsViewModel.Instance.InputSelectorState[1];
            ComponentsViewModel.Instance.InputSelectorState[1] = temp;
            ComponentsViewModel.Instance.IInputSelectorState = parameter.Parameter;
          }
        };
        break;
      case DeviceParameterType.CalibrationSuccess:
        CalibrationViewModel.Instance.CalJustFailed = false;
        _ = App.Current.Dispatcher.BeginInvoke(() =>
        {
          CalibrationViewModel.Instance.CalibrationSuccess();
        });
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

          source.PressureMon[0] = dd.ToString(FormatWith3FloatingDecimals);
          source.PressureMon[1] = maxPressure.ToString(FormatWith3FloatingDecimals);
        };
        break;
      case DeviceParameterType.BeadConcentration:
        update = () => MainViewModel.Instance.SetBeadConcentrationMonitorValue(parameter.Parameter);
        break;
      case DeviceParameterType.CalibrationParameter:
        var type16 = (CalibrationParameter)parameter.Parameter;
        switch (type16)
        {
          case CalibrationParameter.DNRCoefficient:
            update = () => CalibrationViewModel.Instance.DNRContents[0] = parameter.FloatParameter.ToString();
            break;
        }
        break;
      case DeviceParameterType.ChannelBias30C:
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
        update = () => ChannelsViewModel.Instance.Bias30Parameters[pos] = Math.Round(parameter.FloatParameter).ToString(FormatWithNoFloatingDedcimals);
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
        update = () => SyringeSpeedsViewModel.Instance.SheathSyringeParameters[pos2] = parameter.FloatParameter.ToString(FormatWithNoFloatingDedcimals);
        break;
      case DeviceParameterType.SyringeSpeedSample:
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
        update = () => SyringeSpeedsViewModel.Instance.SamplesSyringeParameters[pos3] = parameter.FloatParameter.ToString(FormatWithNoFloatingDedcimals);
        break;
      case DeviceParameterType.SampleSyringeType:
        var type17 = (SampleSyringeType)parameter.Parameter;
        bool out17 = false;
        switch (type17)
        {
          case SampleSyringeType.Single:
            out17 = false;
            break;
          case SampleSyringeType.Double:
            out17 = true;
            break;
          default:
            throw new Exception($"{nameof(DeviceParameterType.SampleSyringeType)} TbHandler should never happen");
        }
        update = () => SyringeSpeedsViewModel.Instance.SingleSyringeMode[0] = out17;
        break;
      case DeviceParameterType.IsWellEdgeAgitateActive:
        bool sign = parameter.Parameter != 0;
        update = () => SyringeSpeedsViewModel.Instance.WellEdgeAgitate[0] = sign;
        break;
      case DeviceParameterType.DistanceToWellEdge:
        update = () => SyringeSpeedsViewModel.Instance.EdgeDistance[0] = parameter.Parameter.ToString();
        break;
      case DeviceParameterType.WellEdgeDeltaHeight:
        update = () => SyringeSpeedsViewModel.Instance.EdgeHeight[0] = parameter.Parameter.ToString();
        break;
      case DeviceParameterType.FlushCycles:
        update = () => SyringeSpeedsViewModel.Instance.FlushCycles[0] = parameter.Parameter.ToString();
        break;
      case DeviceParameterType.MotorX:
        var type4 = (MotorParameterType)parameter.Parameter;
        var pos4 = 0;
        string format4 = FormatWithNoFloatingDedcimals;
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
            format4 = FormatWith3FloatingDecimals;
            break;
          case MotorParameterType.EncoderSteps:
            pos4 = 6;
            break;
        }
        update = () => MotorsViewModel.Instance.ParametersX[pos4] = parameter.FloatParameter.ToString(format4);
        break;
      case DeviceParameterType.MotorY:
        var type5 = (MotorParameterType)parameter.Parameter;
        var pos5 = 0;
        string format5 = FormatWithNoFloatingDedcimals;
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
            format5 = FormatWith3FloatingDecimals;
            break;
          case MotorParameterType.EncoderSteps:
            pos5 = 6;
            break;
        }
        update = () => MotorsViewModel.Instance.ParametersY[pos5] = parameter.FloatParameter.ToString(format5);
        break;
      case DeviceParameterType.MotorZ:
        var type6 = (MotorParameterType)parameter.Parameter;
        var pos6 = 0;
        string format6 = FormatWithNoFloatingDedcimals;
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
            format6 = FormatWith3FloatingDecimals;
            break;
          case MotorParameterType.EncoderSteps:
            pos6 = 6;
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
          case MotorStepsX.Plate96Column1:
            pos7 = 0;
            break;
          case MotorStepsX.Plate96Column12:
            pos7 = 1;
            break;
          case MotorStepsX.Plate384Column1:
            pos7 = 2;
            break;
          case MotorStepsX.Plate384Column24:
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
      case DeviceParameterType.WashStationDepth:
        update = () => MotorsViewModel.Instance.StepsParametersZ[5] = parameter.Parameter.ToString();
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
        update = () => ChannelsViewModel.Instance.TempParameters[pos10] = parameter.FloatParameter.ToString(FormatWith1FloatingDedcimals);
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
        update = () => ChannelsViewModel.Instance.TcompBiasParameters[pos11] = parameter.FloatParameter.ToString(FormatWithNoFloatingDedcimals);
        break;
      case DeviceParameterType.ChannelOffset:
        var type12 = (Channel)parameter.Parameter;
        var pos12 = 0;
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
          default:
            throw new NotImplementedException();
        }
        update = () =>
        {
          ChannelOffsetViewModel.Instance.ChannelsOffsetParameters[pos12] = parameter.FloatParameter.ToString(FormatWithNoFloatingDedcimals);
          ChannelOffsetViewModel.Instance.OverrideSliderChange = true;
          ChannelOffsetViewModel.Instance.SliderValues[pos12] = (double)(parameter.FloatParameter);
        };
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
          case VolumeType.ProbeWash:
            pos13 = 2;
            break;
          case VolumeType.Agitate:
            pos13 = 3;
            break;
        }
        update = () => DashboardViewModel.Instance.Volumes[pos13] = parameter.FloatParameter.ToString(FormatWithNoFloatingDedcimals);
        break;
      case DeviceParameterType.GreenAVoltage:
        update = () => ChannelOffsetViewModel.Instance.GreenAVoltage[0] = parameter.FloatParameter.ToString(FormatWith3FloatingDecimals);
        break;
      case DeviceParameterType.IsLaserActive:
        //0Red 1Green 2Violet
        update = () =>
        {
          ComponentsViewModel.Instance.LasersActive[0] = (parameter.Parameter & 0x01) == 1;
          ComponentsViewModel.Instance.LasersActive[1] = (parameter.Parameter & 0x02) == 2;
          ComponentsViewModel.Instance.LasersActive[2] = (parameter.Parameter & 0x04) == 4;
        };
        break;
      case DeviceParameterType.LaserPower:
        var type15 = (LaserType)parameter.Parameter;
        switch (type15)
        {
          case LaserType.Red:
            update = () => ComponentsViewModel.Instance.LaserRedPowerValue[0] = parameter.FloatParameter.ToString(FormatWith1FloatingDedcimals) + " mw";
            break;
          case LaserType.Green:
            update = () => ComponentsViewModel.Instance.LaserGreenPowerValue[0] = parameter.FloatParameter.ToString(FormatWith1FloatingDedcimals) + " mw";
            break;
          case LaserType.Violet:
            update = () => ComponentsViewModel.Instance.LaserVioletPowerValue[0] = parameter.FloatParameter.ToString(FormatWith1FloatingDedcimals) + " mw";
            break;
        }
        break;
      case DeviceParameterType.SystemActivityStatus:  //TODO: simplify interaction with lib. here logic should be less complicated
        update = () =>
        {
          var list = MainButtonsViewModel.Instance.ActiveList;
          for (var i = 0; i < 16; i++)
          {
            if ((parameter.Parameter & (1 << i)) is not 0)
            {
              if (!list.Contains(SyncElements[i]))
                list.Add(SyncElements[i]);
            }
            else if (list.Contains(SyncElements[i]))
              list.Remove(SyncElements[i]);
          }
        };
        break;
      case DeviceParameterType.SheathFlowError:
        if (parameter.Parameter == (int)SheathFlowError.SheathEmpty)
        {
          void Act()
          {
            App.DiosApp.Device.Hardware.RenewSheath();
            lock (_callingThreadLock)
            {
              Monitor.Pulse(_callingThreadLock);
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
          lock (_callingThreadLock) //will hang "ReplyFromMC" thread until pulsed. Is that the desired behaviour?
          {
            Monitor.Wait(_callingThreadLock);
          }
        }
        else if (parameter.Parameter == (int)SheathFlowError.HighPressure)
        {
          void Act()
          {
            lock (_callingThreadLock)
            {
              Monitor.Pulse(_callingThreadLock);
            }
          }

          App.Current.Dispatcher.Invoke(() =>
          {
            var msg1 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Pressure_Overload),
              Language.TranslationSource.Instance.CurrentCulture);
            var msg2 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_CheckForWasteLineObstructions),
              Language.TranslationSource.Instance.CurrentCulture);
            Notification.Show($"{msg1}\n{msg2}", Act,
              "OK", new SolidColorBrush(Color.FromRgb(0xFF, 0x67, 0x00)));
          });
          lock (_callingThreadLock) //will hang "ReplyFromMC" thread until pulsed. Is that the desired behaviour?
          {
            Monitor.Wait(_callingThreadLock);
          }
        }
        break;
      case DeviceParameterType.SampleSyringeStatus: //make command=>parameter in the lib. syringe A if floatparam < 1. check the Parameter thing. reshape to send > 0
        if (parameter.Parameter != 1)
          break;

        string ws;
        if (parameter.FloatParameter < 1)  //sample syringe A
        {
          ws = "Sample syringe A Error " + parameter.Parameter.ToString();
        }
        else ws = "Sample syringe B Error " + parameter.Parameter.ToString();

        void Act1()
        {
          //App.DiosApp.Publisher.PublishEverything();

          //Environment.Exit(0);
          lock (_callingThreadLock)
          {
            Monitor.Pulse(_callingThreadLock);
          }
        }
        App.Current.Dispatcher.Invoke(() =>
        {
          var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Power_Off_Sys),
            Language.TranslationSource.Instance.CurrentCulture);
          Notification.Show(ws + $"\n{msg}", Act1, "OK");
        });
        lock (_callingThreadLock)
        {
          Monitor.Wait(_callingThreadLock);
        }
        break;
      case DeviceParameterType.WellWarning:
        if (!ComponentsViewModel.Instance.SuppressWarnings && App.DiosApp.Device.IsMeasurementGoing)
        {
          update = () =>
          {
            var currentWell = PlatePictogramViewModel.Instance.PlatePictogram.CurrentlyReadCell;
            if (currentWell.row == -1 && currentWell.col == -1)
              return;
            PlatePictogramViewModel.Instance.PlatePictogram.ChangeState(currentWell.row, currentWell.col,
              warning: Models.WellWarningState.YellowWarning);
            if (parameter.Parameter != 0) //aspirating next
              App._nextWellWarning = true;
          };
        }
        break;
      case DeviceParameterType.BubbleDetectorStatus:
        if (parameter.Parameter == 0)
        {
          var msg1 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Bubble_Detector_Fault),
            Language.TranslationSource.Instance.CurrentCulture);
          var msg2 = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Press_OK_ToContinue),
            Language.TranslationSource.Instance.CurrentCulture);
          Notification.Show($"{msg1}\n{msg2}");
          App.Logger.Log("Bubble Detector Fault");
        }
        break;
      case DeviceParameterType.SampleSyringeSize:
        update = () => SyringeSpeedsViewModel.Instance.SampleSyringeSize[0] = parameter.Parameter.ToString();
        break;
      case DeviceParameterType.SheathFlushVolume:
        update = () => SyringeSpeedsViewModel.Instance.SheathFlushVolume[0] = parameter.Parameter.ToString();
        break;
      case DeviceParameterType.TraySteps:
        update = () => MotorsViewModel.Instance.TraySteps[0] = parameter.FloatParameter.ToString(FormatWith3FloatingDecimals);
        break;
      case DeviceParameterType.DirectFlashValue:
        update = () =>
        {
          Views.DirectMemoryAccessView.Instance.UnBlockUI();
          DirectMemoryAccessViewModel.Instance.IntValue[0] = parameter.Parameter.ToString();
          DirectMemoryAccessViewModel.Instance.FloatValue[0] = parameter.FloatParameter.ToString("F4");
        };
        break;
      case DeviceParameterType.ExtendedRangeMultiplier:
        //ExtendedRangeMultiplier must be requested always as a couple. otherwise the logic breaks
        var type18 = (Channel)parameter.Parameter;
        var pos18 = 0;
        switch (type18)
        {
          case Channel.RedC:
            pos18 = 0;
            _externalRangeMultipliersbuffer[0] = parameter.FloatParameter;
            break;
          case Channel.RedD:
            pos18 = 1;
            _externalRangeMultipliersbuffer[1] = parameter.FloatParameter;
            break;
        }

        if (Interlocked.Increment(ref _externalRangeMultipliersObtained) > 1)
        {
          _externalRangeMultipliersObtained = 0;
          Action overwrite = () =>
          {
            ComponentsViewModel.Instance.ExtendedRangeMultipliers[0] = _externalRangeMultipliersbuffer[0].ToString(FormatWith3FloatingDecimals);
            ComponentsViewModel.Instance.ExtendedRangeMultipliers[1] = _externalRangeMultipliersbuffer[1].ToString(FormatWith3FloatingDecimals);
            App.DiosApp.Device.ExtendedRangeCL1Multiplier = _externalRangeMultipliersbuffer[0];
            App.DiosApp.Device.ExtendedRangeCL2Multiplier = _externalRangeMultipliersbuffer[1];
            ComponentsViewModel.Instance.SaveExtRangeValuesToMap();
          };
          App.Current.Dispatcher.BeginInvoke(() =>
          {
            Notification.Show($"New extended range Multipliers are:\n\tCL1: {_externalRangeMultipliersbuffer[0]}\n\tCL2: {_externalRangeMultipliersbuffer[1]}",
              overwrite, "Apply new", null, "Keep Previous", fontSize: 34);
          });
        }
        //update = () => CalibrationViewModel.Instance.ExtendedRangeMultipliers[pos18] = parameter.FloatParameter.ToString(FormatWith3FloatingDecimals);
        break;
      case DeviceParameterType.FluidicPathLength:
        var type19 = (FluidicPathLength)parameter.Parameter;
        var pos19 = 0;
        switch (type19)
        {
          case FluidicPathLength.LoopAVolume:
            pos19 = 0;
            break;
          case FluidicPathLength.LoopBVolume:
            pos19 = 1;
            break;
          case FluidicPathLength.LoopAToPickupNeedle:
            pos19 = 2;
            break;
          case FluidicPathLength.LoopBToPickupNeedle:
            pos19 = 3;
            break;
          case FluidicPathLength.LoopAToFlowcellBase:
            pos19 = 4;
            break;
          case FluidicPathLength.LoopBToFlowcellBase:
            pos19 = 5;
            break;
          case FluidicPathLength.FlowCellNeedleVolume:
            pos19 = 6;
            break;
          case FluidicPathLength.SpacerSlug:
            pos19 = 7;
            break;
        }
        update = () => SyringeSpeedsViewModel.Instance.FluidicPathLengths[pos19] = parameter.FloatParameter.ToString(FormatWithNoFloatingDedcimals);
        break;
      case DeviceParameterType.WashStationXCenterCoordinate:
        update = () => MotorsViewModel.Instance.WashStationXCenterCoordinate[0] = parameter.Parameter.ToString();
        break;
      case DeviceParameterType.ChannelConfiguration:
        var type20 = (ChannelConfiguration)parameter.Parameter;
        var pos20 = 0;
        switch (type20)
        {
          case ChannelConfiguration.Standard:
            pos20 = 0;
            break;
          case ChannelConfiguration.Cells:
            pos20 = 1;
            break;
          case ChannelConfiguration.FM3D:
            pos20 = 2;
            break;
          case ChannelConfiguration.StandardPlusFsc:
            pos20 = 3;
            break;
          case ChannelConfiguration.StandardPlusExt:
            pos20 = 4;
            break;
          case ChannelConfiguration.OEMA:
            pos20 = 5;
            break;
          case ChannelConfiguration.OEMPMT:
            pos20 = 6;
            break;
        }
        update = () => ComponentsViewModel.Instance.ChConfigItems[pos20].Click(4);
        break;
      case DeviceParameterType.PressureWarningLevel:
        update = () =>
        {
          double maxPressureVal = parameter.FloatParameter;
          if (!Settings.Default.PressureUnitsPSI)
          {
            maxPressureVal *= ComponentsViewModel.TOKILOPASCALCOEFFICIENT;
          }
          ComponentsViewModel.Instance.MaxPressureBox[0] = maxPressureVal.ToString(FormatWith3FloatingDecimals);
        };
        break;
    }
    if (update != null)
      App.Current.Dispatcher.Invoke(update);
  }
}