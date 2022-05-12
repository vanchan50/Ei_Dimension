using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DIOS.Core.FileIO
{
  internal class VerificationReportPublisher
  {
    private StringBuilder _data = new StringBuilder();
    private string _path;

    public VerificationReportPublisher()
    {
      _path = DateTime.Now.ToString("dd.MM.yyyy.hh-mm-ss",
        System.Globalization.CultureInfo.CreateSpecificCulture("en-US")) + "VerificationReport.txt";
    }
    /// <summary>
    /// To be called every time before you make a new validation measurement
    /// </summary>
    internal void Reset()
    {
      _data.Clear();
      _path = DateTime.Now.ToString("dd.MM.yyyy.hh-mm-ss",
        System.Globalization.CultureInfo.CreateSpecificCulture("en-US")) + "VerificationReport.txt";
      _data.Append("Verification Fail Report\n");
      _data.Append($"{DateTime.Now.ToString("dd.MM.yyyy.hh-mm-ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"))}\n");
    }

    internal void AddData(string data)
    {
      _data.Append(data);
    }

    internal void PublishReport()
    {
      var directoryName = $"{Device.RootDirectory.FullName}\\VerificationReports";
      try
      {
        if (!Directory.Exists(directoryName))
          _ = Directory.CreateDirectory(directoryName);
      }
      catch
      {
        Console.WriteLine($"Failed to create {directoryName}");
        return;
      }

      try
      {
        File.AppendAllText(_path, _data.ToString());
        Console.WriteLine($"Verification Report saved as {_path}");
      }
      catch
      {
        Console.WriteLine($"Failed to append data to {_path}");
      }
    }
  }
}
