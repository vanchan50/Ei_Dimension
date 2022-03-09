using System;
using System.Collections.Generic;

namespace DIOS.Core
{
  [Serializable]
  public class WellReport
  {
    public int row;
    public int col;
    public List<RegionReport> rpReg = new List<RegionReport>();
  }
}
