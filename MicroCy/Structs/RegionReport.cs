using System;

namespace MicroCy
{
  [Serializable]
  public struct RegionReport
  {
    public ushort region;
    public uint count;
    public float medfi;
    public float meanfi;
    public float coefVar;
  }
}
