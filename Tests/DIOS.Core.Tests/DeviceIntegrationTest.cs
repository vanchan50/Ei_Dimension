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
      var fakeUSB = new FakeUSBConnection();
      Device device = new Device(fakeUSB);
      device.MapCtroller.SetMap(device.MapCtroller.MapList[1]);
      device.WellController.Init(new List<Well>{new Well{ RowIdx = 1, ColIdx = 1 }});
      device.Normalization.Enable();
      device.StartOperation();
      //fakeUSB.ReadBead(new RawBead
      //{
      //  Header = 0xadbeadbe,
      //  fsc = 2.36f,
      //  cl1 = 13400.1632f
      //});

      Thread.Sleep(1000);
      var t = device.DataOut;
    }
  }
}