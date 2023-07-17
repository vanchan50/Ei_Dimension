using System;
using System.IO;

namespace DIOS.Application.FileIO
{
  public class PlateStatusFileWriter
  {
    private const string STATUSFILENAME = "StatusFile.json";
    private ResultsPublisher _publisher;
    public PlateStatusFileWriter(ResultsPublisher publisher)
    {
      _publisher = publisher;
    }

    public void Overwrite(string plateData)
    {
      var directoryName = Path.Combine(_publisher.Outdir, ResultsPublisher.STATUSFOLDERNAME);
      if (!_publisher.OutputDirectoryExists(directoryName))
        return;
      //overwrite the whole thing
      try
      {
        var fullName = Path.Combine(_publisher.Outdir, ResultsPublisher.STATUSFOLDERNAME, STATUSFILENAME);
        File.WriteAllText(fullName, plateData);
      }
      catch (Exception e)
      {
        _publisher._logger.Log($"Problem with status file save, Please report this issue to the Manufacturer {e.Message}");
      }
    }
  }
}
