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
        App.Current.Dispatcher.Invoke(() => {
          if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.J))
          {
            App.Device.JBeadADD();
          }
        });
#endif
      }
      ServiceMenuEnabler.Update();
      _uiUpdateIsActive = 0;
    }
  }
}
