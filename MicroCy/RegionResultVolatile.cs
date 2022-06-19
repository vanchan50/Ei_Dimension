using System.Linq;

namespace DIOS.Core
{
  public class RegionResultVolatile : RegionResult
  {
    public RegionResultVolatile()
    {
        
    }

    public RegionResultVolatile(RegionResult copy)
    {
      regionNumber = copy.regionNumber;
      var count = copy.ReporterValues.Count < CAPACITY ? copy.ReporterValues.Count : CAPACITY;
      for (var j = 0; j < count; j++)
      {
        ReporterValues.Add(copy.ReporterValues[j]);
      }
    }

    //  TODO: WARNING modifies (sorts) the instance
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
