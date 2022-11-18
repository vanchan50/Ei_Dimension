using System;
using System.Collections.Concurrent;
using System.Threading;

namespace DIOS.Core
{
  internal class DataController
  {
    private ConcurrentQueue<(string name, CommandStruct cs)> OutCommands { get; } = new ConcurrentQueue<(string name, CommandStruct cs)>();

    private readonly object _usbOutCV = new object();
    private readonly ISerial _serialConnection;
    private readonly Thread _prioUsbInThread;
    private readonly Thread _prioUsbOutThread;
    private readonly Device _device;
    private readonly RunResults _results;

    public DataController(Device device, RunResults runResults, ISerial connection)
    {
      _device = device;
      _results = runResults;
      _serialConnection = connection;
      if (_serialConnection.IsActive)
      {
        _prioUsbInThread = new Thread(ReplyFromMC);
        _prioUsbInThread.Priority = ThreadPriority.Highest;
        _prioUsbInThread.IsBackground = true;
        _prioUsbInThread.Name = "USBIN";
        _prioUsbInThread.Start();

        _prioUsbOutThread = new Thread(WriteToMC);
        _prioUsbOutThread.Priority = ThreadPriority.AboveNormal;
        _prioUsbOutThread.IsBackground = true;
        _prioUsbOutThread.Name = "USBOUT";
        _prioUsbOutThread.Start();
      }
    }
    
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

        if (_device.IsMeasurementGoing) //  this condition avoids the necessity of cleaning up leftover data in the system USB interface. That could happen after operation abortion and program restart
        {
          for (byte i = 0; i < 8; i++)
          {
            //might be less than 8 beads in buffer
            if (!GetBeadFromBuffer(i, out var outbead))
              break;
            _results.AddRawBeadEvent(in outbead);
          }
          _results.TerminationReadyCheck();
        }
        _serialConnection.ClearBuffer();
      }
    }

    private void WriteToMC()
    {
      var timeOut = new TimeSpan(0, 0, seconds: 2);
      while (true)
      {
        while (OutCommands.TryDequeue(out var cmd))
        {
          RunCmd(cmd.name, cmd.cs);
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

    public void AddCommand(string command, CommandStruct cs)
    {
      OutCommands.Enqueue((command, cs));
      #if DEBUG
      Console.Error.WriteLine($"{DateTime.Now.ToString()} Enqueued [{command}]: {cs.ToString()}");
      #endif
      NotifyCommandReceived();
    }

    /// <summary>
    /// Sends a command OUT to the USB device, then checks the IN pipe for a return value.
    /// </summary>
    /// <param name="sCmdName">A friendly name for the command.</param>
    /// <param name="cs">The CommandStruct object containing the command parameters.  This will get converted to an 8-byte array.</param>
    private void RunCmd(string sCmdName, CommandStruct cs)
    {
      if (sCmdName == null)
        return;
      if (_serialConnection.IsActive)
        _serialConnection.Write(StructToByteArray(in cs));
      Console.WriteLine($"{DateTime.Now.TimeOfDay.ToString()} Sending [{sCmdName}]: {cs.ToString()}"); //  MARK1 END
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

    /// <summary>
    /// Move the received command to Commands Queue
    /// </summary>
    private void GetCommandFromBuffer()
    {
      var newcmd = ByteArrayToStruct(_serialConnection.InputBuffer);
      InnerCommandProcessing(in newcmd);
    }
    
    #if DEBUG
    internal void DEBUGGetCommandFromBuffer(CommandStruct cs)
    {
      InnerCommandProcessing(in cs);
    }
    #endif

    private bool GetBeadFromBuffer(byte shift, out RawBead outbead)
    {
      outbead = BeadArrayToStruct(_serialConnection.InputBuffer, shift);
      return outbead.Header == 0xadbeadbe;
    }

    public void ReconnectUSB()
    {
      _serialConnection.Reconnect();
    }

    public void DisconnectedUSB()
    {
      _serialConnection.Disconnect();
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
        case 0x06:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.TotalBeadsInFirmware, floatParameter: cs.FParameter);
          break;
        case 0x08:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.CalibrationMargin, floatParameter: cs.FParameter);
          break;
        case 0x0C:  //pressure at startup
          _device.SelfTester.Data.SetPressure(cs.FParameter);
          _device.MainCommand("Set FProperty", code: 0x0C); //Reset Pressure, since firmware forgets to do that
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
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.IsPollStepActive, intParameter: cs.Parameter);
          break;
        case 0x18:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.IsInputSelectorAtPickup, intParameter: cs.Parameter);
          break;
        case 0x1B:
          if (cs.Parameter == 0)
          {
            _device.StartStateMachine();  //Fired only on success (Parameter == 0)
            outParameters = new ParameterUpdateEventArgs(DeviceParameterType.CalibrationSuccess);
          }
          break;
        case 0x1D:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.BeadConcentration, intParameter: cs.Parameter);
          break;
        case 0x20:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.DNRCoefficient, floatParameter: cs.FParameter);
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
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsX, intParameter: (int)MotorStepsX.Plate96C1, floatParameter: cs.FParameter);
          break;
        case 0x5A:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsX, intParameter: (int)MotorStepsX.Plate96C12, floatParameter: cs.FParameter);
          break;
        case 0x5C:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsX, intParameter: (int)MotorStepsX.Plate384C1, floatParameter: cs.FParameter);
          break;
        case 0x5E:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.MotorStepsX, intParameter: (int)MotorStepsX.Plate384C24, floatParameter: cs.FParameter);
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
        case 0xAF:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.Volume, intParameter: (int)VolumeType.Sample, floatParameter: cs.Parameter);
          break;
        case 0xC4:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.Volume, intParameter: (int)VolumeType.Agitate, floatParameter: cs.FParameter);
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
        case 0xCC:
          for (var i = 0; i < _device.SystemActivity.Length; i++)
          {
            if ((cs.Parameter & (1 << i)) != 0)
              _device.SystemActivity[i] = true;
            else
              _device.SystemActivity[i] = false;
          }
          //currently used only for probe autoheight feature
          if (cs.Parameter == 0)
          {
            lock (_device.SystemActivityNotBusyNotificationLock)
            {
              Monitor.PulseAll(_device.SystemActivityNotBusyNotificationLock);
            }
          }
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SystemActivityStatus, intParameter: cs.Parameter);
          break;
        case 0xF1:
          SheathFlowErrorType errorType = SheathFlowErrorType.Unspecified;
          if (cs.Command == 1)
            errorType=SheathFlowErrorType.SheathEmpty;
          else if (cs.Command == 2)
            errorType = SheathFlowErrorType.PressureOverload;

          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SheathFlow, intParameter: (int)errorType, floatParameter: cs.FParameter);
          break;
        case 0xF2:
          int validCommand = cs.Command > 0x63 ? 1 : 0;
          float sampleA = cs.Parameter == 0x501 ? 0 : 2;
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.SampleSyringeStatus, intParameter: validCommand, floatParameter: sampleA);
          break;
        case 0xF3:
          outParameters = new ParameterUpdateEventArgs(DeviceParameterType.NextWellWarning);
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
              lock (_device.ScriptF9FinishedLock)
              {
                Monitor.PulseAll(_device.ScriptF9FinishedLock);
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
          Console.WriteLine($"{DateTime.Now.ToString()} E-series script [{cs.ToString()}]");
          return;
        case 0xFD:
        case 0xFE:
          _device.StartStateMachine();
          break;
        default:
          return;
      }
      Console.WriteLine($"{DateTime.Now.ToString()} Received [{cs.ToString()}]");
      if (outParameters != null)
      {
        _device.OnParameterUpdate(outParameters);
      }
    }
  }
}