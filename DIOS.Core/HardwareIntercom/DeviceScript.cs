namespace DIOS.Core.HardwareIntercom;

public enum DeviceScript
{
  Startup,
  Prime,
  Rinse,
  WashA,
  WashB,
  EjectPlate,
  LoadPlate,
  ///<summary>Move motors. Target position is set with properties WellRowIndex, WellColumnIndex</summary>
  PositionWellPlate,
  AspirateA,
  AspirateB,
  ReadA,
  ReadB,
  ReadAAspirateB,
  ReadBAspirateA,
  EndReadA,
  EndReadB
}