using System;
using System.Collections.Generic;

namespace DIOS.Core
{
  [Serializable]
  public class PlateReport
  {
    public Guid plateID;
    public Guid beadMapId;
    public DateTime completedDateTime;
    public List<WellReport> rpWells = new List<WellReport>();
  }
}
