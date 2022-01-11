using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MicroCy
{
  internal class DataController
  {
    public static ConcurrentQueue<(string name, CommandStruct cs)> OutCommands { get; } = new ConcurrentQueue<(string name, CommandStruct cs)>();
    public static object UsbOutCV { get; } = new object();

    private readonly ISerial _serialConnection;
    private static Thread _prioUsbInThread;
    private static Thread _prioUsbOutThread;

    public DataController(Type connectionType)
    {
      _serialConnection = ConnectionFactory.MakeNewConnection(connectionType);
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

        if ((_serialConnection.InputBuffer[0] == 0xbe) && (_serialConnection.InputBuffer[1] == 0xad))
        {
          if (MicroCyDevice.IsMeasurementGoing) //  this condition avoids the necessity of cleaning up leftover data in the system USB interface. That could happen after operation abortion and program restart
          {
            for (byte i = 0; i < 8; i++)
            {
              BeadInfoStruct outbead;
              if (!GetBeadFromBuffer(_serialConnection.InputBuffer, i, out outbead))
                break;
              BeadProcessor.CalculateBeadParams(ref outbead);

              MicroCyDevice.FillActiveWellResults(in outbead);
              if (outbead.region == 0 && MicroCyDevice.OnlyClassified)
                continue;
              MicroCyDevice.DataOut.Enqueue(outbead);
              if (MicroCyDevice.Everyevent)
                ResultReporter.AddBeadStats(in outbead);
              switch (MicroCyDevice.Mode)
              {
                case OperationMode.Normal:
                  break;
                case OperationMode.Calibration:
                  break;
                case OperationMode.Verification:
                  Validator.FillStats(in outbead);
                  break;
              }
              //accum stats for run as a whole, used during aligment and QC
              BeadProcessor.FillCalibrationStatsRow(in outbead);
              MicroCyDevice.BeadCount++;
              MicroCyDevice.TotalBeads++;
            }
          }
          Array.Clear(_serialConnection.InputBuffer, 0, _serialConnection.InputBuffer.Length);
          MicroCyDevice.TerminationReadyCheck();
        }
        else
          GetCommandFromBuffer();
      }
    }

    private void WriteToMC()
    {
      while (true)
      {
        while (OutCommands.TryDequeue(out var cmd))
        {
          RunCmd(cmd.name, cmd.cs);
        }
        lock (UsbOutCV)
        {
          Monitor.Wait(UsbOutCV);
        }
      }
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

    private static BeadInfoStruct BeadArrayToStruct(byte[] beadmsg, byte shift)
    {
      unsafe
      {
        fixed (byte* cs = &beadmsg[shift * 64])
        {
          return *(BeadInfoStruct*)cs;
        }
      }
    }

    /// <summary>
    /// Move the received command to Commands Queue
    /// </summary>
    private void GetCommandFromBuffer()
    {
      var newcmd = ByteArrayToStruct(_serialConnection.InputBuffer);
      if(newcmd.Code != 0)
        MicroCyDevice.Commands.Enqueue(newcmd);
      if ((newcmd.Code >= 0xd0) && (newcmd.Code <= 0xdf))
      {
        Console.WriteLine($"{DateTime.Now.ToString()} E-series script [{newcmd.ToString()}]");
      }
      else if (newcmd.Code > 0)
      {
        Console.WriteLine($"{DateTime.Now.ToString()} Received [{newcmd.ToString()}]");
      }
    }

    private bool GetBeadFromBuffer(byte[] buffer,byte shift, out BeadInfoStruct outbead)
    {
      outbead = BeadArrayToStruct(buffer, shift);
      return outbead.Header == 0xadbeadbe;
    }
  }
}