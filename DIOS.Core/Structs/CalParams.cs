namespace DIOS.Core;

[Serializable]
public class CalParams
{
  public float compensation;
  public ushort gate;
  public ushort height;
  public ushort minmapssc;
  public ushort maxmapssc;
  public float DNRCoef;
  public float DNRTrans;
  public string HiSensChannel = "GreenC";
  public string ExtendedDNRChannel = "GreenB";
  public int att;
  public int CL0;
  public int CL1;
  public int CL2;
  public int CL3;
  public int RP1;
}