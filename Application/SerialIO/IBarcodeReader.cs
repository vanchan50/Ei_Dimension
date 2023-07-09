using System.Threading.Tasks;

namespace DIOS.Application.SerialIO
{
  public interface IBarcodeReader
  {
    Task<string> QueryReadAsync(int millisecondsTimeout);
  }
}
