using System;

namespace MicroCy
{
  [Serializable]
  public struct BeadRegion
  {
    public ushort regionNumber;
    public string regionName;
    public bool isActive;
    public bool isvector;          //vector type maps are computed instead of described by map array
    public byte bitmaptype;
    public int centerhighorderidx; //log index of mean value of intesity measured during map create
    public int centermidorderidx;
    public int centerloworderidx;
    public int meanhighorder; //mean value of intesity measured during map create
    public int meanmidorder;
    public int meanloworder;
    public int meanrp1bg;
  }
}
