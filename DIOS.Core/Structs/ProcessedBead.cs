namespace DIOS.Core;

public struct ProcessedBead
{
  public uint EventTime;

  public byte fsc_bg;
  public byte vssc_bg;
  public byte cl0_bg;
  public ushort cl1_bg;
  public ushort cl2_bg;
  public byte cl3_bg;
  public byte rssc_bg;
  public byte gssc_bg;

  public ushort greenB_bg;    //measured background used in Compensation 
  public ushort greenC_bg;
  public float greenB;   //ADC measurement of optical split on RP1
  public float greenC;

  public byte l_offset_rg;    //samples between green and red peak
  public byte l_offset_gv;    //samples between green and violet peak
  public int region;
  public int zone;     //*1K shift and inject into region
  public float fsc;       //Forward Scatter -> changed to "External PMT"

  public float violetssc; //Violet A
  public float cl0;       //Violet B

  public float redssc;    //Red B
  public float cl1;       //Red C

  public float cl2;       //Red D
  public float cl3;       //Red A

  public float greenssc;  //Green A
  public float reporter;  //computed from Green B and Green C

  //Const:
  public const int ZONEOFFSET = 1000;
    
}