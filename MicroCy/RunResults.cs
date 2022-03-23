using System;
using System.Collections.Generic;
using System.Linq;

namespace DIOS.Core
{
  public class RunResults
  {
    private Device _device;
    private ICollection<int> _regionsToOutput;
    private List<WellResult> _wellResults = new List<WellResult>();
    private SortedDictionary<ushort, int> _regionIndexDictionary = new SortedDictionary<ushort, int>();
    private bool _chkRegionCount;

    public RunResults(Device device)
    {
      _device = device;
    }

    public void SetupRunRegions(ICollection<int> regions)
    {
      _regionsToOutput = regions;
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
            _wellResults.Add(new WellResult { regionNumber = (ushort)region.Number });
          }
        }
      }
      if (_device.Reg0stats)
      {
        _regionIndexDictionary.Add(0, _wellResults.Count);
        _wellResults.Add(new WellResult { regionNumber = 0 });
      }
      _chkRegionCount = false;
    }

    //TODO:bad design to have it public. get rid somehow
    public List<WellResult> MakeDeepCopy()
    {
      var copy = new List<WellResult>(_wellResults.Count);
      for (var i = 0; i < _wellResults.Count; i++)
      {
        var r = new WellResult();
        r.regionNumber = _wellResults[i].regionNumber;
        var count = _wellResults[i].RP1vals.Count;
        r.RP1vals = new List<float>(count);
        for (var j = 0; j < count; j++)
        {
          r.RP1vals.Add(_wellResults[i].RP1vals[j]);
        }
        copy.Add(r);
      }
      return copy;
    }

    public List<OutResults> GetOutResults()
    {
      var copy = MakeDeepCopy();
      var list = new List<OutResults>();
      foreach (var wellResult in copy)
      {
        OutResults rout = new OutResults
        {
          count = wellResult.RP1vals.Count,
          region = wellResult.regionNumber
        };
        float avg = wellResult.RP1vals.Average();
        if (rout.count >= 20)
        {
          wellResult.RP1vals.Sort();
          int quarterIndex = rout.count / 4;
          float sum = 0;
          for (var i = quarterIndex; i < rout.count - quarterIndex; i++)
          {
            sum += wellResult.RP1vals[i];
          }

          float mean = sum / (rout.count - 2 * quarterIndex);
          rout.meanfi = mean;

          rout.medfi = (float)Math.Round(wellResult.RP1vals[rout.count / 2]);

          double sumsq = wellResult.RP1vals.Sum(dataout => Math.Pow(dataout - rout.meanfi, 2));
          double stddev = Math.Sqrt(sumsq / wellResult.RP1vals.Count() - 1);
          rout.cv = (float) stddev / rout.meanfi * 100;
          if (double.IsNaN(rout.cv))
            rout.cv = 0;
        }
        else
          rout.meanfi = avg;
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
      if (_wellResults.Count > 0)
      {
        foreach (var wr in _wellResults)
        {
          var diff = wr.RP1vals.Count - _device.MinPerRegion;
          res = diff < res? diff : res;
        }
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
        _wellResults[index].RP1vals.Add(outBead.reporter);
        _wellResults[index].RP1bgnd.Add(outBead.greenC_bg);
        if (!_chkRegionCount)
          _chkRegionCount = _wellResults[index].RP1vals.Count == _device.MinPerRegion;  //see if assay is done via sufficient beads in each region
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
