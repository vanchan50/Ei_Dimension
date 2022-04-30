using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIOS.Core.SelfTests
{
  internal class SelfTester
  {
    internal SelfTestData Data { get; } = new SelfTestData();

    private Device _device;

    internal SelfTester(Device device)
    {
      _device = device;
    }

    internal void FluidicsTest()
    {
      _device.MainCommand("Get FProperty", code: 0x0C);
      _device.MainCommand("Set Property", code: 0xE0);
      // result to DataController.InnerCommandProcessing(). Probably should form a SelfTestError class
      //SelfTestError should be a property of this class
    }

    internal bool GetResult(out string message)
    {
      if (!Data.ResultReady)
      {
        message = null;
        return false;
      }

      message = Data.ResultMessage;
      return true;
    }

    internal void ScriptFinishedSignal(byte code)
    {
      switch (code)
      {
        case 0xE0:
          _device.MainCommand("Get FProperty", code: 0x0C); //Pressure
          _device.MainCommand("Get FProperty", code: 0x44); //Z Motor
          _device.MainCommand("Get FProperty", code: 0x54); //X Motor
          _device.MainCommand("Get FProperty", code: 0x64); //Y Motor
          break;
      }
    }
  }
}