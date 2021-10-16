using System;

namespace MicroCy
{
  public class ReadingWellEventArgs : EventArgs
  {
    public byte Row { get; set; }
    public byte Column { get; set; }
    public ReadingWellEventArgs(byte row, byte col)
    {
      Row = row;
      Column = col;
    }
  }
}
