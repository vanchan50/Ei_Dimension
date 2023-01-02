using System.IO;

namespace DIOS.Core.FileIO
{
  public class LegacyReportFileWriter
  {
    private ResultsPublisher _publisher;

    public LegacyReportFileWriter(ResultsPublisher publisher)
    {
      _publisher = publisher;
    }

    public void CreateAndWrite(string LegacyReport)
    {
      if (!_publisher.IsLegacyPlateReportPublishingActive)
      {
        _publisher._logger.Log("Legacy Plate Report Inactive");
        return;
      }

      var directoryName = Path.Combine(_publisher.Outdir, ResultsPublisher.DATAFOLDERNAME);
      if (!_publisher.OutputDirectoryExists(directoryName))
        return;

      var fileName = $"{directoryName}\\LxResults_{_publisher.ReportFileName}_{_publisher.Date}.csv";
      try
      {
        File.WriteAllText(fileName, LegacyReport);
        _publisher._logger.Log($"Legacy Plate Report saved as {fileName}");
      }
      catch
      {
        _publisher._logger.Log("Failed to create Legacy Plate Report");
      }
    }
  }
}
