using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroCy
{
  internal class DataController
  {
    public bool StopUSBPolling { get; set; }
    private readonly ISerial _serialConnection;
    private Thread _prioUsbInThread;
    private Thread _prioUsbOutThread;

    public DataController(Type connectionType)
    {
      _serialConnection = ConnectionFactory.MakeNewConnection(connectionType);
      if (_serialConnection.IsActive)
      {
        _prioUsbInThread = new Thread(NewReplyFromMC);
        _prioUsbInThread.Priority = ThreadPriority.Highest;
        _prioUsbInThread.Start();

        _prioUsbOutThread = new Thread(WriteToMC);
        _prioUsbOutThread.Priority = ThreadPriority.AboveNormal;
        _prioUsbOutThread.Start();
      }
    }
    
    private void NewReplyFromMC()
    {
      while (!StopUSBPolling)
      {
        _serialConnection.Read();

        if ((_serialConnection.InputBuffer[0] == 0xbe) && (_serialConnection.InputBuffer[1] == 0xad))
        {
          if (IsMeasurementGoing) //  this condition avoids the necessity of cleaning up leftover data in the system USB interface. That could happen after operation abortion and program restart
          {
            for (byte i = 0; i < 8; i++)
            {
              BeadInfoStruct outbead;
              if (!GetBeadFromBuffer(_serialConnection.InputBuffer, i, out outbead))
                break;
              CalculateBeadParams(ref outbead);

              FillActiveWellResults(in outbead);
              if (outbead.region == 0 && OnlyClassified)
                continue;
              DataOut.Enqueue(outbead);
              if (Everyevent)
                _ = _dataout.Append(outbead.ToString());
              switch (Mode)
              {
                case OperationMode.Normal:
                  break;
                case OperationMode.Calibration:
                  break;
                case OperationMode.Verification:
                  Verificator.FillStats(in outbead);
                  break;
              }
              //accum stats for run as a whole, used during aligment and QC
              FillCalibrationStatsRow(in outbead);
              BeadCount++;
              TotalBeads++;
            }
          }
          Array.Clear(_serialConnection.InputBuffer, 0, _serialConnection.InputBuffer.Length);
          TerminationReadyCheck();
        }
        else
          GetCommandFromBuffer();
      }
    }

    private void WriteToMC()
    {
      while (true)
      {
        while (_outCommands.TryDequeue(out var cmd))
        {
          RunCmd(cmd.name, cmd.cs);
        }
        if (StopUSBPolling)
          return;
        lock (_usbOutCV)
        {
          Monitor.Wait(_usbOutCV);
        }
      }
    }
  }
}
