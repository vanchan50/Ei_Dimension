using DIOS.Core;
using System.Collections.Frozen;

namespace DIOS.Application.Domain;
public static class BeadParamsHelper
{
  internal static readonly FrozenDictionary<string, byte> _shiftData = new Dictionary<string, byte>()
  {
    ["GreenA"] = 68,
    ["GreenB"] = 20,
    ["GreenC"] = 24,
    ["GreenD"] = 48,
    ["RedA"] = 64,
    ["RedB"] = 52,
    ["RedC"] = 56,
    ["RedD"] = 60,
  }.ToFrozenDictionary();
  internal static readonly FrozenDictionary<string, string> _swapData = new Dictionary<string, string>()
  {
    ["GreenB"] = "RedC",
    ["GreenC"] = "RedD",
    ["RedC"] = "GreenB",
    ["RedD"] = "GreenC",
  }.ToFrozenDictionary();
  private static bool _isOEM;
  private static byte _param1Shift = _shiftData["RedC"];
  private static byte _param2Shift = _shiftData["RedD"];
  private static byte _hiSensitivityShift = _shiftData["GreenC"];
  private static byte _extendedChannelShift = _shiftData["GreenB"];
  private static byte _reporter1Shift;
  private static byte _reporter2Shift;
  private static byte _reporter3Shift;
  private static byte _reporter4Shift;
  private static ReporterGetter _reporter1Func;
  private static ReporterGetter _reporter2Func;
  private static ReporterGetter _reporter3Func;
  private static ReporterGetter _reporter4Func;

  private delegate float ReporterGetter(in ProcessedBead bead, byte shift);

  public static void ChooseOEMMode(bool isOEM)
  {
    _isOEM = isOEM;
  }

  public static void ChooseProperClassification(string param1, string param2)
  {
    //OEM channel swap substitution
    if (_isOEM)
    {
      if (_swapData.TryGetValue(param1, out var temp))
      {
        param1 = temp;
      }
      if (_swapData.TryGetValue(param2, out var temp2))
      {
        param2 = temp2;
      }
    }

    try
    {
      _param1Shift = _shiftData[param1];
    }
    catch
    {
      _param1Shift = _shiftData["RedC"];
    }
    try
    {
      _param2Shift = _shiftData[param2];
    }
    catch
    {
      _param2Shift = _shiftData["RedD"];
    }
  }

  public static void ChooseProperSensitivityChannels(string hiSensChannel, string extendedChannel)
  {
    //OEM channel swap substitution
    if (_isOEM)
    {
      if (_swapData.TryGetValue(hiSensChannel, out var temp1))
      {
        hiSensChannel = temp1;
      }
      if (_swapData.TryGetValue(extendedChannel, out var temp2))
      {
        extendedChannel = temp2;
      }
    }

    try
    {
      _hiSensitivityShift = _shiftData[hiSensChannel];
    }
    catch
    {
      _hiSensitivityShift = _shiftData["GreenC"];
    }
    try
    {
      _extendedChannelShift = _shiftData[extendedChannel];
    }
    catch
    {
      _extendedChannelShift = _shiftData["GreenB"];
    }
  }

  public static void ChooseProperReporterChannels(string reporter1, string reporter2, string reporter3, string reporter4)
  {
    _reporter1Func = UnsafeBeadParamAcquisition;
    _reporter2Func = UnsafeBeadParamAcquisition;
    _reporter3Func = UnsafeBeadParamAcquisition;
    _reporter4Func = UnsafeBeadParamAcquisition;
    //OEM channel swap substitution
    if (_isOEM)
    {
      if (_swapData.TryGetValue(reporter1, out var temp1))
      {
        reporter1 = temp1;
      }
      if (_swapData.TryGetValue(reporter2, out var temp2))
      {
        reporter2 = temp2;
      }
      if (_swapData.TryGetValue(reporter3, out var temp3))
      {
        reporter3 = temp3;
      }
      if (_swapData.TryGetValue(reporter4, out var temp4))
      {
        reporter4 = temp4;
      }
    }

    try
    {
      _reporter1Shift = _shiftData[reporter1];
    }
    catch
    {
      _reporter1Func = NullFunc;
    }
    try
    {
      _reporter2Shift = _shiftData[reporter2];
    }
    catch
    {
      _reporter2Func = NullFunc;
    }
    try
    {
      _reporter3Shift = _shiftData[reporter3];
    }
    catch
    {
      _reporter3Func = NullFunc;
    }
    try
    {
      _reporter4Shift = _shiftData[reporter4];
    }
    catch
    {
      _reporter4Func = NullFunc;
    }

  }

  public static float GetClassificationParam1(in ProcessedBead bead)
  {
    return UnsafeBeadParamAcquisition(bead, _param1Shift);
  }

  public static float GetClassificationParam2(in ProcessedBead bead)
  {
    return UnsafeBeadParamAcquisition(bead, _param2Shift);
  }

  public static float GetHiSensitivityChannelValue(in ProcessedBead bead)
  {
    return UnsafeBeadParamAcquisition(bead, _hiSensitivityShift);
  }

  public static float GetExtendedChannelValue(in ProcessedBead bead)
  {
    return UnsafeBeadParamAcquisition(bead, _extendedChannelShift);
  }

  public static float GetReporter1ChannelValue(in ProcessedBead bead)
  {
    return _reporter1Func.Invoke(bead, _reporter1Shift);
  }

  public static float GetReporter2ChannelValue(in ProcessedBead bead)
  {
    return _reporter2Func.Invoke(bead, _reporter2Shift);
  }

  public static float GetReporter3ChannelValue(in ProcessedBead bead)
  {
    return _reporter3Func.Invoke(bead, _reporter3Shift);
  }

  public static float GetReporter4ChannelValue(in ProcessedBead bead)
  {
    return _reporter4Func.Invoke(bead, _reporter4Shift);
  }

  private static float UnsafeBeadParamAcquisition(in ProcessedBead bead, byte paramShift)
  {
    unsafe
    {
      fixed (ProcessedBead* pBead = &bead)
      {
        return *(float*)((byte*)pBead + paramShift);
      }
    }
  }

  private static float NullFunc(in ProcessedBead bead, byte paramShift)
  {
    return 0f;
  }
}