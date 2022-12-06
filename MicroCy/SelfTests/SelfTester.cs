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
      _device.RequestHardwareParameter(DeviceParameterType.PressureAtStartup);
      _device.SendHardwareCommand(DeviceCommandType.Startup);
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
      _device.RequestHardwareParameter(DeviceParameterType.PressureAtStartup);
      Motorsinit[2] = false;
      _device.RequestHardwareParameter(DeviceParameterType.MotorZ, MotorParameterType.CurrentStep);
      Motorsinit[0] = false;
      _device.RequestHardwareParameter(DeviceParameterType.MotorX, MotorParameterType.CurrentStep);
      Motorsinit[1] = false;
      _device.RequestHardwareParameter(DeviceParameterType.MotorY, MotorParameterType.CurrentStep);
    }
  }
}