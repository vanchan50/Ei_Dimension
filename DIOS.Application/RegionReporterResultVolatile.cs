namespace DIOS.Application;

/// <summary>
/// A Deep Copy of RegionReporterResult (List of reporter values for a region)
/// <br>Intended to be passed around for stats calculation etc.</br>
/// </summary>
public class RegionReporterResultVolatile : RegionReporterResult
{
  public RegionReporterResultVolatile(int region) : base(region)
  {

  }

  public RegionReporterResultVolatile(RegionReporterResult copySource) : base(copySource.regionNumber)
  {
    var count = copySource.Count;
    for (var j = 0; j < count; j++)
    {
      Add(copySource.ReporterValues[j]);
    }
  }

  //  TODO: WARNING modifies (sorts) the instance
  //  TODO: duplicates RegionReporterStats.MakeStats() behavior
  public void MakeStats(out int count, out float mean)
  {
    if (Count == 0)
    {
      count = 0;
      mean = 0;
      return;
    }
    var avg = Average();
    count = Count;
    if (Count >= 20)
    {
      mean = CalculateMean();
      return;
    }
    mean = avg;
  }

  private float CalculateMean()
  {
    ReporterValues.Sort();
    int quarterIndex = Count / 4;

    float sum = 0;
    for (var i = quarterIndex; i < Count - quarterIndex; i++)
    {
      sum += ReporterValues[i];
    }

    return sum / (Count - 2 * quarterIndex);
  }
}