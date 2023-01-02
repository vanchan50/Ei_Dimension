using System.IO;

namespace DIOS.Core.FileIO
{
  public class BeadEventFileWriter
  {
    private string _fullName;

    private ResultsPublisher _publisher;

    public BeadEventFileWriter(ResultsPublisher publisher)
    {
      _publisher = publisher;
    }

    public void CreateAndWrite(string beadEventsList)
    {
      if (!_publisher.IsBeadEventPublishingActive)
      {
        _publisher._logger.Log("Bead event Saving inactive");
        return;
      }

      var directoryName = Path.Combine(_publisher.Outdir, ResultsPublisher.DATAFOLDERNAME);
      if (!_publisher.OutputDirectoryExists(directoryName))
        return;

      try
      {
        File.WriteAllText(_fullName, beadEventsList);
        _publisher._logger.Log($"Bead event saved as {_fullName}");
      }
      catch
      {
        _publisher._logger.Log($"Failed to write Bead event to {_fullName}");
      }
    }

    public string MakeNewFileName(Well currentWell)
    {
      var directoryName = Path.Combine(_publisher.Outdir, ResultsPublisher.DATAFOLDERNAME);
      if (!_publisher.OutputDirectoryExists(directoryName))
        return "";

      _fullName = Path.Combine(_publisher.Outdir, ResultsPublisher.DATAFOLDERNAME, $"{_publisher.Outfilename}{currentWell.CoordinatesString()}_{_publisher.Date}.csv");
      return _fullName;
    }
  }
}
