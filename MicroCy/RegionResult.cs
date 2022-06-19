using System;
using System.Collections.Generic;
using System.Linq;

namespace DIOS.Core
{
  [Serializable]
  public class RegionResult
  {
    public ushort regionNumber;
    public List<float> ReporterValues = new List<float>(CAPACITY);
    //public List<float> RP1bgnd = new List<float>(CAPACITY);
    public const int CAPACITY = 40000;
  }
}
