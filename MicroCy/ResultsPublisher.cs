using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DIOS.Core
{
  public class ResultsPublisher
  {
    //TODO:these should all be independent objects in results, with a function Publish()
    public string Outdir { get; set; }  //  user selectable
    public string Outfilename { get; set; } = "ResultFile";
    public bool MakePlateReport { get; set; }
    public string WorkOrderPath { get; set; }
    internal string FullBeadEventFileName { get; private set; }
    private string _thisRunStatsFileName;
    private readonly StringBuilder BeadEventDataOut = new StringBuilder();
    private readonly StringBuilder StatsData = new StringBuilder();
    private Device _device;
    private const string BHEADER = "Preamble,Time(1 us Tick),FSC bg,Viol SSC bg,CL0 bg,CL1 bg,CL2 bg,CL3 bg,Red SSC bg,Green SSC bg," +
                                   "Green B bg,Green C bg,Green B,Green C,Red-Grn Offset,Grn-Viol Offset,Region,Forward Scatter,Violet SSC,CL0," +
                                   "Red SSC,CL1,CL2,CL3,Green SSC,Reporter\r";
    private const string SHEADER = "Row,Col,Region,Bead Count,Median FI,Trimmed Mean FI,CV%\r";

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

    /// <summary>
    /// Perform the check for the case of a removed drive
    /// </summary>
    public void OutDirCheck()
    {
      var root = Path.GetPathRoot(Outdir);
      if (!Directory.Exists(root))
      {
        Outdir = Device.RootDirectory.FullName;
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

    internal void AddBeadEvent(in BeadInfoStruct beadInfo)
    {
      _device.DataOut.Enqueue(beadInfo);
      if (_device.SaveIndividualBeadEvents)
        _ = BeadEventDataOut.Append(beadInfo.ToString());
    }

    private void AddToStatsData(RegionStats oResult)
    {
      _ = StatsData.Append(oResult);
    }

    private void WriteResultDataToFile()
    {
      if (!_device.RMeans || StatsData.Length == 0)
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
        var contents = StatsData.ToString();
        File.AppendAllText(_thisRunStatsFileName, contents);
        Console.WriteLine($"Results summary saved as {_thisRunStatsFileName}");
      }
      catch
      {
        Console.WriteLine($"Failed to append data to {_thisRunStatsFileName}");
      }
    }

    internal void ResetResultData()
    {
      _ = StatsData.Clear();
      _ = StatsData.Append(SHEADER);
      GetThisRunResultsFileName();
      WriteResultDataToFile();
    }

    private void GetThisRunResultsFileName()
    {
      OutDirCheck();
      string date = DateTime.Now.ToString("dd.MM.yyyy.hh-mm-ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
      _thisRunStatsFileName = $"{Outdir}\\AcquisitionData\\Results_{Outfilename}_{date}.csv";
    }

    public void OutputPlateReport()
    {
      if (!MakePlateReport)
      {
        Console.WriteLine("Plate Report Inactive");
        return;
      }

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
          var jcontents = JsonConvert.SerializeObject(_device.Results.PlateReport);
          jwriter.Write(jcontents);
          Console.WriteLine($"Plate Report saved as {fileName}");
        }
      }
      catch
      {
        Console.WriteLine("Failed to create Plate Report");
      }
    }

    public void SaveBeadFile(List<RegionResult> wellres, Well savingWell) //cancels the begin read from endpoint 2
    {
      SaveBeadEventFile();

      StatsData.Clear();
      for (var i = 0; i < wellres.Count; i++)
      {
        RegionStats rout = new RegionStats(wellres[i], savingWell);
        AddToStatsData(rout);
        _device.Results.PlateReport.AddResultsToLastWell(rout);//TODO: done not in the right place
      }
      WriteResultDataToFile();
      wellres = null;

      Console.WriteLine($"{DateTime.Now.ToString()} Reporting Background File Save Complete");
      if (File.Exists(WorkOrderPath))
        File.Delete(WorkOrderPath);   //result is posted, delete work order
      //need to clear textbox in UI. this has to be an event
    }

    private void SaveBeadEventFile()
    {
      if (!_device.SaveIndividualBeadEvents)
        Console.WriteLine("Bead event Saving inactive");

      try
      {
        var contents = BeadEventDataOut.ToString();
        File.WriteAllText(FullBeadEventFileName, contents);
        Console.WriteLine($"Bead event saved as {FullBeadEventFileName}");
      }
      catch
      {
        Console.WriteLine($"Failed to write Bead event to {FullBeadEventFileName}");
      }
    }
  }
}