using System;

namespace MicroCy
{
  public interface ISerial
  {
    byte[] InputBuffer { get; }
    bool IsActive { get; }

    void BeginRead(AsyncCallback func);
    void EndRead(IAsyncResult result);
    void Write(byte[] buffer);
  }
}