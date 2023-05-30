using DIOS.Core;
using System.Threading;

namespace DIOS.Application
{
  public class ResultsProcessor
  {
    private readonly Thread _resultsProcessingThread;
    private int resultsProcessed;
    private readonly object _processingCV = new object();
    private Device _device;
    private RunResults _results;

    public ResultsProcessor(Device device, RunResults results)
    {
      _device = device;
      _results = results;
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
        while (resultsProcessed < size)
        {
          var bead = _results.OutputBeadsCollector[resultsProcessed++];
          _results.AddProcessedBeadEvent(in bead);
          _results.DataOut.Enqueue(bead);

          if (resultsProcessed == 1)
          {
            _results.ResetTerminationTimer();
          }

          if (_results.IsMeasurementTerminationAchieved())//endofsample never triggers!
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

    internal void NewWellStarting()
    {
      _results.OutputBeadsCollector.Clear();
      resultsProcessed = 0;
    }
  }
}
