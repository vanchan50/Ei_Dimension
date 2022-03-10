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
      return $"{row},{col.ToString()},{region.ToString()},{count.ToString()},{medfi.ToString()},{meanfi.ToString("F3")},{cv.ToString("F3")}\r";
    }
  }
}
