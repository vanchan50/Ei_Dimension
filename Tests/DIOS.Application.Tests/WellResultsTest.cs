using DIOS.Core;
using Xunit.Abstractions;

namespace DIOS.Application.Tests
{
  public class WellResultsTest
  {
    private WellResults SUT = new WellResults();
    private readonly ITestOutputHelper output;

    public WellResultsTest(ITestOutputHelper output)
    {
      //SUT.BeadEventsData
      this.output = output;
    }

    [Fact]
    public void GetNewBeads_ShouldReturnNothing_WhenThereAreNoBeads()
    {
      var enumerator = SUT.GetNewBeads();

      CountEntriesInEnumerator(enumerator).Should().Be(0);
    }

    [Fact]
    public void GetNewBeads_ShouldReturnTwo_WhenThereAreTwoBeads()
    {
      SUT.Add(new ProcessedBead { region = 1 });
      SUT.Add(new ProcessedBead { region = 2 });
      var enumerator = SUT.GetNewBeads();

      var i = 0;
      foreach (var processedBead in SUT.GetNewBeads())
      {
        i++;
      }

      i.Should().Be(2);
      //foreach (var processedBead in enumerator)
      //{
      //  output.WriteLine(processedBead.region.ToString());
      //  SUT.Add(new ProcessedBead { region = 2 });
      //}
      //r.Should().Be(i);
    }

    private static int CountEntriesInEnumerator(IEnumerable<ProcessedBead> enumerator)
    {
      var i = 0;
      foreach (var processedBead in enumerator)
      {
        i++;
      }
      return i;
    }
  }
}
