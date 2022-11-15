using System;
using System.Threading;

namespace DIOS.Core.Tests
{
  public class FakeUSBConnection : ISerial
  {
    public byte[] InputBuffer { get; }
    public bool IsActive { get; }
    private readonly object _readReady = new object();
    public FakeUSBConnection()
    {
      InputBuffer = new byte[512];
      IsActive = true;
    }
    public void BeginRead(AsyncCallback func)
    {
      throw new NotImplementedException();
    }

    public void Read()
    {
      lock (_readReady)
      {
        Monitor.Wait(_readReady);
      }
    }

    public void EndRead(IAsyncResult result)
    {
      throw new NotImplementedException();
    }

    public void Write(byte[] buffer)
    {

    }

    public void Reconnect()
    {

    }

    public void Disconnect()
    {

    }
    
    public void ClearBuffer()
    {
      Array.Clear(InputBuffer, 0, InputBuffer.Length);
    }

    public bool IsBeadInBuffer()
    {
      throw new NotImplementedException();
    }

    internal void ReadBead(in RawBead bead)
    {
      var bd =  BeadInfoToByteArray(bead);
      Array.Copy(bd, InputBuffer, bd.Length);
      lock (_readReady)
      {
        Monitor.Pulse(_readReady);
      }
    }

    /// <summary>
    /// Utility to produce byte[] that contains bead data
    /// </summary>
    /// <param name="bead"></param>
    /// <returns></returns>
    private static byte[] BeadInfoToByteArray(in RawBead bead)
    {
      byte[] arrRet = new byte[64];
      unsafe
      {
        fixed (RawBead* pCS = &bead)
        {
          for (var i = 0; i < 64; i++)
          {
            arrRet[i] = *((byte*)pCS + i);
          }
        }
      }

      return arrRet;
    }
  }
}