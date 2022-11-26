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

    internal void StartNewBeadEventReport()
    {
      GetNewBeadEventFileName();
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
      FullBeadEventFileName = $"{Outdir}\\AcquisitionData\\{Outfilename}{rowletter}{colLetter}_{Date}.csv";
    }

    private void WriteResultDataToFile()
    {
      var contents = _device.Results.PublishWellStats();
      if (!_device.RMeans || contents.Length == 0)
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

    public void OutputPlateReport()
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
        var contents = _device.Results.PlateReport.JSONify();
        var fileName = $"{directoryName}" +
                       "\\Summary_" + rfilename + "_" + Date + ".json";
        using (TextWriter jwriter = new StreamWriter(fileName))
        {
          jwriter.Write(contents);
          Console.WriteLine($"Plate Report saved as {fileName}");
        }
      }
      catch
      {
        Console.WriteLine("Failed to create Plate Report");
      }
    }

    public void PublishEverything()
    {
      _device.Results.MakeStats();  //TODO: need to check if the well is finished reading before call
      WriteResultDataToFile();
      SaveBeadEventFile();
      DoSomethingWithWorkOrder();
      Console.WriteLine($"{DateTime.Now.ToString()} Reporting Background File Save Complete");
    }

    private void DoSomethingWithWorkOrder()
    {
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
        var contents = _device.Results.PublishBeadEvents();
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