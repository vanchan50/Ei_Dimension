using System;

namespace DIOS.Core
{
  [Serializable]
  public class Well //TODO:make it immutable (make it struct?)
  {
    public byte RowIdx;
    public byte ColIdx;
    public WellReadingSpeed RunSpeed;
    public ChannelConfiguration ChanConfig;
    public short SampVol;
    public short WashVol;
    public short AgitateVol;
    public Termination TermType;
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
      AgitateVol = well.AgitateVol;
      TermType = well.TermType;
      BeadsToCapture = well.BeadsToCapture;
      MinPerRegion = well.MinPerRegion;
    }
  }
}
