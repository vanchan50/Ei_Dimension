using DIOS.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using Ei_Dimension.Controllers;
using DIOS.Core.HardwareIntercom;
using Ei_Dimension.ViewModels;

namespace Ei_Dimension
{
  internal static class UITimer
  {
    private static int _uiUpdateIsActive;
    private static Timer _timer;
    private static bool _started;

    #if DEBUG
    private static List<CommandStruct> DEBUGCommandList = new List<CommandStruct>
    {
      new CommandStruct{ Code = 0x3E, Command = 0x02, Parameter = 0xFFFF, FParameter = 32},
      new CommandStruct{ Code = 0x61, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x62, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x63, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x64, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x91, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x24, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x25, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x26, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x28, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x29, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x2a, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x2c, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x2d, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x2E, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x2F, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x80, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x81, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x84, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xB0, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xB1, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xB2, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xB3, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xB4, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xB5, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xB6, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x93, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x94, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x95, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x96, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x98, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x99, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x9A, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x9B, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xA6, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xA7, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x9C, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x9D, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x9E, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0x9F, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xA0, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xA1, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xA2, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xA3, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xA4, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xA5, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xAC, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xAF, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xC4, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xB8, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xC0, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xC7, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xC8, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xC9, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xCC, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xF1, Command = 0x01, Parameter = 1, FParameter = 32},  //SheathFlow
      new CommandStruct{ Code = 0xF2, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xF3, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xF4, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xF9, Command = 0x00, Parameter = 1, FParameter = 32},
      new CommandStruct{ Code = 0xDE, Command = 0x00, Parameter = 1, FParameter = 32},
      //new CommandStruct{ Code = 0xFD, Command = 0x00, Parameter = 1, FParameter = 32}
      
      //Fill me up and test
    };
    #endif

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
      
      if (App.DiosApp.Device.IsMeasurementGoing)
      {
        GraphsController.Instance.Update();
        ActiveRegionsStatsController.Instance.UpdateCurrentStats();
        App.Current.Dispatcher.Invoke(TextBoxHandler.UpdateEventCounter);
        App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.BeadConcentration);
      }
      TextBoxHandler.UpdatePressureMonitor();
      ServiceMenuEnabler.Update();
      _uiUpdateIsActive = 0;

      //DashboardViewModel.Instance.ChConfigItems[3].Click(3);
      //MaintenanceViewModel.Instance.LanguageItems[0].Click();

      #if DEBUG
      App.Current.Dispatcher.Invoke(() => {
        if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.J))
        {
          DEBUGJBeadADD();
        }
        if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.F3))
        {
          //App.DiosApp.Device.DEBUGOnParameterUpdate(DeviceParameterType.FlushCycles, 43);
          ////App.DiosApp.Device.DEBUGCommandTest(DEBUGCommandList[DEBUGCommandCounter++]);
          //foreach (var cs in DEBUGCommandList)
          //{
          //  App.DiosApp.Device.DEBUGCommandTest(cs);
          //}
        }
      });
#endif
    }

    public static void DEBUGJBeadADD()
    {
      var r = new Random();
      var choose = r.Next(0, 3);
      ProcessedBead bead = new ProcessedBead();
      switch (choose)
      {
        case 0:
          bead = new ProcessedBead
          {
            fsc = 2.36f,
            redssc = r.Next(1000, 20000),
            cl0 = r.Next(1050, 1300),
            cl1 = r.Next(1450, 1700),
            cl2 = r.Next(1500, 1650),
            greenB = (ushort)r.Next(9, 12),
            greenC = 48950
          };
          break;
        case 1:
          bead = new ProcessedBead
          {
            fsc = 15.82f,
            redssc = r.Next(1000, 20000),
            cl0 = 250f,
            cl1 = 500f,
            cl2 = 500f,
            greenB = (ushort)r.Next(80, 150),
            greenC = 65212
          };
          break;
        case 2:
          bead = new ProcessedBead
          {
            fsc = 2.36f,
            redssc = r.Next(1000, 20000),
            cl0 = r.Next(1050, 1300),
            cl1 = 35000,
            cl2 = 200,
            greenB = (ushort)r.Next(9, 12),
            greenC = 48950
          };
          break;
      }
      App.DiosApp.Results.AddProcessedBeadEvent(in bead);
      if (App.DiosApp.Results.IsMeasurementTerminationAchieved())
      {
        App.DiosApp.Device.StopOperation();
      }
    }
  }
}
