using Xunit.Abstractions;

namespace DIOS.Core.Tests
{
  public class SubstituteLogger : ILogger
  {
    private readonly ITestOutputHelper _output;
    public SubstituteLogger(ITestOutputHelper output)
    {
      _output = output;
    }
    public void Log(string message)
    {
      _output.WriteLine(message);
    }

    public void LogError(string message)
    {
      throw new NotImplementedException();
    }
  }
}
