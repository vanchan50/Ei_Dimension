using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using NetMQ;
using NetMQ.Sockets;

namespace SerialService;

public class Worker : BackgroundService
{
  private readonly ILogger<Worker> _logger;
  private readonly USBConnection _serialConnection;
  private readonly RequestSocket _serverFromUSB = new();
  private readonly ResponseSocket _serverToUSB = new();
  private readonly Thread _toUsbThread;
  private readonly Thread _toDiosThread;
  private readonly ConcurrentQueue<byte[]> que = new();
  private readonly USBWatchdog _wd;
  private readonly byte[] arrRet = new byte[8];

  public Worker(ILogger<Worker> logger)
  {
    _logger = logger;
    _logger.LogInformation($"SerialService Version: {Assembly.GetAssembly(typeof(USBConnection)).GetName().Version}");

    _serialConnection = new USBConnection(logger);
    _serialConnection.Start();
    _serverToUSB.Bind("tcp://*:9020");
    _serverFromUSB.Connect("tcp://localhost:9021");

    _toUsbThread = new Thread(WriteToMC);
    _toUsbThread.IsBackground = true;
    _toUsbThread.Name = "USBOUT";
    _toUsbThread.Start();

    _toDiosThread = new Thread(SendBuffer);
    _toDiosThread.IsBackground = true;
    _toDiosThread.Name = "TODIOS";
    _toDiosThread.Start();

    IntPtr windowHandle = GetConsoleWindow();
    _wd = new(windowHandle, ReconnectUSB, DisconnectedUSB);
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        if (_serialConnection.Read())
        {
          que.Enqueue(_serialConnection.InputBuffer.ToArray());
          _serialConnection.ClearBuffer(); //TODO: is it necessary?
        }
      }
      catch
      {
        
      }
    }
    await Task.Delay(1000, stoppingToken);
  }

  private void WriteToMC()
  {
    while (true)
    {
      try
      {
        var bytes = _serverToUSB.ReceiveFrameBytes();
        _serverToUSB.SendFrameEmpty();
        _serialConnection.Write(bytes);
      }
      catch (Exception e)
      {
        _logger.LogInformation($"WriteToMC Exception\n{e.Data}\n{e.Message}\n{e.StackTrace}");
      }
    }
  }

  private void SendBuffer()
  {
    while (true)
    {
      while (que.TryDequeue(out var result))
      {
        try
        {
          _serverFromUSB.SendFrame(result);
          _serverFromUSB.SkipFrame();
        }
        catch (Exception e)
        {
          _logger.LogInformation($"SendBuffer Exception\n{e.Data}\n{e.Message}");
          //_logger.Log("[CRITICAL] USB WRITE failed");
          //_logger.Log(e.Message);
        }
      }
      Thread.Sleep(50);
    }
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

  /// <summary>
  /// Thread unsafe
  /// </summary>
  /// <param name="cs"></param>
  /// <returns></returns>
  private byte[] StructToByteArray(in CommandStruct cs)
  {
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

  public void ReconnectUSB()
  {
    _serialConnection.Reconnect();
    _logger.LogInformation($"USB Reconnected");
    //!!!!!!!!
    //!!!!!!!!
    //!!!!!!!!
    // Device.ReconnectUSB has 2 necessary commands
    // They are here in raw
    //!!!!!!!!
    //!!!!!!!!
    //!!!!!!!!
    que.Enqueue(StructToByteArray(new(){ Code = 0xCC }));//Hardware.SetParameter(DeviceParameterType.SystemActivityStatus);
    que.Enqueue(StructToByteArray(new(){ Code = 0xCB }));//Hardware.SetToken(HardwareToken.Synchronization);
  }

  public void DisconnectedUSB()
  {
    _serialConnection.Disconnect();
    _logger.LogInformation($"USB Disconnected");
  }

  [DllImport("kernel32.dll")]
  static extern IntPtr GetConsoleWindow();
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