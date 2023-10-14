using DIOS.Core.HardwareIntercom;

namespace DIOS.Core;

public class Well
{ 
  public byte RowIdx { get; }
  public byte ColIdx { get; }
  public WellReadingSpeed RunSpeed { get; }
  public ChannelConfiguration ChanConfig { get; }
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
  public IReadOnlyCollection<int> Regions { get; }

  public Well(byte row, byte col)
  {
    RowIdx = row;
    ColIdx = col;
    Regions = new List<int>();
  }

  public Well(byte row, byte col, IReadOnlyCollection<int> regions, WellReadingSpeed speed, ChannelConfiguration chConfig,
    uint sampleVolume, uint washVolume, uint probeWashVol, uint agitateVolume,
    uint washRepeats, uint probeWashRepeats, uint agitateRepeats,
    uint minPerRegion, uint beadsToCapture, uint terminationTimer, Termination terminationType)
  {
    RowIdx = row;
    ColIdx = col;
    Regions = regions;
    RunSpeed = speed;
    ChanConfig = chConfig;
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
    Regions = well.Regions;
    RunSpeed = well.RunSpeed;
    ChanConfig = well.ChanConfig;
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
}