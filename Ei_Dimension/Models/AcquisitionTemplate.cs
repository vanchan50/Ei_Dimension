using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ei_Dimension.Models
{
  [Serializable]
  public class AcquisitionTemplate
  {
    public string Name;
    public List<bool> ActiveRegions;
    public List<string> RegionsNamesList;
    public byte SysControl;
    public byte ChConfig;
    public string Map;
    public byte Order;
    public byte Speed;
    public uint SampleVolume;
    public uint WashVolume;
    public uint AgitateVolume;
    public byte EndRead;
    public uint MinPerRegion;
    public uint TotalEvents;
    public uint FileSaveCheckboxes;

    public AcquisitionTemplate()
    {
      ActiveRegions = new List<bool>();
      RegionsNamesList = new List<string>();
    }
  }
}
