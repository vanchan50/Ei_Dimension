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
    public static string Outfilename { get; set; } = "ResultFile";
    public static string WorkOrderPath { get; set; }
    internal static Well SavingWell { get; set; }
    internal static string FullFileName { get; private set; }
    private static string _thisRunResultsFileName = null;
    private static readonly StringBuilder DataOut = new StringBuilder();
    private static readonly StringBuilder SummaryOut = new StringBuilder();
    private static PlateReport _plateReport;
    private Device _device;
    private const string BHEADER = "Preamble,Time(1 us Tick),FSC bg,Viol SSC bg,CL0 bg,CL1 bg,CL2 bg,CL3 bg,Red SSC bg,Green SSC bg," +
                                   "Green B bg,Green C bg,Green B,Green C,Red-Grn Offset,Grn-Viol Offset,Region,Forward Scatter,Violet SSC,CL0," +
                                   "Red SSC,CL1,CL2,CL3,Green SSC,Reporter\r";
    private const string SHEADER = "Row,Col,Region,Bead Count,Median FI,Trimmed Mean FI,CV%\r";
    private static readonly char[] Alphabet = Enumerable.Range('A', 16).Select(x => (char)x).ToArray();

    public ResultsPublisher(Device device)
    {
      _device = device;
      Outdir = device.RootDirectory.FullName;
    }

    internal static void StartNewWellReport()
    {
      _ = DataOut.Clear();
      _ = DataOut.Append(BHEADER);
    }

    public void OutDirCheck()
    {
      var root = Path.GetPathRoot(Outdir);
      if (!Directory.Exists(root))
      {
        Outdir = _device.RootDirectory.FullName;
      }
    }

    internal static void StartNewPlateReport()
    {
      _plateReport = new PlateReport();
    }

    internal static void StartNewSummaryReport()
    {
      _thisRunResultsFileName = null;
    }

    internal void GetNewFileName()
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
      //open file
      //first create unique filename

      char rowletter = (char)(0x41 + _device.WellController.CurrentWell.RowIdx);
      //if(!isTube)
      string colLetter = (_device.WellController.CurrentWell.ColIdx + 1).ToString();  //use 0 for tubes and true column for plates
      for (var differ = 0; differ < int.MaxValue; differ++)
      {
        FullFileName = $"{Outdir}\\AcquisitionData\\{Outfilename}{rowletter}{colLetter}_{differ.ToString()}.csv";
        if (!File.Exists(FullFileName))
          break;
      }
    }

    internal static void AddBeadStats(in BeadInfoStruct beadInfo)
    {
      _ = DataOut.Append(beadInfo.ToString());
    }

    internal static void AddOutResults(in OutResults oResults)
    {
      _ = SummaryOut.Append(oResults.ToString());
    }

    internal static string GetWellReport()
    {
      return DataOut.ToString();
    }

    private void OutputSummaryFile()
    {
      //end of read session (plate, plate section or tube) write summary stat file
      if (SummaryOut.Length == 0)
        return;

      try
      {
        OutDirCheck();
        if (!Directory.Exists($"{Outdir}\\AcquisitionData"))
          Directory.CreateDirectory($"{Outdir}\\AcquisitionData");
      }
      catch
      {
        Console.WriteLine($"Failed to create {Outdir}\\AcquisitionData");
        return;
      }
      GetThisRunFileName();
      try
      {
        File.AppendAllText(_thisRunResultsFileName, SummaryOut.ToString());
      }
      catch
      {
        Console.WriteLine($"Failed to append data to {_thisRunResultsFileName}");
      }
    }

    internal static void ClearSummary()
    {
      _ = SummaryOut.Clear();
      _ = SummaryOut.Append(SHEADER);
    }

    private void GetThisRunFileName()
    {
      if (_thisRunResultsFileName != null)
        return;
      OutDirCheck();
      for (var i = 0; i < int.MaxValue; i++)
      {
        string summaryFileName = $"{Outdir}\\AcquisitionData\\Results_{Outfilename}_{i.ToString()}.csv";
        if (!File.Exists(summaryFileName))
        {
          _thisRunResultsFileName = summaryFileName;
          break;
        }
      }
    }

    public void OutputPlateReport()
    {
      if (SummaryOut.Length == 0)
        return;

      string rfilename = _device.Control == SystemControl.Manual ? Outfilename : _device.WorkOrder.plateID.ToString();
      try
      {
        if (!Directory.Exists($"{_device.RootDirectory.FullName}\\Result\\Summary"))
          _ = Directory.CreateDirectory($"{_device.RootDirectory.FullName}\\Result\\Summary");
      }
      catch
      {
        Console.WriteLine($"Failed to create {_device.RootDirectory.FullName}\\Result\\Summary");
        return;
      }

      try
      {
        using (TextWriter jwriter = new StreamWriter($"{_device.RootDirectory.FullName}\\Result\\Summary\\" +
                                                     "Summary_" + rfilename + ".json"))
        {
          var jcontents = JsonConvert.SerializeObject(_plateReport);
          jwriter.Write(jcontents);
        }
      }
      catch
      {
        Console.WriteLine($"Failed to create Plate Report");
      }
    }

    private static void AddToPlateReport(in OutResults outRes)
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

    public void SaveBeadFile(List<WellResult> wellres) //cancels the begin read from endpoint 2
    {
      //write file
      Console.WriteLine($"{DateTime.Now.ToString()} Reporting Background results cloned for save");
      if ((FullFileName != null) && _device.Everyevent)
      {
        try
        {
          File.WriteAllText(FullFileName, GetWellReport());
        }
        catch
        {
          Console.WriteLine($"Failed to write to {FullFileName}");
        }
      }
      if (_device.RMeans)
      {
        ClearSummary();
        if (_plateReport != null)
          _plateReport.Wells.Add(new WellReport
          {
            // OutResults grouped by row and col
            row = SavingWell.RowIdx,
            col = SavingWell.ColIdx
          });
        for (var i = 0; i < wellres.Count; i++)
        {
          WellResult regionNumber = wellres[i];
          OutResults rout = FillOutResults(regionNumber);
          AddOutResults(in rout);
          AddToPlateReport(in rout);
        }
        OutputSummaryFile();
        wellres = null;
      }
      Console.WriteLine($"{DateTime.Now.ToString()} Reporting Background File Save Complete");
      if (File.Exists(WorkOrderPath))
        File.Delete(WorkOrderPath);   //result is posted, delete work order
      //need to clear textbox in UI. this has to be an event
    }

    private static OutResults FillOutResults(WellResult regionNumber)
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
  }
}