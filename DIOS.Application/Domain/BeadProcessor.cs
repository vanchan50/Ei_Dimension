﻿using DIOS.Core;

namespace DIOS.Application.Domain;

public class BeadProcessor
{
  public delegate ProcessedBead ProcessBead(ref RawBead rawBead);
  public delegate void PreProcessBead(ref RawBead rawBead);
  public ProcessBead CalculateBeadParams { get; private set; }
  public NormalizationSettings Normalization { get; } = new();
  private float _greenMin;
  private float _greenMaj;
  public readonly ClassificationMap _classificationMap = new();
  internal MapModel _map;
  public HiSensitivityChannel SensitivityChannel { get; set; }
  public bool SpectraPlexEnabled
  {
    get => _spectraPlexEnabled;
    set
    {
      _spectraPlexEnabled = value;
      CalculateBeadParams = _spectraPlexEnabled ? CalculateBeadParamsSpectraPlex : CalculateBeadParamsRegular;
    }
  }
  public bool BeadCompensationEnabled
  {
    get => _beadCompensationEnabled;
    set
    {
      _beadCompensationEnabled = value;
      if (_spectraPlexEnabled)
      {
        CompensateBeadParams = _beadCompensationEnabled ? PrecalculateBeadCompensationSpectraPlex : EmptyFunc;
        return;
      }
      CompensateBeadParams = _beadCompensationEnabled ? PrecalculateBeadCompensationRegular : EmptyFunc;
    }
  }

  private PreProcessBead CompensateBeadParams;
  public bool _channelRedirectionEnabled = false;
  public float HdnrTrans;
  public float HDnrCoef;
  private readonly float[] _compensatedCoordinatesCache = { 0,0,0,0 }; //cl0,cl1,cl2,cl3
  internal float _inverseReporterScaling;
  internal float _actualCompensation;

  private bool _spectraPlexEnabled;
  private bool _beadCompensationEnabled;

  public BeadProcessor()
  {
    CalculateBeadParams = CalculateBeadParamsRegular;
    CompensateBeadParams = EmptyFunc;
  }

  public void SetMap(MapModel map)
  {
    _map = map;
    _classificationMap.ConstructClassificationMap(_map, _channelRedirectionEnabled);
  }

  public void UpdateClassificationParameters(string param1, string param2)
  {
    _classificationMap.ChooseProperClassification(param1, param2, _channelRedirectionEnabled);
  }

  private void PrecalculateBeadCompensationRegular(ref RawBead rawBead)
  {
    rawBead.greenC -= (ushort)(_map.CMatrix.GreenB1 * rawBead.greenB);
    rawBead.greenD -=          _map.CMatrix.GreenB2 * rawBead.greenB;
    rawBead.cl1 -=             _map.CMatrix.GreenB3 * rawBead.greenB;
    rawBead.cl2 -=             _map.CMatrix.GreenB4 * rawBead.greenB;
    rawBead.redA -=            _map.CMatrix.GreenB5 * rawBead.greenB;

    rawBead.greenD -=          _map.CMatrix.GreenC1 * rawBead.greenC;
    rawBead.cl1 -=             _map.CMatrix.GreenC2 * rawBead.greenC;
    rawBead.cl2 -=             _map.CMatrix.GreenC3 * rawBead.greenC;
    rawBead.redA -=            _map.CMatrix.GreenC4 * rawBead.greenC;

    rawBead.cl1 -=             _map.CMatrix.GreenD1 * rawBead.greenD;
    rawBead.cl2 -=             _map.CMatrix.GreenD2 * rawBead.greenD;
    rawBead.redA -=            _map.CMatrix.GreenD3 * rawBead.greenD;

    rawBead.cl2 -=             _map.CMatrix.RedC1   * rawBead.cl1;
    rawBead.redA -=            _map.CMatrix.RedC2   * rawBead.cl1;

    rawBead.redA -=            _map.CMatrix.RedD1   * rawBead.cl2;
  }

  private void PrecalculateBeadCompensationSpectraPlex(ref RawBead rawBead)
  {
    rawBead.greenC -= (ushort)(_map.CMatrix.GreenB1 * rawBead.greenB);
    rawBead.greenD -= _map.CMatrix.GreenB2 * rawBead.greenB;
    rawBead.cl1 -= _map.CMatrix.GreenB3 * rawBead.greenB;
    rawBead.cl2 -= _map.CMatrix.GreenB4 * rawBead.greenB;
    rawBead.redA -= _map.CMatrix.GreenB5 * rawBead.greenB;

    rawBead.greenD -= _map.CMatrix.GreenC1 * rawBead.greenC;
    rawBead.cl1 -= _map.CMatrix.GreenC2 * rawBead.greenC;
    rawBead.cl2 -= _map.CMatrix.GreenC3 * rawBead.greenC;
    rawBead.redA -= _map.CMatrix.GreenC4 * rawBead.greenC;

    rawBead.cl1 -= _map.CMatrix.GreenD1 * rawBead.greenD;
    rawBead.cl2 -= _map.CMatrix.GreenD2 * rawBead.greenD;
    rawBead.redA -= _map.CMatrix.GreenD3 * rawBead.greenD;

    rawBead.cl2 -= _map.CMatrix.RedC1 * rawBead.cl1;
    rawBead.redA -= _map.CMatrix.RedC2 * rawBead.cl1;

    rawBead.redA -= _map.CMatrix.RedD1 * rawBead.cl2;
  }

  private ProcessedBead CalculateBeadParamsRegular(ref RawBead rawBead)
  {
    CompensateBeadParams.Invoke(ref rawBead);
    var outBead = new ProcessedBead
    {
      EventTime = rawBead.EventTime,
      fsc_bg = 0,
      vssc_bg = 0,
      greenD_bg = rawBead.greenD_bg,
      cl1_bg = rawBead.cl1_bg,
      cl2_bg = rawBead.cl2_bg,
      redA_bg = rawBead.redA_bg,
      rssc_bg = rawBead.rssc_bg,
      gssc_bg = rawBead.gssc_bg,
      greenB_bg = rawBead.greenB_bg,
      greenC_bg = rawBead.greenC_bg,
      greenB = rawBead.greenB,
      greenC = rawBead.greenC,
      l_offset_rg = rawBead.l_offset_rg,
      l_offset_gv = 0,
      ratio1 = rawBead.fsc,
      ratio2 = rawBead.violetssc,
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
    outBead.greenD = _compensatedCoordinatesCache[0];
    outBead.cl1 = _compensatedCoordinatesCache[1];
    outBead.cl2 = _compensatedCoordinatesCache[2];
    outBead.redA = _compensatedCoordinatesCache[3];
    var reg = (ushort)_classificationMap.ClassifyBeadToRegion(in outBead);
    var rep = CalculateReporter(reg);
    var zon = (ushort) ClassifyBeadToZone(outBead.greenD);
    outBead.ratio1 = 0;
    outBead.ratio2 = 0;

    //finalize outBead data
    outBead.region = reg;
    outBead.reporter = rep;
    outBead.zone = zon;

    return outBead;
  } //reporter is the last calculated thing

  private ProcessedBead CalculateBeadParamsSpectraPlex(ref RawBead rawBead)
  {
    CompensateBeadParams.Invoke(ref rawBead);
    var outBead = new ProcessedBead
    {
      EventTime = rawBead.EventTime,
      fsc_bg = 0,
      vssc_bg = 0,
      greenD_bg = rawBead.greenD_bg,
      cl1_bg = rawBead.cl1_bg,
      cl2_bg = rawBead.cl2_bg,
      redA_bg = rawBead.redA_bg,
      rssc_bg = rawBead.rssc_bg,
      gssc_bg = rawBead.gssc_bg,
      greenB_bg = rawBead.greenB_bg,
      greenC_bg = rawBead.greenC_bg,
      greenB = rawBead.greenB,
      greenC = rawBead.greenC,
      l_offset_rg = rawBead.l_offset_rg,
      l_offset_gv = 0,
      ratio1 = rawBead.fsc,
      ratio2 = rawBead.violetssc,
      greenD = rawBead.greenD,
      redssc = rawBead.redssc,
      redA = rawBead.redA,
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
      outBead.cl1 = rawBead.greenB;
      outBead.cl2 = rawBead.greenC;
      outBead.greenB = rawBead.cl1;
      outBead.greenC = rawBead.cl2;
      //red channels are processed, and are written in the finalization part

      //ChannelRedirection(in rawBead);
    }
    else
    {
      //normal case
      //CalculateCompensatedCoordinates(in rawBead);
      outBead.cl1 = rawBead.cl1;
      outBead.cl2 = rawBead.cl2;
    }
    //finalize outBead data
    var reg = (ushort)_classificationMap.ClassifyBeadToRegion(in outBead);
    var rep = CalculateSpectraPlexReporter(in outBead);
    var zon = (ushort)ClassifyBeadToZone(outBead.greenD);
    outBead.ratio1 = CalculateRatio1(in outBead);
    outBead.ratio2 = CalculateRatio2(in outBead);

    //finalize outBead data
    outBead.region = reg;
    outBead.reporter = rep;
    outBead.zone = zon;

    return outBead;
  }

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
    _compensatedCoordinatesCache[0] = rawBead.greenD;
    _compensatedCoordinatesCache[1] = compensatedCl1;
    _compensatedCoordinatesCache[2] = compensatedCl2;
    _compensatedCoordinatesCache[3] = rawBead.redA;
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
    _compensatedCoordinatesCache[0] = rawBead.greenD;
    _compensatedCoordinatesCache[1] = compensatedCl1;
    _compensatedCoordinatesCache[2] = compensatedCl2;
    _compensatedCoordinatesCache[3] = rawBead.redA;
  }

  private float CalculateReporter(ushort region)
  {
    var basicReporter = _greenMin > HdnrTrans ? _greenMaj * HDnrCoef : _greenMin;
    var scaledReporter = basicReporter * _inverseReporterScaling;

    if (!Normalization.IsEnabled || region == 0)
      return scaledReporter;
    var rep = _map.GetFactorizedNormalizationForRegion(region);
    scaledReporter -= rep;
    if (scaledReporter < 0)
      return 0;
    return scaledReporter;
  }

  private float CalculateSpectraPlexReporter(in ProcessedBead processedBead)
  {
    float basicReporter;
    if (_channelRedirectionEnabled)
    {
      //redB+redC+redD
      basicReporter = processedBead.greenB + processedBead.greenC + processedBead.redA;
    }
    else
    {
      basicReporter = processedBead.greenB + processedBead.greenC + processedBead.greenD;
    }
    return basicReporter;
    //var scaledReporter = (basicReporter * _inverseReporterScaling);
    //if (!Normalization.IsEnabled || region == 0)
    //  return scaledReporter;
    //var rep = _map.GetFactorizedNormalizationForRegion(region);
    //scaledReporter -= rep;
    //if (scaledReporter < 0)
    //  return 0;
    //return scaledReporter;
  }

  private float CalculateRatio1(in ProcessedBead processedBead)
  {
    float ratio1;
    if (_channelRedirectionEnabled)
    {
      ratio1 = processedBead.redA / processedBead.greenB;
    }
    else
    {
      ratio1 = processedBead.greenB / processedBead.greenC;
    }
    return ratio1;
  }

  private float CalculateRatio2(in ProcessedBead processedBead)
  {
    float ratio2;
    if (_channelRedirectionEnabled)
    {
      ratio2 = processedBead.greenC / processedBead.greenB;
    }
    else
    {
      ratio2 = processedBead.greenD / processedBead.greenC;
    }
    return ratio2;
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

  private void EmptyFunc(ref RawBead rawBead)
  {
  }
}