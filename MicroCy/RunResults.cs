using System;
using System.Collections.Generic;
using System.Linq;
using DIOS.Core.Structs;

namespace DIOS.Core
{
  public class RunResults
  {
    public bool Reg0stats { get; set; }
    private Device _device;
    private ICollection<int> _regionsToOutput;
    private WellResults _wellResults;
    private bool _minPerRegCheckTrigger;
    public PlateReport PlateReport { get; private set; }

    public RunResults(Device device)
    {
      _device = device;
    }

    public bool SetupRunRegions(ICollection<int> regions)
    {
      //validate data
      //TODO: reg0stats can stay a UI thing, _regionsToOutput should have 0 in itself if necessary. that has to be done in the UI
      if (regions == null)//count !=0
        return false;
      if (regions.Count == 0 && !Reg0stats)
        return false;
      _regionsToOutput = regions;
      return true;
    }

    internal void StartNewPlateReport()
    {
      PlateReport = new PlateReport();
    }

    public void StartNewWell(Well well)
    {
       _wellResults.Reset(well, _regionsToOutput, Reg0stats);
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