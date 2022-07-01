namespace DIOS.Core
{
  internal class BeadProcessor
  {
    public NormalizationSettings Normalization { get; } = new NormalizationSettings();
    private float _greenMin;
    private float _greenMaj;
    private readonly Device _device;
    private readonly ClassificationMap _classificationMap = new ClassificationMap();
    private CustomMap _map;

    public BeadProcessor(Device device)
    {
      _device = device;
    }

    public void SetMap(CustomMap map)
    {
      _map = map;
      _classificationMap.ConstructClassificationMap(_map);
    }

    public void CalculateBeadParams(ref BeadInfoStruct rawBead)
    {
      //The order of operations matters here
      AssignChannels(in rawBead);
      var compensated = CalculateCompensatedCoordinates(in rawBead);
      rawBead.cl1 = compensated.cl1;
      rawBead.cl2 = compensated.cl2;
      //outbead.fsc = (float)Math.Pow(10, outbead.fsc);
      rawBead.region = (ushort)_classificationMap.ClassifyBeadToRegion(in rawBead);
      rawBead.reporter = CalculateReporter(in rawBead);
    }

    private void AssignChannels(in BeadInfoStruct rawBead)
    {
      //greenMaj is the hi dyn range channel,
      //greenMin is the high sensitivity channel(depends on filter placement)
      if (_device.SensitivityChannel == HiSensitivityChannel.GreenB)
      {
        _greenMaj = rawBead.greenC;
        _greenMin = rawBead.greenB;
        return;
      }
      _greenMaj = rawBead.greenB;
      _greenMin = rawBead.greenC;
    }

    private (float cl0, float cl1, float cl2, float cl3) CalculateCompensatedCoordinates(in BeadInfoStruct outbead)
    {
      var cl1comp = _greenMaj * _device.Compensation / 100;
      var cl2comp = cl1comp * 0.26f;
      return (
        outbead.cl0,
        outbead.cl1 - cl1comp,  //Compensation
        outbead.cl2 - cl2comp,  //Compensation
        outbead.cl3
      );
    }

    private float CalculateReporter(in BeadInfoStruct rawBead)
    {
      var basicReporter = _greenMin > _device.HdnrTrans ? _greenMaj * _device.HDnrCoef : _greenMin;
      var scaledReporter = (basicReporter / _device.ReporterScaling);
      if (!Normalization.IsEnabled || rawBead.region == 0)
        return scaledReporter;
      var rep = _map.GetFactorizedNormalizationForRegion(rawBead.region);
      scaledReporter -= rep;
      if (scaledReporter < 0)
        return 0;
      return scaledReporter;
    }
  }
}