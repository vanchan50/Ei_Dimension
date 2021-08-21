using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ei_Dimension.Models
{
  public class BeadRegion
  {
    public ushort regionNumber { get; set; }
    public bool isActive { get; set; }
    public bool isvector { get; set; }          //vector type maps are computed instead of described by map array
    public byte bitmaptype { get; set; }
    public int centerhighorderidx { get; set; } //log index of mean value of intesity measured during map create
    public int centermidorderidx { get; set; }
    public int centerloworderidx { get; set; }
    public int meanhighorder { get; set; } //mean value of intesity measured during map create
    public int meanmidorder { get; set; }
    public int meanloworder { get; set; }
    public int meanrp1bg { get; set; }
  }
}
