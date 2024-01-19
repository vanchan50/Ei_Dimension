using MadWizard.WinUSBNet;

namespace SerialService;
public class USBWatchdog
{
  private USBNotifier usbnotif;
  private Action _connect;
  private Action _disConnect;
  private bool _done;

  public USBWatchdog()
  {
    
  }
  public USBWatchdog(IntPtr mainWindowHandle, Action connectAction, Action disconnectAction)
  {
    if (_done)
      throw new Exception("Setup must be called only once");
    _done = true;

    usbnotif = new USBNotifier(mainWindowHandle, Guid.ParseExact("F70242C7-FB25-443B-9E7E-A4260F373982", "D"));
    usbnotif.Arrival += Usbnotif_Arrival;
    usbnotif.Removal += Usbnotif_Removal;
    _connect = connectAction;
    _disConnect = disconnectAction;
  }

  private void Usbnotif_Removal(object sender, USBEvent e)
  {
    _connect();
  }

  private void Usbnotif_Arrival(object sender, USBEvent e)
  {
    _disConnect();
  }
}