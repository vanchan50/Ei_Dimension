using System;
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
        Console.WriteLine("Bead event Saving inactive");
        return;
      }

      var directoryName = Path.Combine(_publisher.Outdir, ResultsPublisher.DATAFOLDERNAME);
      if (!_publisher.OutputDirectoryExists(directoryName))
        return;

      try
      {
        File.WriteAllText(_fullName, beadEventsList);
        Console.WriteLine($"Bead event saved as {_fullName}");
      }
      catch
      {
        Console.WriteLine($"Failed to write Bead event to {_fullName}");
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
