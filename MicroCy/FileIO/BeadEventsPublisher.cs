using System;
using System.IO;
using System.Text;

namespace DIOS.Core.FileIO
{
  internal class BeadEventsPublisher
  {
    private readonly StringBuilder Data = new StringBuilder();
    private const string BHEADER = "Preamble,Time(1 us Tick),FSC bg,Viol SSC bg,CL0 bg,CL1 bg,CL2 bg,CL3 bg,Red SSC bg,Green SSC bg," +
                                   "Green B bg,Green C bg,Green B,Green C,Red-Grn Offset,Grn-Viol Offset,Region,Forward Scatter,Violet SSC,CL0," +
                                   "Red SSC,CL1,CL2,CL3,Green SSC,Reporter\r";


    internal void StartNewBeadEventReport()
    {
      GetNewBeadEventFileName();
      _ = Data.Clear();
      _ = Data.Append(BHEADER);
    }

    internal void AddBeadEvent(in BeadInfoStruct beadInfo)
    {
      if (_device.SaveIndividualBeadEvents)
        _ = Data.Append(beadInfo.ToString());
    }

    public void SaveBeadEventFile()
    {
      if ((FullFileName != null) && _device.SaveIndividualBeadEvents)
      {
        try
        {
          File.WriteAllText(FullFileName, Data.ToString());
          Console.WriteLine($"Bead event saved as {FullFileName}");
        }
        catch
        {
          Console.WriteLine($"Failed to write Bead event to {FullFileName}");
        }
      }
    }

    private void GetNewBeadEventFileName()
    {
      OutDirCheck();
      try
      {
        if (!Directory.Exists($"{Outdir}\\AcquisitionData"))
          Directory.CreateDirectory($"{Outdir}\\AcquisitionData");
      }
      catch
      {
        Console.WriteLine($"Failed to create {Outdir}\\AcquisitionData");
        return;
      }

      char rowletter = (char)(0x41 + _device.WellController.CurrentWell.RowIdx);
      string colLetter = (_device.WellController.CurrentWell.ColIdx + 1).ToString();
      string date = DateTime.Now.ToString("dd.MM.yyyy.hh-mm-ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
      FullBeadEventFileName = $"{Outdir}\\AcquisitionData\\{Outfilename}{rowletter}{colLetter}_{date}.csv";
    }
  }
}
