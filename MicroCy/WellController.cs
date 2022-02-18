using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCy
{
  public class WellController
  {
    public bool IsLastWell { get; private set; }
    public Well CurrentWell { get; private set; }
    public Well NextWell { get; private set; }

    private Queue<Well> WellsToRead = new Queue<Well>();

    public void Init(IEnumerable<Well> Wells)
    {
      CurrentWell = null;
      NextWell = null;
      IsLastWell = true;
      WellsToRead.Clear();
      foreach (var well in Wells)
      {
        WellsToRead.Enqueue(well);
      }

      if (WellsToRead.Count == 0)
        throw new Exception();
      NextWell = WellsToRead.Peek();

      if (WellsToRead.Count > 1)
        IsLastWell = false;
    }

    internal void Advance()
    {
      CurrentWell = WellsToRead.Dequeue();
      IsLastWell = WellsToRead.Count == 0;
      if (IsLastWell)
        NextWell = null;
      else
        NextWell = WellsToRead.Peek();
    }

    internal void PrepareEmergencyStop()
    {
      if (WellsToRead.Count <= 1)
        //if end read on tube or single well, nothing else is aspirated otherwise
        //just read the next well in order since it is already aspirated
        return;

      //Drop Every well except the next one
      WellsToRead.Clear();
      WellsToRead.Enqueue(NextWell);
    }
  }
}
