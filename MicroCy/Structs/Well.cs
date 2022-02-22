using System;

namespace DIOS.Core
{
  [Serializable]
  public class Well
  {
    public byte RowIdx;
    public byte ColIdx;
    public byte RunSpeed;
    public byte ChanConfig;
    public short SampVol;
    public short WashVol;
    public short AgitateVol;
    public Termination TermType;
    public int BeadsToCapture;
    public int MinPerRegion;
  }
}
