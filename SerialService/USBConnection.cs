using MadWizard.WinUSBNet;

namespace SerialService;

public class USBConnection
{
  public byte[] InputBuffer { get; } = new byte[512];
  public bool IsActive { get; private set; }
  private USBDevice _usbDevice;
  private readonly Guid _interfaceGuid = Guid.ParseExact("F70242C7-FB25-443B-9E7E-A4260F373982", "D");  // interface GUID, not device guid
  private readonly object _disconnectionLock = new();
  private ILogger _logger;
  private int _retryCounter;

  public USBConnection(ILogger logger)
  {
    _logger = logger;
  }

  public void Start()
  {
    if (Init())
    {
#if DEBUG
      Console.Error.WriteLine("OutPipe Address:");
      Console.Error.WriteLine($"\t\t\t0x{Convert.ToString(_usbDevice.Interfaces[0].OutPipe.Address, 2).PadLeft(8, '0')}");
      ListAvailablePipes();
#endif
    }
  }

  private bool Init()
  {
    USBDeviceInfo[] di = USBDevice.GetDevices(_interfaceGuid);
    if (di.Length > 0)
    {
      try
      {
        _usbDevice = new USBDevice(di[0].DevicePath); // just grab the first one for now, but should support multiples
        _logger.LogInformation(string.Format("{0}:{1}", _usbDevice.Descriptor.FullName, _usbDevice.Descriptor.SerialNumber));
        _usbDevice.Interfaces[0].OutPipe.Policy.PipeTransferTimeout = 400;
        //USBDevice.Interfaces[0].InPipe.Policy.PipeTransferTimeout = 600;
        _usbDevice.Interfaces[0].InPipe.Policy.AutoClearStall = true;
        _usbDevice.Interfaces[0].OutPipe.Policy.AutoClearStall = true;
        IsActive = true;
      }
      catch
      {
        _logger.LogInformation("Could not connect to serial USB device");
        return false;
      }
      return true;
    }
    _logger.LogInformation("USB devices not found");
    return false;
  }

  public void Write(byte[] buffer)
  {
    try
    {
      _usbDevice.Interfaces[0].OutPipe.Write(buffer, 0, buffer.Length);
      _retryCounter = 0;
    }
    catch (USBException e)
    {
      _logger.LogInformation($"{e.Message} {e.InnerException}");
      if (!IsActive)
      {
        lock (_disconnectionLock)
        {
          Monitor.Wait(_disconnectionLock);
        }
      }

      if (_retryCounter++ < 5)
      {
        Write(buffer);
        Thread.Sleep(300);
      }
      else
      {
        Disconnect();
      }
    }
  }

  public void BeginRead(AsyncCallback func)
  {
    if (IsActive)
      _ = _usbDevice.Interfaces[0].Pipes[0x81].BeginRead(InputBuffer, 0, InputBuffer.Length, func, null);
  }

  public bool Read()
  {
    if (!IsActive)
    {
      Thread.Sleep(300);// if connection was broken - try again in 300ms
      return false;
    }

    try
    {
      _usbDevice.Interfaces[0].Pipes[0x81].Read(InputBuffer, 0, InputBuffer.Length);
    }
    catch (USBException e)
    {
      _logger.LogInformation($"{e.Message} {e.InnerException}");
      Disconnect();

      lock (_disconnectionLock)
      {
        Monitor.Wait(_disconnectionLock);
      }
      return false;
    }
    return true;
  }

  public void EndRead(IAsyncResult result)
  {
    _ = _usbDevice.Interfaces[0].Pipes[0x81].EndRead(result);
  }

  public void Reconnect()
  {
    if (Init())
    {
      _retryCounter = 0;
      lock (_disconnectionLock)
      {
        Monitor.PulseAll(_disconnectionLock);
      }
    }
  }

  public void Disconnect()
  {
    IsActive = false;
    _usbDevice.Dispose();
  }

  public void ClearBuffer()
  {
    Array.Clear(InputBuffer, 0, InputBuffer.Length);
  }

  #if DEBUG
  private void ListAvailablePipes()
  {
    Console.Error.WriteLine();
    Console.Error.WriteLine("##################################");
    Console.Error.WriteLine("List of Available Pipes:");
    foreach (var pipe in _usbDevice.Interfaces[0].Pipes)
    {
      Console.Error.WriteLine($"\t\t\t0x{Convert.ToString(pipe.Address, 2).PadLeft(8, '0')}");
    }
    Console.Error.WriteLine("##################################");
    Console.Error.WriteLine();
  }
  #endif

  public bool IsBeadInBuffer()
  {
    return InputBuffer[0] == 0xbe && InputBuffer[1] == 0xad;
  }
}