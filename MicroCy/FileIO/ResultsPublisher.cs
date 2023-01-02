using System;
using System.IO;
using System.Globalization;

namespace DIOS.Core.FileIO
{
  public class ResultsPublisher
  {
    public string Outdir { get; set; }  //  user selectable
    public string Outfilename { get; set; } = "ResultFile";
    public bool IsPlateReportPublishingActive { get; set; }
    public bool IsResultsPublishingActive { get; set; }
    public bool IsBeadEventPublishingActive { get; set; }
    public bool IsLegacyPlateReportPublishingActive { get; set; }
    public string WorkOrderPath { get; set; }
    public ResultsFileWriter ResultsFile { get; }
    public BeadEventFileWriter BeadEventFile { get; }
    public PlateReportFileWriter PlateReportFile { get; }
    public LegacyReportFileWriter LegacyReportFile { get; }
    public PlateStatusFileWriter PlateStatusFile { get; }
    public string ReportFileName => _device.Control == SystemControl.Manual ? Outfilename : _device.WorkOrder.plateID.ToString();
    public string Date => DateTime.Now.ToString("dd.MM.yyyy.HH-mm-ss", CultureInfo.CreateSpecificCulture("en-GB"));

    public const string DATAFOLDERNAME = "AcquisitionData";
    public const string STATUSFOLDERNAME = "Status";
    public readonly string SUMMARYFOLDERNAME;

    private string _folder;
    private Device _device;
    internal ILogger _logger;

    public ResultsPublisher(Device device, string folder, ILogger logger)
    {
      _device = device;
      _folder = folder;
      Outdir = _folder;
      SUMMARYFOLDERNAME = $"{_folder}\\Result\\Summary";
      ResultsFile = new ResultsFileWriter(this);
      BeadEventFile = new BeadEventFileWriter(this);
      PlateReportFile = new PlateReportFileWriter(this);
      LegacyReportFile = new LegacyReportFileWriter(this);
      PlateStatusFile = new PlateStatusFileWriter(this);
      _logger = logger;
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

    public void DoSomethingWithWorkOrder()
    {
      if (File.Exists(WorkOrderPath))
        File.Delete(WorkOrderPath);   //result is posted, delete work order
      //need to clear textbox in UI. this has to be an event
    }

    public bool OutputDirectoryExists(string fullDirectoryPath)
    {
      bool directoryExists = true;
      try
      {
        OutDirCheck();
        if (!Directory.Exists(fullDirectoryPath))
          Directory.CreateDirectory(fullDirectoryPath);
      }
      catch
      {
        _logger.Log($"Failed to create {fullDirectoryPath}");
        directoryExists = false;
      }

      return directoryExists;
    }
  }
}