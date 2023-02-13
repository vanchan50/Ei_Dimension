using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DIOS.Core;

namespace DIOS.Application
{
  public class RunResults
  {
    public ConcurrentQueue<ProcessedBead> DataOut { get; } = new ConcurrentQueue<ProcessedBead>();
    public BeadEventSink OutputBeadsCollector { get; } = new BeadEventSink(2000000);
    public PlateReport PlateReport { get; } = new PlateReport();
    public WellResults WellResults { get; } = new WellResults();
    public ResultsProcessor ResultsProc { get; }
    private readonly Device _device;
    private ICollection<int> _regionsToOutput;
    private bool _minPerRegCheckTrigger;
    private readonly ResultingWellStatsData _measuredWellStats = new ResultingWellStatsData();
    private readonly DIOSApp _diosApp;

    public RunResults(Device device, DIOSApp diosApp)
    {
      _device = device;
      _diosApp = diosApp;
      ResultsProc = new ResultsProcessor(_device, this);
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
    public int MinPerRegionAchieved(int minPerRegion)
    {
      return WellResults.MinPerAllRegionsAchieved(minPerRegion);
    }

    public void StartNewPlateReport()
    {
      PlateReport.Reset();
    }

    public void MakeWellStats()
    {
      var stats = new WellStats(WellResults.Well, MakeWellResultsClone(), _device.BeadCount);
      PlateReport.Add(stats);
      _measuredWellStats.Add(stats.ToString());
    }

    public void StartNewWell(Well well)
    {
      if (_regionsToOutput == null)
        throw new Exception("SetupRunRegions() must be called before the run");
      WellResults.Reset(well, _regionsToOutput);
      _measuredWellStats.Reset();
      _minPerRegCheckTrigger = false;
      ResultsProc.NewWellStarting();
    }

    public void AddProcessedBeadEvent(in ProcessedBead processedBead)
    {
      var count = WellResults.Add(in processedBead);//TODO:move to normal mode case?
      //it also checks region 0, but it is only a trigger, the real check is done in MinPerRegionAchieved()
      if (!_minPerRegCheckTrigger)
        _minPerRegCheckTrigger = count == _diosApp.MinPerRegion;  //see if well is done via sufficient beads in each region

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

    public BeadEventsData PublishBeadEvents()
    {
      return WellResults.BeadEventsData;
    }

    public string PublishWellStats()
    {
      return _measuredWellStats.Publish();
    }

    public void EndOfOperationReset()
    {
      _regionsToOutput = null;
    }

    public bool IsMeasurementTerminationAchieved()
    {
      bool stopMeasurement = false;
      switch (_diosApp.TerminationType)
      {
        case Termination.MinPerRegion:
          if (_minPerRegCheckTrigger)  //a region made it, are there more that haven't
          {
            if (MinPerRegionAchieved(_diosApp.MinPerRegion) >= 0)
            {
              stopMeasurement = true;
            }
            _minPerRegCheckTrigger = false;
          }
          break;
        case Termination.TotalBeadsCaptured:
          if (_device.BeadCount >= _diosApp.TotalBeadsToCapture)
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