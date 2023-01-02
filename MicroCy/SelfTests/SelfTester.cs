using DIOS.Core.HardwareIntercom;

namespace DIOS.Core.SelfTests
{
  internal class SelfTester
  {
    internal SelfTestData Data { get; }
    internal bool[] Motorsinit { get; set; } = { true, true, true };
    internal bool IsActive { get; private set; } = true;

    private Device _device;
    private ILogger _logger;

    internal SelfTester(Device device, ILogger logger)
    {
      _device = device;
      Data = new SelfTestData(device, logger);
      _logger = logger;
    }

    internal void FluidicsTest()
    {
      _device.Hardware.RequestParameter(DeviceParameterType.PressureAtStartup);
      _device.Hardware.SendCommand(DeviceCommandType.Startup);
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
      _device.Hardware.RequestParameter(DeviceParameterType.PressureAtStartup);
      Motorsinit[2] = false;
      _device.Hardware.RequestParameter(DeviceParameterType.MotorZ, MotorParameterType.CurrentStep);
      Motorsinit[0] = false;
      _device.Hardware.RequestParameter(DeviceParameterType.MotorX, MotorParameterType.CurrentStep);
      Motorsinit[1] = false;
      _device.Hardware.RequestParameter(DeviceParameterType.MotorY, MotorParameterType.CurrentStep);
    }
  }
}