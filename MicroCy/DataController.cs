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

        if (IsBead())
        {
          if (_device.IsMeasurementGoing) //  this condition avoids the necessity of cleaning up leftover data in the system USB interface. That could happen after operation abortion and program restart
          {
            for (byte i = 0; i < 8; i++)
            {
              if (!GetBeadFromBuffer(_serialConnection.InputBuffer, i, out var outbead))
                break;
              _device.Results.AddRawBeadEvent(ref outbead);
            }
          }
          Array.Clear(_serialConnection.InputBuffer, 0, _serialConnection.InputBuffer.Length);
          _results.TerminationReadyCheck();
        }
        else
          GetCommandFromBuffer();
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
      Console.WriteLine($"{DateTime.Now.ToString()} Sending [{sCmdName}]: {cs.ToString()}"); //  MARK1 END
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
      _device.Commands.Enqueue(newcmd);
    }

    private static bool GetBeadFromBuffer(byte[] buffer, byte shift, out RawBead outbead)
    {
      outbead = BeadArrayToStruct(buffer, shift);
      return outbead.Header == 0xadbeadbe;
    }

    private void InnerCommandProcessing(in CommandStruct cs)
    {
      switch (cs.Code)
      {
        case 0:
          //Skip Error
          return;
        case 0x0C:
          _device.SelfTester.Data.SetPressure(cs.FParameter);
          _device.MainCommand("Set FProperty", code: 0x0C); //Reset Pressure, since firmware forgets to do that
          break;
        case 0x1D:
          _device.OnBeadConcentrationStatusUpdate(cs.Parameter);
          break;
        case 0xF9:
          _device.SelfTester.ScriptFinishedSignal(cs.Command);
          break;
        case 0x44:
          if (!_device.SelfTester.Motorsinit[2])
          {
            _device.SelfTester.Data.MotorZ = cs.FParameter;
            _device.SelfTester.Motorsinit[2] = true;
          }
          break;
        case 0x54:
          if (!_device.SelfTester.Motorsinit[0])
          {
            _device.SelfTester.Data.MotorX = cs.FParameter;
            _device.SelfTester.Motorsinit[0] = true;
          }
          break;
        case 0x64:
          if (!_device.SelfTester.Motorsinit[1])
          {
            _device.SelfTester.Data.MotorY = cs.FParameter;
            _device.SelfTester.Motorsinit[1] = true;
          }
          break;
        case 0x01:
          _device.BoardVersion = cs.Parameter;
          _device.FirmwareVersion = FloatToVersion(cs.FParameter);
          break;
        case 0x1B:
          if (cs.Parameter == 0)
          {
            _device.StartStateMachine();  //OnCalibrationSuccess
          }
          break;
        case 0xCC:
          for (var i = 0; i < _device.SystemActivity.Length; i++)
          {
            if ((cs.Parameter & (1 << i)) != 0)
              _device.SystemActivity[i] = true;
            else
              _device.SystemActivity[i] = false;
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
      }
      Console.WriteLine($"{DateTime.Now.ToString()} Received [{cs.ToString()}]");
    }

    private bool IsBead()
    {
      return _serialConnection.InputBuffer[0] == 0xbe && _serialConnection.InputBuffer[1] == 0xad;
    }
  }
}