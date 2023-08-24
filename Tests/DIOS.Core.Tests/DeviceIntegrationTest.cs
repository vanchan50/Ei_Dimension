namespace DIOS.Core.Tests;

public class DeviceIntegrationTest
{
  [Fact]
  public void Test1()
  {
    var fakeUSB = new FakeUSBConnection();
    Device device = new Device(fakeUSB, null);
    //DiosApp.MapController.SetMap(DiosApp.MapController.MapList[1]);
    device._wellController.Init(new List<Well>{ new(1, 1) });
    device.Normalization.Enable();
    device.StartOperation(null,null);
    //fakeUSB.ReadBead(new RawBead
    //{
    //  Header = 0xadbeadbe,
    //  fsc = 2.36f,
    //  cl1 = 13400.1632f
    //});

    Thread.Sleep(1000);
    //var t = device.DataOut;
  }
}