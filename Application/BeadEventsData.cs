using System.Collections.Generic;
using DIOS.Core;

namespace DIOS.Application
{
  public class BeadEventsData
  {
    public List<ProcessedBead> List
    {
      get
      {
        return _list;
      }
    }

    private readonly List<ProcessedBead> _list = new List<ProcessedBead>(2000000);

    public void Add(in ProcessedBead bead)
    {
      _list.Add(bead);
    }

    public void Reset()
    {
      _list.Clear();
    }
  }
}