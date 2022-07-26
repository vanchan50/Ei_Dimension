using DIOS.Core;
using System;
using System.Threading;
using Ei_Dimension.Controllers;

namespace Ei_Dimension
{
  internal static class UITimer
  {
    private static int _uiUpdateIsActive;
    private static Timer _timer;
    private static bool _started;

    public static void Start()
    {
      if (_started)
        throw new Exception("UITimer is already Started");

      _timer = new Timer(Tick);
      _ = _timer.Change(new TimeSpan(0, 0, 0, 0, 100),
        new TimeSpan(0, 0, 0, 0, 500));
      _started = true;
    }

    private static void Tick(object state)
    {
      if (Interlocked.CompareExchange(ref _uiUpdateIsActive, 1, 0) == 1)
        return;

      TextBoxHandler.Update();
      if (App.Device.IsMeasurementGoing)
      {
        GraphsController.Instance.Update();
        ActiveRegionsStatsController.Instance.UpdateCurrentStats();
        TextBoxHandler.UpdateEventCounter();
        App.Device.UpdateStateMachine();

        #if DEBUG
        JKBeadADD();
        #endif
      }
      ServiceMenuEnabler.Update();
      _uiUpdateIsActive = 0;
    }

    #if DEBUG
    private static void JKBeadADD()
    {
      var r = new Random();
      App.Current.Dispatcher.Invoke(() => { 
        var kek = new RawBead
        {
          Header = 0xadbeadbe,
          fsc = 2.36f,
          cl0 = r.Next(1050, 1300),
          cl1 = r.Next(1450, 1700),
          cl2 = r.Next(1500, 1650),
          greenB = (ushort)r.Next(9, 12),
          greenC = 48950
        };
        var pek = new RawBead
        {
          Header = 0xadbeadbe,
          fsc = 15.82f,
          cl0 = 250f,
          cl1 = 500f,
          cl2 = 500f,
          greenB = (ushort)r.Next(80,150),
          greenC = 65212
        };
        if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.J))
        {
          App.Device.Results.AddRawBeadEvent(ref kek);
        }
        if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.K))
        {
          App.Device.Results.AddRawBeadEvent(ref pek);
        }
      });
    }
    #endif
  }
}
