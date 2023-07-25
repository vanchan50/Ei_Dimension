using DIOS.Core.HardwareIntercom;

namespace DIOS.Core;

public class Well //TODO:make it immutable
{
  public byte RowIdx;
  public byte ColIdx;
  public WellReadingSpeed RunSpeed;
  public ChannelConfiguration ChanConfig;
  public short SampVol;
  public short WashVol;
  public short ProbewashVol;
  public short AgitateVol;
  //public Termination TermType;
  public int BeadsToCapture;
  public int MinPerRegion;

  public Well()
  {
      
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