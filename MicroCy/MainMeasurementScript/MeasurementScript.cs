using DIOS.Core.HardwareIntercom;

namespace DIOS.Core.MainMeasurementScript
{
  internal class MeasurementScript
  {
    public readonly StateMachine StateMach;
    private readonly Device _device;
    private readonly HardwareInterface _hardware;
    private WellController _wellController;

    public MeasurementScript(Device device)
    {
      _device = device;
      _hardware = device.Hardware;
      _wellController = device.WellController;
      StateMach = new StateMachine(_device, this);
    }

    public void Start()
    {
      _hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.MinSSC, _device._beadProcessor._map.calParams.minmapssc);
      _hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.MaxSSC, _device._beadProcessor._map.calParams.maxmapssc);
      //read section of plate
      _hardware.RequestParameter(DeviceParameterType.MotorStepsX, MotorStepsX.Plate96Column1);
      _hardware.RequestParameter(DeviceParameterType.MotorStepsY, MotorStepsY.Plate96RowA);
      SetReadingParamsForWell();
      SetAspirateParamsForWell();  //setup for first read
      _device.OnStartingToReadWell();
      _hardware.SetParameter(DeviceParameterType.IsBubbleDetectionActive, 1);
      _hardware.SendCommand(DeviceCommandType.PositionWellPlate);

      if (!_device._singleSyringeMode)
        _hardware.SendCommand(DeviceCommandType.AspirateA);

      _device.TotalBeads = 0;

      _device._isReadingA = false;
      StartBeadRead();

      if (_device.TerminationType != Termination.TotalBeadsCaptured) //set some limit for running to eos or if regions are wrong
        _device.BeadsToCapture = 100000;
    }

    public void SetupRead()
    {
      SetReadingParamsForWell();
      if (_device._singleSyringeMode)
      {
        SetAspirateParamsForWell();
        _hardware.SendCommand(DeviceCommandType.PositionWellPlate);
      }

      _device.OnStartingToReadWell();
      StartBeadRead();
    }

    public bool EndBeadRead()
    {
      _device.Hardware.SendCommand(DeviceCommandType.FlushCommandQueue);
      _device.Hardware.SetToken(HardwareToken.EmptySyringeTrigger); //clear empty syringe token
      _device.Hardware.SetToken(HardwareToken.Synchronization); //clear sync token to allow next sequence to execute

      if (_device._isReadingA || _device._singleSyringeMode)
        _hardware.SendCommand(DeviceCommandType.EndReadA);
      else
        _hardware.SendCommand(DeviceCommandType.EndReadB);
      return _wellController.IsLastWell;
    }

    private void SetReadingParamsForWell()
    {
      _hardware.SetParameter(DeviceParameterType.WellReadingSpeed, (ushort)_wellController.NextWell.RunSpeed);
      _hardware.SetParameter(DeviceParameterType.ChannelConfiguration, (ushort)_wellController.NextWell.ChanConfig);
      _device.BeadsToCapture = _wellController.NextWell.BeadsToCapture;
      _device.MinPerRegion = _wellController.NextWell.MinPerRegion;
      _device.TerminationType = _wellController.NextWell.TermType;
    }

    private void SetAspirateParamsForWell()
    {
      _hardware.SetParameter(DeviceParameterType.WellRowIndex, _wellController.NextWell.RowIdx);
      _hardware.SetParameter(DeviceParameterType.WellColumnIndex, _wellController.NextWell.ColIdx);
      _hardware.SetParameter(DeviceParameterType.Volume, VolumeType.Sample, (ushort)_wellController.NextWell.SampVol);
      _hardware.SetParameter(DeviceParameterType.Volume, VolumeType.Wash, (ushort)_wellController.NextWell.WashVol);
      _hardware.SetParameter(DeviceParameterType.Volume, VolumeType.Agitate, (ushort)_wellController.NextWell.AgitateVol);
    }

    private void StartBeadRead()
    {
      if (_device._singleSyringeMode)
      {
        _hardware.SendCommand(DeviceCommandType.AspirateA);
        _hardware.SendCommand(DeviceCommandType.ReadA);
        return;
      }

      if (_wellController.IsLastWell)
      {
        if (_device._isReadingA)
          _hardware.SendCommand(DeviceCommandType.ReadB);
        else
          _hardware.SendCommand(DeviceCommandType.ReadA);
        return;
      }

      SetAspirateParamsForWell();
      if (_device._isReadingA)
        _hardware.SendCommand(DeviceCommandType.ReadBAspirateA);
      else
        _hardware.SendCommand(DeviceCommandType.ReadAAspirateB);
    }
  }
}