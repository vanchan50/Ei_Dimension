using System.Collections.Concurrent;
using System.Reflection;
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
    que.Enqueue([1, 2, 3]);
    
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
        _logger.LogInformation($"WriteToMC Exception\n{e.Data}\n{e.Message}");
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
          _logger.LogInformation($"trying to send buffer size {_serialConnection.InputBuffer.Length}");
          _serverFromUSB.SendFrame(result);
          _serverFromUSB.SkipFrame();
          _logger.LogInformation($"Sent buffer size {_serialConnection.InputBuffer.Length}");
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