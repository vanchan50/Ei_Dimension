using System;
using System.IO;

namespace DIOS.Application.FileIO
{
  public class PlateStatusFileWriter
  {
    private ResultsPublisher _publisher;
    public PlateStatusFileWriter(ResultsPublisher publisher)
    {
      _publisher = publisher;
    }

    public void Overwrite(string plateData)
    {
      //overwrite the whole thing
      try
      {
        var fullName = Path.Combine(_publisher.Outdir, ResultsPublisher.STATUSFOLDERNAME, "StatusFile.json");
        File.WriteAllText(fullName, plateData);
      }
      catch (Exception e)
      {
        _publisher._logger.Log($"Problem with status file save, Please report this issue to the Manufacturer {e.Message}");
      }
    }
  }
}
