namespace DIOS.Core
{
  public class AveragesStats
  {
    public double Greenssc { get; }
    public double GreenB { get; }
    public double GreenC { get; }
    public double Cl3 { get; }
    public double Redssc { get; }
    public double Cl1 { get; }
    public double Cl2 { get; }
    public double Violetssc { get; }
    public double Cl0 { get; }
    public double Fsc { get; }

    public AveragesStats(double greenssc, double greenb, double greenc, double redssc,
      double cl1, double cl2, double cl3, double violetssc,
      double cl0, double fsc)
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
