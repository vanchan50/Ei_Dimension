using System;

namespace MicroCy
{
  [Serializable]
  public class Well
  {
    public byte rowIdx;
    public byte colIdx;
    public byte runSpeed;
    public byte chanConfig;
    public short sampVol;
    public short washVol;
    public short agitateVol;
  }
}
