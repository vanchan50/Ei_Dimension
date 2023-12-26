using System.Globalization;
using System.Text;
using DIOS.Core;

namespace DIOS.Application.FileIO;

public class ResultsPublisher
{
  public string Outdir { get; set; }  //  user selectable
  public string Outfilename { get; set; } = "ResultFile";
  public bool IsPlateReportPublishingActive { get; set; }
  public bool IncludeReg0InPlateSummary { get; set; }
  public bool IsResultsPublishingActive { get; set; }
  public bool IsBeadEventPublishingActive { get; set; }
  public bool IsLegacyPlateReportPublishingActive { get; set; }
  public bool IsOnlyClassifiedBeadsPublishingActive { get; set; }
  public bool IsOEMModeActive { get; set; }
  public ResultsFileWriter ResultsFile { get; }
  public BeadEventFileWriter BeadEventFile { get; }
  public PlateReportFileWriter PlateReportFile { get; }
  public LegacyReportFileWriter LegacyReportFile { get; }
  public PlateStatusFileWriter PlateStatusFile { get; }
  public CalibrationReportFileWriter CalibrationFile { get; }
  public VerificationReportFileWriter VerificationFile { get; }
  public string Date => DateTime.Now.ToString("dd.MM.yyyy.HH-mm-ss", CultureInfo.CreateSpecificCulture("en-GB"));
  public string LegacyReportDate => DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.CreateSpecificCulture("en-GB"));

  public const string DATAFOLDERNAME = "AcquisitionData";
  public const string STATUSFOLDERNAME = "Status";
  public const string RESULTFOLDERNAME = "Result";
  public readonly string SUMMARYFOLDERNAME;

  private string _folder;
  internal ILogger _logger;

  public ResultsPublisher(string folder, ILogger logger)
  {
    _folder = folder;
    Outdir = _folder;
    SUMMARYFOLDERNAME = $"{_folder}\\Result\\Summary";
    ResultsFile = new(this);
    BeadEventFile = new(this);
    PlateReportFile = new(this);
    LegacyReportFile = new(this);
    PlateStatusFile = new(this);
    CalibrationFile = new(this);
    VerificationFile = new(this);
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

  public string ReportActivePublishingFlags()
  {
    var bldr = new StringBuilder(256);
    bldr.AppendLine("PlateReportPublishing: " + IsFlagActive(IsPlateReportPublishingActive));
    bldr.AppendLine("IncludeRegion0InPlateSummary: " + IsFlagActive(IncludeReg0InPlateSummary));
    bldr.AppendLine("ResultsPublishing: " + IsFlagActive(IsResultsPublishingActive));
    bldr.AppendLine("BeadEventPublishing: " + IsFlagActive(IsBeadEventPublishingActive));
    bldr.AppendLine("LegacyPlateReportPublishing: " + IsFlagActive(IsLegacyPlateReportPublishingActive));
    bldr.AppendLine("OnlyClassifiedBeadsPublishing: " + IsFlagActive(IsOnlyClassifiedBeadsPublishingActive));

    return bldr.ToString();

    string IsFlagActive(bool flag)
    {
      return flag ? "Active" : "Inactive";
    }
  }
}