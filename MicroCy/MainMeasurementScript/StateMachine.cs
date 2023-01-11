using System;
using System.Threading;
using System.Threading.Tasks;
using DIOS.Core.HardwareIntercom;

namespace DIOS.Core.MainMeasurementScript
{
  internal class StateMachine
  {
    private readonly Device _device;
    private readonly MeasurementScript _script;
    private int _started;
    private ILogger _logger;

    public StateMachine(Device device, MeasurementScript script, ILogger logger)
    {
      _device = device;
      _script = script;
      _logger = logger;
    }

    public void Start()
    {
      if (Interlocked.CompareExchange(ref _started, 1, 0) == 1)
        return;

      _logger.Log("SM start");
      Task.Run(() =>
      {
        Action1();
        Thread.Sleep(500);  //not necessary?
        while (WashingIsOngoing())
        {
          Thread.Sleep(500);
        }
        Action3();

        _started = 0;
      });
    }
    
    private void Action1()
    {
      _device.OnFinishedReadingWell();
      _ = Task.Run(() =>
      {
        _device.Results.MakeWellStats();  //TODO: need to check if the well is finished reading before call
        _device.Publisher.ResultsFile.AppendAndWrite(_device.Results.PublishWellStats()); //makewellstats should be awaited only for this method
        _device.Publisher.BeadEventFile.CreateAndWrite(_device.Results.PublishBeadEvents());
        _device.Publisher.DoSomethingWithWorkOrder();
        _logger.Log($"{DateTime.Now.ToString()} Reporting Background File Save Complete");
      });
      _device.OnNewStatsAvailable();
    }
    
    private bool WashingIsOngoing()
    {
      if (_device.SystemMonitor.ContainsWashing())  //does not contain Washing
      {
        _device.Hardware.RequestParameter(DeviceParameterType.SystemActivityStatus);
        return true;
      }
      return false;
    }
    
    private void Action3()
    {
      if(_script.EndBeadRead())
        _device.OnFinishedMeasurement();
      else
      {
        _script.SetupRead();
      }
      Task.Run(()=>
      {
        GC.Collect();
        GC.WaitForPendingFinalizers();
      });
    }
  }
}