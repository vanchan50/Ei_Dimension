using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DIOS.Core
{
  public class ResultsPublisher
  {
    public string Outdir { get; set; }  //  user selectable
    public string Outfilename { get; set; } = "ResultFile";
    public static string WorkOrderPath { get; set; }
    internal Well SavingWell { get; set; }
    internal string FullBeadEventFileName { get; private set; }
    private string _thisRunResultsFileName;
    private readonly StringBuilder BeadEventDataOut = new StringBuilder();
    private readonly StringBuilder SummaryOut = new StringBuilder();
    private PlateReport _plateReport;
    private Device _device;
    private const string BHEADER = "Preamble,Time(1 us Tick),FSC bg,Viol SSC bg,CL0 bg,CL1 bg,CL2 bg,CL3 bg,Red SSC bg,Green SSC bg," +
                                   "Green B bg,Green C bg,Green B,Green C,Red-Grn Offset,Grn-Viol Offset,Region,Forward Scatter,Violet SSC,CL0," +
                                   "Red SSC,CL1,CL2,CL3,Green SSC,Reporter\r";
    private const string SHEADER = "Row,Col,Region,Bead Count,Median FI,Trimmed Mean FI,CV%\r";
    private static readonly char[] Alphabet = Enumerable.Range('A', 16).Select(x => (char)x).ToArray();

    public ResultsPublisher(Device device)
    {
      _device = device;
      Outdir = Device.RootDirectory.FullName;
    }

    internal void StartNewBeadEventReport()
    {
      GetNewBeadEventFileName();
      _ = BeadEventDataOut.Clear();
      _ = BeadEventDataOut.Append(BHEADER);
    }

    public void OutDirCheck()
    {
      var root = Path.GetPathRoot(Outdir);
      if (!Directory.Exists(root))
      {
        Outdir = Device.RootDirectory.FullName;
      }
    }

    public List<(int region, int mfi)> GetRegionalReporterMFI()
    {
      if (_plateReport == null || _plateReport.Wells == null || _plateReport.Wells[0].rpReg == null)
        return null;

      List<(int region, int mfi)> list = new List<(int region, int mfi)>();

      foreach (var regionReport in _plateReport.Wells[0].rpReg)
      {
        if (regionReport.region == 0)
        {
          continue;
        }
        list.Add((regionReport.region, (int)regionReport.meanfi));
      }
      return list;
    }

    internal void StartNewPlateReport()
    {
      _plateReport = new PlateReport();
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

    internal void AddBeadEvent(in BeadInfoStruct beadInfo)
    {
      if (_device.SaveIndividualBeadEvents)
        _ = BeadEventDataOut.Append(beadInfo.ToString());
    }

    private void AddOutResultsToSummary(in OutResults oResults)
    {
      _ = SummaryOut.Append(oResults.ToString());
    }

    private string GetWellReport()
    {
      return BeadEventDataOut.ToString();
    }

    private void OutputSummaryFile()
    {
      //end of read session (plate, plate section or tube) write summary stat file
      if (SummaryOut.Length == 0)
        return;
      var directoryName = $"{Outdir}\\AcquisitionData";
      try
      {
        OutDirCheck();
        if (!Directory.Exists(directoryName))
          Directory.CreateDirectory(directoryName);
      }
      catch
      {
        Console.WriteLine($"Failed to create {directoryName}");
        return;
      }

      try
      {
        File.AppendAllText(_thisRunResultsFileName, SummaryOut.ToString());
        Console.WriteLine($"Results summary saved as {_thisRunResultsFileName}");
      }
      catch
      {
        Console.WriteLine($"Failed to append data to {_thisRunResultsFileName}");
      }
    }

    internal void ResetSummary()
    {
      _ = SummaryOut.Clear();
      _ = SummaryOut.Append(SHEADER);
      GetThisRunFileName();
      OutputSummaryFile();
    }

    private void GetThisRunFileName()
    {
      OutDirCheck();
      string date = DateTime.Now.ToString("dd.MM.yyyy.hh-mm-ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
      _thisRunResultsFileName = $"{Outdir}\\AcquisitionData\\Results_{Outfilename}_{date}.csv";
    }

    public void OutputPlateReport()
    {
      string rfilename = _device.Control == SystemControl.Manual ? Outfilename : _device.WorkOrder.plateID.ToString();
      var directoryName = $"{Device.RootDirectory.FullName}\\Result\\Summary";
      try
      {
        if (!Directory.Exists(directoryName))
          _ = Directory.CreateDirectory(directoryName);
      }
      catch
      {
        Console.WriteLine($"Failed to create {directoryName}");
        return;
      }

      try
      {
        var fileName = $"{directoryName}" +
                       "\\Summary_" + rfilename + ".json";
        using (TextWriter jwriter = new StreamWriter(fileName))
        {
          var jcontents = JsonConvert.SerializeObject(_plateReport);
          jwriter.Write(jcontents);
          Console.WriteLine($"Plate Report saved as {fileName}");
        }
      }
      catch
      {
        Console.WriteLine($"Failed to create Plate Report");
      }
    }

    private void AddToPlateReport(in OutResults outRes)
    {
      _plateReport.Wells[_plateReport.Wells.Count - 1].rpReg.Add(new RegionReport
      {
        region = outRes.region,
        count = (uint)outRes.count,
        medfi = outRes.medfi,
        meanfi = outRes.meanfi,
        coefVar = outRes.cv
      });
    }

    public void SaveBeadFile(List<RegionResult> wellres) //cancels the begin read from endpoint 2
    {
      SaveBeadEventFile();
      if (_device.RMeans)
      {
        SummaryOut.Clear();
        if (_plateReport != null)
          _plateReport.Wells.Add(new WellReport
          {
            // OutResults grouped by row and col
            row = SavingWell.RowIdx,
            col = SavingWell.ColIdx
          });
        for (var i = 0; i < wellres.Count; i++)
        {
          RegionResult regionNumber = wellres[i];
          OutResults rout = FillOutResults(regionNumber);
          AddOutResultsToSummary(in rout);
          AddToPlateReport(in rout);//TODO: done not in the right place
        }
        OutputSummaryFile();
        wellres = null;
      }
      Console.WriteLine($"{DateTime.Now.ToString()} Reporting Background File Save Complete");
      if (File.Exists(WorkOrderPath))
        File.Delete(WorkOrderPath);   //result is posted, delete work order
      //need to clear textbox in UI. this has to be an event
    }

    private OutResults FillOutResults(RegionResult regionNumber)
    {
      OutResults rout = new OutResults();
      rout.row = Alphabet[SavingWell.RowIdx].ToString();
      rout.col = SavingWell.ColIdx + 1;  //columns are 1 based
      rout.count = regionNumber.RP1vals.Count;
      rout.region = regionNumber.regionNumber;
      var rp1Temp = regionNumber.RP1vals;
      if (rout.count >= 20)
      {
        rp1Temp.Sort();
        //float rpbg = regionNumber.RP1bgnd.Average() * 16;
        int quarter = rout.count / 4;
        rp1Temp.RemoveRange(rout.count - quarter, quarter);
        rp1Temp.RemoveRange(0, quarter);
        rout.meanfi = rp1Temp.Average();
        double sumsq = rp1Temp.Sum(dataout => Math.Pow(dataout - rout.meanfi, 2));
        double stddev = Math.Sqrt(sumsq / rp1Temp.Count() - 1);
        rout.cv = (float)stddev / rout.meanfi * 100;
        if (double.IsNaN(rout.cv))
          rout.cv = 0;
        rout.medfi = (float)Math.Round(rp1Temp[quarter]);// - rpbg);
        //rout.meanfi -= rpbg;
      }
      else if (rout.count > 2)
        rout.meanfi = rp1Temp.Average();
      return rout;
    }

    private void SaveBeadEventFile()
    {
      if ((FullBeadEventFileName != null) && _device.SaveIndividualBeadEvents)
      {
        try
        {
          File.WriteAllText(FullBeadEventFileName, GetWellReport());
          Console.WriteLine($"Bead event saved as {FullBeadEventFileName}");
        }
        catch
        {
          Console.WriteLine($"Failed to write Bead event to {FullBeadEventFileName}");
        }
      }
    }
  }
}