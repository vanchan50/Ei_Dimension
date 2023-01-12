using System.IO;

namespace DIOS.Application.FileIO
{
  public class LegacyReportFileWriter
  {
    private ResultsPublisher _publisher;

    public LegacyReportFileWriter(ResultsPublisher publisher)
    {
      _publisher = publisher;
    }

    /// <summary>
    /// Saves report with a custom OutFileName
    /// </summary>
    /// <param name="legacyReport">Report to save</param>
    /// <param name="filename">Customizable file name. Used for PlateID</param>
    public void CreateAndWrite(string legacyReport, string filename)
    {
      if (!_publisher.IsLegacyPlateReportPublishingActive)
      {
        _publisher._logger.Log("Legacy Plate Report Inactive");
        return;
      }

      var directoryName = Path.Combine(_publisher.Outdir, ResultsPublisher.DATAFOLDERNAME);
      if (!_publisher.OutputDirectoryExists(directoryName))
        return;

      var fullFilePath = $"{directoryName}\\LxResults_{filename}_{_publisher.Date}.csv";
      try
      {
        File.WriteAllText(fullFilePath, legacyReport);
        _publisher._logger.Log($"Legacy Plate Report saved as {fullFilePath}");
      }
      catch
      {
        _publisher._logger.Log("Failed to create Legacy Plate Report");
      }
    }

    /// <summary>
    /// Saves report with a regular OutFileName, specified in the Publisher
    /// </summary>
    /// <param name="legacyReport">Report to save</param>
    public void CreateAndWrite(string legacyReport)
    {
      CreateAndWrite(legacyReport, _publisher.Outfilename);
    }
  }
}
