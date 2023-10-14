using System.Text;

namespace DIOS.Application.FileIO;

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
      _publisher._logger.Log($"Results summary file created at \"{_thisRunStatsFileName}\"");
    }
    catch (Exception e)
    {
      _publisher._logger.Log($"Failed to create file at \"{_thisRunStatsFileName}\"");
      _publisher._logger.Log(e.Message);
    }
  }

  public void AppendAndWrite(string WellStats)
  {
    if (!_publisher.IsResultsPublishingActive || WellStats.Length == 0)
      return;

    if (!_publisher.IncludeReg0InPlateSummary)
      RemoveRegion0(ref WellStats);

    var directoryName = Path.Combine(_publisher.Outdir, ResultsPublisher.DATAFOLDERNAME);
    if (!_publisher.OutputDirectoryExists(directoryName))
      return;

    try
    {
      File.AppendAllText(_thisRunStatsFileName, WellStats);
      _publisher._logger.Log($"Results summary saved as {_thisRunStatsFileName}");
    }
    catch (Exception e)
    {
      _publisher._logger.Log($"Failed to append data to {_thisRunStatsFileName}");
      _publisher._logger.Log(e.Message);
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