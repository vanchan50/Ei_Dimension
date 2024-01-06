using DIOS.Core;

namespace DIOS.Application;

public sealed class BeadEventSink<T> : IBeadEventSink<T>
{
  public T this[int key] => _list[key];
  public int Count => _size;
  private List<T> _list;
  private int _size;
  private int _nextNewBeadIndex;

  public BeadEventSink(int capacity)
  {
    _list = new List<T>(capacity);
  }

  public void Add(in T bead)
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

  public IEnumerable<T> GetNewBeadsEnumerable()
  {
    while (_nextNewBeadIndex < _size)
    {
      yield return _list[_nextNewBeadIndex++];
    }
  }

  public IEnumerable<T> GetAllBeadsEnumerable()
  {
    for (var i = 0; i < _size; i++)
    {
      yield return _list[i];
    }
  }
}