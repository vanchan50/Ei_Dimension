namespace DIOS.Core
{
  public class CalibrationStats
  {
    public DistributionStats Greenssc { get; }
    public DistributionStats GreenB { get; }
    public DistributionStats GreenC { get; }
    public DistributionStats Redssc { get; }
    public DistributionStats Cl1 { get; }
    public DistributionStats Cl2 { get; }
    public DistributionStats Cl3 { get; }
    public DistributionStats Violetssc { get; }
    public DistributionStats Cl0 { get; }
    public DistributionStats Fsc { get; }

    public CalibrationStats(DistributionStats greenssc, DistributionStats greenb, DistributionStats greenc, DistributionStats redssc,
      DistributionStats cl1, DistributionStats cl2, DistributionStats cl3, DistributionStats violetssc,
      DistributionStats cl0, DistributionStats fsc)
    {
      Greenssc = greenssc;
      GreenB = greenb;
      GreenC = greenc;
      Redssc = redssc;
      Cl1 = cl1;
      Cl2 = cl2;
      Cl3 = cl3;
      Violetssc = violetssc;
      Cl0 = cl0;
      Fsc = fsc;
    }
  }
}
