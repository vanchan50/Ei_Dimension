namespace DIOS.Core
{
  public struct BeadInfoStruct
  {
    public uint Header;
    public uint EventTime;

    public byte fsc_bg;
    public byte vssc_bg;
    public byte cl0_bg;
    public byte cl1_bg;
    public byte cl2_bg;
    public byte cl3_bg;
    public byte rssc_bg;
    public byte gssc_bg;

    public ushort greenB_bg;    //measured background used in Compensation 
    public ushort greenC_bg;
    public ushort greenB;   //ADC measurement of optical split on RP1
    public ushort greenC;

    public byte l_offset_rg;    //samples between green and red peak
    public byte l_offset_gv;    //samples between green and violet peak
    public ushort region;
    public float fsc;       //Forward Scatter

    public float violetssc; //Violet A
    public float cl0;       //Violet B

    public float redssc;    //Red D
    public float cl1;       //Red C

    public float cl2;       //Red B
    public float cl3;       //Red A

    public float greenssc;
    public float reporter;  //computed from Green B and Green C

    public override string ToString()   //setup for csv output
    {
      return $"{Header.ToString()},{EventTime.ToString()},{fsc_bg.ToString()},{vssc_bg.ToString()},{cl0_bg.ToString()},{cl1_bg.ToString()},{cl2_bg.ToString()},{cl3_bg.ToString()},{rssc_bg.ToString()},{gssc_bg.ToString()},{greenB_bg.ToString()},{greenC_bg.ToString()},{greenB.ToString()},{greenC.ToString()},{l_offset_rg.ToString()},{l_offset_gv.ToString()},{region.ToString()},{fsc.ToString()},{violetssc.ToString()},{cl0.ToString()},{redssc.ToString()},{cl1.ToString()},{cl2.ToString()},{cl3.ToString()},{greenssc.ToString()},{reporter.ToString($"{0:0.000}")}\r";
    }
  }
}