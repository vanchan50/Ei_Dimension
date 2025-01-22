using System.Runtime.InteropServices;

namespace DIOS.Core;

[StructLayout(LayoutKind.Sequential)]
public struct ProcessedBead
{
  public uint EventTime;

  public byte fsc_bg; //void
  public byte vssc_bg;  //void
  public byte greenD_bg;
  public ushort cl1_bg;
  public ushort cl2_bg;
  public byte redA_bg;
  public byte rssc_bg;
  public byte gssc_bg;

  public ushort greenB_bg;    //measured background used in Compensation 
  public ushort greenC_bg;
  public float greenB;   //ADC measurement of optical split on RP1
  public float greenC;

  public byte l_offset_rg;    //samples between green and red peak
  public byte l_offset_gv;    //void
  public int region;
  public int zone;     //*1K shift and inject into region
  public float ratio1;

  public float ratio2;
  public float greenD;    //CL0

  public float redssc;    //Red B
  public float cl1;       //Red C

  public float cl2;       //Red D
  public float redA;       //cl3

  public float greenssc;  //Green A
  public float reporter;  //computed from Green B and Green C

  //Const:
  public const int ZONEOFFSET = 1000;
    
}