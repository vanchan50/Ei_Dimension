using System;
using System.Collections.Generic;

namespace MicroCy
{
  [Serializable]
  public class WellResult
  {
    public ushort regionNumber;
    public List<float> RP1vals = new List<float>(40000);
    public List<float> RP1bgnd = new List<float>(40000);
  }
}
