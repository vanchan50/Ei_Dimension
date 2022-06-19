using System;
using System.Collections.Generic;
using DIOS.Core.Structs;

namespace DIOS.Core
{
  public class RunResults
  {
    private Device _device;
    private ICollection<int> _regionsToOutput;
    private readonly MeasurementResults _measurementResults = new MeasurementResults();
    private bool _minPerRegCheckTrigger;
    public PlateReport PlateReport { get; } = new PlateReport();
    private readonly BeadEventsData _beadEventsData = new BeadEventsData();
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

    internal void StartNewPlateReport()
    {
      PlateReport.Reset();
    }

    internal void MakeStats()
    {
      var stats = new WellStats(_measurementResults.Well, MakeDeepCopy());
      PlateReport.Add(stats);
      _wellstatsData.Add(stats.ToString());
    }

    internal void StartNewWell(Well well)
    {
      if (_regionsToOutput == null)
        throw new Exception("SetupRunRegions() must be called before the run");
      _measurementResults.Reset(well, _regionsToOutput);
      _beadEventsData.Reset();
      _wellstatsData.Reset();
      _minPerRegCheckTrigger = false;
    }

    internal void AddBeadEvent(ref BeadInfoStruct beadInfo)
    {
      _device.BeadCount++;
      _device.TotalBeads++;
      _device._beadProcessor.CalculateBeadParams(ref beadInfo);
      _device.DataOut.Enqueue(beadInfo);
      if (_device.SaveIndividualBeadEvents)
        _beadEventsData.Add(in beadInfo);
      FillWellResults(in beadInfo);
      //accum stats for run as a whole, used during aligment and QC
      _device._beadProcessor.FillCalibrationStatsRow(in beadInfo);
      _device._beadProcessor.FillBackgroundAverages(in beadInfo);
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

    public string PublishBeadEvents()
    {
      return _beadEventsData.Publish(_device.OnlyClassified);
    }

    public string PublishWellStats()
    {
      return _wellstatsData.Publish();
    }

    private void FillWellResults(in BeadInfoStruct outBead)
    {
      var count = _measurementResults.Add(in outBead);
      //it also checks region 0, but it is only a trigger, the real check is done in MinPerRegionAchieved()
      if (!_minPerRegCheckTrigger)
        _minPerRegCheckTrigger = count == _device.MinPerRegion;  //see if assay is done via sufficient beads in each region
    }


    public List<RegionResultVolatile> MakeDeepCopy()
    {
      return _measurementResults.GetResults();
    }

    /// <summary>
    /// Checks if MinPerRegion Condition is met. Not thread safe. Supposed to be called after the well is read or in the measurement sequence thread
    /// </summary>
    /// <returns>A positive number or 0, if MinPerRegions is met; otherwise returns a negative number of lacking beads</returns>
    public int MinPerRegionAchieved()
    {
      return _measurementResults.MinPerAllRegionsAchieved(_device.MinPerRegion);
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