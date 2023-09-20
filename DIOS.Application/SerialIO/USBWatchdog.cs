using MadWizard.WinUSBNet;

namespace DIOS.Application.SerialIO
{
  public static class USBWatchdog
  {
    private static USBNotifier usbnotif;
    private static Action _connect;
    private static Action _disConnect;
    private static bool _done;

    public static void Setup(IntPtr mainWindowHandle, Action connectAction, Action disconnectAction)
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

    private static void Usbnotif_Removal(object sender, USBEvent e)
    {
      _connect();
    }

    private static void Usbnotif_Arrival(object sender, USBEvent e)
    {
      _disConnect();
    }
  }
}
