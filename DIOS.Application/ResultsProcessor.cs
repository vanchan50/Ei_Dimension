using DIOS.Application.Domain;
using DIOS.Core;
using System.Threading;

namespace DIOS.Application
{
  public class ResultsProcessor
  {
    private readonly Thread _resultsProcessingThread;
    private int _resultsProcessed;
    private readonly object _processingCV = new object();
    private Device _device;
    private RunResults _results;
    internal readonly ReadTerminator Terminator;

    public ResultsProcessor(Device device, RunResults results)
    {
      _device = device;
      _results = results;
      Terminator = new ReadTerminator(_results.MinPerRegionAchieved);
      //setup thread
      _resultsProcessingThread = new Thread(ResultsProcessing);
      _resultsProcessingThread.IsBackground = true;
      _resultsProcessingThread.Name = "ResultsProcessing";
      _resultsProcessingThread.Start();
    }

    private void ResultsProcessing()  //dont forget to run it somehow. only start it on measurement start and stop on measurementfinished
    {
      while (true)
      {
        var size = _results.OutputBeadsCollector.Count;
        while (_resultsProcessed < size)
        {
          var bead = _results.OutputBeadsCollector[_resultsProcessed++];
          _results.AddProcessedBeadEvent(in bead);
          _results.DataOut.Enqueue(bead);

          if (_resultsProcessed == 1)  //Trigger on 1st bead arrived is the simplest solution, at least for now;
          { //Comes from the asynchronous nature of the instrument
            Terminator.StartTerminationTimer();
          }

          if (Terminator.IsMeasurementTerminationAchieved(_device.BeadCount))//endofsample never triggers!
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
      _results.OutputBeadsCollector.Clear();
      _resultsProcessed = 0;
      Terminator.MinPerRegCheckTrigger = false;
    }
  }
}