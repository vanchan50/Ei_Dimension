﻿using DIOS.Core;

namespace DIOS.Application;

public class RunResults
{
  public BeadEventSink OutputBeadsCollector { get; }
  public PlateReport PlateReport { get; } = new ();
  public WellResults CurrentWellResults { get; } = new ();
  private readonly Device _device;
  private IReadOnlyCollection<int> _regionsToOutput = null;
  private readonly DIOSApp _diosApp;
  private bool _isFrozen = false;

  public RunResults(Device device, DIOSApp diosApp, BeadEventSink sink)
  {
    _device = device;
    _diosApp = diosApp;
    OutputBeadsCollector = sink;
  }

  /// <summary>
  /// Setup and Validate the Output Regions
  /// </summary>
  /// <param name="regions"> the region numbers to output</param>
  /// <returns></returns>
  public void SetupRunRegions(IReadOnlyCollection<Well> wells)
  {
    _regionsToOutput = wells.First().Regions;
    _regionsToOutput ??= new List<int>();//TODO:not necessary anymore? since wells have at least new list<> (null can be passed)
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

  public void StartNewPlateReport(string plateId = null)
  {
    PlateReport.Reset(plateId);
  }

  public void MakeWellStats()
  {
    var stats = new WellStats(CurrentWellResults.Well, MakeWellResultsClone(), _device.BeadCount);
    PlateReport.Add(stats);
    CurrentWellResults.AddStats(stats);
  }

  public void StartNewWell(Well well)
  {
    if (_regionsToOutput is null)
      throw new Exception("SetupRunRegions() must be called before the run");
    CurrentWellResults.Reset(well, _regionsToOutput);
    OutputBeadsCollector.Clear();
    _isFrozen = false;
  }

  public void AddProcessedBeadEvent(in ProcessedBead processedBead)
  {
    if (_isFrozen)
      return;

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
        _diosApp.Verificator.FillStats(in processedBead);
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
    return OutputBeadsCollector.GetNewBeadsEnumerable();
  }

  public IEnumerable<ProcessedBead> PublishBeadEvents()
  {
    return OutputBeadsCollector.GetAllBeadsEnumerable();
  }

  public void EndOfOperationReset()
  {
    _regionsToOutput = null!;
  }
}