using DIOS.Core;
using Xunit.Abstractions;

namespace DIOS.Application.Tests;

public class BeadEventSinkTest
{
  private readonly BeadEventSink SUT = new(2000000);
  private readonly Random _r = new();
  private readonly ITestOutputHelper _output;

  public BeadEventSinkTest(ITestOutputHelper output)
  {
    _output = output;
  }

  [Theory]
  [InlineData(0, 0)]
  [InlineData(1, 1)]
  [InlineData(2, 2)]
  [InlineData(141, 141)]
  [InlineData(10001, 10001)]
  public void GetNewBeadsEnumerable_ShouldReturnAllBeads(int amount, int expected)
  {
    for (var i = 0; i < amount; i++)
    {
      SUT.Add(new ProcessedBead { region = _r.Next(0,101) });
    }

    var enumerator = SUT.GetNewBeadsEnumerable();

    CountEntriesInEnumerator(enumerator).Should().Be(expected);
  }

  [Fact]
  public void GetNewBeadsEnumerable_Should_WhenMultithreaded()
  {
    int amount = 0;
    Task.Run(() =>
    {
      while (true)
      {
        SUT.Add(new ProcessedBead { region = _r.Next(0, 101) });
        amount++;
        Thread.Sleep(_r.Next(0, 10));
      }
    });

    int receivedAmount = 0;
    while (true)
    {
      var enumerator = SUT.GetNewBeadsEnumerable();
      receivedAmount += CountEntriesInEnumerator(enumerator);
      Thread.Sleep(_r.Next(0, 2));
      _output.WriteLine(receivedAmount.ToString());
      if (receivedAmount > 200)
        break;
    }
  }

  [Fact]
  public void GetNewBeadsEnumerable_ShouldNotThrow_WhenMultithreaded_Reset()
  {
    int amount = 1000;
    for (var i = 0; i < amount; i++)
    {
      SUT.Add(new ProcessedBead { region = _r.Next(0, 101) });
    }

    Task.Run(() =>
    {
      Thread.Sleep(500);
      SUT.Clear();
    });

    int receivedAmount = 0;
    while (true)
    {
      foreach (var bead in SUT.GetNewBeadsEnumerable())
      {
        receivedAmount ++;
        Thread.Sleep(_r.Next(0, 15));
      }
      _output.WriteLine(receivedAmount.ToString());
      if (receivedAmount > 900)
        break;
    }
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