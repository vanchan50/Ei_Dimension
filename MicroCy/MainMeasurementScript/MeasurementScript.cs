using DIOS.Core.HardwareIntercom;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace DIOS.Core.MainMeasurementScript
{
  internal class MeasurementScript
  {
    private readonly Device _device;
    private readonly HardwareInterface _hardware;
    private WellController _wellController;
    private int _finalizerStarted;
    private ILogger _logger;

    public MeasurementScript(Device device, ILogger logger)
    {
      _device = device;
      _hardware = device.Hardware;
      _wellController = device.WellController;
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

      _device._isReadingA = false;
      StartBeadRead();

      if (_device.TerminationType != Termination.TotalBeadsCaptured) //set some limit for running to eos or if regions are wrong
        _device.TotalBeadsToCapture = 100000;
    }

    public void FinalizeWellReading()
    {
      if (Interlocked.CompareExchange(ref _finalizerStarted, 1, 0) == 1)
        return;

      Task.Run(() =>
      {
        _device.OnFinishedReadingWell();
        Thread.Sleep(500);  //not necessary?
        while (WashingIsOngoing())
        {
          Thread.Sleep(500);
        }
        Action3();

        Task.Run(() =>
        {
          GC.Collect();
          GC.WaitForPendingFinalizers();
        });

        _finalizerStarted = 0;
      });
    }

    private bool WashingIsOngoing()
    {
      if (_device.SystemMonitor.ContainsWashing())  //does not contain Washing
      {
        _device.Hardware.RequestParameter(DeviceParameterType.SystemActivityStatus);
        return true;
      }
      return false;
    }

    private void Action3()
    {
      if (EndBeadRead())
        _device.OnFinishedMeasurement();
      else
      {
        SetupRead();
      }
    }

    private void SetupRead()
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

    private bool EndBeadRead()
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
      _device.TotalBeadsToCapture = _wellController.NextWell.BeadsToCapture;
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