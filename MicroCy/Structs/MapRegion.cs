using System;
using System.Collections.Generic;

namespace DIOS.Core
{
  [Serializable]
  public class MapRegion
  {
    public int Number;
    public double VerificationTargetReporter;
    public bool isValidator;
    public (int x, int y) Center; //coords in 256x256 space
    public List<(int x, int y)> Points; //contains coords in 256x256 space for region numbers
  }
}