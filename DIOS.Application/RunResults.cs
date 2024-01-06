using DIOS.Core;
using DIOS.Core.HardwareIntercom;

namespace DIOS.Application;

public class RunResults
{
  public BeadEventSink<RawBead> RawBeadsCollector { get; }
  private BeadEventSink<ProcessedBead> ProcessedBeadsCollector { get; } = new(2_000_000);
  public PlateReport PlateReport { get; } = new ();
  public WellResults CurrentWellResults { get; } = new ();
  private readonly Device _device;
  private readonly DIOSApp _diosApp;
  private bool _isFrozen = false;

  public RunResults(Device device, DIOSApp diosApp, BeadEventSink<RawBead> sink)
  {
    _device = device;
    _diosApp = diosApp;
    RawBeadsCollector = sink;
  }

  public List<RegionReporterResultVolatile> MakeWellResultsClone()
  {
    List<RegionReporterResultVolatile> ret;
    try
    {
      ret = CurrentWellResults.GetResultsClone();
    }
    catch
    {
      //case for UpdateCurrentStats() which is called frequently and can be called in a transition(between wells) time
      ret = new List<RegionReporterResultVolatile>();
    }
    return ret;
  }

  /// <summary>
  /// Checks if MinPerRegion Condition is met. Not thread safe. Supposed to be called after the well is read or in the measurement sequence thread
  /// </summary>
  /// <returns>A positive number or 0, if MinPerRegions is met; otherwise returns a negative number of lacking beads</returns>
  internal int MinPerRegionAchieved(int minPerRegion)
  {
    return CurrentWellResults.MinPerAllRegionsAchieved(minPerRegion);
  }

  public void StartNewPlateReport(PlateSize plateSize, string plateId = null)
  {
    PlateReport.Reset(plateSize, plateId);
  }

  internal WellStats MakeWellStats()
  {
    return new WellStats(CurrentWellResults.Well, MakeWellResultsClone(), _device.BeadCount);
  }

  public void StartNewWell(Well well)
  {
    CurrentWellResults.Reset(well);
    RawBeadsCollector.Clear();
    ProcessedBeadsCollector.Clear();
    _isFrozen = false;
  }

  public void AddProcessedBeadEvent(in ProcessedBead processedBead)
  {
    if (_isFrozen)
      return;
    ProcessedBeadsCollector.Add(in processedBead);
    var countForTheRegion = CurrentWellResults.Add(in processedBead);//TODO:move to normal mode case?
    //it also checks region 0, but it is only a trigger, the real check is done in MinPerRegionAchieved()
    if (!_diosApp.Terminator.MinPerRegCheckTrigger)
      _diosApp.Terminator.MinPerRegCheckTrigger = countForTheRegion == _diosApp.Terminator.MinPerRegion;  //see if well is done via sufficient beads in each region

    switch (_device.Mode)
    {
      case OperationMode.Normal:
        break;
      case OperationMode.Calibration:
        break;
      case OperationMode.Verification:
        _diosApp.Verificator.Add(in processedBead);
        break;
    }
  }

  /// <summary>
  /// Freezes well results update until next well is started
  /// </summary>
  public void FreezeWellResults()
  {
    _isFrozen = true;
  }

  /// <summary>
  /// The method to get all the new beads in a measurement cycle
  /// </summary>
  /// <returns>IEnumerable of all the beads since last query</returns>
  /// <exception cref="ArgumentOutOfRangeException"></exception>
  public IEnumerable<ProcessedBead> GetNewBeads()
  {
    return ProcessedBeadsCollector.GetNewBeadsEnumerable();
  }

  public IEnumerable<ProcessedBead> PublishBeadEvents()
  {
    return ProcessedBeadsCollector.GetAllBeadsEnumerable();
  }
}