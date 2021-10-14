using System;

namespace MicroCy
{
  [Serializable]
  public struct Wells
  {
    public byte rowIdx;
    public byte colIdx;
    public byte runSpeed;
    public byte termType;
    public byte chanConfig;
    public short sampVol;
    public short washVol;
    public short agitateVol;
    public int termCnt;
    public int regTermCnt;
    public CustomMap thisWellsMap;
  }
}
