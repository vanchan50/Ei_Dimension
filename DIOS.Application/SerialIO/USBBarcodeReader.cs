using DIOS.Core;
using System.IO.Ports;

namespace DIOS.Application.SerialIO;

public class USBBarcodeReader : IBarcodeReader
{
  public bool IsAvailable { get; }

  private SerialPort _comPort;
  private ILogger _logger;
  private CancellationToken _cancellationToken;
  private int _readInProcess;
  private static readonly byte[] _readCommand = { 0x04, 0xE4, 0x04, 0x00, 0xFF, 0x14 };
  private const string VID = "AF99";
  private const string PID = "8003";

  public USBBarcodeReader(ILogger logger)
  {
    _logger = logger;
    IsAvailable = Init();
  }

  private bool Init()
  {
    try
    {
      var comPortAddress = ComDeviceSeeker.FindCOMDevice(VID, PID);
      if (comPortAddress is null)
      {
        _logger.Log("BCR not found");
        return false;
      }
      _comPort = new SerialPort(comPortAddress, 9600, Parity.None, 8, StopBits.One);
      _logger.Log($"BCR found at: {comPortAddress}");
      _comPort.ReadTimeout = 20000;
      _comPort.WriteTimeout = 500;
    }
    catch (Exception e)
    {
      _logger.Log(e.Message);
      _logger.Log("Could not connect to Barcode Reader");
      return false;
    }

    return true;
  }

  public async Task<string> QueryReadAsync(int millisecondsTimeout)
  {
    if (!IsAvailable)
      throw new IOException("BCR Unavailable");
    if (Interlocked.CompareExchange(ref _readInProcess, 1, 0) == 1)
      throw new IOException("Only one read query is available at a time");
      
    _comPort.Open();
    string output = "";
    _comPort.Write(_readCommand, 0, _readCommand.Length);
    var cts = new CancellationTokenSource(millisecondsTimeout);
    _cancellationToken = cts.Token;

    var tsk = new Task<string>(Read, _cancellationToken);
    tsk.Start();
    try
    {
      output = await tsk;
    }
    catch (OperationCanceledException)
    {
    }
    finally
    {
      cts.Dispose();
      _comPort.Close();
      _readInProcess = 0;
    }
    return output;
  }

  private string Read()
  {
    string output = "";
    while (true)
    {
      _cancellationToken.ThrowIfCancellationRequested();
      try
      {
        output = _comPort.ReadExisting();
      }
      catch
      {
      }
      if (!string.IsNullOrEmpty(output) && output != "\u0004?\0\0?,")
        break;
      Thread.Sleep(100);
    }
    output = output.TrimEnd('\r', '\n');
    return output;
  }
}