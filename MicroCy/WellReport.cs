﻿using System;
using System.Collections.Generic;

namespace DIOS.Core
{
  [Serializable]
  public class WellReport
  {
    public ushort prow;
    public ushort pcol;
    public List<RegionReport> rpReg = new List<RegionReport>();
  }
}
