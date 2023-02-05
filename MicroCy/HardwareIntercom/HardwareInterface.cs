using System;
using System.Threading;

namespace DIOS.Core.HardwareIntercom
{
  public class HardwareInterface
  {
    private byte _latestDirectGetCode;
    private DataController _dataController;
    private Device _device;
    private readonly ILogger _logger;
    internal HardwareInterface(Device device, DataController dataController, ILogger logger)
    {
      _device = device;
      _dataController = dataController;
      _logger = logger;
    }

    public void SendCommand(DeviceCommandType command)
    {
      _logger.Log($"{DateTime.Now.ToString()} CMD {command.ToString()}");
      ushort param = 0;
      byte commandCode = 0;
      byte extraAction = 0;
      switch (command)
      {
        case DeviceCommandType.CalibrationModeActivate:
          commandCode = 0x1B;
          param = 1;
          extraAction = 0x02;
          break;
        case DeviceCommandType.CalibrationModeDeactivate:
          commandCode = 0x1B;
          extraAction = 0x02;
          break;
        case DeviceCommandType.RefreshDAC:
          commandCode = 0xD3;
          break;
        case DeviceCommandType.SetBaseLine:
          commandCode = 0xD5;
          break;
        case DeviceCommandType.FlashSave:
          commandCode = 0xD6;
          break;
        case DeviceCommandType.FlashRestore:
          commandCode = 0xD8;
          break;
        case DeviceCommandType.FlashFactoryReset:
          commandCode = 0xD8;
          extraAction = 0x01;
          break;
        case DeviceCommandType.FlushCommandQueue:
          commandCode = 0xD9;
          extraAction = 0x02;
          break;
        case DeviceCommandType.StartSampling:
          commandCode = 0xDA;
          break;
        case DeviceCommandType.EndSampling:
          commandCode = 0xDB;
          break;
        case DeviceCommandType.Startup:
          commandCode = 0xE0;
          break;
        case DeviceCommandType.Prime:
          commandCode = 0xE1;
          break;
        case DeviceCommandType.RenewSheath:
          commandCode = 0xE2;
          break;
        case DeviceCommandType.WashA:
          commandCode = 0xE3;
          break;
        case DeviceCommandType.WashB:
          commandCode = 0xE4;
          break;
        case DeviceCommandType.EjectPlate:
          commandCode = 0xE5;
          break;
        case DeviceCommandType.LoadPlate:
          commandCode = 0xE6;
          break;
        case DeviceCommandType.PositionWellPlate:
          commandCode = 0xE7;
          break;
        case DeviceCommandType.AspirateA:
          commandCode = 0xE8;
          break;
        case DeviceCommandType.AspirateB:
          commandCode = 0xE9;
          break;
        case DeviceCommandType.ReadA:
          commandCode = 0xEA;
          _device._isReadingA = true;
          break;
        case DeviceCommandType.ReadB:
          commandCode = 0xEB;
          _device._isReadingA = false;
          break;
        case DeviceCommandType.ReadAAspirateB:
          commandCode = 0xEC;
          _device._isReadingA = true;
          break;
        case DeviceCommandType.ReadBAspirateA:
          commandCode = 0xED;
          _device._isReadingA = false;
          break;
        case DeviceCommandType.EndReadA:
          commandCode = 0xEE;
          break;
        case DeviceCommandType.EndReadB:
          commandCode = 0xEF;
          break;
        case DeviceCommandType.UpdateFirmware:
          commandCode = 0xF5;
          break;
        case DeviceCommandType.Synchronize:
          commandCode = 0xFA;
          break;
        default:
          throw new NotImplementedException();
      }
      MainCommand(code: commandCode, parameter: param, cmd: extraAction);
    }

    public void MoveIdex(bool isClockwise, byte position, ushort steps)
    {
      MainCommand(code: 0xD7, cmd: position, parameter: steps, fparameter: Convert.ToByte(isClockwise));
    }

    public void RenewSheath()
    {
      SendCommand(DeviceCommandType.RenewSheath);
      SetToken(HardwareToken.Synchronization); //clear sync token to allow recovery to run
    }

    public void SetParameter(DeviceParameterType primaryParameter, float value = 0f)
    {
      SetParameter(primaryParameter, null, value);
    }

    public void SetParameter(DeviceParameterType primaryParameter, Enum subParameter, float value = 0f)
    {
      var subparReport = subParameter == null ? "" : subParameter.ToString();
      _logger.Log($"{DateTime.Now.ToString()} SET {primaryParameter.ToString()} {subparReport} {value.ToString()}");
      ushort param = 0;
      float fparam = 0;
      byte commandCode = 0x00;
      byte extraAction = 0x02;
      var intValue = (ushort)Math.Round(value);
      switch (primaryParameter)
      {
        case DeviceParameterType.SiPMTempCoeff:
          commandCode = 0x02;
          fparam = value;
          break;
        /*Not used in SET
        case DeviceParameterType.IdexPosition:
          commandCode = 0x04;
          break;
        */
        case DeviceParameterType.SampleSyringeSize:
          commandCode = 0x05;
          param = intValue;
          break;
        case DeviceParameterType.TotalBeadsInFirmware:  //reset totalbeads in firmware
          commandCode = 0x06;
          fparam = value;
          break;
        case DeviceParameterType.CalibrationMargin:
          commandCode = 0x08;
          fparam = value;
          break;
        case DeviceParameterType.PressureAtStartup:
          commandCode = 0x0C;
          fparam = value;
          break;
        case DeviceParameterType.ValveCuvetDrain:
          commandCode = 0x10;
          param = intValue;
          break;
        case DeviceParameterType.ValveFan1:
          commandCode = 0x11;
          param = intValue;
          break;
        case DeviceParameterType.ValveFan2:
          commandCode = 0x12;
          param = intValue;
          break;
        case DeviceParameterType.IsSyringePositionActive:
          commandCode = 0x15;
          param = intValue;
          break;
        case DeviceParameterType.PollStepActivity:
          commandCode = 0x16;
          param = intValue;
          break;
        case DeviceParameterType.IsInputSelectorAtPickup:
          commandCode = 0x18;
          param = intValue;
          break;
        case DeviceParameterType.IsBubbleDetectionActive:
          commandCode = 0x19;
          param = intValue;
          break;
        case DeviceParameterType.BeadConcentration:
          commandCode = 0x1D;
          param = intValue;
          break;
        case DeviceParameterType.HiSensitivityChannel:
          commandCode = 0x1E;
          param = intValue;
          break;
        case DeviceParameterType.UVCSanitize:
          commandCode = 0x1F;
          param = intValue;
          break;
        case DeviceParameterType.CalibrationParameter:
          switch (subParameter)
          {
            case CalibrationParameter.Height:
              param = intValue;
              commandCode = 0xCD;
              break;
            case CalibrationParameter.MinSSC:
              param = intValue;
              commandCode = 0xCE;
              break;
            case CalibrationParameter.MaxSSC:
              param = intValue;
              commandCode = 0xCF;
              break;
            case CalibrationParameter.Attenuation:
              param = intValue;
              commandCode = 0xBF;
              break;
            case CalibrationParameter.DNRCoefficient:
              fparam = value;
              commandCode = 0x20;
              break;
            case CalibrationParameter.DNRTransition:
              fparam = value;
              commandCode = 0x0A;
              break;
            case CalibrationParameter.ScatterGate:
              param = intValue;
              commandCode = 0xCA;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.Pressure:
          commandCode = 0x22;
          fparam = value;
          break;
        case DeviceParameterType.ChannelBias30C:
          param = intValue;
          switch (subParameter)
          {
            case Channel.GreenA:
              commandCode = 0x28;
              break;
            case Channel.GreenB:
              commandCode = 0x29;
              break;
            case Channel.GreenC:
              commandCode = 0x2A;
              break;
            case Channel.RedA:
              commandCode = 0x2C;
              break;
            case Channel.RedB:
              commandCode = 0x2D;
              break;
            case Channel.RedC:
              commandCode = 0x2E;
              break;
            case Channel.RedD:
              commandCode = 0x2F;
              break;
            case Channel.VioletA:
              commandCode = 0x25;
              break;
            case Channel.VioletB:
              commandCode = 0x26;
              break;
            case Channel.ForwardScatter:
              commandCode = 0x24;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.SampleSyringeType:
          commandCode = 0x3E;
          switch (subParameter)
          {
            case SampleSyringeType.Single:
              param = 0xFFFF;
              _device._singleSyringeMode = true;
              break;
            case SampleSyringeType.Double:
              param = 0;
              _device._singleSyringeMode = false;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        /*  //Not used in SET
        case DeviceParameterType.ChannelCompensationBias:
          switch (subParameter)
          {
            case Channel.GreenA:
              commandCode = 0xA6;
              break;
            case Channel.GreenB:
              commandCode = 0x9A;
              break;
            case Channel.GreenC:
              commandCode = 0x9B;
              break;
            case Channel.RedA:
              commandCode = 0x99;
              break;
            case Channel.RedB:
              commandCode = 0x98;
              break;
            case Channel.RedC:
              commandCode = 0xA7;
              break;
            case Channel.RedD:
              commandCode = 0x96;
              break;
            case Channel.VioletA:
              commandCode = 0x95;
              break;
            case Channel.VioletB:
              commandCode = 0x94;
              break;
            case Channel.ForwardScatter:
              commandCode = 0x93;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        */
        /*  //Not used in SET
        case DeviceParameterType.ChannelTemperature:
          switch (subParameter)
          {
            case Channel.GreenA:
              commandCode = 0xB0;
              break;
            case Channel.GreenB:
              commandCode = 0xB1;
              break;
            case Channel.GreenC:
              commandCode = 0xB2;
              break;
            case Channel.RedA:
              commandCode = 0xB3;
              break;
            case Channel.RedB:
              commandCode = 0xB4;
              break;
            case Channel.RedC:
              commandCode = 0xB5;
              break;
            case Channel.RedD:
              commandCode = 0xB6;
              break;
            case Channel.VioletA:
              commandCode = 0x80;
              break;
            case Channel.VioletB:
              commandCode = 0x81;
              break;
            case Channel.ForwardScatter:
              commandCode = 0x84;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        */
        case DeviceParameterType.ChannelOffset:
          param = intValue;
          switch (subParameter)
          {
            case Channel.GreenA:
              commandCode = 0xA0;
              break;
            case Channel.GreenB:
              commandCode = 0xA4;
              break;
            case Channel.GreenC:
              commandCode = 0xA5;
              break;
            case Channel.RedA:
              commandCode = 0xA3;
              break;
            case Channel.RedB:
              commandCode = 0xA2;
              break;
            case Channel.RedC:
              commandCode = 0xA1;
              break;
            case Channel.RedD:
              commandCode = 0x9F;
              break;
            case Channel.VioletA:
              commandCode = 0x9D;
              break;
            case Channel.VioletB:
              commandCode = 0x9C;
              break;
            case Channel.ForwardScatter:
              commandCode = 0x9E;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.SyringeSpeedSheath:
          param = intValue;
          switch (subParameter)
          {
            case SyringeSpeed.Normal:
              commandCode = 0x30;
              break;
            case SyringeSpeed.HiSpeed:
              commandCode = 0x31;
              break;
            case SyringeSpeed.HiSensitivity:
              commandCode = 0x32;
              break;
            case SyringeSpeed.Flush:
              commandCode = 0x33;
              break;
            case SyringeSpeed.Pickup:
              commandCode = 0x34;
              break;
            case SyringeSpeed.MaxSpeed:
              commandCode = 0x35;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.SyringeSpeedSample:
          param = intValue;
          switch (subParameter)
          {
            case SyringeSpeed.Normal:
              commandCode = 0x38;
              break;
            case SyringeSpeed.HiSpeed:
              commandCode = 0x39;
              break;
            case SyringeSpeed.HiSensitivity:
              commandCode = 0x3A;
              break;
            case SyringeSpeed.Flush:
              commandCode = 0x3B;
              break;
            case SyringeSpeed.Pickup:
              commandCode = 0x3C;
              break;
            case SyringeSpeed.MaxSpeed:
              commandCode = 0x3D;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.MotorX:
          param = intValue;
          switch (subParameter)
          {
            case MotorParameterType.Slope:
              commandCode = 0x53;
              break;
            case MotorParameterType.StartSpeed:
              commandCode = 0x51;
              break;
            case MotorParameterType.RunSpeed:
              commandCode = 0x52;
              break;
            case MotorParameterType.EncoderSteps:
              commandCode = 0x50;
              break;
            /*
            case MotorParameterType.CurrentStep:
              commandCode = 0x54;
              break;
            */
            case MotorParameterType.CurrentLimit:
              commandCode = 0x90;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.MotorY:
          param = intValue;
          switch (subParameter)
          {
            case MotorParameterType.Slope:
              commandCode = 0x63;
              break;
            case MotorParameterType.StartSpeed:
              commandCode = 0x61;
              break;
            case MotorParameterType.RunSpeed:
              commandCode = 0x62;
              break;
            case MotorParameterType.EncoderSteps:
              commandCode = 0x60;
              break;
            /*
            case MotorParameterType.CurrentStep:
              commandCode = 0x64;
              break;
            */
            case MotorParameterType.CurrentLimit:
              commandCode = 0x91;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.MotorZ:
          param = intValue;
          switch (subParameter)
          {
            case MotorParameterType.Slope:
              commandCode = 0x43;
              break;
            case MotorParameterType.StartSpeed:
              commandCode = 0x41;
              break;
            case MotorParameterType.RunSpeed:
              commandCode = 0x42;
              break;
            case MotorParameterType.EncoderSteps:
              commandCode = 0x40;
              break;
            /*
            case MotorParameterType.CurrentStep:
              commandCode = 0x44;
              break;
            */
            case MotorParameterType.CurrentLimit:
              commandCode = 0x92;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.MotorStepsX:
          fparam = value;
          switch (subParameter)
          {
            case MotorStepsX.Plate96Column1:
              commandCode = 0x58;
              break;
            case MotorStepsX.Plate96Column12:
              commandCode = 0x5A;
              break;
            case MotorStepsX.Plate384Column1:
              commandCode = 0x5C;
              break;
            case MotorStepsX.Plate384Column24:
              commandCode = 0x5E;
              break;
            case MotorStepsX.Tube:
              commandCode = 0x56;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.MotorStepsY:
          fparam = value;
          switch (subParameter)
          {
            case MotorStepsY.Plate96RowA:
              commandCode = 0x68;
              break;
            case MotorStepsY.Plate96RowH:
              commandCode = 0x6A;
              break;
            case MotorStepsY.Plate384RowA:
              commandCode = 0x6C;
              break;
            case MotorStepsY.Plate384RowP:
              commandCode = 0x6E;
              break;
            case MotorStepsY.Tube:
              commandCode = 0x66;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.MotorStepsZ:
          fparam = value;
          switch (subParameter)
          {
            case MotorStepsZ.A1:
              commandCode = 0x48;
              break;
            case MotorStepsZ.A12:
              commandCode = 0x4A;
              break;
            case MotorStepsZ.H1:
              commandCode = 0x4C;
              break;
            case MotorStepsZ.H12:
              commandCode = 0x4E;
              break;
            case MotorStepsZ.Tube:
              commandCode = 0x46;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.MotorStepsZTemporary:
          fparam = value;
          extraAction = 0x03;
          switch (subParameter)
          {
            case MotorStepsZ.A1:
              commandCode = 0x48;
              break;
            case MotorStepsZ.A12:
              commandCode = 0x4A;
              break;
            case MotorStepsZ.H1:
              commandCode = 0x4C;
              break;
            case MotorStepsZ.H12:
              commandCode = 0x4E;
              break;
            case MotorStepsZ.Tube:
              commandCode = 0x46;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.CalibrationTarget:
          param = intValue;
          switch (subParameter)
          {
            case CalibrationTarget.CL0:
              commandCode = 0x8B;
              break;
            case CalibrationTarget.CL1:
              commandCode = 0x8C;
              break;
            case CalibrationTarget.CL2:
              commandCode = 0x8D;
              break;
            case CalibrationTarget.CL3:
              commandCode = 0x8E;
              break;
            case CalibrationTarget.RP1:
              commandCode = 0x8F;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.WashRepeatsAmount:
          commandCode = 0x86;
          param = intValue;
          break;
        case DeviceParameterType.AgitateRepeatsAmount:
          commandCode = 0x7F;
          param = intValue;
          break;
        case DeviceParameterType.WellReadingOrder:
          commandCode = 0xA8;
          param = intValue;
          break;
        case DeviceParameterType.WellReadingSpeed:
          commandCode = 0xAA;
          param = intValue;
          break;
        case DeviceParameterType.PlateType:
          commandCode = 0xAB;
          param = intValue;
          break;
        case DeviceParameterType.WellRowIndex:
          commandCode = 0xAD;
          param = intValue;
          break;
        case DeviceParameterType.WellColumnIndex:
          commandCode = 0xAE;
          param = intValue;
          break;
        case DeviceParameterType.Volume:
          switch (subParameter)
          {
            case VolumeType.Wash:
              commandCode = 0xAC;
              param = intValue;
              break;
            case VolumeType.Sample:
              commandCode = 0xAF;
              param = intValue;
              break;
            case VolumeType.Agitate:
              commandCode = 0xC4;
              param = intValue;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.IsLaserActive:
          commandCode = 0xC0;
          param = intValue;
          break;
        case DeviceParameterType.ChannelConfiguration:
          commandCode = 0xC2;
          param = intValue;
          break;
        /*  //Not used in SET
        case DeviceParameterType.LaserPower:
          switch (subParameter)
          {
            case LaserType.Violet:
              commandCode = 0xC7;
              break;
            case LaserType.Green:
              commandCode = 0xC8;
              break;
            case LaserType.Red:
              commandCode = 0xC9;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        */
        case DeviceParameterType.AutoAlignState:
          commandCode = 0xC5;
          switch (subParameter)
          {
            case AutoAlignState.Off:
              param = 0;
              break;
            case AutoAlignState.Green:
              param = 1;
              break;
            case AutoAlignState.Red:
              param = 2;
              break;
            case AutoAlignState.Violet:
              param = 3;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.SystemActivityStatus:
          commandCode = 0xCC;
          param = intValue;
          break;
        case DeviceParameterType.PumpSheath:
          commandCode = 0xD0;
          param = intValue;
          switch (subParameter)
          {
            case SyringeControlState.Halt:
              extraAction = 0;
              break;
            case SyringeControlState.MoveAbsolute:
              extraAction = 1;
              break;
            case SyringeControlState.Pickup:
              extraAction = 2;
              break;
            case SyringeControlState.PreInject:
              extraAction = 3;
              break;
            case SyringeControlState.Speed:
              extraAction = 4;
              break;
            case SyringeControlState.Initialize:
              extraAction = 5;
              break;
            case SyringeControlState.Boot:
              extraAction = 6;
              break;
            case SyringeControlState.ValveLeft:
              extraAction = 7;
              break;
            case SyringeControlState.ValveRight:
              extraAction = 8;
              break;
            case SyringeControlState.MicroStep:
              extraAction = 9;
              break;
            case SyringeControlState.SpeedPreset:
              extraAction = 10;
              break;
            case SyringeControlState.Position:
              extraAction = 11;
              break;
            default:
              throw new NotImplementedException($"{nameof(DeviceParameterType.PumpSheath)} subParameter expected to be {nameof(SyringeControlState)}");
          }
          break;
        case DeviceParameterType.PumpSampleA:
          commandCode = 0xD1;
          param = intValue;
          switch (subParameter)
          {
            case SyringeControlState.Halt:
              extraAction = 0;
              break;
            case SyringeControlState.MoveAbsolute:
              extraAction = 1;
              break;
            case SyringeControlState.Pickup:
              extraAction = 2;
              break;
            case SyringeControlState.PreInject:
              extraAction = 3;
              break;
            case SyringeControlState.Speed:
              extraAction = 4;
              break;
            case SyringeControlState.Initialize:
              extraAction = 5;
              break;
            case SyringeControlState.Boot:
              extraAction = 6;
              break;
            case SyringeControlState.ValveLeft:
              extraAction = 7;
              break;
            case SyringeControlState.ValveRight:
              extraAction = 8;
              break;
            case SyringeControlState.MicroStep:
              extraAction = 9;
              break;
            case SyringeControlState.SpeedPreset:
              extraAction = 10;
              break;
            case SyringeControlState.Position:
              extraAction = 11;
              break;
            default:
              throw new NotImplementedException($"{nameof(DeviceParameterType.PumpSampleA)} subParameter expected to be {nameof(SyringeControlState)}");
          }
          break;
        case DeviceParameterType.PumpSampleB:
          commandCode = 0xD2;
          param = intValue;
          switch (subParameter)
          {
            case SyringeControlState.Halt:
              extraAction = 0;
              break;
            case SyringeControlState.MoveAbsolute:
              extraAction = 1;
              break;
            case SyringeControlState.Pickup:
              extraAction = 2;
              break;
            case SyringeControlState.PreInject:
              extraAction = 3;
              break;
            case SyringeControlState.Speed:
              extraAction = 4;
              break;
            case SyringeControlState.Initialize:
              extraAction = 5;
              break;
            case SyringeControlState.Boot:
              extraAction = 6;
              break;
            case SyringeControlState.ValveLeft:
              extraAction = 7;
              break;
            case SyringeControlState.ValveRight:
              extraAction = 8;
              break;
            case SyringeControlState.MicroStep:
              extraAction = 9;
              break;
            case SyringeControlState.SpeedPreset:
              extraAction = 10;
              break;
            case SyringeControlState.Position:
              extraAction = 11;
              break;
            default:
              throw new NotImplementedException($"{nameof(DeviceParameterType.PumpSampleB)} subParameter expected to be {nameof(SyringeControlState)}");
          }
          break;
        case DeviceParameterType.MotorMoveX:
          commandCode = 0xDD;
          fparam = value;
          switch (subParameter)
          {
            case MotorDirection.Halt:
              extraAction = 0;
              break;
            case MotorDirection.Left:
              extraAction = 1;
              break;
            case MotorDirection.Right:
              extraAction = 2;
              break;
            default:
              throw new NotImplementedException($"{nameof(DeviceParameterType.MotorMoveX)} subParameter expected to be {nameof(MotorDirection.Halt)} / {nameof(MotorDirection.Left)} / {nameof(MotorDirection.Right)}");
          }
          break;
        case DeviceParameterType.MotorMoveY:
          commandCode = 0xDE;
          fparam = value;
          switch (subParameter)
          {
            case MotorDirection.Halt:
              extraAction = 0;
              break;
            case MotorDirection.Back:
              extraAction = 1;
              break;
            case MotorDirection.Front:
              extraAction = 2;
              break;
            default:
              throw new NotImplementedException($"{nameof(DeviceParameterType.MotorMoveY)} subParameter expected to be {nameof(MotorDirection.Halt)} / {nameof(MotorDirection.Back)} / {nameof(MotorDirection.Front)}");
          }
          break;
        case DeviceParameterType.MotorMoveZ:
          commandCode = 0xDF;
          fparam = value;
          switch (subParameter)
          {
            case MotorDirection.Halt:
              extraAction = 0;
              break;
            case MotorDirection.Up:
              extraAction = 1;
              break;
            case MotorDirection.Down:
              extraAction = 2;
              break;
            default:
              throw new NotImplementedException($"{nameof(DeviceParameterType.MotorMoveZ)} subParameter expected to be {nameof(MotorDirection.Halt)} / {nameof(MotorDirection.Up)} / {nameof(MotorDirection.Down)}");
          }
          break;
        case DeviceParameterType.AlignMotor:
          commandCode = 0xDC;
          fparam = value;
          switch (subParameter)
          {
            case AlignMotorSequence.Scan:
              extraAction = 3;
              break;
            case AlignMotorSequence.FindPeak:
              extraAction = 4;
              break;
            case AlignMotorSequence.GoTo:
              extraAction = 5;
              break;
            default:
              throw new NotImplementedException($"{nameof(DeviceParameterType.AlignMotor)} subParameter expected to be {nameof(AlignMotorSequence)}");
          }
          break;
        case DeviceParameterType.IsSingleStepDebugActive:
          commandCode = 0xF7;
          param = intValue;
          break;
        default:
          throw new NotImplementedException();
      }

      MainCommand(code: commandCode, parameter: param, fparameter: fparam, cmd: extraAction);
    }

    public void RequestParameter(DeviceParameterType primaryParameter)
    {
      RequestParameter(primaryParameter, null);
    }

    public void RequestParameter(DeviceParameterType primaryParameter, Enum subParameter)
    {
      if (primaryParameter != DeviceParameterType.BeadConcentration)
      {
        var subparReport = subParameter == null ? "" : subParameter.ToString();
        _logger.Log($"{DateTime.Now.ToString()} GET {primaryParameter.ToString()} {subparReport}");
      }
      ushort selector = 0;
      byte commandCode = 0;
      switch (primaryParameter)
      {
        case DeviceParameterType.BoardVersion:
          commandCode = 0x01;
          break;
        case DeviceParameterType.SiPMTempCoeff:
          commandCode = 0x02;
          break;
        case DeviceParameterType.IdexPosition:
          commandCode = 0x04;
          break;
        case DeviceParameterType.SampleSyringeSize:
          commandCode = 0x05;
          break;
        case DeviceParameterType.TotalBeadsInFirmware:
          commandCode = 0x06;
          break;
        case DeviceParameterType.CalibrationMargin:
          commandCode = 0x08;
          break;
        case DeviceParameterType.PressureAtStartup:
          commandCode = 0x0C;
          break;
        case DeviceParameterType.ValveCuvetDrain:
          commandCode = 0x10;
          break;
        case DeviceParameterType.ValveFan1:
          commandCode = 0x11;
          break;
        case DeviceParameterType.ValveFan2:
          commandCode = 0x12;
          break;
        case DeviceParameterType.SyringePosition: //uses selector
          commandCode = 0x14;
          switch (subParameter)
          {
            case SyringePosition.Sheath:
              selector = 0;
              break;
            case SyringePosition.SampleA:
              selector = 1;
              break;
            case SyringePosition.SampleB:
              selector = 2;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.PollStepActivity:
          commandCode = 0x16;
          break;
        case DeviceParameterType.IsInputSelectorAtPickup:
          commandCode = 0x18;
          break;
        case DeviceParameterType.IsBubbleDetectionActive:
          commandCode = 0x19;
          break;
        case DeviceParameterType.BeadConcentration:
          commandCode = 0x1D;
          break;
        case DeviceParameterType.HiSensitivityChannel:
          commandCode = 0x1E;
          break;
        case DeviceParameterType.CalibrationTarget:
          switch (subParameter)
          {
            case CalibrationTarget.CL0:
              commandCode = 0x8B;
              break;
            case CalibrationTarget.CL1:
              commandCode = 0x8C;
              break;
            case CalibrationTarget.CL2:
              commandCode = 0x8D;
              break;
            case CalibrationTarget.CL3:
              commandCode = 0x8E;
              break;
            case CalibrationTarget.RP1:
              commandCode = 0x8F;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.CalibrationParameter:
          switch (subParameter)
          {
            case CalibrationParameter.Height:
              commandCode = 0xCD;
              break;
            case CalibrationParameter.MinSSC:
              commandCode = 0xCE;
              break;
            case CalibrationParameter.MaxSSC:
              commandCode = 0xCF;
              break;
            case CalibrationParameter.Attenuation:
              commandCode = 0xBF;
              break;
            case CalibrationParameter.DNRCoefficient:
              commandCode = 0x20;
              break;
            case CalibrationParameter.DNRTransition:
              commandCode = 0x0A;
              break;
            case CalibrationParameter.ScatterGate:
              commandCode = 0xCA;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.Pressure:
          commandCode = 0x22;
          break;
        case DeviceParameterType.ChannelBias30C:
          switch (subParameter)
          {
            case Channel.GreenA:
              commandCode = 0x28;
              break;
            case Channel.GreenB:
              commandCode = 0x29;
              break;
            case Channel.GreenC:
              commandCode = 0x2A;
              break;
            case Channel.RedA:
              commandCode = 0x2C;
              break;
            case Channel.RedB:
              commandCode = 0x2D;
              break;
            case Channel.RedC:
              commandCode = 0x2E;
              break;
            case Channel.RedD:
              commandCode = 0x2F;
              break;
            case Channel.VioletA:
              commandCode = 0x25;
              break;
            case Channel.VioletB:
              commandCode = 0x26;
              break;
            case Channel.ForwardScatter:
              commandCode = 0x24;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.SampleSyringeType:
          commandCode = 0x3E;
          break;
        case DeviceParameterType.ChannelCompensationBias:
          switch (subParameter)
          {
            case Channel.GreenA:
              commandCode = 0xA6;
              break;
            case Channel.GreenB:
              commandCode = 0x9A;
              break;
            case Channel.GreenC:
              commandCode = 0x9B;
              break;
            case Channel.RedA:
              commandCode = 0x99;
              break;
            case Channel.RedB:
              commandCode = 0x98;
              break;
            case Channel.RedC:
              commandCode = 0xA7;
              break;
            case Channel.RedD:
              commandCode = 0x96;
              break;
            case Channel.VioletA:
              commandCode = 0x95;
              break;
            case Channel.VioletB:
              commandCode = 0x94;
              break;
            case Channel.ForwardScatter:
              commandCode = 0x93;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.ChannelTemperature:
          switch (subParameter)
          {
            case Channel.GreenA:
              commandCode = 0xB0;
              break;
            case Channel.GreenB:
              commandCode = 0xB1;
              break;
            case Channel.GreenC:
              commandCode = 0xB2;
              break;
            case Channel.RedA:
              commandCode = 0xB3;
              break;
            case Channel.RedB:
              commandCode = 0xB4;
              break;
            case Channel.RedC:
              commandCode = 0xB5;
              break;
            case Channel.RedD:
              commandCode = 0xB6;
              break;
            case Channel.VioletA:
              commandCode = 0x80;
              break;
            case Channel.VioletB:
              commandCode = 0x81;
              break;
            case Channel.ForwardScatter:
              commandCode = 0x84;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.ChannelOffset:
          switch (subParameter)
          {
            case Channel.GreenA:
              commandCode = 0xA0;
              break;
            case Channel.GreenB:
              commandCode = 0xA4;
              break;
            case Channel.GreenC:
              commandCode = 0xA5;
              break;
            case Channel.RedA:
              commandCode = 0xA3;
              break;
            case Channel.RedB:
              commandCode = 0xA2;
              break;
            case Channel.RedC:
              commandCode = 0xA1;
              break;
            case Channel.RedD:
              commandCode = 0x9F;
              break;
            case Channel.VioletA:
              commandCode = 0x9D;
              break;
            case Channel.VioletB:
              commandCode = 0x9C;
              break;
            case Channel.ForwardScatter:
              commandCode = 0x9E;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.SyringeSpeedSheath:
          switch (subParameter)
          {
            case SyringeSpeed.Normal:
              commandCode = 0x30;
              break;
            case SyringeSpeed.HiSpeed:
              commandCode = 0x31;
              break;
            case SyringeSpeed.HiSensitivity:
              commandCode = 0x32;
              break;
            case SyringeSpeed.Flush:
              commandCode = 0x33;
              break;
            case SyringeSpeed.Pickup:
              commandCode = 0x34;
              break;
            case SyringeSpeed.MaxSpeed:
              commandCode = 0x35;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.SyringeSpeedSample:
          switch (subParameter)
          {
            case SyringeSpeed.Normal:
              commandCode = 0x38;
              break;
            case SyringeSpeed.HiSpeed:
              commandCode = 0x39;
              break;
            case SyringeSpeed.HiSensitivity:
              commandCode = 0x3A;
              break;
            case SyringeSpeed.Flush:
              commandCode = 0x3B;
              break;
            case SyringeSpeed.Pickup:
              commandCode = 0x3C;
              break;
            case SyringeSpeed.MaxSpeed:
              commandCode = 0x3D;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.MotorX:
          switch (subParameter)
          {
            case MotorParameterType.Slope:
              commandCode = 0x53;
              break;
            case MotorParameterType.StartSpeed:
              commandCode = 0x51;
              break;
            case MotorParameterType.RunSpeed:
              commandCode = 0x52;
              break;
            case MotorParameterType.CurrentStep:
              commandCode = 0x54;
              break;
            case MotorParameterType.CurrentLimit:
              commandCode = 0x90;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.MotorY:
          switch (subParameter)
          {
            case MotorParameterType.Slope:
              commandCode = 0x63;
              break;
            case MotorParameterType.StartSpeed:
              commandCode = 0x61;
              break;
            case MotorParameterType.RunSpeed:
              commandCode = 0x62;
              break;
            case MotorParameterType.CurrentStep:
              commandCode = 0x64;
              break;
            case MotorParameterType.CurrentLimit:
              commandCode = 0x91;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.MotorZ:
          switch (subParameter)
          {
            case MotorParameterType.Slope:
              commandCode = 0x43;
              break;
            case MotorParameterType.StartSpeed:
              commandCode = 0x41;
              break;
            case MotorParameterType.RunSpeed:
              commandCode = 0x42;
              break;
            case MotorParameterType.CurrentStep:
              commandCode = 0x44;
              break;
            case MotorParameterType.CurrentLimit:
              commandCode = 0x92;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.MotorStepsX:
          switch (subParameter)
          {
            case MotorStepsX.Plate96Column1:
              commandCode = 0x58;
              break;
            case MotorStepsX.Plate96Column12:
              commandCode = 0x5A;
              break;
            case MotorStepsX.Plate384Column1:
              commandCode = 0x5C;
              break;
            case MotorStepsX.Plate384Column24:
              commandCode = 0x5E;
              break;
            case MotorStepsX.Tube:
              commandCode = 0x56;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.MotorStepsY:
          switch (subParameter)
          {
            case MotorStepsY.Plate96RowA:
              commandCode = 0x68;
              break;
            case MotorStepsY.Plate96RowH:
              commandCode = 0x6A;
              break;
            case MotorStepsY.Plate384RowA:
              commandCode = 0x6C;
              break;
            case MotorStepsY.Plate384RowP:
              commandCode = 0x6E;
              break;
            case MotorStepsY.Tube:
              commandCode = 0x66;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.MotorStepsZ:
          switch (subParameter)
          {
            case MotorStepsZ.A1:
              commandCode = 0x48;
              break;
            case MotorStepsZ.A12:
              commandCode = 0x4A;
              break;
            case MotorStepsZ.H1:
              commandCode = 0x4C;
              break;
            case MotorStepsZ.H12:
              commandCode = 0x4E;
              break;
            case MotorStepsZ.Tube:
              commandCode = 0x46;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.WashRepeatsAmount:
          commandCode = 0x86;
          break;
        case DeviceParameterType.AgitateRepeatsAmount:
          commandCode = 0x7F;
          break;
        case DeviceParameterType.WellReadingOrder:
          commandCode = 0xA8;
          break;
        case DeviceParameterType.WellReadingSpeed:
          commandCode = 0xAA;
          break;
        case DeviceParameterType.PlateType:
          commandCode = 0xAB;
          break;
        case DeviceParameterType.WellRowIndex:
          commandCode = 0xAD;
          break;
        case DeviceParameterType.WellColumnIndex:
          commandCode = 0xAE;
          break;
        case DeviceParameterType.Volume:
          switch (subParameter)
          {
            case VolumeType.Wash:
              commandCode = 0xAC;
              break;
            case VolumeType.Sample:
              commandCode = 0xAF;
              break;
            case VolumeType.Agitate:
              commandCode = 0xC4;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.IsLaserActive:
          commandCode = 0xC0;
          break;
        case DeviceParameterType.ChannelConfiguration:
          commandCode = 0xC2;
          break;
        case DeviceParameterType.LaserPower:
          switch (subParameter)
          {
            case LaserType.Violet:
              commandCode = 0xC7;
              break;
            case LaserType.Green:
              commandCode = 0xC8;
              break;
            case LaserType.Red:
              commandCode = 0xC9;
              break;
            default:
              throw new NotImplementedException();
          }
          break;
        case DeviceParameterType.SystemActivityStatus:
          commandCode = 0xCC;
          break;
        default:
          throw new NotImplementedException();
      }
      MainCommand(code: commandCode, parameter: selector, cmd: 0x01);
    }

    /// <summary>
    /// Unsafe method for direct flash access<br></br>
    /// Use with caution<br></br>
    /// If used with GET - the response would be an event of type DeviceParameterType.DirectFlashValue
    /// </summary>
    /// <param name="getValue">True for "get" false for "set"</param>
    /// <param name="code">Command code as a hex byte</param>
    /// <param name="parameter">int value, if applicable</param>
    /// <param name="floatParameter">float value, if applicable</param>
    public void DirectFlashAccess(bool getValue, byte code, ushort parameter = 0, float floatParameter = 0.0f)
    {
      var getset = getValue ? 0x01 : 0x02;
      _logger.Log($"{DateTime.Now.ToString()} DIR [{code},{getset},{parameter},{floatParameter}]");
      if (getValue)
      {
        _latestDirectGetCode = code;
      }
      MainCommand(code: code, parameter: parameter, cmd: (byte)getset, fparameter: floatParameter);
    }

    /// <summary>
    /// If DirectFlashAccess Get is called, this method raises an event with response<br></br>
    /// If not - nothing happens
    /// </summary>
    internal void DirectFlashReturnValue(in CommandStruct cs)
    {
      if (cs.Code == _latestDirectGetCode)
      {
        _device.OnParameterUpdate(new ParameterUpdateEventArgs(DeviceParameterType.DirectFlashValue, intParameter: cs.Parameter, floatParameter: cs.FParameter));
        _latestDirectGetCode = 0;
      }
    }

    internal void SetToken(HardwareToken token, ushort value = 0)
    {
      _logger.Log($"{DateTime.Now.ToString()} TOKEN {token.ToString()} {value.ToString()}");
      byte commandCode = 0x00;
      switch (token)
      {
        case HardwareToken.Synchronization:
          commandCode = 0xCB;
          break;
        case HardwareToken.ActiveCommandQueueIndex:  //switch to recovery command buffer #parameter
          commandCode = 0xC1;
          break;
        case HardwareToken.EmptySyringeTrigger:
          commandCode = 0xC3;
          break;
      }
      MainCommand(code: commandCode, parameter: value, cmd: 0x02);
    }
    /// <summary>
    /// Blocking until finished movement<br></br>
    /// Moves plate to a designated well.
    /// </summary>
    /// <param name="well"></param>
    public void MovePlateToWell(Well well)
    {
      SetParameter(DeviceParameterType.WellRowIndex, well.RowIdx);
      SetParameter(DeviceParameterType.WellColumnIndex, well.ColIdx);
      SendCommand(DeviceCommandType.PositionWellPlate);
      lock (_dataController.ScriptF9FinishedLock)
      {
        Monitor.Wait(_dataController.ScriptF9FinishedLock);
      }
    }

    /// <summary>
    /// Blocking until finished movement<br></br>
    /// </summary>
    /// <param name="height"></param>
    public void AscendProbe(ushort height)
    {
      MoveProbeZ(height, MotorDirection.Up);
    }

    /// <summary>
    /// Blocking until finished movement<br></br>
    /// </summary>
    /// <param name="height"></param>
    public void DescendProbe(ushort height)
    {
      MoveProbeZ(height, MotorDirection.Down);
    }

    private void MoveProbeZ(ushort height, MotorDirection direction)
    {
      if (direction != MotorDirection.Up && direction != MotorDirection.Down)
      {
        throw new ArgumentException("MotorDirection can only be Up or Down");
      }

      SetParameter(DeviceParameterType.MotorMoveZ, direction, height);
      lock (_dataController.SystemActivityNotBusyNotificationLock)
      {
        Monitor.Wait(_dataController.SystemActivityNotBusyNotificationLock);
      }
    }

    private void MainCommand(byte cmd = 0, byte code = 0, ushort parameter = 0, float fparameter = 0)
    {
      CommandStruct cs = new CommandStruct
      {
        Code = code,
        Command = cmd,
        Parameter = parameter,
        FParameter = fparameter
      };
      _dataController.AddCommand(cs);
    }
  }
}
