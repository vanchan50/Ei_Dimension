using DIOS.Core.HardwareIntercom;

namespace DIOS.Core;

public class Well
{ 
  public byte RowIdx { get; }
  public byte ColIdx { get; }
  public WellReadingSpeed RunSpeed { get; }
  public ChannelConfiguration ChanConfig { get; }
  public short SampVol { get; }
  public short WashVol { get; }
  public short ProbewashVol { get; }
  public short AgitateVol { get; }
  //public Termination TermType { get; }
  public int BeadsToCapture { get; }
  public int MinPerRegion { get; }
  public int TerminationTimer { get; }

  public Well(byte row, byte col)
  {
    RowIdx = row;
    ColIdx = col;
  }
  public Well(byte row, byte col, WellReadingSpeed speed, ChannelConfiguration chConfig,
    short sampleVolume, short washVolume, short probeWashVol, short agitateVolume, int minPerRegion, int beadsToCapture, int terminationTimer)
  {
    RowIdx = row;
    ColIdx = col;
    RunSpeed = speed;
    ChanConfig = chConfig;
    SampVol = sampleVolume;
    WashVol = washVolume;
    ProbewashVol = probeWashVol;
    AgitateVol = agitateVolume;
    MinPerRegion = minPerRegion;
    BeadsToCapture = beadsToCapture;
    TerminationTimer = terminationTimer;
  }

  public Well( Well well )
  {
    RowIdx = well.RowIdx;
    ColIdx = well.ColIdx;
    RunSpeed = well.RunSpeed;
    ChanConfig = well.ChanConfig;
    SampVol = well.SampVol;
    WashVol = well.WashVol;
    ProbewashVol = well.ProbewashVol;
    AgitateVol = well.AgitateVol;
    //TermType = well.TermType;
    BeadsToCapture = well.BeadsToCapture;
    MinPerRegion = well.MinPerRegion;
  }

  public string CoordinatesString()
  {
    char rowletter = (char)(0x41 + RowIdx);
    string colLetter = (ColIdx + 1).ToString();
    return $"{rowletter}{colLetter}";
  }
}