using System;
using System.Collections.Generic;
using DIOS.Core.Structs;

namespace DIOS.Core
{
  public class RunResults
  {
    public PlateReport PlateReport { get; } = new PlateReport();
    internal WellResults WellResults { get; } = new WellResults();
    private Device _device;
    private ICollection<int> _regionsToOutput;
    private bool _minPerRegCheckTrigger;
    private readonly WellStatsData _wellstatsData = new WellStatsData();

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

    public List<RegionReporterResultVolatile> MakeDeepCopy()
    {
      return WellResults.GetResults();
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

    internal void MakeStats()
    {
      var stats = new WellStats(WellResults.Well, MakeDeepCopy(), _device.BeadCount);
      PlateReport.Add(stats);
      _wellstatsData.Add(stats.ToString());
    }

    internal void StartNewWell(Well well)
    {
      if (_regionsToOutput == null)
        throw new Exception("SetupRunRegions() must be called before the run");
      WellResults.Reset(well, _regionsToOutput);
      _wellstatsData.Reset();
      _minPerRegCheckTrigger = false;
    }

    public void AddRawBeadEvent(ref RawBead beadInfo)
    {
      _device.BeadCount++;
      _device.TotalBeads++;
      _device._beadProcessor.CalculateBeadParams(ref beadInfo);
      _device.DataOut.Enqueue(beadInfo);
      FillWellResults(in beadInfo);
      switch (_device.Mode)
      {
        case OperationMode.Normal:
          break;
        case OperationMode.Calibration:
          break;
        case OperationMode.Verification:
          Verificator.FillStats(in beadInfo);
          break;
      }
    }

    internal string PublishBeadEvents()
    {
      return WellResults.BeadEventsData.Publish(_device.OnlyClassified);
    }

    internal string PublishWellStats()
    {
      return _wellstatsData.Publish();
    }

    private void FillWellResults(in RawBead outBead)
    {
      var count = WellResults.Add(in outBead);
      //it also checks region 0, but it is only a trigger, the real check is done in MinPerRegionAchieved()
      if (!_minPerRegCheckTrigger)
        _minPerRegCheckTrigger = count == _device.MinPerRegion;  //see if assay is done via sufficient beads in each region
    }

    internal void EndOfOperationReset()
    {
      _regionsToOutput = null;
    }

    internal void TerminationReadyCheck()
    {
      switch (_device.TerminationType)
      {
        case Termination.MinPerRegion:
          if (_minPerRegCheckTrigger)  //a region made it, are there more that haven't
          {
            if (MinPerRegionAchieved() >= 0)
              _device.StartStateMachine();
            _minPerRegCheckTrigger = false;
          }
          break;
        case Termination.TotalBeadsCaptured:
          if (_device.BeadCount >= _device.BeadsToCapture)
            _device.StartStateMachine();
          break;
        case Termination.EndOfSample:
          break;
      }
    }
  }
}