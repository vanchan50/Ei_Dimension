using DIOS.Core;
using System;
using System.Threading;
using Ei_Dimension.Controllers;
using DIOS.Core.HardwareIntercom;

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
      
      if (App.DiosApp.Device.IsMeasurementGoing)
      {
        GraphsController.Instance.Update();
        ActiveRegionsStatsController.Instance.UpdateCurrentStats();
        App.Current.Dispatcher.Invoke(IncomingUpdateHandler.UpdateEventCounter);
        App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.BeadConcentration);
      }
      IncomingUpdateHandler.UpdatePressureMonitor();
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
          //App.DiosApp.Device.DEBUGOnParameterUpdate(DeviceParameterType.FluidicPathLength, (int)FluidicPathLength.LoopAToPickupNeedle, 1);
          //App.DiosApp.Device.DEBUGOnParameterUpdate(DeviceParameterType.FluidicPathLength, (int)FluidicPathLength.LoopBToPickupNeedle, 2);
          //App.DiosApp.Device.DEBUGOnParameterUpdate(DeviceParameterType.FluidicPathLength, (int)FluidicPathLength.LoopAToFlowcellBase, 3);
          //App.DiosApp.Device.DEBUGOnParameterUpdate(DeviceParameterType.FluidicPathLength, (int)FluidicPathLength.LoopBToFlowcellBase, 4);
          //App.DiosApp.Device.DEBUGOnParameterUpdate(DeviceParameterType.FluidicPathLength, (int)FluidicPathLength.LoopAVolume, 5);
          //App.DiosApp.Device.DEBUGOnParameterUpdate(DeviceParameterType.FluidicPathLength, (int)FluidicPathLength.LoopBVolume, 6);
          //App.DiosApp.Device.DEBUGOnParameterUpdate(DeviceParameterType.FluidicPathLength, (int)FluidicPathLength.FlowCellNeedleVolume, 7);
          //App.DiosApp.Device.DEBUGOnParameterUpdate(DeviceParameterType.FluidicPathLength, (int)FluidicPathLength.PickupNeedleVolume, 8);
          //App.DiosApp.Device.DEBUGOnParameterUpdate(DeviceParameterType.IsFlowCellInverted, 0);
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

    #if DEBUG
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
            greenC = 48950,
            reporter = 23.9f
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
            greenC = 65212,
            reporter = 84342261.623467f
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
            greenC = 48950,
            reporter = 1239.123f
          };
          break;
      }
      App.DiosApp.Results.AddProcessedBeadEvent(in bead);
      if (App.DiosApp.Results.IsMeasurementTerminationAchieved())
      {
        App.DiosApp.Device.StopOperation();
      }
    }
    #endif
  }
}
