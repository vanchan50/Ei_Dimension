using System.Text;
using Xunit.Abstractions;

namespace DIOS.Core.Tests;

public class SystemActivityTest
{
  readonly string[] SyncElements = { "SHEATH", "SAMPLE_A", "SAMPLE_B", "FLASH", "END_WELL", "VALVES", "X_MOTOR",
    "Y_MOTOR", "Z_MOTOR", "WASH PUMP", "PRESSURE", "WASHING", "FAULT", "ALIGN MOTOR", "MAIN VALVE", "SINGLE STEP" };
  public SystemActivity SUT = new();

  public SystemActivityTest(ITestOutputHelper output)
  {
  }

  [Theory]
  [InlineData(0b0000000010000000)]
  [InlineData(0b0000000010000101)]
  [InlineData(0b1000000010000101)]
  [InlineData(0b1111111111111111)]
  [InlineData(0b0000000011111111)]
  [InlineData(0b1110111010100101)]
  public void DecodeMessageTest(ushort message)
  {
    var expected = ExpectedString(message);
    var result = SUT.DecodeMessage(message);
    result.Should().Be(expected);
  }

  private string ExpectedString(ushort message)
  {
    var _sb = new StringBuilder()
      .Append("System Monitor: ");
    for (var i = 0; i < 16; i++)
    {
      if ((message & (1 << i)) is not 0)
      {
        _sb.Append($"{SyncElements[i]}, ");
      }
    }
    _sb.Remove(_sb.Length - 2, 2);
    return _sb.ToString();
  }
}