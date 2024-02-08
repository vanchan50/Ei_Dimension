using System.Text;
using DIOS.Core.HardwareIntercom;

namespace DIOS.Core;

public class Well
{ 
  public byte RowIdx { get; }
  public byte ColIdx { get; }
  public WellReadingSpeed RunSpeed { get; }
  public uint SampVol { get; }
  public uint WashVol { get; }
  public uint ProbewashVol { get; }
  public uint AgitateVol { get; }
  public uint WashRepeats { get; }
  public uint ProbeWashRepeats { get; }
  public uint AgitateRepeats { get; }
  public Termination TerminationType { get; }
  public uint BeadsToCapture { get; }
  public uint MinPerRegion { get; }
  public uint TerminationTimer { get; }
  public IReadOnlyCollection<(int Number, string Name )> ActiveRegions { get; }

  public Well(byte row, byte col)
  {
    RowIdx = row;
    ColIdx = col;
    ActiveRegions = new List<(int Number, string Name)>();
  }

  public Well(byte row, byte col, IReadOnlyCollection<(int Number, string Name)> regions, WellReadingSpeed speed,
    uint sampleVolume, uint washVolume, uint probeWashVol, uint agitateVolume,
    uint washRepeats, uint probeWashRepeats, uint agitateRepeats,
    uint minPerRegion, uint beadsToCapture, uint terminationTimer, Termination terminationType)
  {
    RowIdx = row;
    ColIdx = col;
    ActiveRegions = regions.OrderBy(x => x.Number).ToList();
    RunSpeed = speed;
    SampVol = sampleVolume;
    WashVol = washVolume;
    ProbewashVol = probeWashVol;
    AgitateVol = agitateVolume;
    WashRepeats = washRepeats;
    ProbeWashRepeats = probeWashRepeats;
    AgitateRepeats = agitateRepeats;
    MinPerRegion = minPerRegion;
    BeadsToCapture = beadsToCapture;
    TerminationTimer = terminationTimer;
    TerminationType = terminationType;
  }

  public Well( Well well )
  {
    RowIdx = well.RowIdx;
    ColIdx = well.ColIdx;
    ActiveRegions = well.ActiveRegions;
    RunSpeed = well.RunSpeed;
    SampVol = well.SampVol;
    WashVol = well.WashVol;
    ProbewashVol = well.ProbewashVol;
    AgitateVol = well.AgitateVol;
    WashRepeats = well.WashRepeats;
    ProbeWashRepeats = well.ProbeWashRepeats;
    AgitateRepeats = well.AgitateRepeats;
    MinPerRegion = well.MinPerRegion;
    BeadsToCapture = well.BeadsToCapture;
    TerminationTimer = well.TerminationTimer;
    TerminationType = well.TerminationType;
  }

  public string CoordinatesString()
  {
    char rowletter = (char)(0x41 + RowIdx);
    string colLetter = (ColIdx + 1).ToString();
    return $"{rowletter}{colLetter}";
  }

  /// <summary>
  /// Produce a similar Well; No regions defined, and 256BeadsToRead
  /// </summary>
  /// <returns></returns>
  public Well ToCalibrationWell(uint beadsToCapture)
  {
    var calibrationWell = new Well(
      RowIdx,
      ColIdx,
      Array.Empty<(int, string)>(),
      RunSpeed,
      SampVol,
      WashVol,
      ProbewashVol,
      AgitateVol,
      WashRepeats,
      ProbeWashRepeats,
      AgitateRepeats,
      0, beadsToCapture, 0, Termination.TotalBeadsCaptured);
    return calibrationWell;
  }
}