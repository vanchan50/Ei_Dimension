using DIOS.Core;

namespace DIOS.Application.Tests;

public class MapControllerTest
{
  private MapController SUT = new MapController($"{Path.Combine(@"C:\Emissioninc", Environment.MachineName)}\\Config", new Fakelogger());

  [Fact]
  public void GetMapIndexByName_ShouldReturnProperIndex_WhenMapNameExists()
  {
    for (var i = 0; i < SUT.MapList.Count; i++)
    {
      var r = SUT.GetMapIndexByName(SUT.MapList[i].mapName);

      r.Should().Be(i);
    }
  }

  [Fact]
  public void GetMapIndexByName_ShouldReturnNegative_WhenMapNameDoesntExist()
  {
    var result = SUT.GetMapIndexByName("INEXISTENTMAPNAME");

    result.Should().Be(-1);
  }

  private class Fakelogger : ILogger
  {
    public void Log(string message)
    {
    }

    public void LogError(string message)
    {
    }
  }
}