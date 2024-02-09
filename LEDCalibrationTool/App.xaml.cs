using NetMQ.Sockets;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using NetMQ;
using System.Collections.Concurrent;

namespace LEDCalibrationTool
{
  public partial class App : Application
  {
    public Process BackgroundProcess { get; }
    private readonly RequestSocket _clientToUSB = new();
    private static readonly object _usbOutCV = new();
    private static readonly ConcurrentQueue<CommandStruct> _csQueue = new();
    private readonly Thread _UsbOutThread;
    public App()
    {
      BackgroundProcess = Process.Start(new ProcessStartInfo("SerialService.exe") { UseShellExecute = true });
      _clientToUSB.Connect("tcp://localhost:9020");
      _UsbOutThread = new Thread(SendToMC);
      _UsbOutThread.IsBackground = true;
      _UsbOutThread.Name = "USBOUT";
      _UsbOutThread.Start();
      MainCommand(code: 0xFA);//Synchronize
    }

    private void SendToMC()
    {
      var timeOut = new TimeSpan(0, 0, seconds: 2);
      while (true)//endless thread
      {
        while (_csQueue.TryDequeue(out var cs))//Wait until command appears
        {
          var failed = true;
          while (failed)//resend until succesful
          {
            try
            {
              _clientToUSB.SendFrame(StructToByteArray(in cs));
              _clientToUSB.SkipFrame();
              failed = false;
            }
            catch (FiniteStateMachineException e)
            {
              Thread.Sleep(10);
            }
          }
        }
        lock (_usbOutCV)
        {
          Monitor.Wait(_usbOutCV, timeOut);
        }
      }
    }

    public static void MainCommand(byte cmd = 0, byte code = 0, ushort parameter = 0, float fparameter = 0)
    {
      CommandStruct cs = new CommandStruct
      {
        Code = code,
        Command = cmd,
        Parameter = parameter,
        FParameter = fparameter
      };
      AddCommand(in cs);
    }

    private static void AddCommand(in CommandStruct cs)
    {
      _csQueue.Enqueue(cs);
      NotifyCommandReceived();
      Console.WriteLine($"{cs}");
    }

    private static void NotifyCommandReceived()
    {
      lock (_usbOutCV)
      {
        Monitor.Pulse(_usbOutCV);
      }
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
  }

  public struct CommandStruct
  {
    public byte Code;
    public byte Command;
    public ushort Parameter;
    public float FParameter;
    public override string ToString()
    {
      return string.Format("{0:X2},{1:X2},{2},{3:F3}", Code, Command, Parameter, FParameter);
    }
  }
}