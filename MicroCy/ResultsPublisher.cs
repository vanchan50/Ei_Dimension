using System;
using System.IO;

namespace DIOS.Core
{
  public class ResultsPublisher
  {
    public string Outdir { get; set; }  //  user selectable
    public string Outfilename { get; set; } = "ResultFile";
    public bool MakePlateReport { get; set; }
    public string WorkOrderPath { get; set; }
    internal string FullBeadEventFileName { get; private set; }
    private string _folder;

    public string Date
    {
      get
      {
        return DateTime.Now.ToString("dd.MM.yyyy.HH-mm-ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"));
      }
    }

    private string _thisRunStatsFileName;
    private Device _device;

    public ResultsPublisher(Device device, string folder)
    {
      _device = device;
      _folder = folder;
      Outdir = _folder;
    }

    internal void StartNewBeadEventReport(Well currentWell)
    {
      GetNewBeadEventFileName(currentWell);
    }

    /// <summary>
    /// Perform the check for the case of a removed drive
    /// </summary>
    public void OutDirCheck()
    {
      var root = Path.GetPathRoot(Outdir);
      if (!Directory.Exists(root))
      {
        Outdir = _folder;
      }
    }

    private void GetNewBeadEventFileName(Well currentWell)
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

      char rowletter = (char)(0x41 + currentWell.RowIdx);
      string colLetter = (currentWell.ColIdx + 1).ToString();
      FullBeadEventFileName = $"{Outdir}\\AcquisitionData\\{Outfilename}{rowletter}{colLetter}_{Date}.csv";
    }

    public void WriteResultDataToFile(string WellStats)
    {
      if (!_device.RMeans || WellStats.Length == 0)
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
        File.AppendAllText(_thisRunStatsFileName, WellStats);
        Console.WriteLine($"Results summary saved as {_thisRunStatsFileName}");
      }
      catch
      {
        Console.WriteLine($"Failed to append data to {_thisRunStatsFileName}");
      }
    }

    internal void ResetResultData()
    {
      GetThisRunResultsFileName();
      if (!_device.RMeans)
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
        File.AppendAllText(_thisRunStatsFileName, WellStatsData.HEADER);
        Console.WriteLine($"Results summary file created {_thisRunStatsFileName}");
      }
      catch
      {
        Console.WriteLine($"Failed to create file {_thisRunStatsFileName}");
      }
    }

    private void GetThisRunResultsFileName()
    {
      OutDirCheck();
      _thisRunStatsFileName = $"{Outdir}\\AcquisitionData\\Results_{Outfilename}_{Date}.csv";
    }

    public void OutputPlateReport(string PlateReportJSON)
    {
      if (!MakePlateReport)
      {
        Console.WriteLine("Plate Report Inactive");
        return;
      }

      string rfilename = _device.Control == SystemControl.Manual ? Outfilename : _device.WorkOrder.plateID.ToString();
      var directoryName = $"{_folder}\\Result\\Summary";
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
                       "\\Summary_" + rfilename + "_" + Date + ".json";
        using (TextWriter jwriter = new StreamWriter(fileName))
        {
          jwriter.Write(PlateReportJSON);
          Console.WriteLine($"Plate Report saved as {fileName}");
        }
      }
      catch
      {
        Console.WriteLine("Failed to create Plate Report");
      }
    }

    public void DoSomethingWithWorkOrder()
    {
      if (File.Exists(WorkOrderPath))
        File.Delete(WorkOrderPath);   //result is posted, delete work order
      //need to clear textbox in UI. this has to be an event
    }

    public void SaveBeadEventFile(string beadEventsList)
    {
      if (!_device.SaveIndividualBeadEvents)
        Console.WriteLine("Bead event Saving inactive");

      try
      {
        File.WriteAllText(FullBeadEventFileName, beadEventsList);
        Console.WriteLine($"Bead event saved as {FullBeadEventFileName}");
      }
      catch
      {
        Console.WriteLine($"Failed to write Bead event to {FullBeadEventFileName}");
      }
    }
  }
}