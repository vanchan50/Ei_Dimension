namespace DIOS.Core.HardwareIntercom;

public enum DeviceCommandType
{
  UpdateFirmware,
  FlushCommandQueue,
  Synchronize,
  LoadPlate,
  EjectPlate,
  WashA,
  WashB,
  AspirateA,
  AspirateB,
  ReadA,
  ReadB,
  ReadAAspirateB,
  ReadBAspirateA,
  EndReadA,
  EndReadB,
  Prime,
  FlashFactoryReset,
  FlashRestore,
  FlashSave,
  ///<summary>Move motors. Target position is set with properties WellRowIndex, WellColumnIndex</summary>
  PositionWellPlate,
  RefreshDAC,
  SetBaseLine,
  StartSampling,
  EndSampling,
  Startup,
  CalibrationModeActivate,
  CalibrationModeDeactivate,
  RenewSheath
}