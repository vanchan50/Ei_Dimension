using System;
using System.Collections.Generic;
using System.Linq;

namespace DIOS.Core
{
  public class RunResults
  {
    public bool Reg0stats { get; set; }
    private Device _device;
    private ICollection<int> _regionsToOutput;
    private List<RegionResult> _wellResults = new List<RegionResult>();
    private SortedDictionary<ushort, int> _regionIndexDictionary = new SortedDictionary<ushort, int>();
    private bool _chkRegionCount;
    private int _minPerRegCount;  //discard region 0 from calculation
    public PlateReport PlateReport { get; private set; }

    public RunResults(Device device)
    {
      _device = device;
    }

    public void SetupRunRegions(ICollection<int> regions)
    {
      //TODO: validate data
      _regionsToOutput = regions;
    }

    internal void StartNewPlateReport()
    {
      PlateReport = new PlateReport();
    }

    public void Reset()
    {
      _wellResults.Clear();
      _regionIndexDictionary.Clear();
      if (_regionsToOutput != null && _regionsToOutput.Count != 0)
      {
        foreach (var region in _device.MapCtroller.ActiveMap.regions)
        {
          if (_regionsToOutput.Contains(region.Number))
          {
            _regionIndexDictionary.Add((ushort)region.Number, _wellResults.Count);
            _wellResults.Add(new RegionResult { regionNumber = (ushort)region.Number });
          }
        }
      }
      //region 0 has to be the last in _wellResults
      if (Reg0stats)  //TODO: remove the IF from here and don't damage the rest of reg0stats logic
      {
        _regionIndexDictionary.Add(0, _wellResults.Count);
        _wellResults.Add(new RegionResult { regionNumber = 0 });
      }
      _chkRegionCount = false;

      //MinPerRegion logic does not apply to region 0.
      if (_device.TerminationType == Termination.MinPerRegion)
      {
        _minPerRegCount = _wellResults.Count;
        if (_minPerRegCount != 0 && _wellResults[_wellResults.Count - 1].regionNumber == 0)
          _minPerRegCount -= 1;
      }
    }

    //TODO:bad design to have it public. get rid somehow
    public List<RegionResult> MakeDeepCopy()
    {
      //TODO: cache previous copy per well somehow, dont make new allocation all the time
      var copy = new List<RegionResult>(_wellResults.Count);
      for (var i = 0; i < _wellResults.Count; i++)
      {
        var r = new RegionResult();
        r.regionNumber = _wellResults[i].regionNumber;
        var count = _wellResults[i].ReporterValues.Count < RegionResult.CAPACITY ? _wellResults[i].ReporterValues.Count : RegionResult.CAPACITY;
        for (var j = 0; j < count; j++)
        {
          r.ReporterValues.Add(_wellResults[i].ReporterValues[j]);
        }
        copy.Add(r);
      }
      return copy;
    }

    public List<RegionStats> GetOutResults()
    {
      var copy = MakeDeepCopy();
      var list = new List<RegionStats>();
      foreach (var wellResult in copy)
      {
        RegionStats rout = new RegionStats
        {
          Count = wellResult.ReporterValues.Count,
          Region = wellResult.regionNumber
        };
        float avg = wellResult.ReporterValues.Average();
        if (rout.Count >= 20)
        {
          wellResult.ReporterValues.Sort();
          int quarterIndex = rout.Count / 4;
          float sum = 0;
          for (var i = quarterIndex; i < rout.Count - quarterIndex; i++)
          {
            sum += wellResult.ReporterValues[i];
          }

          float mean = sum / (rout.Count - 2 * quarterIndex);
          rout.MeanFi = mean;

          rout.MedFi = (float)Math.Round(wellResult.ReporterValues[rout.Count / 2]);

          double sumsq = wellResult.ReporterValues.Sum(dataout => Math.Pow(dataout - rout.MeanFi, 2));
          double stddev = Math.Sqrt(sumsq / wellResult.ReporterValues.Count() - 1);
          rout.CoeffVar = (float) stddev / rout.MeanFi * 100;
          if (double.IsNaN(rout.CoeffVar))
            rout.CoeffVar = 0;
        }
        else
          rout.MeanFi = avg;
        list.Add(rout);
      }
      return list;
    }

    /// <summary>
    /// Checks if MinPerRegion Condition is met. Not thread safe. Supposed to be called after the well is read or in the measurement sequence thread
    /// </summary>
    /// <returns>A positive number or 0, if MinPerRegions is met; otherwise returns a negative number of lacking beads</returns>
    public int MinPerRegionAchieved()
    {
      int res = int.MaxValue;
      for (var i = 0; i < _minPerRegCount; i++)
      {
        var diff = _wellResults[i].ReporterValues.Count - _device.MinPerRegion;
        res = diff < res ? diff : res;
      }
      return res;
    }

    internal void EndOfOperationReset()
    {
      _regionsToOutput = null;
    }

    internal void FillActiveWellResults(in BeadInfoStruct outBead)
    {
      //WellResults is a list of region numbers that are active
      //each entry has a list of rp1 values from each bead in that region
      if (_regionIndexDictionary.TryGetValue(outBead.region, out var index))
      {
        _wellResults[index].ReporterValues.Add(outBead.reporter);
        //_wellResults[index].RP1bgnd.Add(outBead.greenC_bg);
        if (!_chkRegionCount)
          _chkRegionCount = _wellResults[index].ReporterValues.Count == _device.MinPerRegion;  //see if assay is done via sufficient beads in each region
      }
    }

    internal void TerminationReadyCheck()
    {
      switch (_device.TerminationType)
      {
        case Termination.MinPerRegion:
          if (_chkRegionCount)  //a region made it, are there more that haven't
          {
            if (MinPerRegionAchieved() >= 0)
              _device.StartStateMachine();
            _chkRegionCount = false;
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