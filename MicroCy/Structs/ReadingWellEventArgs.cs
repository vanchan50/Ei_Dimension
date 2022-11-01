using System;

namespace DIOS.Core
{
  public class ReadingWellEventArgs
  {
    public byte Row { get; set; }
    public byte Column { get; set; }
    public string FilePath { get; set; }
    public ReadingWellEventArgs(byte row, byte col, string filepath = null)
    {
      Row = row;
      Column = col;
      FilePath = filepath;
    }
  }
}
