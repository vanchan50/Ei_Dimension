namespace DIOS.Core.Tests;

public class BeadProcessorTest
{
  private Device _device;
  private BeadProcessor _bProcessor;

  public BeadProcessorTest()
  {
    _device = new Device(null, null);
    _device.Compensation = 1;
    _device.HdnrTrans = 1;
    _device.HDnrCoef = 1;
    _device.ReporterScaling = 1;
    _bProcessor = new BeadProcessor(_device);
    var map = new MapModel();
    map.regions = new List<MapRegion>();
    map.Init();
    _bProcessor.SetMap(map);
  }

  [Fact]
  public void CalculateBeadParamsTest()
  {
    var expected = new ProcessedBead
    {
      EventTime = 143,
      fsc_bg = 222,
      vssc_bg = 123,
      cl0_bg = 1,
      cl1_bg = 2,
      cl2_bg = 3,
      cl3_bg = 4,
      rssc_bg = 5,
      gssc_bg = 6,
      greenB_bg = 7,
      greenC_bg = 8,
      l_offset_rg = 9,
      l_offset_gv = 10,
      fsc = 145.94f,
      violetssc = 13.5f,
      redssc = 935.874f,
      greenssc = 1318.720f,
    };
    var input = new RawBead
    {
      EventTime = expected.EventTime,
      fsc_bg = expected.fsc_bg,
      vssc_bg = expected.vssc_bg,
      cl0_bg = expected.cl0_bg,
      //cl1_bg = expected.cl1_bg,
      //cl2_bg = expected.cl2_bg,
      cl3_bg = expected.cl3_bg,
      rssc_bg = expected.rssc_bg,
      gssc_bg = expected.gssc_bg,
      //greenB_bg = expected.greenB_bg,
      //greenC_bg = expected.greenC_bg,
      l_offset_rg = expected.l_offset_rg,
      l_offset_gv = expected.l_offset_gv,
      fsc = expected.fsc,
      violetssc = expected.violetssc,
      redssc = expected.redssc,
      greenssc = expected.greenssc
    };

    var result = _bProcessor.CalculateBeadParams(in input);

    Assert.Equal(expected, result);
  }
}