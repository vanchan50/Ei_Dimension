using System;
using System.IO;
using DIOS.Core;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace DIOS.Application.SerialIO
{
  public class USBBarcodeReader : IBarcodeReader
  {
    private ILogger _logger;
    private SerialPort _comPort;
    private static byte[] _readCommand = { 0x04, 0xE4, 0x04, 0x00, 0xFF, 0x14 };
    private CancellationToken _cancellationToken;
    private int _readInProcess;

    public USBBarcodeReader(ILogger logger)
    {
      _logger = logger;
      Init();
    }

    private bool Init()
    {
      try
      {
        var comPorts = SerialPort.GetPortNames();
        _comPort = new SerialPort(comPorts[0], 9600, Parity.None, 8, StopBits.One);
        _comPort.ReadTimeout = 20000;
        _comPort.WriteTimeout = 500;
        _comPort.Open();
        _comPort.Close();
      }
      catch
      {
        _logger.Log("Could not connect to Barcode Reader");
        return false;
      }

      return true;
    }

    public async Task<string> QueryReadAsync(int millisecondsTimeout)
    {
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
}