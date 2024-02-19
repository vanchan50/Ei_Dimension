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

    var fullFilePath = $"{directoryName}\\CalibrationReport_{_publisher.Date}.pdf";
    try
    {
      new CalibrationReportPdfFileWriter(report).CreateAndSaveVerificationPdf(fullFilePath);
      _publisher._logger.Log($"Calibration Report saved at \"{fullFilePath}\"");
    }
    catch
    {
      _publisher._logger.Log($"Failed to create Calibration Report at \"{fullFilePath}\"");
    }
  }
}