using System.Collections;
using System.Collections.Generic;
using DIOS.Core;

namespace DIOS.Application;

public class BeadEventsData : IEnumerable<ProcessedBead>
{
  public ProcessedBead this[int index] => _list[index];
  public int Count => _list.Count;

  private readonly List<ProcessedBead> _list = new List<ProcessedBead>(2000000);

  public void Add(in ProcessedBead bead)
  {
    _list.Add(bead);
  }

  public void Reset()
  {
    _list.Clear();
  }

  public IEnumerator<ProcessedBead> GetEnumerator()
  {
    return _list.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
  //use yield return instead of DataOut.TryDequeue()
}