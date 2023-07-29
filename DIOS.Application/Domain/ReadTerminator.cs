namespace DIOS.Application.Domain;

public class ReadTerminator
{
  public Termination TerminationType { get; set; } = Termination.MinPerRegion;
  public int TotalBeadsToCapture { get; set; }
  public int MinPerRegion { get; set; }
  public int TerminationTime { get; set; }
  internal bool MinPerRegCheckTrigger { get; set; }
  private Timer _timer;
  private int _timeoutAchieved;
  private Func<int, int> _minPerRegionAchieved;

  internal ReadTerminator(Func<int, int> minPerRegionAchieved)
  {
    _timer = new Timer(TerminateMeasurementByTimeout);
    _minPerRegionAchieved = minPerRegionAchieved;
  }

  internal void StartTerminationTimer()//int seconds)
  {
    _ = _timer.Change(new TimeSpan(0, 0, 0, TerminationTime, 0),
      new TimeSpan(0, 0, 0, 0, -1));
  }

  internal bool IsMeasurementTerminationAchieved(int totalBeads)
  {
    bool stopMeasurement = false;
    switch (TerminationType)
    {
      case Termination.MinPerRegion:
        if (MinPerRegCheckTrigger)  //a region made it, are there more that haven't
        {
          if (_minPerRegionAchieved(MinPerRegion) >= 0)
          {
            stopMeasurement = true;
          }
          MinPerRegCheckTrigger = false;
        }
        break;
      case Termination.TotalBeadsCaptured:
        if (totalBeads >= TotalBeadsToCapture)
        {
          stopMeasurement = true;
        }
        break;
      case Termination.EndOfSample:
        break;
      case Termination.Timer:
        if (_timeoutAchieved > 0)
        {
          stopMeasurement = true;
          _timeoutAchieved = 0;
        }
        break;
    }

    return stopMeasurement;
  }

  private void TerminateMeasurementByTimeout(object state)
  {
    Interlocked.CompareExchange(ref _timeoutAchieved, 1, 0);
  }
}