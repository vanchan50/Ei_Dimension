
namespace DIOS.Application;

public class ChannelsHistogramPeaks
{
  public string GreenA { get; }  //Greenssc
  public string GreenB { get; }
  public string GreenC { get; }
  public string Redssc { get; }  //RedB
  public string Cl1 { get; }  //GreenB
  public string Cl2 { get; }  //GreenC
  public string Cl3 { get; }
  public string GreenD { get; }

  public ChannelsHistogramPeaks(int greenA, int greenB, int greenC, int redssc,
    int cl1, int cl2, int cl3, int greenD)
  {
    GreenA = greenA.ToString();
    GreenB = greenB.ToString();
    GreenC = greenC.ToString();
    Redssc = redssc.ToString();
    Cl1 = cl1.ToString();
    Cl2 = cl2.ToString();
    Cl3 = cl3.ToString();
    GreenD = greenD.ToString();
  }
}