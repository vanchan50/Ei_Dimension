using Newtonsoft.Json;

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

    var fullFilePath = $"{directoryName}\\VerificationReport_{_publisher.Date}.json";
    try
    {
      var publishableReport = JsonConvert.SerializeObject(report);
      File.WriteAllText(fullFilePath, publishableReport);
      _publisher._logger.Log($"Verification Report saved at \"{fullFilePath}\"");
    }
    catch
    {
      _publisher._logger.Log($"Failed to create Verification Report at \"{fullFilePath}\"");
    }
  }
}