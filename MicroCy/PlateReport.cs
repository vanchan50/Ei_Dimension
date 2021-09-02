using System;
using System.Collections.Generic;

namespace MicroCy
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
