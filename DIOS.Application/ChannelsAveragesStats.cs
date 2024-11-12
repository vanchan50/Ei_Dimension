namespace DIOS.Application;

public class ChannelsAveragesStats
{
  public double GreenA { get; }
  public double GreenB { get; }
  public double GreenC { get; }
  public double RedA { get; }
  public double RedB { get; }
  public double Cl1 { get; }
  public double Cl2 { get; }
  public double GreenD { get; }

  public ChannelsAveragesStats(double greenA, double greenb, double greenc, double redB,
    double cl1, double cl2, double redA, double greenD)
  {
    GreenA = greenA;
    GreenB = greenb;
    GreenC = greenc;
    RedB = redB;
    Cl1 = cl1;
    Cl2 = cl2;
    RedA = redA;
    GreenD = greenD;
  }
}