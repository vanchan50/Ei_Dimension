using System.Reflection;
using DIOS.Application.Domain;
using DIOS.Application.FileIO;
using DIOS.Application.SerialIO;
using DIOS.Core;
using DIOS.Core.HardwareIntercom;

namespace DIOS.Application;

public class DIOSApp
{
  public Device Device { get; }
  public MapController MapController { get; }
  public WorkOrderController WorkOrderController { get; }
  public DirectoryInfo RootDirectory { get; }
  public ResultsPublisher Publisher { get; }
  public RunResults Results { get; }
  public ResultsProcessor ResultsProc { get; }  //TODO make private
  public SystemControl Control { get; set; } = SystemControl.Manual;
  public ReadTerminator Terminator { get; }
  public bool RunPlateContinuously { get; set; }
  public Verificator Verificator { get; }
  public IBarcodeReader BarcodeReader { get; }
  public readonly string BUILD = Assembly.GetCallingAssembly().GetName().Version.ToString();
  public ILogger Logger { get; }
  public NormalizationSettings Normalization
  {
    get { return _beadProcessor.Normalization; }
  }
  /// <summary>
  /// A coefficient for high dynamic range channel
  /// </summary>
  public float Compensation
  {
    get
    {
      return _compensation;
    }
    set
    {
      _compensation = value;
      _beadProcessor._actualCompensation = value / 100f;
    }
  }
  /// <summary>
  /// An inverse coefficient for the bead Reporter<br></br>
  /// Applied before Reporter Normalization
  /// </summary>
  public float ReporterScaling
  {
    get
    {
      return _reporterScaling;
    }
    set
    {
      _reporterScaling = value;
      _beadProcessor._inverseReporterScaling = 1 / value;
    }
  }
  public readonly BeadProcessor _beadProcessor;
  private float _reporterScaling = 1;
  private float _compensation = 1;

  public DIOSApp(string rootDirectory, ILogger logger)
  {
    RootDirectory = new(rootDirectory);
    SetSystemDirectories(); 
    Logger = logger;
    MapController = new MapController($"{rootDirectory}\\Config", Logger);
    WorkOrderController = new WorkOrderController($"{rootDirectory}\\WorkOrder");
    Publisher = new ResultsPublisher(rootDirectory, Logger);
    Device = new Device(Logger);
    Results = new RunResults(Device, this, new BeadEventSink<RawBead>(2_000_000));
    Terminator = new ReadTerminator(Results.MinPerRegionAchieved);
    Verificator = new Verificator();
    BarcodeReader = new USBBarcodeReader(Logger);
    _beadProcessor = new BeadProcessor();
    ResultsProc = new ResultsProcessor(Device, Results, Terminator, _beadProcessor);
  }

  public async Task StartOperation(IReadOnlyCollection<Well> wells, PlateSize plateSize, string plateId = null)
  {
    Publisher.ResultsFile.MakeNew();
    Results.StartNewPlateReport(plateSize, plateId);
    if (Device.Mode is OperationMode.Verification)
    {
      var regions = wells.First().ActiveRegions.Select(x => x.Number);
      Verificator.Reset(regions);
    }
    Logger.Log(Publisher.ReportActivePublishingFlags());

    if (Device.Mode != OperationMode.Normal)
    {
      Normalization.SuspendForTheRun();
      Logger.Log("Normalization: Suspended;");
    }
    else
    {
      if (Normalization.IsEnabled)
        Logger.Log("Normalization: Enabled");
      else
        Logger.Log("Normalization: Disabled");
    }

    Logger.Log($"SensitivityChannel: {_beadProcessor.SensitivityChannel}");
    await Device.StartOperation(wells, Results.RawBeadsCollector);
  }

  private void SetSystemDirectories()
  {
    List<string> subDirectories = new List<string> { "Config", "WorkOrder", "SavedImages", "Archive", "Status" };
    try
    {
      foreach (var d in subDirectories)
      {
        RootDirectory.CreateSubdirectory(d);
      }
      Directory.CreateDirectory(RootDirectory.FullName + @"\Result" + @"\Detail");
    }
    catch
    {
      Console.WriteLine("Directory Creation Failed");
    }
  }

  public void SetMap(MapModel map)
  {
    _beadProcessor.SetMap(map);
  }

  public WellType GetWellStateForPictogram()
  {
    var type = WellType.Success;

    //feature only for Normal mode, MinPerRegion Termination
    if (Device.Mode != OperationMode.Normal
        || Terminator.TerminationType != Termination.MinPerRegion)
      return type;

    var lacking = Results.MinPerRegionAchieved(Terminator.MinPerRegion);
    //not achieved
    if (lacking < 0)
    {
      //if lacking more then 25% of minperregion beads 
      if (-lacking > Terminator.MinPerRegion * 0.25)
      {
        type = WellType.Fail;
      }
      else
      {
        type = WellType.LightFail;
      }
    }
    return type;
  }

  public void SaveWellFiles(WellStats stats)
  {
    _ = Task.Run(() =>
    {
      Results.PlateReport.Add(stats);
      Results.CurrentWellResults.AddWellStats(stats);
      var wellResults = Results.CurrentWellResults.PublishWellStats();
      Publisher.ResultsFile.AppendAndWrite(wellResults); //makewellstats should be awaited only for this method
      WorkOrderController.DoSomethingWithWorkOrder();
    });
    Publisher.BeadEventFile.CreateAndWrite(Results.PublishBeadEvents());
  }
}