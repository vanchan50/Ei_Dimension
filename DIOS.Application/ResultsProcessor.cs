using DIOS.Application.Domain;
using DIOS.Core;

namespace DIOS.Application;

public class ResultsProcessor
{
  private readonly Thread _resultsProcessingThread;
  private int _resultsProcessed;
  private readonly object _processingCV = new();
  private readonly Device _device;
  private readonly RunResults _results;
  private readonly ReadTerminator _terminator;
  private readonly BeadProcessor _beadProcessor;

  public ResultsProcessor(Device device, RunResults results, ReadTerminator terminator, BeadProcessor beadProcessor)
  {
    _device = device;
    _results = results;
    _terminator = terminator;
    _beadProcessor = beadProcessor;
    //setup thread
    _resultsProcessingThread = new Thread(ResultsProcessing);
    _resultsProcessingThread.IsBackground = true;
    _resultsProcessingThread.Name = nameof(ResultsProcessing);
    _resultsProcessingThread.Start();
  }

  private void ResultsProcessing()  //dont forget to run it somehow. only start it on measurement start and stop on measurementfinished
  {
    while (true)
    {
      var size = _results.RawBeadsCollector.Count;
      while (_resultsProcessed < size)
      {
        var bead = _results.RawBeadsCollector[_resultsProcessed++];
        var processedBead = _beadProcessor.CalculateBeadParams(in bead);
        _results.AddProcessedBeadEvent(in processedBead);

        if (_resultsProcessed == 1)  //Trigger on 1st bead arrived is the simplest solution, at least for now;
        { //Comes from the asynchronous nature of the instrument
          _terminator.StartTerminationTimer();
        }

        if (_terminator.IsMeasurementTerminationAchieved(_device.BeadCount))//endofsample never triggers!
        {
          _device.StopOperation();
        }
      }
      Thread.Sleep(10);//dumb overload protection

      if (!_device.IsMeasurementGoing)
      {
        LockThread();
      }
    }
  }

  private void LockThread()
  {
    lock (_processingCV)
    {
      Monitor.Wait(_processingCV);
    }
  }

  public void StartBeadProcessing()
  {
    lock (_processingCV)
    {
      Monitor.Pulse(_processingCV);
    }
  }

  public void NewWellStarting()
  {
    _resultsProcessed = 0;
    _terminator.MinPerRegCheckTrigger = false;
  }
}