using System;
using System.IO;
using System.Text;
using DIOS.Core;

namespace DIOS.Application.FileIO
{
  public class BeadEventFileWriter
  {
    private string _fullName;
    private ResultsPublisher _publisher;
    private readonly StringBuilder _dataOut = new StringBuilder();

    public BeadEventFileWriter(ResultsPublisher publisher)
    {
      _publisher = publisher;
    }

    public void CreateAndWrite(BeadEventsData eventsData)
    {
      if (!_publisher.IsBeadEventPublishingActive)
      {
        _publisher._logger.Log("Bead event Saving inactive");
        return;
      }

      var beadsString = Decode(eventsData);

      var directoryName = Path.Combine(_publisher.Outdir, ResultsPublisher.DATAFOLDERNAME);
      if (!_publisher.OutputDirectoryExists(directoryName))
        return;

      try
      {
        File.WriteAllText(_fullName, beadsString);
        _publisher._logger.Log($"Bead event saved as {_fullName}");
      }
      catch (Exception e)
      {
        _publisher._logger.Log($"Failed to write Bead event to {_fullName}");
        _publisher._logger.Log(e.Message);
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

    private string Decode(BeadEventsData eventsData)
    {
      _ = _dataOut.Clear();
      _ = _dataOut.AppendLine(ProcessedBead.HEADER);

      foreach (var bead in eventsData.List)
      {
        if (bead.region == 0 && _publisher.IsOnlyClassifiedBeadsPublishingActive)
          continue;
        _dataOut.AppendLine(bead.ToString());
      }
      return _dataOut.ToString();
    }
  }
}
