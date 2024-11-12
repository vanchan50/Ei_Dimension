using System.Text;
using DIOS.Core;

namespace DIOS.Application.FileIO;

public class BeadEventFileWriter
{
  private string _fullName;
  private ResultsPublisher _publisher;
  private readonly StringBuilder _dataOut = new(AVGSTRLENGTH * 2_000_000);
  private const int AVGSTRLENGTH = 110;
  public const string HEADER = "Time(1 ms Tick),void 1,void 2,Green D bg,Red C bg,Red D bg,Red A bg,Red SSC bg,Green SSC bg," +
                               "Green B bg,Green C bg,Green B,Green C,Red-Grn Offset,void 3,Region,Ratio 1,Ratio 2,Green D," +
                               "Red SSC,Red C,Red D,Red A,Green SSC,Reporter";
  public const string OEMHEADER = "Time(1 ms Tick),void 1,void 2,Green D bg,Red C bg,Red D bg,Red A bg,Red B bg,Green A bg," +
                                  "Green B bg,Green C bg,Red C,Red D,Red-Grn Offset,void 3,Region,Ratio 1,Ratio 2,Green D," +
                                  "Red SSC,Green B,Green C,Red A,Green SSC,Reporter";

  public BeadEventFileWriter(ResultsPublisher publisher)
  {
    _publisher = publisher;
  }

  public void CreateAndWrite(IEnumerable<ProcessedBead> eventsData)
  {
    if (!_publisher.IsBeadEventPublishingActive)
    {
      _publisher._logger.Log("Bead event Saving inactive");
      return;
    }

    string beadsString = null;
    try
    {
      beadsString = Decode(eventsData);
    }
    catch (OutOfMemoryException)
    {
      _publisher._logger.Log("[PROBLEM] BeadEventFileWriter out of memory");
    }
    catch (Exception e)
    {
      _publisher._logger.Log($"[PROBLEM] BeadEventFileWriter {e.Message}");
    }

    var directoryName = Path.Combine(_publisher.Outdir, ResultsPublisher.DATAFOLDERNAME);
    if (!_publisher.OutputDirectoryExists(directoryName))
      return;

    try
    {
      File.WriteAllText(_fullName, beadsString);
      _publisher._logger.Log($"Bead event saved as \"{_fullName}\"");
    }
    catch (Exception e)
    {
      _publisher._logger.Log($"Failed to write Bead event to \"{_fullName}\"");
      _publisher._logger.Log(e.Message);
    }
  }

  public string GenerateNewFileName(Well currentWell)
  {
    var directoryName = Path.Combine(_publisher.Outdir, ResultsPublisher.DATAFOLDERNAME);
    if (!_publisher.OutputDirectoryExists(directoryName))
      return "";

    _fullName = Path.Combine(_publisher.Outdir, ResultsPublisher.DATAFOLDERNAME, $"{_publisher.Outfilename}{currentWell.CoordinatesString()}_{_publisher.Date}.csv");
    return _fullName;
  }

  private string Decode(IEnumerable<ProcessedBead> eventsData)
  {
    _ = _dataOut.Clear();
    var header = _publisher.IsOEMModeActive ? OEMHEADER : HEADER;
    _ = _dataOut.AppendLine(header);

    foreach (var bead in eventsData)
    {
      if (bead.region == 0 && _publisher.IsOnlyClassifiedBeadsPublishingActive)
        continue;
      _dataOut.AppendLine(Stringify(in bead));
    }
    return _dataOut.ToString();
  }

  public static string Stringify(in ProcessedBead bead)   //setup for csv output
  {
    return $"{bead.EventTime},{bead.fsc_bg},{bead.vssc_bg},{bead.greenD_bg},{bead.cl1_bg},{bead.cl2_bg},{bead.redA_bg},{bead.rssc_bg},{bead.gssc_bg},{bead.greenB_bg},{bead.greenC_bg},{bead.greenB:F0},{bead.greenC:F0},{bead.l_offset_rg},{bead.l_offset_gv},{(bead.zone * ProcessedBead.ZONEOFFSET + bead.region).ToString()},{bead.ratio1:F0},{bead.ratio2:F0},{bead.greenD:F0},{bead.redssc:F0},{bead.cl1:F0},{bead.cl2:F0},{bead.redA:F0},{bead.greenssc:F0},{bead.reporter:F3}";
  }
}