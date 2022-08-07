using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIOS.Core.SelfTests
{
  internal class SelfTester
  {
    internal SelfTestData Data { get; }
    internal bool[] Motorsinit { get; set; } = { true, true, true };
    internal bool IsActive { get; private set; } = true;

    private Device _device;

    internal SelfTester(Device device)
    {
      _device = device;
      Data = new SelfTestData(device);
    }

    internal void FluidicsTest()
    {
      _device.MainCommand("Get FProperty", code: 0x0C);
      _device.MainCommand("Set Property", code: 0xE0);
      // result to DataController.InnerCommandProcessing(). Probably should form a SelfTestError class
      //SelfTestError should be a property of this class
    }

    internal bool GetResult(out SelfTestData data)
    {
      if (!Data.ResultReady)
      {
        data = null;
        return false;
      }

      data = Data;
      IsActive = false;
      return true;
    }

    internal void ScriptFinishedSignal()
    {
      _device.MainCommand("Get FProperty", code: 0x0C); //Pressure
      Motorsinit[2] = false;
      _device.MainCommand("Get FProperty", code: 0x44); //Z Motor
      Motorsinit[0] = false;
      _device.MainCommand("Get FProperty", code: 0x54); //X Motor
      Motorsinit[1] = false;
      _device.MainCommand("Get FProperty", code: 0x64); //Y Motor
    }
  }
}