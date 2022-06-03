using System;
using System.Collections.Generic;
using DIOS.Core.Structs;
using Newtonsoft.Json;

namespace DIOS.Core
{
  [JsonObject(MemberSerialization.Fields)]
  public class PlateReport
  {
    public Guid plateID;
    public Guid beadMapId;
    public DateTime completedDateTime;
    private List<WellStats> _wells = new List<WellStats>(384);
    [JsonIgnore]
    private int _size;

    /// <summary>
    /// Get the reporter Mean values. Works only for the first well (tube)
    /// </summary>
    /// <returns>a list of reporter means for the respective regions</returns>
    public List<(int region, int mfi)> GetRegionalReporterMFI()
    {
      if (_wells[0] == null)
        return null;
      return _wells[0].GetReporterMFI();
    }

    internal void Add(WellStats stats)
    {
      _wells.Add(stats);
      _size++;
    }

    public void Reset()
    {
      plateID = Guid.Empty;
      beadMapId = Guid.Empty;
      completedDateTime = DateTime.MinValue;
      _wells.Clear();
      _size = 0;
    }
  }
}
