namespace DIOS.Core;

internal class BeadProcessor
{
  public NormalizationSettings Normalization { get; } = new();
  private float _greenMin;
  private float _greenMaj;
  private readonly Device _device;
  private readonly ClassificationMap _classificationMap = new();
  internal MapModel _map;
  internal HiSensitivityChannel SensitivityChannel { get; set; }
  internal bool _channelRedirectionEnabled = false;
  private float[] _compensatedCoordinatesCache = { 0,0,0,0 }; //cl0,cl1,cl2,cl3
  internal float _inverseReporterScaling;
  internal float _actualCompensation;

  public BeadProcessor(Device device)
  {
    _device = device;
  }

  public void SetMap(MapModel map)
  {
    _map = map;
    _classificationMap.ConstructClassificationMap(_map);
  }

  public ProcessedBead CalculateBeadParams(in RawBead rawBead)
  {
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
      //fsc = (float)Math.Pow(10, rawBead.fsc),
      fsc = rawBead.fsc,
      violetssc = rawBead.violetssc,
      redssc = rawBead.redssc,
      greenssc = rawBead.greenssc
    };

    //The order of operations matters here

    if (_channelRedirectionEnabled)
    {
      //OEM case
      outBead.cl1_bg = rawBead.greenB_bg;
      outBead.cl2_bg = rawBead.greenC_bg;
      outBead.greenB_bg = rawBead.cl1_bg;
      outBead.greenC_bg = rawBead.cl2_bg;
      outBead.greenB = rawBead.cl1;
      outBead.greenC = rawBead.cl2;
      //red channels are processed, and are written in the finalization part

      ChannelRedirection(in rawBead);
    }
    else
    {
      //normal case
      CalculateCompensatedCoordinates(in rawBead);
    }
    //finalize outBead data
    outBead.cl0 = _compensatedCoordinatesCache[0];
    outBead.cl1 = _compensatedCoordinatesCache[1];
    outBead.cl2 = _compensatedCoordinatesCache[2];
    outBead.cl3 = _compensatedCoordinatesCache[3];

    var reg = (ushort) _classificationMap.ClassifyBeadToRegion(in outBead);
    var rep = CalculateReporter(reg);
    var zon = (ushort) ClassifyBeadToZone(outBead.cl0);

    //finalize outBead data
    outBead.region = reg;
    outBead.reporter = rep;
    outBead.zone = zon;

    return outBead;
  } //reporter is the last calculated thing

  private void AssignSensitivityChannels(in RawBead rawBead)
  {
    //greenMaj is the hi dyn range channel,
    //greenMin is the high sensitivity channel(depends on filter placement)
      
    if (SensitivityChannel == HiSensitivityChannel.GreenB)
    {
      _greenMaj = rawBead.greenC;
      _greenMin = rawBead.greenB;
      return;
    }
    _greenMaj = rawBead.greenB;
    _greenMin = rawBead.greenC;
  }

  private void CalculateCompensatedCoordinates(in RawBead rawBead)
  {
    //Compensation exists for the cases when in normal config, the broad reporter freq band extends into the CL1 and CL2 band
    //we subtract a fraction of the reporter signal from CL1 and CL2,
    //so really bright reporter signals don’t push the cl1 and cl2 coordinates out of the region
    //there is about 1% excitation of the reporter fluorophore by the red laser and that causes the shift
    AssignSensitivityChannels(in rawBead);
    var cl1Comp = _greenMaj * _actualCompensation;  //also called adjust
    var cl2Comp = cl1Comp * 0.25f;

    var compensatedCl1 = rawBead.cl1 - cl1Comp;
    var compensatedCl2 = rawBead.cl2 - cl2Comp;

    //Thread unsafe
    _compensatedCoordinatesCache[0] = rawBead.cl0;
    _compensatedCoordinatesCache[1] = compensatedCl1;
    _compensatedCoordinatesCache[2] = compensatedCl2;
    _compensatedCoordinatesCache[3] = rawBead.cl3;
  }

  private void ChannelRedirection(in RawBead rawBead)
  {
    //greenMaj is the hi dyn range channel,
    //greenMin is the high sensitivity channel(depends on filter placement)

    if (SensitivityChannel == HiSensitivityChannel.GreenB)
    {
      _greenMaj = rawBead.cl2;
      _greenMin = rawBead.cl1;
      return;
    }
    _greenMaj = rawBead.cl1;
    _greenMin = rawBead.cl2;


    var cl1Comp = _greenMaj * _actualCompensation;  //also called adjust
    var cl2Comp = cl1Comp * 0.25f;
    
    var compensatedCl1 = rawBead.greenB - cl1Comp;
    var compensatedCl2 = rawBead.greenC - cl2Comp;

    //Thread unsafe
    _compensatedCoordinatesCache[0] = rawBead.cl0;
    _compensatedCoordinatesCache[1] = compensatedCl1;
    _compensatedCoordinatesCache[2] = compensatedCl2;
    _compensatedCoordinatesCache[3] = rawBead.cl3;
  }

  private float CalculateReporter(ushort region)
  {
    var basicReporter = _greenMin > _device.HdnrTrans ? _greenMaj * _device.HDnrCoef : _greenMin;
    var scaledReporter = (basicReporter * _inverseReporterScaling);
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