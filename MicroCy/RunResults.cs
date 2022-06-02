using System;
using System.Collections.Generic;
using System.Linq;
using DIOS.Core.Structs;

namespace DIOS.Core
{
  public class RunResults
  {
    private Device _device;
    private ICollection<int> _regionsToOutput;
    private WellResults _wellResults;
    private bool _minPerRegCheckTrigger;
    public PlateReport PlateReport { get; private set; }

    public RunResults(Device device)
    {
      _device = device;
      _wellResults = new WellResults();
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
      PlateReport = new PlateReport();
    }

    internal void StartNewWell(Well well)
    {
       _wellResults.Reset(well, _regionsToOutput);
       _minPerRegCheckTrigger = false;
    }

    internal void FillActiveWellResults(in BeadInfoStruct outBead)
    {
      var count = _wellResults.Add(in outBead);
      //it also checks region 0, but it is only a trigger, the real check is done in MinPerRegionAchieved()
      if (!_minPerRegCheckTrigger)
        _minPerRegCheckTrigger = count == _device.MinPerRegion;  //see if assay is done via sufficient beads in each region
    }

    //TODO:bad design to have it public. get rid somehow
    public List<RegionResult> MakeDeepCopy()
    {
      return _wellResults.MakeDeepCopy();
    }

    /// <summary>
    /// Checks if MinPerRegion Condition is met. Not thread safe. Supposed to be called after the well is read or in the measurement sequence thread
    /// </summary>
    /// <returns>A positive number or 0, if MinPerRegions is met; otherwise returns a negative number of lacking beads</returns>
    public int MinPerRegionAchieved()
    {
      return _wellResults.MinPerAllRegionsAchieved(_device.MinPerRegion);
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