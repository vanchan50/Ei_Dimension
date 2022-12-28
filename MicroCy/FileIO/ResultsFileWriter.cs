using System;
using System.IO;
using System.Text;

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

      if (!Device.IncludeReg0InPlateSummary)
        RemoveRegion0(ref WellStats);

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

    private void RemoveRegion0(ref string output)
    {
      var sb = new StringBuilder();
      var r = new StringReader(output);
      string s = r.ReadLine();
      while (s != null)
      {
        if (!s.StartsWith("0"))
        {
          sb.AppendLine(s);
        }
        s = r.ReadLine();
      }
      output = sb.ToString();
    }
  }
}
