using DIOS.Core;
using System.Collections.Generic;

namespace DIOS.Application
{
  public class BeadEventSink : IBeadEventSink
  {
    public int Count => _outputBeadsCollector.Count;
    public ProcessedBead this[int key] => _outputBeadsCollector[key];
    private List<ProcessedBead> _outputBeadsCollector;

    public BeadEventSink(int capacity)
    {
      _outputBeadsCollector = new List<ProcessedBead>(capacity);
    }

    public void Add(ProcessedBead bead)
    {
      _outputBeadsCollector.Add(bead);
    }

    public void Clear()
    {
      _outputBeadsCollector.Clear();
    }
  }
}
