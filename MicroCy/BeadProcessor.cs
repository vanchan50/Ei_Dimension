namespace DIOS.Core
{
  internal class BeadProcessor
  {
    public NormalizationSettings Normalization { get; } = new NormalizationSettings();
    private float _greenMin;
    private float _greenMaj;
    private readonly Device _device;
    private readonly ClassificationMap _classificationMap = new ClassificationMap();
    internal CustomMap _map;

    public BeadProcessor(Device device)
    {
      _device = device;
    }

    public void SetMap(CustomMap map)
    {
      _map = map;
      _classificationMap.ConstructClassificationMap(_map);
    }

    public ProcessedBead CalculateBeadParams(in RawBead rawBead)
    {
      //The order of operations matters here
      AssignSensitivityChannels(in rawBead);
      var compensated = CalculateCompensatedCoordinates(in rawBead);
      var reg = (ushort) _classificationMap.ClassifyBeadToRegion(in rawBead);
      var rep = CalculateReporter(reg);
      var zon = (ushort) ClassifyBeadToZone(compensated.cl0);
      var outBead = new ProcessedBead
      {
        EventTime = rawBead.EventTime,
        fsc_bg = rawBead.fsc_bg,
        vssc_bg = rawBead.vssc_bg,
        cl0_bg = rawBead.cl0_bg,
        cl1_bg = rawBead.cl1_bg,
        cl2_bg = rawBead.cl2_bg,
        cl3_bg = rawBead.cl3_bg,
        rssc_bg = rawBead.rssc_bg,
        gssc_bg = rawBead.gssc_bg,
        greenB_bg = rawBead.greenB_bg,
        greenC_bg = rawBead.greenC_bg,
        greenB = rawBead.greenB,
        greenC = rawBead.greenC,
        l_offset_rg = rawBead.l_offset_rg,
        l_offset_gv = rawBead.l_offset_gv,
        region = reg,
        //fsc = (float)Math.Pow(10, rawBead.fsc),
        fsc = rawBead.fsc,
        violetssc = rawBead.violetssc,
        cl0 = compensated.cl0,
        redssc = rawBead.redssc,
        cl1 = compensated.cl1,
        cl2 = compensated.cl2,
        cl3 = compensated.cl3,
        greenssc = rawBead.greenssc,
        reporter = rep,
        zone = zon
      };
      return outBead;
    }

    private void AssignSensitivityChannels(in RawBead rawBead)
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

    private (float cl0, float cl1, float cl2, float cl3) CalculateCompensatedCoordinates(in RawBead outbead)
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

    private float CalculateReporter(ushort region)
    {
      var basicReporter = _greenMin > _device.HdnrTrans ? _greenMaj * _device.HDnrCoef : _greenMin;
      var scaledReporter = (basicReporter / _device.ReporterScaling);
      if (!Normalization.IsEnabled || region == 0)
        return scaledReporter;
      var rep = _map.GetFactorizedNormalizationForRegion(region);
      scaledReporter -= rep;
      if (scaledReporter < 0)
        return 0;
      return scaledReporter;
    }

    private int ClassifyBeadToZone(float cl0)
    {
      if (!_map.CL0ZonesEnabled)
        return 0;
      //for the sake of robustness. Going from right to left;
      //checks if the value is higher than zone's left boundary.
      //if yes, no need to check other zones
      //check if it falls into the right boundary. else out of any zone
      for (var i = _map.zones.Count - 1; i < 0; i--)
      {
        var zone = _map.zones[i];
        if (cl0 >= zone.Start)
        {
          if (cl0 <= zone.End)
            return zone.Number;
          return 0;
        }
      }
      return 0;
    }
  }
}