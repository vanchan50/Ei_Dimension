using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ei_Dimension.Models
{
  public struct BeadInfoStruct
  {
    public UInt32 Header;
    public UInt32 EventTime;

    public byte fsc_bg;
    public byte vssc_bg;
    public byte cl0_bg;
    public byte cl1_bg;
    public byte cl2_bg;
    public byte cl3_bg;
    public byte rssc_bg;
    public byte gssc_bg;

    public UInt16 greenB_bg;    //measured background used in Compensation 
    public UInt16 greenC_bg;
    public UInt16 greenB;   //ADC measurement of optical split on RP1
    public UInt16 greenC;

    public byte l_offset_rg;    //samples between green and red peak
    public byte l_offset_gv;    //samples between green and violet peak
    public UInt16 region;
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
      return $"{Header},{EventTime},{fsc_bg},{vssc_bg},{cl0_bg},{cl1_bg},{cl2_bg},{cl3_bg},{rssc_bg},{gssc_bg},{greenB_bg},{greenC_bg},{greenB},{greenC},{l_offset_rg},{l_offset_gv},{region},{fsc},{violetssc},{cl0},{redssc},{cl1},{cl2},{cl3},{greenssc},{reporter}\r";
    }
  }
}
