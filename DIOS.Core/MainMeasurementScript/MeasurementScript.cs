using DIOS.Core.HardwareIntercom;

namespace DIOS.Core.MainMeasurementScript;

internal class MeasurementScript
{
  private readonly Device _device;
  private readonly HardwareInterface _hardware;
  private WellController _wellController;
  private int _finalizerStarted;
  private ILogger _logger;
  private bool _isReadingA;

  public MeasurementScript(Device device, HardwareInterface hardware, WellController wellController, ILogger logger)
  {
    _device = device;
    _hardware = hardware;
    _wellController = wellController;
    _logger = logger;
  }

  public async Task Start()
  {
    _logger.Log("Starting read sequence");
    //read section of plate
    _hardware.RequestParameter(DeviceParameterType.MotorStepsX, MotorStepsX.Plate96Column1);
    _hardware.RequestParameter(DeviceParameterType.MotorStepsY, MotorStepsY.Plate96RowA);
    SetReadingParamsForWell();
    SetAspirateParamsForWell();  //setup for first read
    _hardware.SetParameter(DeviceParameterType.IsBubbleDetectionActive, 1);
    await _hardware.SendScriptAsync(DeviceScript.PositionWellPlate);

    if (!_device.SingleSyringeMode)
      await _hardware.SendScriptAsync(DeviceScript.AspirateA);

    _isReadingA = false;
    _device.OnStartingToReadWell();
    await StartBeadRead();
  }

  public void FinalizeWellReading()
  {
    if (Interlocked.CompareExchange(ref _finalizerStarted, 1, 0) == 1)
      return;

    Task.Run(async () =>
    {
      _device.OnFinishedReadingWell();

      await WashingIsNotPresent();

      await EndBeadRead();

      _finalizerStarted = 0;

      return _wellController.IsLastWell;
    })
      .ContinueWith(async (x) =>
      {
        if (x.Result)
        {
          _device.OnFinishedMeasurement();
        }
        else
        {
          await SetupRead();
        }
      });
  }

  private async Task WashingIsNotPresent()
  {
    await Task.Delay(500);  //not necessary?
    while (_device.SystemMonitor.ContainsWashing())
    {
      _hardware.RequestParameter(DeviceParameterType.SystemActivityStatus);
      await Task.Delay(500);
    }
    //does not contain Washing
  }

  private async Task SetupRead()
  {
    SetReadingParamsForWell();
    if (_device.SingleSyringeMode)
    {
      SetAspirateParamsForWell();
      await _hardware.SendScriptAsync(DeviceScript.PositionWellPlate);
    }

    _device.OnStartingToReadWell();
    await StartBeadRead();
  }

  private async Task EndBeadRead()
  {
    _hardware.SendCommand(DeviceCommandType.FlushCommandQueue);
    _hardware.SetToken(HardwareToken.EmptySyringeTrigger); //clear empty syringe token
    _hardware.SetToken(HardwareToken.Synchronization); //clear sync token to allow next sequence to execute

    if (_isReadingA || _device.SingleSyringeMode)
      await _hardware.SendScriptAsync(DeviceScript.EndReadA);
    else
      await _hardware.SendScriptAsync(DeviceScript.EndReadB);
  }

  private void SetReadingParamsForWell()
  {
    var nextWell = _wellController.NextWell;
    _logger.Log("Reading Setup\n{");
    _hardware.SetParameter(DeviceParameterType.WellReadingSpeed, (ushort)nextWell.RunSpeed);
    _logger.Log("}");
  }

  private void SetAspirateParamsForWell()
  {
    var nextWell = _wellController.NextWell;
    _logger.Log("Aspiration Setup\n{");
    _hardware.SetParameter(DeviceParameterType.WellRowIndex, nextWell.RowIdx);
    _hardware.SetParameter(DeviceParameterType.WellColumnIndex, nextWell.ColIdx);
    _hardware.SetParameter(DeviceParameterType.Volume, VolumeType.Sample, (ushort)nextWell.SampVol);
    _hardware.SetParameter(DeviceParameterType.Volume, VolumeType.Wash, (ushort)nextWell.WashVol);
    _hardware.SetParameter(DeviceParameterType.Volume, VolumeType.ProbeWash, (ushort)nextWell.ProbewashVol);
    _hardware.SetParameter(DeviceParameterType.Volume, VolumeType.Agitate, (ushort)nextWell.AgitateVol);
    _hardware.SetParameter(DeviceParameterType.WashRepeatsAmount, (ushort)nextWell.WashRepeats);
    _hardware.SetParameter(DeviceParameterType.ProbewashRepeatsAmount, (ushort)nextWell.ProbeWashRepeats);
    _hardware.SetParameter(DeviceParameterType.AgitateRepeatsAmount, (ushort)nextWell.AgitateRepeats);
    _logger.Log("}");
  }

  private async Task StartBeadRead()
  {
    if (_device.SingleSyringeMode)
    {
      await _hardware.SendScriptAsync(DeviceScript.AspirateA);
      _isReadingA = true;
      await _hardware.SendScriptAsync(DeviceScript.ReadA);
      return;
    }

    if (_wellController.IsLastWell)
    {
      if (_isReadingA)
      {
        _isReadingA = false;
        await _hardware.SendScriptAsync(DeviceScript.ReadB);
      }
      else
      {
        _isReadingA = true;
        await _hardware.SendScriptAsync(DeviceScript.ReadA);
      }
      return;
    }

    SetAspirateParamsForWell();
    if (_isReadingA)
    {
      _isReadingA = false;
      await _hardware.SendScriptAsync(DeviceScript.ReadBAspirateA);
    }
    else
    {
      _isReadingA = true;
      await _hardware.SendScriptAsync(DeviceScript.ReadAAspirateB);
    }
  }
}