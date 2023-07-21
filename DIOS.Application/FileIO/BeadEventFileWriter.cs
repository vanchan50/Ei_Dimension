using System.Text;
using DIOS.Core;

namespace DIOS.Application.FileIO
{
  public class BeadEventFileWriter
  {
    private string _fullName;
    private ResultsPublisher _publisher;
    private readonly StringBuilder _dataOut = new StringBuilder(AVGSTRLENGTH * 2000000);
    private const int AVGSTRLENGTH = 110;
    public const string HEADER = "Time(1 ms Tick),FSC bg,Viol SSC bg,CL0 bg,CL1 bg,CL2 bg,CL3 bg,Red SSC bg,Green SSC bg," +
                                 "Green B bg,Green C bg,Green B,Green C,Red-Grn Offset,Grn-Viol Offset,Region,Forward Scatter,Violet SSC,CL0," +
                                 "Red SSC,CL1,CL2,CL3,Green SSC,Reporter";
    public const string OEMHEADER = "Time(1 ms Tick),FSC bg,Viol A bg,CL0 bg,CL4 bg,CL5 bg,CL3 bg,Red SSC bg,Green SSC bg," +
                                 "Red C bg,Red D bg,Red C,Red D,Red-Grn Offset,Grn-Viol Offset,Region,Forward Scatter,Violet SSC,CL0," +
                                 "Red SSC,CL4,CL5,CL3,Green SSC,Reporter";

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

    private string Decode(BeadEventsData eventsData)
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
      return $"{bead.EventTime},{bead.fsc_bg},{bead.vssc_bg},{bead.cl0_bg},{bead.cl1_bg},{bead.cl2_bg},{bead.cl3_bg},{bead.rssc_bg},{bead.gssc_bg},{bead.greenB_bg},{bead.greenC_bg},{bead.greenB:F0},{bead.greenC:F0},{bead.l_offset_rg},{bead.l_offset_gv},{(bead.zone * ProcessedBead.ZONEOFFSET + bead.region).ToString()},{bead.fsc:F0},{bead.violetssc:F0},{bead.cl0:F0},{bead.redssc:F0},{bead.cl1:F0},{bead.cl2:F0},{bead.cl3:F0},{bead.greenssc:F0},{bead.reporter:F3}";
    }
  }
}