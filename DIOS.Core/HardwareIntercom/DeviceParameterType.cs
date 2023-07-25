namespace DIOS.Core.HardwareIntercom;

public enum DeviceParameterType
{
  SiPMTempCoeff,
  IdexSteps,
  TotalBeadsInFirmware,
  CalibrationMargin,
  ValveCuvetDrain,
  ValveFan1,
  ValveFan2,
  SyringePosition,
  IsSyringePositionActive,
  PollStepActivity,
  IsInputSelectorAtPickup,
  CalibrationSuccess,
  Pressure,
  PressureAtStartup,
  ///<summary>Subparamter: Channel</summary>
  ChannelBias30C,
  ///<summary>Subparamter: SyringeSpeed</summary>
  SyringeSpeedSheath,
  ///<summary>Subparamter: SyringeSpeed</summary>
  SyringeSpeedSample,
  ///<summary>Subparamter: MotorParameterType</summary>
  MotorX,
  ///<summary>Subparamter: MotorParameterType</summary>
  MotorY,
  ///<summary>Subparamter: MotorParameterType</summary>
  MotorZ,
  ///<summary>Subparamter: MotorStepsX</summary>
  MotorStepsX,
  ///<summary>Subparamter: MotorStepsY</summary>
  MotorStepsY,
  ///<summary>Subparamter: MotorStepsZ</summary>
  MotorStepsZ,
  ///<summary>Subparamter: MotorStepsZ</summary>
  MotorStepsZTemporary,
  ///<summary>Subparamter: Channel</summary>
  ChannelTemperature,
  ///<summary>Subparamter: Channel</summary>
  ChannelCompensationBias,
  ///<summary>Subparamter: Channel</summary>
  ChannelOffset,
  ///<summary>Subparamter: VolumeType</summary>
  Volume,
  GreenAVoltage,
  IsLaserActive,
  LaserPower,
  SystemActivityStatus,
  ///<summary>Subparamter: SheathFlowErrorType</summary>
  SheathFlowError,
  SampleSyringeStatus,
  WellWarning,
  BubbleDetectorStatus,
  ///<summary>For internal use</summary>
  BoardVersion,
  BeadConcentration,
  WellReadingSpeed,
  WellReadingOrder,
  ///<summary>Subparamter: ChannelConfiguration</summary>
  ChannelConfiguration,
  PlateType,
  WashRepeatsAmount,
  ProbewashRepeatsAmount,
  AgitateRepeatsAmount,
  ///<summary>Subparamter: CalibrationTarget</summary>
  CalibrationTarget,
  ///<summary>Subparamter: CalibrationParameter</summary>
  CalibrationParameter,
  IsSingleStepDebugActive,
  IsBubbleDetectionActive,
  HiSensitivityChannel,
  ///<summary>Setup for PositionWellPlate command</summary>
  WellRowIndex,
  ///<summary>Setup for PositionWellPlate command</summary>
  WellColumnIndex,
  ///<summary>Subparamter: SampleSyringeType</summary>
  SampleSyringeType,
  ///<summary>Subparamter: SyringeControlState</summary>
  PumpSheath,
  ///<summary>Subparamter: SyringeControlState</summary>
  PumpSampleA,
  ///<summary>Subparamter: SyringeControlState</summary>
  PumpSampleB,
  ///<summary>Subparamter: MotorDirection</summary>
  MotorMoveX,
  ///<summary>Subparamter: MotorDirection</summary>
  MotorMoveY,
  ///<summary>Subparamter: MotorDirection</summary>
  MotorMoveZ,
  ///<summary>Subparamter: AlignMotorSequence</summary>
  AlignMotor,
  UVCSanitize,
  ///<summary>Subparamter: AutoAlignState</summary>
  AutoAlignState,
  SampleSyringeSize,
  ///<summary>unique parameter, used by DirectFlashAccess feature</summary>
  DirectFlashValue,
  ///<summary>Subparamter: Channel</summary>
  ExtendedRangeMultiplier,
  TraySteps,
  SheathFlushVolume,
  IsWellEdgeAgitateActive,
  DistanceToWellEdge,
  WellEdgeDeltaHeight,
  FlushCycles,
  ///<summary>Subparamter: FluidicPathLength</summary>
  FluidicPathLength,
  UseWashStation,
  WashStationXCenterCoordinate
}