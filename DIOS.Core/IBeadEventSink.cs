namespace DIOS.Core;

public interface IBeadEventSink<T>
{
  void Add(in T bead);
}