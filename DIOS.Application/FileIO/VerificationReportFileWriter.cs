namespace DIOS.Application.FileIO;

public class VerificationReportFileWriter
{
  private ResultsPublisher _publisher;

  public VerificationReportFileWriter(ResultsPublisher publisher)
  {
    _publisher = publisher;
  }

  public void CreateAndWrite(VerificationReport report)
  {
    var directoryName = Path.Combine(_publisher.Outdir, ResultsPublisher.RESULTFOLDERNAME);
    if (!_publisher.OutputDirectoryExists(directoryName))
      return;

    var fullFilePath = $"{directoryName}\\VerificationReport_{_publisher.Date}.pdf";
    try
    {
      new VerificationReportPdfFileWriter(report).CreateAndSaveVerificationPdf(fullFilePath);
      _publisher._logger.Log($"Verification Report saved at \"{fullFilePath}\"");
    }
    catch(Exception e)
    {
      _publisher._logger.Log($"{e.GetType()}");
      _publisher._logger.Log($"{e.Message}");
      _publisher._logger.Log($"{e.StackTrace}");
      _publisher._logger.Log($"Failed to create Verification Report at \"{fullFilePath}\"");
    }
  }
}