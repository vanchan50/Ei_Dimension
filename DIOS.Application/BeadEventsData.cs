using DIOS.Core;

namespace DIOS.Application;

public class BeadEventsData
{
  /// <summary>
  /// Raw access to Processed beads
  /// </summary>
  public ProcessedBead this[int index] => _list[index];
  public int Count => _size;

  private readonly List<ProcessedBead> _list = new(2000000);
  private int _size;
  private int _nextNewBeadIndex;

  public void Add(in ProcessedBead bead)
  {
    _list.Add(bead);
    _size++;
  }

  public void Reset()
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