using DIOS.Core;

namespace DIOS.Application;

public sealed class BeadEventSink : IBeadEventSink
{
  public ProcessedBead this[int key] => _list[key];
  public int Count => _size;
  private List<ProcessedBead> _list;
  private int _size;
  private int _nextNewBeadIndex;

  public BeadEventSink(int capacity)
  {
    _list = new List<ProcessedBead>(capacity);
  }

  public void Add(ProcessedBead bead)
  {
    _list.Add(bead);
    _size++;
  }

  public void Clear()
  {
    _size = 0;
    _nextNewBeadIndex = 0;
    _list.Clear();
  }

  public IEnumerable<ProcessedBead> GetNewBeadsEnumerable()
  {
    while (_nextNewBeadIndex < _size)
    {
      yield return _list[_nextNewBeadIndex++];
    }
  }

  public IEnumerable<ProcessedBead> GetAllBeadsEnumerable()
  {
    for (var i = 0; i < _size; i++)
    {
      yield return _list[i];
    }
  }
}