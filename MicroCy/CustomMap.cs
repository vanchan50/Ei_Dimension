using System;
using System.Collections.Generic;

namespace MicroCy
{
  [Serializable]
  public class CustomMap
  {
    public string mapName;

    public bool dimension3;   //is it 3 dimensional
    public int highorderidx;   //is 0 for cl0, 1 for cl1, etc
    public int midorderidx;
    public int loworderidx;
    public ushort minmapssc;
    public ushort maxmapssc;
    public int calcl0;
    public int calcl1;
    public int calcl2;
    public int calcl3;
    public int calrpmaj;
    public int calrpmin;
    public int calrssc;
    public int calgssc;
    public int calvssc;
    public List<BeadRegion> mapRegions = new List<BeadRegion>();
  }
}
