using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ei_Dimension.Models
{
  public class CustomMap
  {
    public string mapName { get; set; }

    public bool dimension3 { get; set; }    //is it 3 dimensional
    public int highorderidx { get; set; }   //is 0 for cl0, 1 for cl1, etc
    public int midorderidx { get; set; }
    public int loworderidx { get; set; }
    public ushort minmapssc { get; set; }
    public ushort maxmapssc { get; set; }
    public int calcl0 { get; set; }
    public int calcl1 { get; set; }
    public int calcl2 { get; set; }
    public int calcl3 { get; set; }
    public int calrpmaj { get; set; }
    public int calrpmin { get; set; }
    public int calrssc { get; set; }
    public int calgssc { get; set; }
    public int calvssc { get; set; }
    public List<BeadRegion> mapRegions = new List<BeadRegion>();
  }
}
