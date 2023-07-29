using DIOS.Core;
using Xunit.Abstractions;

namespace DIOS.Application.Tests
{
  public class RunResultsTest
  {
    private static readonly ILogger _logger = new FakeLogger();
    private readonly ITestOutputHelper _output;

    public RunResultsTest(ITestOutputHelper output)
    {
      _output = output;
    }

    [Fact]
    public void BeadDequeueTest()
    {
      //Setup
      var device = new Device(null, _logger);
      device.Mode = OperationMode.Normal;
      var drives = DriveInfo.GetDrives();
      var appFolder = Path.Combine($"{drives[0].Name}", "Emissioninc", Environment.MachineName);
      var SUT = new RunResults(device, new DIOSApp(appFolder, _logger));

      var putList = new List<ProcessedBead>(200);
      var takeList = new List<ProcessedBead>(200);
      //Action
      Task.Run(() =>
      {
        var r = new Random();
        while (SUT.CurrentWellResults.Count < 200)
        {
          var bead = new ProcessedBead
          {
            region = r.Next(0,101)
          };
          SUT.AddProcessedBeadEvent(bead);
          putList.Add(bead);
          Thread.Sleep(r.Next(0,100));
        }
      });

      while (takeList.Count < 200)
      {
        foreach (var bead in SUT.GetNewBeads())
        {
          takeList.Add(bead);
        }
      }

      for (var i = 0; i < 200; i++)
      {
        var take = takeList[i];
        var put = putList[i];
        take.Should().Be(put);
        _output.WriteLine($"{take.region} : {put.region}");
      }

    }

    private class FakeLogger : ILogger
    {
      public void Log(string message)
      {
      }

      public void LogError(string message)
      {
        throw new NotImplementedException();
      }
    }

  }
}