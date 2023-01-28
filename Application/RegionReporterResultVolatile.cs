using System.Linq;

namespace DIOS.Application
{
  /// <summary>
  /// A Deep Copy of RegionReporterResult (List of reporter values for a region)
  /// <br>Intended to be passed around for stats calculation etc.</br>
  /// </summary>
  public class RegionReporterResultVolatile : RegionReporterResult
  {
    public RegionReporterResultVolatile()
    {
        
    }

    public RegionReporterResultVolatile(RegionReporterResult copySource)
    {
      regionNumber = copySource.regionNumber;
      var count = copySource.ReporterValues.Count < CAPACITY ? copySource.ReporterValues.Count : CAPACITY;
      for (var j = 0; j < count; j++)
      {
        ReporterValues.Add(copySource.ReporterValues[j]);
      }
    }

    //  TODO: WARNING modifies (sorts) the instance
    //  TODO: duplicates RegionReporterStats.MakeStats() behavior
    public void MakeStats(out int count, out float mean)
    {
      if (ReporterValues.Count == 0)
      {
        count = 0;
        mean = 0;
        return;
      }
      var avg = ReporterValues.Average();
      count = ReporterValues.Count;
      if (count >= 20)
      {
        mean = CalculateMean();
        return;
      }
      mean = avg;
    }

    private float CalculateMean()
    {
      ReporterValues.Sort();
      var count = ReporterValues.Count;
      int quarterIndex = count / 4;

      float sum = 0;
      for (var i = quarterIndex; i < count - quarterIndex; i++)
      {
        sum += ReporterValues[i];
      }

      return sum / (count - 2 * quarterIndex);
    }
  }
}
