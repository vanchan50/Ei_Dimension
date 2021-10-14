using System;

namespace MicroCy
{
  [Serializable]
  public struct OutResults
  {
    public string row;
    public int col;
    public ushort region;
    public int count;
    public float medfi;
    public float meanfi;
    public float cv;
    public override string ToString()
    {
      return string.Format("{0},{1},{2},{3},{4},{5:F3},{6:F1}\r",
        row, col, region, count, medfi, meanfi, cv);
    }
  }
}
