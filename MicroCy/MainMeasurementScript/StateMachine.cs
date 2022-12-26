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

    public StateMachine(Device device, MeasurementScript script)
    {
      _device = device;
      _script = script;
    }

    public void Start()
    {
      if (Interlocked.CompareExchange(ref _started, 1, 0) == 1)
        return;

      Console.WriteLine("SM start");
      Task.Run(() =>
      {
        Action1();
        Thread.Sleep(500);  //not necessary?
        while (!Action2())
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
        Console.WriteLine($"{DateTime.Now.ToString()} Reporting Background File Save Complete");
      });
      _device.OnNewStatsAvailable();
    }
    
    private bool Action2()
    {
      if (!_device.SystemActivity[11])  //does not contain Washing
      {
        return true;
      }
      _device.Hardware.RequestParameter(DeviceParameterType.SystemActivityStatus);
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