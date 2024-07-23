namespace DIOS.Core.HardwareIntercom;

public enum DeviceCommandType
{
  UpdateFirmware,
  FlushCommandQueue,
  Synchronize,
  FlashFactoryReset,
  FlashRestore,
  FlashSave,
  RefreshDAC,
  StartSampling,
  EndSampling,
  CalibrationModeActivate,
  CalibrationModeDeactivate,
  SignalLED
}