using DIOS.Application.FileIO;
using DIOS.Core;
using Xunit;
using FluentAssertions;

namespace DIOS.Application.Tests
{
  public class BeadEventFileWriterTest
  {
    private static ProcessedBead sut = new ProcessedBead()
    {
      EventTime = 569,
      fsc_bg = 254,
      vssc_bg = 36,
      cl0_bg = 255,
      cl1_bg = 135,
      cl2_bg = 55,
      cl3_bg = 83,
      rssc_bg = 198,
      gssc_bg = 223,
      greenB_bg = 92,
      greenC_bg = 84,
      greenB = 22.581f,
      greenC = 24.948f,
      l_offset_rg = 27,
      l_offset_gv = 10,
      region = 0,
      zone = 0,

      fsc = 2.539f,
      violetssc = 999.9f,
      cl0 = 158.976f,

      redssc = 8114f,
      cl1 = 14.7888f,
      cl2 = 2.945088f,
      cl3 = 4.9f,
      greenssc = 8337f,
      reporter = 24.568f
    };

    [Fact]
    public void Stringify_ShouldRoundUp_WhenFloatValuesAreMoreThanHalf()
    {
      var expected = "569,254,36,255,135,55,83,198,223,92,84,23,25,27,10,0,3,1000,159,8114,15,3,5,8337,24.568";

      string result = BeadEventFileWriter.Stringify(sut);

      result.Should().Be(expected);
    }
  }
}