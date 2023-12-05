using DIOS.Core.HardwareIntercom;
using Xunit.Abstractions;

namespace DIOS.Core.Tests
{
  public class DataControllerTest
  {
    private ILogger _logger;
    private ISerial _serial;
    private Device SUT;

    public DataControllerTest(ITestOutputHelper output)
    {
      _serial = new FakeUSBConnection();
      _logger = new SubstituteLogger(output);
      SUT = new Device(_serial, _logger);
    }

    [Fact]
    public void DEBUGGetCommandFromBufferTest()
    {

      using var monitoredSubject = SUT.Monitor();
      var testCs0 = new CommandStruct
      {
        Code = 0xCC,
        Parameter = 12
      };
      SUT.DEBUGCommandTest(testCs0);
      monitoredSubject.Should().Raise(nameof(Device.ParameterUpdate)).
        WithArgs<ParameterUpdateEventArgs>(
          e=> e.Type == DeviceParameterType.SystemActivityStatus &&
              e.Parameter == testCs0.Parameter);
    }

  }
}
