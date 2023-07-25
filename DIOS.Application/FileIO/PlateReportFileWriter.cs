using System.IO;

namespace DIOS.Application.FileIO;

public class PlateReportFileWriter
{
  private ResultsPublisher _publisher;

  public PlateReportFileWriter(ResultsPublisher publisher)
  {
    _publisher = publisher;
  }

  public void CreateAndWrite(string plateReportJSON, string filename)
  {
    if (!_publisher.IsPlateReportPublishingActive)
    {
      _publisher._logger.Log("Plate Report Publishing Inactive");
      return;
    }

    if (!_publisher.OutputDirectoryExists(_publisher.SUMMARYFOLDERNAME))
      return;

    var fullFilePath = Path.Combine(_publisher.SUMMARYFOLDERNAME, $"Summary_{filename}_{_publisher.Date}.json");
    try
    {
      File.WriteAllText(fullFilePath, plateReportJSON);
      _publisher._logger.Log($"Plate Report saved as \"{fullFilePath}\"");
    }
    catch
    {
      _publisher._logger.Log($"Failed to create Plate Report at \"{fullFilePath}\"");
    }
  }

  public void CreateAndWrite(string plateReportJSON)
  {
    CreateAndWrite(plateReportJSON, _publisher.Outfilename);
  }
}