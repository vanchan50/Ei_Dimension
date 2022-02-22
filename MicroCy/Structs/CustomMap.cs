using System;
using System.Collections.Generic;

namespace DIOS.Core
{
  [Serializable]
  public class CustomMap
  {
    public string mapName;

    public int highorderidx;   //is 0 for cl0, 1 for cl1, etc
    public int midorderidx; //y
    public int loworderidx; //x
    public int calcl0;
    public int calcl1;
    public int calcl2;
    public int calcl3;
    public int calrpmaj;
    public int calrpmin;
    public int calrssc;
    public int calgssc;
    public int calvssc;
    public int calfsc;
    public string caltime;
    public string valtime;
    public bool validation;
    public CalParams calParams;
    public List<MapRegion> regions;
    //public List<(int x, int y, int r)> classificationMap; //contains coords in 256x256 space for region numbers
    //can contain up to 6 classimaps (01,02,03,12,13,23) if necessary. possibility left for the future
  }
}
