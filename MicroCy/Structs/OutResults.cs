using System;

namespace DIOS.Core
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
      return $"{row},{col.ToString()},{region.ToString()},{count.ToString()},{medfi.ToString()},{meanfi.ToString("5:F3")},{cv.ToString("6:F1")}\r";
    }
  }
}
