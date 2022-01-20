using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MicroCy
{
  public static class ResultReporter
  {
    public static string Outdir { get; set; }  //  user selectable
    public static string Outfilename { get; set; } = "ResultFile";
    public static string WorkOrderPath { get; set; }
    internal static int SavingWellIdx { get; set; }
    internal static string FullFileName { get; private set; }
    private static string _thisRunResultsFileName = null;
    private static readonly StringBuilder DataOut = new StringBuilder();
    private static readonly StringBuilder SummaryOut = new StringBuilder();
    private static PlateReport _plateReport;
    private const string BHEADER = "Preamble,Time(1 us Tick),FSC bg,Viol SSC bg,CL0 bg,CL1 bg,CL2 bg,CL3 bg,Red SSC bg,Green SSC bg," +
                                   "Green B bg,Green C bg,Green Major,Green Minor,Red-Grn Offset,Grn-Viol Offset,Region,Forward Scatter,Violet SSC,CL0," +
                                   "Red SSC,CL1,CL2,CL3,Green SSC,Reporter\r";
    private const string SHEADER = "Row,Col,Region,Bead Count,Median FI,Trimmed Mean FI,CV%\r";

    static ResultReporter()
    {
      Outdir = MicroCyDevice.RootDirectory.FullName;
    }

    internal static void StartNewWellReport()
    {
      _ = DataOut.Clear();
      _ = DataOut.Append(BHEADER);
    }

    public static void OutDirCheck()
    {
      var root = Path.GetPathRoot(Outdir);
      if (!Directory.Exists(root))
      {
        Outdir = MicroCyDevice.RootDirectory.FullName;
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

    internal static void GetNewFileName()
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

      char rowletter = (char)(0x41 + MicroCyDevice.ReadingRow);
      //if(!isTube)
      string colLetter = (MicroCyDevice.ReadingCol + 1).ToString();  //use 0 for tubes and true column for plates
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

    private static void OutputSummaryFile()
    {
      if (SummaryOut.Length > 0)  //end of read session (plate, plate section or tube) write summary stat file
      {
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
    }

    internal static void ClearSummary()
    {
      _ = SummaryOut.Clear();
      _ = SummaryOut.Append(SHEADER);
    }

    private static void GetThisRunFileName()
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

    private static void OutputPlateReport()
    {
      if ((SavingWellIdx == MicroCyDevice.WellsToRead) && (SummaryOut.Length > 0) && MicroCyDevice.PlateReportActive)    //end of read and json results requested
      {
        string rfilename = MicroCyDevice.SystemControl == 0 ? Outfilename : MicroCyDevice.WorkOrder.plateID.ToString();
        try
        {
          if (!Directory.Exists($"{MicroCyDevice.RootDirectory.FullName}\\Result\\Summary"))
            _ = Directory.CreateDirectory($"{MicroCyDevice.RootDirectory.FullName}\\Result\\Summary");
        }
        catch
        {
          Console.WriteLine($"Failed to create {MicroCyDevice.RootDirectory.FullName}\\Result\\Summary");
          return;
        }

        try
        {
          using (TextWriter jwriter = new StreamWriter($"{MicroCyDevice.RootDirectory.FullName}\\Result\\Summary\\" +
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
    }

    private static void AddToPlateReport(in OutResults outRes)
    {
      _plateReport.rpWells[SavingWellIdx].rpReg.Add(new RegionReport
      {
        region = outRes.region,
        count = (uint)outRes.count,
        medfi = outRes.medfi,
        meanfi = outRes.meanfi,
        coefVar = outRes.cv
      });
    }

    public static void SaveBeadFile(List<WellResults> wellres) //cancels the begin read from endpoint 2
    {
      //write file
      Console.WriteLine($"{DateTime.Now.ToString()} Reporting Background results cloned for save");
      if ((FullFileName != null) && MicroCyDevice.Everyevent)
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
      if (MicroCyDevice.RMeans)
      {
        ClearSummary();
        if (_plateReport != null && MicroCyDevice.WellsInOrder.Count != 0)
          _plateReport.rpWells.Add(new WellReport {
            prow = MicroCyDevice.WellsInOrder[SavingWellIdx].rowIdx,
            pcol = MicroCyDevice.WellsInOrder[SavingWellIdx].colIdx
          });
        char[] alphabet = Enumerable.Range('A', 16).Select(x => (char)x).ToArray();
        for (var i = 0; i < wellres.Count; i++)
        {
          WellResults regionNumber = wellres[i];
          SavingWellIdx = SavingWellIdx > MicroCyDevice.WellsInOrder.Count - 1 ? MicroCyDevice.WellsInOrder.Count - 1 : SavingWellIdx;
          OutResults rout = FillOutResults(regionNumber, in alphabet);
          AddOutResults(in rout);
          AddToPlateReport(in rout);
        }
        OutputSummaryFile();
        OutputPlateReport();
        wellres = null;
      }
      Console.WriteLine($"{DateTime.Now.ToString()} Reporting Background File Save Complete");
      if (File.Exists(WorkOrderPath))
        File.Delete(WorkOrderPath);   //result is posted, delete work order
      //need to clear textbox in UI. this has to be an event
    }

    private static OutResults FillOutResults(WellResults regionNumber, in char[] alphabet)
    {
      OutResults rout = new OutResults();
      rout.row = alphabet[MicroCyDevice.WellsInOrder[SavingWellIdx].rowIdx].ToString();
      rout.col = MicroCyDevice.WellsInOrder[SavingWellIdx].colIdx + 1;  //columns are 1 based
      rout.count = regionNumber.RP1vals.Count;
      rout.region = regionNumber.regionNumber;
      var rp1Temp = regionNumber.RP1vals;
      if (rout.count > 2)
        rout.meanfi = rp1Temp.Average();
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
      return rout;
    }
  }
}
