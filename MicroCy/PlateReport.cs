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
    private List<WellReport> Wells = new List<WellReport>();
    private int _size;

    public List<(int region, int mfi)> GetRegionalReporterMFI() //TODO:will be taken from ResultData after refactoring
    {
      if (Wells[0].rpReg == null)
        return null;

      List<(int region, int mfi)> list = new List<(int region, int mfi)>(100);

      foreach (var regionReport in Wells[0].rpReg)
      {
        if (regionReport.region == 0)
        {
          continue;
        }
        list.Add((regionReport.region, (int)regionReport.meanfi));
      }
      return list;
    }

    public void AddResultsToLastWell(RegionStats outRes)
    {
      var lastWell = Wells[_size - 1];
      lastWell.rpReg.Add(new RegionReport
      {
        region = outRes.Region,
        count = (uint)outRes.Count,
        medfi = outRes.MedFi,
        meanfi = outRes.MeanFi,
        coefVar = outRes.CoeffVar
      });
    }

    public void AddWell(Well well)
    {
      Wells.Add(new WellReport
      {
        // OutResults grouped by row and col
        row = well.RowIdx,
        col = well.ColIdx
      });
      _size++;
    }
  }
}
