using System;
using System.Linq;

namespace DIOS.Core
{
  [Serializable]
  public class RegionStats
  {
    public ushort Region;
    public int Count;
    public float MedFi;
    public float MeanFi;
    public float CoeffVar;

    public RegionStats(RegionResult regionNumber, Well well)
    {
      Count = regionNumber.ReporterValues.Count;
      Region = regionNumber.regionNumber;
      var rp1Temp = regionNumber.ReporterValues;
      if (Count >= 20)
      {
        rp1Temp.Sort();
        //float rpbg = regionNumber.RP1bgnd.Average() * 16;
        int quarter = Count / 4;
        rp1Temp.RemoveRange(Count - quarter, quarter);
        rp1Temp.RemoveRange(0, quarter);
        MeanFi = rp1Temp.Average();
        double sumsq = rp1Temp.Sum(dataout => Math.Pow(dataout - MeanFi, 2));
        double stddev = Math.Sqrt(sumsq / rp1Temp.Count() - 1);
        CoeffVar = (float)stddev / MeanFi * 100;
        if (double.IsNaN(CoeffVar))
          CoeffVar = 0;
        MedFi = (float)Math.Round(rp1Temp[quarter]);// - rpbg);
        //rout.meanfi -= rpbg;
      }
      else if (Count > 2)
        MeanFi = rp1Temp.Average();
    }

    public override string ToString()
    {
      return $"{Region.ToString()},{Count.ToString()},{MedFi.ToString()},{MeanFi.ToString("F3")},{CoeffVar.ToString("F3")}";
    }
  }
}