namespace DIOS.Core
{
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
    public const string HEADER = "Time(1 ms Tick),FSC bg,Viol SSC bg,CL0 bg,CL1 bg,CL2 bg,CL3 bg,Red SSC bg,Green SSC bg," +
                                  "Green B bg,Green C bg,Green B,Green C,Red-Grn Offset,Grn-Viol Offset,Region,Forward Scatter,Violet SSC,CL0," +
                                  "Red SSC,CL1,CL2,CL3,Green SSC,Reporter";
    public override string ToString()   //setup for csv output
    {
      return $"{EventTime.ToString()},{fsc_bg.ToString()},{vssc_bg.ToString()},{cl0_bg.ToString()},{cl1_bg.ToString()},{cl2_bg.ToString()},{cl3_bg.ToString()},{rssc_bg.ToString()},{gssc_bg.ToString()},{greenB_bg.ToString()},{greenC_bg.ToString()},{greenB.ToString("F0")},{greenC.ToString("F0")},{l_offset_rg.ToString()},{l_offset_gv.ToString()},{(zone * ZONEOFFSET + region).ToString()},{fsc.ToString()},{violetssc.ToString()},{cl0.ToString()},{redssc.ToString()},{cl1.ToString()},{cl2.ToString()},{cl3.ToString()},{greenssc.ToString()},{reporter.ToString($"{0:0.000}")}";
    }
  }
}
