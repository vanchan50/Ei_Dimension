using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ei_Dimension.Models
{
  public enum WellType
  {
    Empty,
    Standard,
    Control,
    Unknown,
    ReadyForReading,
    NowReading,
    Success,
    LightFail,
    Fail
  }
}
