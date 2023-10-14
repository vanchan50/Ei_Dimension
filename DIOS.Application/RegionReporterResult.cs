namespace DIOS.Application;

public class RegionReporterResult
{
  public readonly int regionNumber;
  public List<float> ReporterValues = new(CAPACITY);
  //public List<float> RP1bgnd = new(CAPACITY);
  public const int CAPACITY = 100000;
  public int Count => ReporterValues.Count;

  public RegionReporterResult(int region)
  {
    regionNumber = region;
  }

  public void Add(float reporterValue)
  {
    ReporterValues.Add(reporterValue);
  }

  public float Average()
  {
    if(ReporterValues.Count == 0)
      return 0;
    return ReporterValues.Average();
  }
}