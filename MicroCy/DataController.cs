using System;
using System.Collections.Concurrent;
using System.Threading;
using DIOS.Core.HardwareIntercom;

namespace DIOS.Core
{
  internal class DataController
  {
    public bool IsMeasurementGoing { get; set; }
    internal IBeadEventSink BeadEventSink { get; set; }
    private ConcurrentQueue<CommandStruct> _outCommands = new ConcurrentQueue<CommandStruct>();

    private readonly object _usbOutCV = new object();
    internal object ScriptF9FinishedLock { get; } = new object(); //TODO: hide into function or maybe system monitor has info. then just poll it
    internal object SystemActivityNotBusyNotificationLock { get; } = new object();
    private readonly ISerial _serialConnection;
    private readonly Thread _prioUsbInThread;
    private readonly Thread _prioUsbOutThread;
    private readonly Device _device;
    private readonly ILogger _logger;

    public DataController(Device device, ISerial connection, ILogger logger)
    {
      _device = device;
      _serialConnection = connection;
      _logger = logger;

      //setup threads
      _prioUsbInThread = new Thread(ReplyFromMC);
      _prioUsbInThread.Priority = ThreadPriority.Highest;
      _prioUsbInThread.IsBackground = true;
      _prioUsbInThread.Name = "USBIN";

      _prioUsbOutThread = new Thread(WriteToMC);
      _prioUsbOutThread.Priority = ThreadPriority.AboveNormal;
      _prioUsbOutThread.IsBackground = true;
      _prioUsbOutThread.Name = "USBOUT";
    }

    public bool Run()
    {
      _serialConnection.Start();
      _prioUsbInThread.Start();
      _prioUsbOutThread.Start();
      return _serialConnection.IsActive;
    }

    public void AddCommand(CommandStruct cs)
    {
      _outCommands.Enqueue(cs);
      #if DEBUG
      Console.Error.WriteLine($"[DEBUG] AddCommand Enqueued {cs.ToString()}");
      #endif
      NotifyCommandReceived();
    }

    public void ReconnectUSB()
    {
      _serialConnection.Reconnect();
    }

    public void DisconnectedUSB()
    {
      _serialConnection.Disconnect();
    }
    
    #if DEBUG
    internal void DEBUGGetCommandFromBuffer(CommandStruct cs)
    {
      InnerCommandProcessing(in cs);
    }
    #endif

    private void ReplyFromMC()
    {
      while (true)
      {
        _serialConnection.Read();

        if (!_serialConnection.IsBeadInBuffer())
        {
          GetCommandFromBuffer();
          continue;
        }

        if (IsMeasurementGoing) //  this condition avoids the necessity of cleaning up leftover data in the system USB interface. That could happen after operation abortion and program restart
        {
          for (byte i = 0; i < 8; i++)
          {
            //might be less than 8 beads in buffer
            if (!GetBeadFromBuffer(i, out var outbead))
              break;
            _device.BeadCount++;
            var processedBead = _device._beadProcessor.CalculateBeadParams(in outbead);
            BeadEventSink.Add(processedBead);
          }
        }
        _serialConnection.ClearBuffer();  //TODO: is it necessary?
      }
    }

    private void WriteToMC()
    {
      var timeOut = new TimeSpan(0, 0, seconds: 2);
      while (true)
      {
        while (_outCommands.TryDequeue(out var cmd))
        {
          RunCmd(cmd);
        }
        lock (_usbOutCV)
        {
          Monitor.Wait(_usbOutCV, timeOut);
        }
      }
    }

    private void NotifyCommandReceived()
    {
      lock (_usbOutCV)
      {
        Monitor.Pulse(_usbOutCV);
      }
    }

    /// <summary>
    /// Sends a command OUT to the USB device, then checks the IN pipe for a return value.
    /// </summary>
    /// <param name="sCmdName">A friendly name for the command.</param>
    /// <param name="cs">The CommandStruct object containing the command parameters.  This will get converted to an 8-byte array.</param>
    private void RunCmd(CommandStruct cs)
    {
      if (_serialConnection.IsActive)
        _serialConnection.Write(StructToByteArray(in cs));
    }

    private static byte[] StructToByteArray(in CommandStruct cs)
    {
      byte[] arrRet = new byte[8];
      unsafe
      {
        fixed (CommandStruct* pCS = &cs)
        {
          for (var i = 0; i < 8; i++)
          {
            arrRet[i] = *((byte*)pCS + i);
          }
        }
      }

      return arrRet;
    }

    private static CommandStruct ByteArrayToStruct(byte[] inmsg)
    {
      unsafe
      {
        fixed (byte* cs = &inmsg[0])
        {
          return *(CommandStruct*)cs;
        }
      }
    }

    private static RawBead BeadArrayToStruct(byte[] beadmsg, byte shift)
    {
      unsafe
      {
        fixed (byte* cs = &beadmsg[shift * 64])
        {
          return *(RawBead*)cs;
        }
      }
    }

    /// <summary>
    /// Represents 4bytes of a float as a 0.0.0.0 version number
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    private static string FloatToVersion(in float param)
    {
      string version = "";
      unsafe
      {
        fixed (float* str = &param)
        {
          for (var i = 0; i < 4; i++)
          {
            version += ((byte*)str + i)->ToString() + ".";
          }
          version = version.TrimEnd('.');
        }
      }
      return version;
    }
    
    private void GetCommandFromBuffer()
    {
      var newcmd = ByteArrayToStruct(_serialConnection.InputBuffer);
      InnerCommandProcessing(in newcmd);
    }

    private bool GetBeadFromBuffer(byte shift, out RawBead outbead)
    {
      outbead = BeadArrayToStruct(_serialConnection.InputBuffer, shift);
      return outbead.Header == 0xadbeadbe;
    }

    private void InnerCommandProcessing(in CommandStruct cs)
    {
      ParameterUpdateEventArgs outParameters = null;
      switch (cs.Code)
      {
        case 0:
          //Skip Error
          return;
        case 0x01:
          _device.BoardVersion = cs.Parameter;
          _device.FirmwareVersion = FloatToVersion(cs.FParameter);
          break;
        case 0x02:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SiPMTempCoeff, floatParameter: cs.FParameter);
          break;
        case 0x04:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.IdexPosition, intParameter: cs.Parameter);
          break;
        case 0x05:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SampleSyringeSize, intParameter: cs.Parameter);
          break;
        case 0x06:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.TotalBeadsInFirmware, floatParameter: cs.FParameter);
          break;
        case 0x08:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.CalibrationMargin, floatParameter: cs.FParameter);
          break;
        case 0x0C:  //pressure at startup
          _device.SelfTester.Data.SetPressure(cs.FParameter);
          _device.Hardware.SetParameter(DeviceParameterType.PressureAtStartup);  //Reset Pressure, since firmware forgets to do that
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.PressureAtStartup, floatParameter: cs.FParameter);
          break;
        case 0x0E:
          //_device._beadProcessor._extendedRangeCL1Multiplier = cs.Parameter / 1000f;
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ExtendedRangeMultiplier, intParameter: (int)Channel.RedC, floatParameter: cs.Parameter / 1000f);
          break;
        case 0x0F:
          //_device._beadProcessor._extendedRangeCL2Multiplier = cs.Parameter / 1000f;
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ExtendedRangeMultiplier, intParameter: (int)Channel.RedD, floatParameter: cs.Parameter / 1000f);
          break;
        case 0x10:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ValveCuvetDrain, intParameter: cs.Parameter);
          break;
        case 0x11:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ValveFan1, intParameter: cs.Parameter);
          break;
        case 0x12:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ValveFan2, intParameter: cs.Parameter);
          break;
        case 0x14:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SyringePosition, intParameter: cs.Parameter, floatParameter: cs.FParameter);
          break;
        case 0x15:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.IsSyringePositionActive, intParameter: cs.Parameter);
          break;
        case 0x16:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.PollStepActivity, intParameter: cs.Parameter);
          break;
        case 0x18:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.IsInputSelectorAtPickup, intParameter: cs.Parameter);
          break;
        case 0x19:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.IsBubbleDetectionActive, intParameter: cs.Parameter);
          break;
        case 0x1B:
          if (cs.Parameter == 0)
          {
            _device.StopOperation();  //Fired only on success (Parameter == 0)
            outParameters = new ParameterUpdateEventArgs(DeviceParameterType.CalibrationSuccess);
          }
          break;
        case 0x1D:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.BeadConcentration, intParameter: cs.Parameter);
          break;
        case 0x1E:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.HiSensitivityChannel, intParameter: cs.Parameter);
          break;
        case 0xCD:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.CalibrationParameter, intParameter: (int)CalibrationParameter.Height, floatParameter: cs.Parameter);
          break;
        case 0xCE:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.CalibrationParameter, intParameter: (int)CalibrationParameter.MinSSC, floatParameter: cs.Parameter);
          break;
        case 0xCF:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.CalibrationParameter, intParameter: (int)CalibrationParameter.MaxSSC, floatParameter: cs.Parameter);
          break;
        case 0xBF:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.CalibrationParameter, intParameter: (int)CalibrationParameter.Attenuation, floatParameter: cs.Parameter);
          break;
        case 0x20:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.CalibrationParameter, intParameter: (int)CalibrationParameter.DNRCoefficient, floatParameter: cs.FParameter);
          break;
        case 0x0A:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.CalibrationParameter, intParameter: (int)CalibrationParameter.DNRTransition, floatParameter: cs.FParameter);
          break;
        case 0xCA:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.CalibrationParameter, intParameter: (int)CalibrationParameter.ScatterGate, floatParameter: cs.Parameter);
          break;
        case 0x22:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.Pressure, floatParameter: cs.FParameter);
          break;
        case 0x24:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelBias30C, intParameter: (int)Channel.ForwardScatter, floatParameter: cs.Parameter);
          break;
        case 0x25:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelBias30C, intParameter: (int)Channel.VioletA, floatParameter: cs.Parameter);
          break;
        case 0x26:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelBias30C, intParameter: (int)Channel.VioletB, floatParameter: cs.Parameter);
          break;
        case 0x28:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelBias30C, intParameter: (int)Channel.GreenA, floatParameter: cs.Parameter);
          break;
        case 0x29:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelBias30C, intParameter: (int)Channel.GreenB, floatParameter: cs.Parameter);
          break;
        case 0x2A:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelBias30C, intParameter: (int)Channel.GreenC, floatParameter: cs.Parameter);
          break;
        case 0x2C:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelBias30C, intParameter: (int)Channel.RedA, floatParameter: cs.Parameter);
          break;
        case 0x2D:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelBias30C, intParameter: (int)Channel.RedB, floatParameter: cs.Parameter);
          break;
        case 0x2E:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelBias30C, intParameter: (int)Channel.RedC, floatParameter: cs.Parameter);
          break;
        case 0x2F:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelBias30C, intParameter: (int)Channel.RedD, floatParameter: cs.Parameter);
          break;
        case 0x30:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SyringeSpeedSheath, intParameter: (int)SyringeSpeed.Normal, floatParameter: cs.Parameter);
          break;
        case 0x31:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SyringeSpeedSheath, intParameter: (int)SyringeSpeed.HiSpeed, floatParameter: cs.Parameter);
          break;
        case 0x32:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SyringeSpeedSheath, intParameter: (int)SyringeSpeed.HiSensitivity, floatParameter: cs.Parameter);
          break;
        case 0x33:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SyringeSpeedSheath, intParameter: (int)SyringeSpeed.Flush, floatParameter: cs.Parameter);
          break;
        case 0x34:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SyringeSpeedSheath, intParameter: (int)SyringeSpeed.Pickup, floatParameter: cs.Parameter);
          break;
        case 0x35:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SyringeSpeedSheath, intParameter: (int)SyringeSpeed.MaxSpeed, floatParameter: cs.Parameter);
          break;
        case 0x36:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SheathFlushVolume, intParameter: cs.Parameter);
          break;
        case 0x37:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.WellEdgeDeltaHeight, intParameter: cs.Parameter);
          break;
        case 0x38:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SyringeSpeedSample, intParameter: (int)SyringeSpeed.Normal, floatParameter: cs.Parameter);
          break;
        case 0x39:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SyringeSpeedSample, intParameter: (int)SyringeSpeed.HiSpeed, floatParameter: cs.Parameter);
          break;
        case 0x3A:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SyringeSpeedSample, intParameter: (int)SyringeSpeed.HiSensitivity, floatParameter: cs.Parameter);
          break;
        case 0x3B:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SyringeSpeedSample, intParameter: (int)SyringeSpeed.Flush, floatParameter: cs.Parameter);
          break;
        case 0x3C:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SyringeSpeedSample, intParameter: (int)SyringeSpeed.Pickup, floatParameter: cs.Parameter);
          break;
        case 0x3D:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SyringeSpeedSample, intParameter: (int)SyringeSpeed.MaxSpeed, floatParameter: cs.Parameter);
          break;
        case 0x3E:
          int outpar = -1;
          var pr = (SampleSyringeType)cs.Parameter;
          switch (pr)
          {
            case SampleSyringeType.Single:
              outpar = (int)SampleSyringeType.Single;
              _device.SingleSyringeMode = true;
              break;
            case SampleSyringeType.Double:
              outpar = (int)SampleSyringeType.Double;
              _device.SingleSyringeMode = false;
              break;
          }
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SampleSyringeType, intParameter: outpar);
          break;
        case 0x3F:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.DistanceToWellEdge, intParameter: cs.Parameter);
          break;
        case 0x41:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorZ, intParameter: (int)MotorParameterType.StartSpeed, floatParameter: cs.Parameter);
          break;
        case 0x42:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorZ, intParameter: (int)MotorParameterType.RunSpeed, floatParameter: cs.Parameter);
          break;
        case 0x43:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorZ, intParameter: (int)MotorParameterType.Slope, floatParameter: cs.Parameter);
          break;
        case 0x44:
          if (_device.SelfTester.IsActive && !_device.SelfTester.Motorsinit[2])
          {
            _device.SelfTester.Data.MotorZ = cs.FParameter;
            _device.SelfTester.Motorsinit[2] = true;
          }
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorZ, intParameter: (int)MotorParameterType.CurrentStep, floatParameter: cs.FParameter);
          break;
        case 0x92:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorZ, intParameter: (int)MotorParameterType.CurrentLimit, floatParameter: cs.Parameter);
          break;
        case 0x46:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsZ, intParameter: (int)MotorStepsZ.Tube, floatParameter: cs.FParameter);
          break;
        case 0x48:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsZ, intParameter: (int)MotorStepsZ.A1, floatParameter: cs.FParameter);
          break;
        case 0x4A:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsZ, intParameter: (int)MotorStepsZ.A12, floatParameter: cs.FParameter);
          break;
        case 0x4C:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsZ, intParameter: (int)MotorStepsZ.H1, floatParameter: cs.FParameter);
          break;
        case 0x4E:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsZ, intParameter: (int)MotorStepsZ.H12, floatParameter: cs.FParameter);
          break;
        case 0x51:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorX, intParameter: (int)MotorParameterType.StartSpeed, floatParameter: cs.Parameter);
          break;
        case 0x52:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorX, intParameter: (int)MotorParameterType.RunSpeed, floatParameter: cs.Parameter);
          break;
        case 0x53:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorX, intParameter: (int)MotorParameterType.Slope, floatParameter: cs.Parameter);
          break;
        case 0x54:
          if (_device.SelfTester.IsActive && !_device.SelfTester.Motorsinit[0])
          {
            _device.SelfTester.Data.MotorX = cs.FParameter;
            _device.SelfTester.Motorsinit[0] = true;
          }
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorX, intParameter: (int)MotorParameterType.CurrentStep, floatParameter: cs.FParameter);
          break;
        case 0x90:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorX, intParameter: (int)MotorParameterType.CurrentLimit, floatParameter: cs.Parameter);
          break;
        case 0x56:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsX, intParameter: (int)MotorStepsX.Tube, floatParameter: cs.FParameter);
          break;
        case 0x58:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsX, intParameter: (int)MotorStepsX.Plate96Column1, floatParameter: cs.FParameter);
          break;
        case 0x5A:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsX, intParameter: (int)MotorStepsX.Plate96Column12, floatParameter: cs.FParameter);
          break;
        case 0x5C:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsX, intParameter: (int)MotorStepsX.Plate384Column1, floatParameter: cs.FParameter);
          break;
        case 0x5E:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsX, intParameter: (int)MotorStepsX.Plate384Column24, floatParameter: cs.FParameter);
          break;
        case 0x61:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorY, intParameter: (int)MotorParameterType.StartSpeed, floatParameter: cs.Parameter);
          break;
        case 0x62:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorY, intParameter: (int)MotorParameterType.RunSpeed, floatParameter: cs.Parameter);
          break;
        case 0x63:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorY, intParameter: (int)MotorParameterType.Slope, floatParameter: cs.Parameter);
          break;
        case 0x64:
          if (_device.SelfTester.IsActive && !_device.SelfTester.Motorsinit[1])
          {
            _device.SelfTester.Data.MotorY = cs.FParameter;
            _device.SelfTester.Motorsinit[1] = true;
          }
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorY, intParameter: (int)MotorParameterType.CurrentStep, floatParameter: cs.FParameter);
          break;
        case 0x91:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorY, intParameter: (int)MotorParameterType.CurrentLimit, floatParameter: cs.Parameter);
          break;
        case 0x66:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsY, intParameter: (int)MotorStepsY.Tube, floatParameter: cs.FParameter);
          break;
        case 0x68:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsY, intParameter: (int)MotorStepsY.Plate96RowA, floatParameter: cs.FParameter);
          break;
        case 0x6A:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsY, intParameter: (int)MotorStepsY.Plate96RowH, floatParameter: cs.FParameter);
          break;
        case 0x6C:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsY, intParameter: (int)MotorStepsY.Plate384RowA, floatParameter: cs.FParameter);
          break;
        case 0x6E:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsY, intParameter: (int)MotorStepsY.Plate384RowP, floatParameter: cs.FParameter);
          break;
        case 0x80:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelTemperature, intParameter: (int)Channel.VioletA, floatParameter: cs.Parameter / 10.0f);
          break;
        case 0x81:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelTemperature, intParameter: (int)Channel.VioletB, floatParameter: cs.Parameter / 10.0f);
          break;
        case 0x84:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelTemperature, intParameter: (int)Channel.ForwardScatter, floatParameter: cs.Parameter / 10.0f);
          break;
        case 0xA9:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.IsWellEdgeAgitateActive, intParameter: cs.Parameter);
          break;
        case 0xB0:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelTemperature, intParameter: (int)Channel.GreenA, floatParameter: cs.Parameter / 10.0f);
          break;
        case 0xB1:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelTemperature, intParameter: (int)Channel.GreenB, floatParameter: cs.Parameter / 10.0f);
          break;
        case 0xB2:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelTemperature, intParameter: (int)Channel.GreenC, floatParameter: cs.Parameter / 10.0f);
          break;
        case 0xB3:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelTemperature, intParameter: (int)Channel.RedA, floatParameter: cs.Parameter / 10.0f);
          break;
        case 0xB4:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelTemperature, intParameter: (int)Channel.RedB, floatParameter: cs.Parameter / 10.0f);
          break;
        case 0xB5:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelTemperature, intParameter: (int)Channel.RedC, floatParameter: cs.Parameter / 10.0f);
          break;
        case 0xB6:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelTemperature, intParameter: (int)Channel.RedD, floatParameter: cs.Parameter / 10.0f);
          break;
        case 0xBE:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.IsFlowCellInverted, intParameter: cs.Parameter );
          break;
        case 0x93:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelCompensationBias, intParameter: (int)Channel.ForwardScatter, floatParameter: cs.Parameter);
          break;
        case 0x94:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelCompensationBias, intParameter: (int)Channel.VioletB, floatParameter: cs.Parameter);
          break;
        case 0x95:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelCompensationBias, intParameter: (int)Channel.VioletA, floatParameter: cs.Parameter);
          break;
        case 0x96:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelCompensationBias, intParameter: (int)Channel.RedD, floatParameter: cs.Parameter);
          break;
        case 0x98:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelCompensationBias, intParameter: (int)Channel.RedB, floatParameter: cs.Parameter);
          break;
        case 0x99:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelCompensationBias, intParameter: (int)Channel.RedA, floatParameter: cs.Parameter);
          break;
        case 0x9A:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelCompensationBias, intParameter: (int)Channel.GreenB, floatParameter: cs.Parameter);
          break;
        case 0x9B:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelCompensationBias, intParameter: (int)Channel.GreenC, floatParameter: cs.Parameter);
          break;
        case 0xA6:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelCompensationBias, intParameter: (int)Channel.GreenA, floatParameter: cs.Parameter);
          break;
        case 0xA7:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelCompensationBias, intParameter: (int)Channel.RedC, floatParameter: cs.Parameter);
          break;
        case 0x9C:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelOffset, intParameter: (int)Channel.VioletB, floatParameter: cs.Parameter);
          break;
        case 0x9D:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelOffset, intParameter: (int)Channel.VioletA, floatParameter: cs.Parameter);
          break;
        case 0x9E:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelOffset, intParameter: (int)Channel.ForwardScatter, floatParameter: cs.Parameter);
          break;
        case 0x9F:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelOffset, intParameter: (int)Channel.RedD, floatParameter: cs.Parameter);
          break;
        case 0xA0:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelOffset, intParameter: (int)Channel.GreenA, floatParameter: cs.Parameter);
          break;
        case 0xA1:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelOffset, intParameter: (int)Channel.RedC, floatParameter: cs.Parameter);
          break;
        case 0xA2:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelOffset, intParameter: (int)Channel.RedB, floatParameter: cs.Parameter);
          break;
        case 0xA3:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelOffset, intParameter: (int)Channel.RedA, floatParameter: cs.Parameter);
          break;
        case 0xA4:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelOffset, intParameter: (int)Channel.GreenB, floatParameter: cs.Parameter);
          break;
        case 0xA5:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelOffset, intParameter: (int)Channel.GreenC, floatParameter: cs.Parameter);
          break;
        case 0x7F:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.AgitateRepeatsAmount, intParameter: cs.Parameter);
          break;
        case 0x8B:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.CalibrationTarget, intParameter: (int)CalibrationTarget.CL0, floatParameter: cs.Parameter);
          break;
        case 0x8C:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.CalibrationTarget, intParameter: (int)CalibrationTarget.CL1, floatParameter: cs.Parameter);
          break;
        case 0x8D:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.CalibrationTarget, intParameter: (int)CalibrationTarget.CL2, floatParameter: cs.Parameter);
          break;
        case 0x8E:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.CalibrationTarget, intParameter: (int)CalibrationTarget.CL3, floatParameter: cs.Parameter);
          break;
        case 0x8F:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.CalibrationTarget, intParameter: (int)CalibrationTarget.RP1, floatParameter: cs.Parameter);
          break;
        case 0x86:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.WashRepeatsAmount, intParameter: cs.Parameter);
          break;
        case 0x87:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.FlushCycles, intParameter: cs.Parameter);
          break;
        case 0xA8:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.WellReadingOrder, intParameter: cs.Parameter);
          break;
        case 0xAA:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.WellReadingSpeed, intParameter: cs.Parameter);
          break;
        case 0xAB:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.PlateType, intParameter: cs.Parameter);
          break;
        case 0xAC:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.Volume, intParameter: (int)VolumeType.Wash, floatParameter: cs.Parameter);
          break;
        case 0xAD:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.WellRowIndex, intParameter: cs.Parameter);
          break;
        case 0xAE:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.WellColumnIndex, intParameter: cs.Parameter);
          break;
        case 0xAF:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.Volume, intParameter: (int)VolumeType.Sample, floatParameter: cs.Parameter);
          break;
        case 0xC4:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.Volume, intParameter: (int)VolumeType.Agitate, floatParameter: cs.Parameter);
          break;
        case 0xB8:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.GreenAVoltage, floatParameter: cs.Parameter * 0.0008f);
          break;
        case 0xC0:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.IsLaserActive, intParameter: cs.Parameter);  //0Red 1Green 2Violet
          break;
        case 0xC2:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.ChannelConfiguration, intParameter: cs.Parameter);
          break;
        case 0xC7:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.LaserPower, intParameter: (int)LaserType.Violet, floatParameter: cs.Parameter / 4096.0f / 0.040f * 3.3f);
          break;
        case 0xC8:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.LaserPower, intParameter: (int)LaserType.Green, floatParameter: cs.Parameter / 4096.0f / 0.040f * 3.3f);
          break;
        case 0xC9:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.LaserPower, intParameter: (int)LaserType.Red, floatParameter: cs.Parameter / 4096.0f / 0.040f * 3.3f);
          break;
        case 0xBC:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.TraySteps, floatParameter: cs.FParameter);
          break;
        case 0xCC:
          _device.SystemMonitor.DecodeMessage(cs.Parameter);
          //currently used only for probe autoheight feature
          if (cs.Parameter == 0)
          {
            lock (SystemActivityNotBusyNotificationLock)
            {
              Monitor.PulseAll(SystemActivityNotBusyNotificationLock);
            }
          }
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SystemActivityStatus, intParameter: cs.Parameter);
          break;
        case 0xF1:
          SheathFlowError errorType = SheathFlowError.Unspecified;
          if (cs.Command == 1)
          {
            errorType = SheathFlowError.SheathEmpty;
            _device.Hardware.SetToken(HardwareToken.Synchronization, 0x1000);
            _device.Hardware.SetParameter(DeviceParameterType.PumpSheath, SyringeControlState.Halt, 0);
            _device.Hardware.SetToken(HardwareToken.ActiveCommandQueueIndex, 1);
          }
          else if (cs.Command == 2)
            errorType = SheathFlowError.HighPressure;

          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SheathFlowError, intParameter: (int)errorType, floatParameter: cs.FParameter);
          break;
        case 0xF2:
          int validCommand = cs.Command > 0x63 ? 1 : 0;
          float sampleA = cs.Parameter == 0x501 ? 0 : 2;
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SampleSyringeStatus, intParameter: validCommand, floatParameter: sampleA);
          break;
        case 0xF3:
          int warnNextWell = _device._wellController.IsLastWell ? 0 : 1; 
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.WellWarning, intParameter: warnNextWell);
          break;
        case 0xF4:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.BubbleDetectorStatus, intParameter: cs.Command);
          break;
        case 0xF9:
          switch (cs.Command)
          {
            case 0xE0:
              if (_device.SelfTester.IsActive)
                _device.SelfTester.ScriptFinishedSignal();
              break;
            case 0xE5:
              _device.IsPlateEjected = true;
              break;
            case 0xE6:
              _device.IsPlateEjected = false;
              break;
            case 0xE7:
              lock (ScriptF9FinishedLock)
              {
                Monitor.PulseAll(ScriptF9FinishedLock);
              }
              break;
          }
          break;
        //FALLTHROUGH
        case 0xD0:
        case 0xD1:
        case 0xD2:
        case 0xD3:
        case 0xD4:
        case 0xD5:
        case 0xD6:
        case 0xD7:
        case 0xD8:
        case 0xD9:
        case 0xDA:
        case 0xDB:
        case 0xDC:
        case 0xDD:
        case 0xDE:
        case 0xDF:
          break;
        case 0xFD:
        case 0xFE:
          _device.StopOperation();
          break;
      }
      if (outParameters != null)
      {
        _device.OnParameterUpdate(outParameters);
      }

      _device.Hardware.DirectFlashReturnValue(in cs);

      #if DEBUG
      if (cs.Code != 0x1D)
      {
        _logger.Log($"REC [{cs.ToString()}]");
      }
      #endif
    }
  }
}