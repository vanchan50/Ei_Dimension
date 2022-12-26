using System;
using System.IO;

namespace DIOS.Core.FileIO
{
  public class ResultsFileWriter
  {
    private string _thisRunStatsFileName;
    private ResultsPublisher _publisher;
    public ResultsFileWriter(ResultsPublisher publisher)
    {
      _publisher = publisher;
    }

    public void MakeNew()
    {
      GetThisRunResultsFileName();
      if (!_publisher.IsResultsPublishingActive)
        return;

      var directoryName = Path.Combine(_publisher.Outdir, ResultsPublisher.DATAFOLDERNAME);
      if (!_publisher.OutputDirectoryExists(directoryName))
        return;

      try
      {
        File.AppendAllText(_thisRunStatsFileName, ResultingWellStatsData.HEADER);
        Console.WriteLine($"Results summary file created {_thisRunStatsFileName}");
      }
      catch
      {
        Console.WriteLine($"Failed to create file {_thisRunStatsFileName}");
      }
    }

    public void AppendAndWrite(string WellStats)
    {
      if (!_publisher.IsResultsPublishingActive || WellStats.Length == 0)
        return;

      var directoryName = Path.Combine(_publisher.Outdir, ResultsPublisher.DATAFOLDERNAME);
      if (!_publisher.OutputDirectoryExists(directoryName))
        return;

      try
      {
        File.AppendAllText(_thisRunStatsFileName, WellStats);
        Console.WriteLine($"Results summary saved as {_thisRunStatsFileName}");
      }
      catch
      {
        Console.WriteLine($"Failed to append data to {_thisRunStatsFileName}");
      }
    }

    private void GetThisRunResultsFileName()
    {
      _publisher.OutDirCheck();
      _thisRunStatsFileName = Path.Combine(_publisher.Outdir, ResultsPublisher.DATAFOLDERNAME, $"Results_{_publisher.Outfilename}_{_publisher.Date}.csv");
    }
  }
}
