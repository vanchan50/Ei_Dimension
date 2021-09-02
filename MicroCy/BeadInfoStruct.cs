namespace MicroCy
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
      return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25}\r",
        Header, EventTime, fsc_bg, vssc_bg, cl0_bg, cl1_bg, cl2_bg, cl3_bg, rssc_bg, gssc_bg, greenB_bg, greenC_bg, greenB, greenC,
        l_offset_rg, l_offset_gv, region, fsc, violetssc, cl0, redssc, cl1, cl2, cl3, greenssc, reporter);
    }
  }
}
