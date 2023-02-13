using System.Text;
using DIOS.Core;

namespace DIOS.Application
{
  internal class VerificationReportPublisher
  {
    private StringBuilder _data = new StringBuilder();
    //private string _path;
    private readonly ILogger _logger;

    public VerificationReportPublisher(ILogger logger)
    {
      _logger = logger;
      //_path = DateTime.Now.ToString("dd.MM.yyyy.hh-mm-ss",
      //  System.Globalization.CultureInfo.CreateSpecificCulture("en-US")) + "VerificationReport.txt";
    }
    /// <summary>
    /// To be called every time before you make a new validation measurement
    /// </summary>
    internal void Reset()
    {
      _data.Clear();
      //_path = DateTime.Now.ToString("dd.MM.yyyy.hh-mm-ss",
      //  System.Globalization.CultureInfo.CreateSpecificCulture("en-US")) + "VerificationReport.txt";
      _data.Append("Verification Fail Report\n");
      //_data.Append($"{DateTime.Now.ToString("dd.MM.yyyy.hh-mm-ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"))}\n");
    }

    internal void AddData(string data)
    {
      _data.Append(data);
    }

    internal void PublishReport()
    {
      //var directoryName = $"{Device.RootDirectory.FullName}\\VerificationReports";
      //try
      //{
      //  if (!Directory.Exists(directoryName))
      //    _ = Directory.CreateDirectory(directoryName);
      //}
      //catch
      //{
      //  _logger.Log($"Failed to create {directoryName}");
      //  return;
      //}
      //
      //try
      //{
      //  File.AppendAllText(_path, _data.ToString());
      //  _logger.Log($"Verification Report saved as {_path}");
      //}
      //catch
      //{
      //  _logger.Log($"Failed to append data to {_path}");
      //}
      _logger.Log(_data.ToString());
    }
  }
}
