using Newtonsoft.Json;

namespace DIOS.Application.FileIO;

public class CalibrationReportFileWriter
{
  private ResultsPublisher _publisher;

  public CalibrationReportFileWriter(ResultsPublisher publisher)
  {
    _publisher = publisher;
  }

  public void CreateAndWrite(CalibrationReport report)
  {
    var directoryName = Path.Combine(_publisher.Outdir, ResultsPublisher.RESULTFOLDERNAME);
    if (!_publisher.OutputDirectoryExists(directoryName))
      return;

    var fullFilePath = $"{directoryName}\\CalibrationReport_{_publisher.Date}.json";
    try
    {
      var publishableReport = JsonConvert.SerializeObject(report);
      File.WriteAllText(fullFilePath, publishableReport);
      _publisher._logger.Log($"Calibration Report saved at \"{fullFilePath}\"");
    }
    catch
    {
      _publisher._logger.Log($"Failed to create Calibration Report at \"{fullFilePath}\"");
    }
  }
}