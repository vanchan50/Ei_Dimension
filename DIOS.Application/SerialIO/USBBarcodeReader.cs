using DIOS.Core;
using System.IO.Ports;
using System.Management;

namespace DIOS.Application.SerialIO
{
  public class USBBarcodeReader : IBarcodeReader
  {
    private ILogger _logger;
    private SerialPort _comPort;
    private static byte[] _readCommand = { 0x04, 0xE4, 0x04, 0x00, 0xFF, 0x14 };
    private CancellationToken _cancellationToken;
    private int _readInProcess;
    private const string VID = "AF99";
    private const string PID = "8003";

    public USBBarcodeReader(ILogger logger)
    {
      _logger = logger;
      Init();
    }

    private bool Init()
    {
      try
      {
        var comPortAddress = FindCOMDevice();
        _logger.Log($"BCR found at: {comPortAddress}");
        _comPort = new SerialPort(comPortAddress, 9600, Parity.None, 8, StopBits.One);
        _comPort.ReadTimeout = 20000;
        _comPort.WriteTimeout = 500;
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

    private string FindCOMDevice()
    {
      ManagementObjectCollection objCollection;
      try
      {
        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity"))
        {
          objCollection = searcher.Get();
        }

        foreach (var queryObj in objCollection)
        {
          if (queryObj is null)
          {
            continue;
          }

          if (queryObj["Caption"] is null)
          {
            continue;
          }

          string Caption = queryObj["Caption"].ToString();

          if (!Caption.Contains("(COM"))
          {
            continue;
          }

          if (queryObj["deviceid"] is null)
          {
            continue;
          }

          var deviceId = queryObj["deviceid"].ToString(); //"DeviceID"

          var localVID = GetVID(deviceId);
          var localPID = GetPID(deviceId);

          if (localVID == VID && localPID == PID)
          {
            var CaptionInfo = GetCaptionInfo(Caption);
            return CaptionInfo;
          }
        }
      }
      catch (ManagementException e)
      {
        _logger.Log(e.Message);
      }

      return null;
    }

    private string GetCaptionInfo(string caption)
    {
      int captionIndex = caption.IndexOf("(COM");
      return caption.Substring(captionIndex + 1).TrimEnd(')'); // make the trimming more correct 
    }

    private string GetVID(string deviceID )
    {
      int vidIndex = deviceID.IndexOf("VID_");
      if (vidIndex == -1)
      {
        return null;
      }
      var startingAtVid = deviceID.Substring(vidIndex + 4); // + 4 to remove "VID_"                    
      return startingAtVid.Substring(0, 4); // vid is four characters long
    }

    private string GetPID(string deviceID)
    {
      int pidIndex = deviceID.IndexOf("PID_");
      if (pidIndex == -1)
      {
        return null;
      }
      var startingAtPid = deviceID.Substring(pidIndex + 4); // + 4 to remove "PID_"                    
      return startingAtPid.Substring(0, 4); // pid is four characters long
    }
  }
}