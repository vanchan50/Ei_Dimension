using System;
using System.Collections.Generic;

namespace DIOS.Application
{
  [Serializable]
  public class RegionReporterResult
  {
    public int regionNumber;
    public List<float> ReporterValues = new List<float>(CAPACITY);
    //public List<float> RP1bgnd = new List<float>(CAPACITY);
    public const int CAPACITY = 40000;
  }
}
