using System.Threading.Tasks;

namespace DIOS.Application.SerialIO;

public interface IBarcodeReader
{
  public bool IsAvailable { get; }
  Task<string> QueryReadAsync(int millisecondsTimeout);
}