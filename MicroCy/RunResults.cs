using System;
using System.Collections.Generic;
using DIOS.Core.Structs;

namespace DIOS.Core
{
  public class RunResults
  {
    public PlateReport PlateReport { get; } = new PlateReport();
    internal WellResults WellResults { get; } = new WellResults();
    private readonly Device _device;
    private ICollection<int> _regionsToOutput;
    private bool _minPerRegCheckTrigger;
    private readonly ResultingWellStatsData _measuredWellStats = new ResultingWellStatsData();

    public RunResults(Device device)
    {
      _device = device;
    }

    /// <summary>
    /// Setup and Validate the Output Regions
    /// </summary>
    /// <param name="regions"> the region numbers to output</param>
    /// <returns></returns>
    public void SetupRunRegions(ICollection<int> regions)
    {
      _regionsToOutput = regions;
      if (regions == null)
        _regionsToOutput = new List<int>();
    }

    public List<RegionReporterResultVolatile> MakeWellResultsClone()
    {
      return WellResults.GetResultsClone();
    }

    /// <summary>
    /// Checks if MinPerRegion Condition is met. Not thread safe. Supposed to be called after the well is read or in the measurement sequence thread
    /// </summary>
    /// <returns>A positive number or 0, if MinPerRegions is met; otherwise returns a negative number of lacking beads</returns>
    public int MinPerRegionAchieved()
    {
      return WellResults.MinPerAllRegionsAchieved(_device.MinPerRegion);
    }

    internal void StartNewPlateReport()
    {
      PlateReport.Reset();
    }

    public void MakeWellStats()
    {
      var stats = new WellStats(WellResults.Well, MakeWellResultsClone(), _device.BeadCount);
      PlateReport.Add(stats);
      _measuredWellStats.Add(stats.ToString());
    }

    internal void StartNewWell(Well well)
    {
      if (_regionsToOutput == null)
        throw new Exception("SetupRunRegions() must be called before the run");
      WellResults.Reset(well, _regionsToOutput);
      _measuredWellStats.Reset();
      _minPerRegCheckTrigger = false;
    }

    internal void AddProcessedBeadEvent(in ProcessedBead processedBead)
    {
      var count = WellResults.Add(in processedBead);//TODO:move to normal mode case?
      //it also checks region 0, but it is only a trigger, the real check is done in MinPerRegionAchieved()
      if (!_minPerRegCheckTrigger)
        _minPerRegCheckTrigger = count == _device.MinPerRegion;  //see if well is done via sufficient beads in each region

      switch (_device.Mode)
      {
        case OperationMode.Normal:
          break;
        case OperationMode.Calibration:
          break;
        case OperationMode.Verification:
          Verificator.FillStats(in processedBead);
          break;
      }
    }

    public string PublishBeadEvents(bool publishOnlyClassified)
    {
      return WellResults.BeadEventsData.Publish(publishOnlyClassified);
    }

    public string PublishWellStats()
    {
      return _measuredWellStats.Publish();
    }

    internal void EndOfOperationReset()
    {
      _regionsToOutput = null;
    }

    internal bool IsMeasurementTerminationAchieved(Termination type)
    {
      bool stopMeasurement = false;
      switch (type)
      {
        case Termination.MinPerRegion:
          if (_minPerRegCheckTrigger)  //a region made it, are there more that haven't
          {
            if (MinPerRegionAchieved() >= 0)
            {
              stopMeasurement = true;
            }
            _minPerRegCheckTrigger = false;
          }
          break;
        case Termination.TotalBeadsCaptured:
          if (_device.BeadCount >= _device.BeadsToCapture)
          {
            stopMeasurement = true;
          }
          break;
        case Termination.EndOfSample:
          break;
      }

      return stopMeasurement;
    }
  }
}