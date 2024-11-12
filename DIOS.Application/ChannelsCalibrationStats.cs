namespace DIOS.Application;

public class ChannelsCalibrationStats
{
  public DistributionStats Greenssc { get; }  //GreenA
  public DistributionStats GreenB { get; }
  public DistributionStats GreenC { get; }
  public DistributionStats Redssc { get; }  //RedB
  public DistributionStats Cl1 { get; }
  public DistributionStats Cl2 { get; }
  public DistributionStats RedA { get; }
  public DistributionStats GreenD { get; }

  public ChannelsCalibrationStats(DistributionStats greenssc, DistributionStats greenb, DistributionStats greenc, DistributionStats redssc,
    DistributionStats cl1, DistributionStats cl2, DistributionStats redA, DistributionStats greenD)
  {
    Greenssc = greenssc;
    GreenB = greenb;
    GreenC = greenc;
    Redssc = redssc;
    Cl1 = cl1;
    Cl2 = cl2;
    RedA = redA;
    GreenD = greenD;
  }
}