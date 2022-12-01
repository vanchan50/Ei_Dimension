using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DIOS.Core.InstrumentParameters;
using DIOS.Core.SelfTests;

/*
 * Most commands on the host side parallel the Properties and Methods document fo QB-1000
 * The only complex action is reading a region of wells defined by the READ SECTION parameters and button
 * They define an rectangular area of the plate that may include the entire plate. The complexity is
 * furthered by the fact that a read can be terminated in many different ways:
 * 1. Manually with the END SECTION READ button
 * 2. Manually with the END READ button which just ends the current well read and goes on the the next well
 * 3. OUT OF SHEATH condition, the sheath syringe is a position 0
 * 4. OUT OF SAMPLE condition, the sample syringe (either A or B) is at position 0
 * 5. Required number of beads read
 * 6. Required number of beads read in each region.
 * 7. Instrument fault (bubbles, plunger overload, clog, low laser power, etc)
 * 
 * When the instrument detects one of these end conditions: QB_cmd_proc.c / SyringeEmpty()
 * 1. The command Queue is cleared (the queue is only holding instructions for the currently read well)
 * 2. The sync token is cleared allowing new commands to execute immediately
 * 3. An FD or FE is sent to the host to tell it to save the data file and then it sends an EE or EF
 * 4. An EE or EF sequence is executed in the instrument, flushing the remaining sample and resetting syringes
 * 5. If regioncount is 1-- == 0 the last sample is read, if > 0 the next well is also aspirated
 * 
 * When the host initiates an end condition (isDone== true)
 * This version is being used at MRBM
 */

namespace DIOS.Core
{
  public class Device
  {
    public ResultsPublisher Publisher { get; }
    public RunResults Results { get; }
    public WorkOrder WorkOrder { get; set; }
    public ConcurrentQueue<ProcessedBead> DataOut { get; } = new ConcurrentQueue<ProcessedBead>();
    public WellController WellController { get; } = new WellController();
    public BitArray SystemActivity { get; } = new BitArray(16, false);
    public object SystemActivityNotBusyNotificationLock { get; } = new object();
    public object ScriptF9FinishedLock { get; } = new object();
    public event EventHandler<ReadingWellEventArgs> StartingToReadWell;
    public event EventHandler<ReadingWellEventArgs> FinishedReadingWell;
    public event EventHandler FinishedMeasurement;
    public event EventHandler<StatsEventArgs> NewStatsAvailable;
    public event EventHandler<ParameterUpdateEventArgs> ParameterUpdate;
    public OperationMode Mode { get; set; } = OperationMode.Normal;
    public SystemControl Control { get; set; } = SystemControl.Manual;
    public Gate ScatterGate
    {
      get
      {
        return _scatterGate;
      }
      set
      {
        _scatterGate = value;
        SetHardwareParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.ScatterGate, (ushort)_scatterGate);
      }
    }

    public Termination TerminationType { get; set; } = Termination.MinPerRegion;
    public int BoardVersion { get; internal set; }
    public string FirmwareVersion { get; internal set; }
    public float ReporterScaling { get; set; } = 1;
    public int BeadsToCapture { get; set; }
    public int BeadCount { get; internal set; }
    public int TotalBeads { get; internal set; }
    public int MinPerRegion { get; set; }
    public bool IsMeasurementGoing { get; private set; }
    public bool SaveIndividualBeadEvents { get; set; }
    public bool RMeans { get; set; }
    public bool OnlyClassifiedInBeadEventFile { get; set; }
    public HiSensitivityChannel SensitivityChannel
    {
      get
      {
        return _sensitivityChannel;
      }
      set
      {
        _sensitivityChannel = value;
        MainCommand("Set Property", code: 0x1E, parameter: (ushort)_sensitivityChannel);
      }
    }
    public float HdnrTrans
    {
      get
      {
        return _hdnrTrans;
      }
      set
      {
        _hdnrTrans = value;
        SetHardwareParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.DNRTransition, _hdnrTrans);
      }
    }
    public float HDnrCoef
    {
      get
      {
        return _hdnrCoef;
      }
      set
      {
        _hdnrCoef = value;
        SetHardwareParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.DNRCoefficient, _hdnrCoef);
      }
    }
    public float Compensation { get; set; }
    public NormalizationSettings Normalization
    {
      get { return _beadProcessor.Normalization; }
    }
    public float MaxPressure { get; set; }
    public bool IsPlateEjected { get; internal set; }
    public static bool IncludeReg0InPlateSummary { get; set; }  //TODO: crutch for filesaving

    private bool _isReadingA;
    private Gate _scatterGate;
    private HiSensitivityChannel _sensitivityChannel;
    private float _hdnrTrans;
    private float _hdnrCoef;
    private readonly DataController _dataController;
    private readonly StateMachine _stateMach;
    internal SelfTester SelfTester { get; }
    internal readonly BeadProcessor _beadProcessor;

    public Device(ISerial connection, string folderPath)
    {
      SelfTester = new SelfTester(this);
      Results = new RunResults(this);
      _beadProcessor = new BeadProcessor(this);
      _dataController = new DataController(this, Results, connection);
      _stateMach = new StateMachine(this, true);
      Publisher = new ResultsPublisher(this, folderPath);
    }

    public void Init()
    {
      if (_dataController.Run())
      {
        MainCommand("Sync");
        RequestHardwareParameter(DeviceParameterType.BoardVersion);
      }
    }

    public void UpdateStateMachine()
    {
      _stateMach.Action();
    }

    public void PrematureStop()
    {
      WellController.PreparePrematureStop();
      _stateMach.Start();
    }

    /// <summary>
    /// Starts a sequence of commands to finalize well measurement. The sequence is in a form of state machine that takes several timer ticks
    /// </summary>
    public void StartStateMachine()
    {
      _stateMach.Start();
    }

    public void StartSelfTest()
    {
      SelfTester.FluidicsTest();
    }

    /// <summary>
    /// Polls the device once per 100 ms until the test result is ready
    /// </summary>
    /// <returns></returns>
    public async Task<SelfTestData> GetSelfTestResultAsync()
    {
      var t = new Task<SelfTestData>(() =>
      {
        SelfTestData data;
        while (!SelfTester.GetResult(out data))
        {
          System.Threading.Thread.Sleep(100);
        }
        #if DEBUG
        Console.WriteLine("\nSelfTest Finished\n\n");
        #endif
        return data;
      });
      t.Start();
      return await t;
    }

    internal void SetupRead()
    {
      SetReadingParamsForWell();
      WellController.Advance();
      Results.StartNewWell(WellController.CurrentWell);
      Publisher.StartNewBeadEventReport();
      BeadCount = 0;

      OnStartingToReadWell();
      StartBeadRead();
    }

    public void StartOperation()
    {
      if (Mode != OperationMode.Normal)
      {
        Normalization.SuspendForTheRun();
      }
      SetHardwareParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.MinSSC, _beadProcessor._map.calParams.minmapssc);
      SetHardwareParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.MaxSSC, _beadProcessor._map.calParams.maxmapssc);
      //read section of plate
      RequestHardwareParameter(DeviceParameterType.MotorStepsX, MotorStepsX.Plate96C1);
      RequestHardwareParameter(DeviceParameterType.MotorStepsY, MotorStepsY.Plate96RowA);
      Results.StartNewPlateReport();
      Publisher.ResetResultData();
      SetAspirateParamsForWell();  //setup for first read
      SetReadingParamsForWell();
      WellController.Advance();
      Results.StartNewWell(WellController.CurrentWell);
      Publisher.StartNewBeadEventReport();
      BeadCount = 0;
      OnStartingToReadWell();
      MainCommand("Set Property", code: 0x19, parameter: 1); //bubble detect on
      MainCommand("Position Well Plate"); //move motors. next position is set in properties 0xad and 0xae
      MainCommand("Aspirate Syringe A"); //handles down and pickup sample
      TotalBeads = 0;

      _isReadingA = false;
      StartBeadRead();

      if (TerminationType != Termination.TotalBeadsCaptured) //set some limit for running to eos or if regions are wrong
        BeadsToCapture = 100000;
    }


    public void EjectPlate()
    {
      MainCommand("Eject Plate");
      IsPlateEjected = true;
    }

    public void LoadPlate()
    {
      MainCommand("Load Plate");
      IsPlateEjected = false;
    }

    private void SetReadingParamsForWell()
    {
      MainCommand("Set Property", code: 0xaa, parameter: (ushort)WellController.NextWell.RunSpeed);
      MainCommand("Set Property", code: 0xc2, parameter: (ushort)WellController.NextWell.ChanConfig);
      BeadsToCapture = WellController.NextWell.BeadsToCapture;
      MinPerRegion = WellController.NextWell.MinPerRegion;
      TerminationType = WellController.NextWell.TermType;
    }

    private void SetAspirateParamsForWell()
    {
      MainCommand("Set Property", code: 0xad, parameter: (ushort)WellController.NextWell.RowIdx);
      MainCommand("Set Property", code: 0xae, parameter: (ushort)WellController.NextWell.ColIdx);
      SetHardwareParameter(DeviceParameterType.Volume, VolumeType.Sample,  (ushort)WellController.NextWell.SampVol);
      SetHardwareParameter(DeviceParameterType.Volume, VolumeType.Wash,    (ushort)WellController.NextWell.WashVol);
      SetHardwareParameter(DeviceParameterType.Volume, VolumeType.Agitate, (ushort)WellController.NextWell.AgitateVol);
    }

    internal bool EndBeadRead()
    {
      if (_isReadingA)
        MainCommand("End Bead Read A");
      else
        MainCommand("End Bead Read B");
      return WellController.IsLastWell;
    }

    private void StartBeadRead()
    {
      if (WellController.IsLastWell)
      {
        if (_isReadingA)
          MainCommand("Read B");
        else
          MainCommand("Read A");
      }
      else
      {
        SetAspirateParamsForWell();
        if (_isReadingA)
          MainCommand("Read B Aspirate A");
        else
          MainCommand("Read A Aspirate B");
      }
    }

    public void MainCommand(string command, byte? cmd = null, byte? code = null, ushort? parameter = null, float? fparameter = null)
    {
      CommandStruct cs = CommandLists.MainCmdTemplatesDict[command];
      cs.Command = cmd ?? cs.Command;
      cs.Code = code ?? cs.Code;
      cs.Parameter = parameter ?? cs.Parameter;
      cs.FParameter = fparameter ?? cs.FParameter;
      switch (command)
      {
        case "Read A":
          _isReadingA = true;
          break;
        case "Read A Aspirate B":
          _isReadingA = true;
          break;
        case "Read B":
          _isReadingA = false;
          break;
        case "Read B Aspirate A":
          _isReadingA = false;
          break;
        case "FlushCmdQueue":
          cs.Command = 0x02;
          break;
        case "Idex":
          cs.Command = Idex.Pos;
          cs.Parameter = Idex.Steps;
          cs.FParameter = Idex.Dir;
          break;
      }
      _dataController.AddCommand(command, cs);
    }

    public void SetHardwareParameter(DeviceParameterType primaryParameter, float value = 0f)
    {
      SetHardwareParameter(primaryParameter, null, value);
    }

    public void SetHardwareParameter(DeviceParameterType primaryParameter, Enum subParameter, float value = 0f )
    {
      ushort param = 0;
      float fparam = 0;
      byte commandCode = 0;
      var intValue = (ushort)Math.Round(value);
      switch (primaryParameter)
      {
        case DeviceParameterType.SiPMTempCoeff:
          commandCode = 0x02;
          fparam = value;
          break;
        case DeviceParameterType.IdexPosition:  //TODO: change to 0xd7 for set. get rid of Idex class?
          commandCode = 0x04;
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
        case DeviceParameterType.BeadConcentration:
          commandCode = 0x1D;
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
            case MotorStepsX.Plate96C1:
              commandCode = 0x58;
              break;
            case MotorStepsX.Plate96C12:
              commandCode = 0x5A;
              break;
            case MotorStepsX.Plate384C1:
              commandCode = 0x5C;
              break;
            case MotorStepsX.Plate384C24:
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
          break;
        case DeviceParameterType.ChannelConfiguration:
          commandCode = 0xC2;
          param = intValue;
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
          param = intValue;
          break;
        default:
          throw new NotImplementedException();
      }

      MainCommand("Set Property", code: commandCode, parameter: param, fparameter: fparam);
    }

    public void RequestHardwareParameter(DeviceParameterType primaryParameter)
    {
      RequestHardwareParameter(primaryParameter, null);
    }

    public void RequestHardwareParameter(DeviceParameterType primaryParameter, Enum subParameter)
    {
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
        case DeviceParameterType.BeadConcentration:
          commandCode = 0x1D;
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
            case MotorStepsX.Plate96C1:
              commandCode = 0x58;
              break;
            case MotorStepsX.Plate96C12:
              commandCode = 0x5A;
              break;
            case MotorStepsX.Plate384C1:
              commandCode = 0x5C;
              break;
            case MotorStepsX.Plate384C24:
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
      MainCommand("Get Property", code: commandCode, parameter: selector, cmd: 0x01);
    }

    private void OnStartingToReadWell()
    {
      IsMeasurementGoing = true;
      StartingToReadWell?.Invoke(this, new ReadingWellEventArgs(WellController.CurrentWell.RowIdx, WellController.CurrentWell.ColIdx,
        Publisher.FullBeadEventFileName));
      SetHardwareParameter(DeviceParameterType.TotalBeadsInFirmware); //reset totalbeads in firmware
      Console.WriteLine("Starting to read well with Params:");
      Console.WriteLine($"Termination: {(int)TerminationType}");
      Console.WriteLine($"TotalBeads: {TotalBeads}");
      Console.WriteLine($"BeadCount: {BeadCount}");
      Console.WriteLine($"BeadsToCapture: {BeadsToCapture}");
    }

    internal void OnFinishedReadingWell()
    {
      MainCommand("End Sampling");    //sends message to instrument to stop sampling
      FinishedReadingWell?.Invoke(this, new ReadingWellEventArgs(WellController.CurrentWell.RowIdx, WellController.CurrentWell.ColIdx,
        Publisher.FullBeadEventFileName));
      RequestHardwareParameter(DeviceParameterType.TotalBeadsInFirmware);
    }

    internal void OnFinishedMeasurement()
    {
      IsMeasurementGoing = false;
      BeadCount = 0;
      Results.PlateReport.completedDateTime = DateTime.Now;
      _ = Task.Run(() => { Publisher.OutputPlateReport(); });
      Results.EndOfOperationReset();
      MainCommand("Set Property", code: 0x19);  //bubble detect off
      if (Mode ==  OperationMode.Verification)
        Verificator.CalculateResults(_beadProcessor._map);

      if (Mode != OperationMode.Normal)
        Normalization.Restore();

      FinishedMeasurement?.Invoke(this, EventArgs.Empty);
    }

    internal void OnNewStatsAvailable()
    {
      var stats = Results.WellResults.GetStats();
      var averageBackgrounds = Results.WellResults.GetBackgroundAverages();
      NewStatsAvailable?.Invoke(this, new StatsEventArgs(stats, averageBackgrounds));
    }

    internal void OnParameterUpdate(ParameterUpdateEventArgs param)
    {
      ParameterUpdate?.Invoke(this, param);
    }

    public void SetMap(CustomMap map)
    {
      _beadProcessor.SetMap(map);
    }

    public void ReconnectUSB()
    {
      _dataController.ReconnectUSB();
      SetHardwareParameter(DeviceParameterType.SystemActivityStatus);
      MainCommand("Set Property", code: 0xCB);
    }

    public void DisconnectedUSB()
    {
      _dataController.DisconnectedUSB();
    }

#if DEBUG
    public void DEBUGSetBoardVersion(int v)
    {
      BoardVersion = v;
    }

    public void DEBUGJBeadADD()
    {
      var r = new Random();
      var kek = new RawBead
      {
        Header = 0xadbeadbe,
        fsc = 2.36f,
        redssc = r.Next(1000,20000),
        cl0 = r.Next(1050, 1300),
        cl1 = r.Next(1450, 1700),
        cl2 = r.Next(1500, 1650),
        greenB = (ushort)r.Next(9, 12),
        greenC = 48950
      };
      var pek = new RawBead
      {
        Header = 0xadbeadbe,
        fsc = 15.82f,
        redssc = r.Next(1000, 20000),
        cl0 = 250f,
        cl1 = 500f,
        cl2 = 500f,
        greenB = (ushort)r.Next(80,150),
        greenC = 65212
      };
      var rek0 = new RawBead
      {
        Header = 0xadbeadbe,
        fsc = 2.36f,
        redssc = r.Next(1000, 20000),
        cl0 = r.Next(1050, 1300),
        cl1 = 35000,
        cl2 = 200,
        greenB = (ushort)r.Next(9, 12),
        greenC = 48950
      };
      var choose = r.Next(0, 3);
      switch (choose)
      {
        case 0:
          Results.AddRawBeadEvent(in kek);
          break;
        case 1:
          Results.AddRawBeadEvent(in pek);
          break;
        case 2:
          Results.AddRawBeadEvent(in rek0);
          break;
      }
      Results.TerminationReadyCheck();
    }

    public void DEBUGCommandTest(CommandStruct cs)
    {
      _dataController.DEBUGGetCommandFromBuffer(cs);
    }
    #endif
  }
}