using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using DIOS.Core;

namespace DIOS.Core.Tests
{
  public class DeviceIntegrationTest
  {
    [Fact]
    public void Test1()
    {
      var fakeUSB = new FakeUSBConenction();
      Device device = new Device(fakeUSB);
      device.MapCtroller.ActiveMap = device.MapCtroller.MapList[1];
      device.WellController.Init(new List<Well>{new Well{ RowIdx = 1, ColIdx = 1 }});
      device.StartOperation();
      fakeUSB.ReadBead(new BeadInfoStruct
      {
        Header = 0xadbeadbe,
        fsc = 2.36f
      });
      Thread.Sleep(1000);
      var t = device.DataOut;
    }
  }
}
