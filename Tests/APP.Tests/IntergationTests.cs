using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using DIOS.Core;
using DIOS.Core.Tests;
using Xunit;
using Ei_Dimension;
using Ei_Dimension.Controllers;
using Ei_Dimension.ViewModels;

namespace APP.Tests
{
  public class IntergationTests
  {
    private FakeUSBConnection _usb;
    private Device _device;
    private App _sut;

    public IntergationTests()
    {
      _usb = new FakeUSBConnection();
      _device = new Device(_usb);
      _sut = new App(_device);
      MainViewModel.Create();
      ResultsViewModel.Create();
      VerificationViewModel.Create();
      MainButtonsViewModel.Create();
      ExperimentViewModel.Create();
      DashboardViewModel.Create();
      //StartupFinalizer.Run();
      var rb = new StackPanel();
      var rnb = new StackPanel();
      var rt = new ListBox();
      var dbnum = new StackPanel();
      var dbname = new StackPanel();
      var valnum = new StackPanel();
      var varrep = new StackPanel();
      var normnum = new StackPanel();
      var normrep = new StackPanel();
      for (var i = 0; i < 13; i++)
      {
        App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary());
      }
      App.MapRegions = new MapRegionsController(rb, rnb, rt,
        dbnum, dbname, valnum, varrep, normnum, normrep);
      ActiveRegionsStatsController.Instance.DisplayCurrentBeadStats();
      //ResultsViewModel.Instance.PlatePictogram.SetGrid(Views.ResultsView.Instance.DrawingPlate);
      //ResultsViewModel.Instance.PlatePictogram.SetWarningGrid(Views.ResultsView.Instance.WarningGrid);
    }

    [WpfFact]
    public void Test1()
    {
      var DashVM = DashboardViewModel.Instance;
      DashVM.SpeedItems[0].Click(1);
      DashVM.ClassiMapItems[0].Click(2);
      DashVM.ChConfigItems[0].Click(3);
      DashVM.OrderItems[0].Click(4);
      DashVM.SysControlItems[0].Click(5);
      DashVM.EndReadItems[0].Click(6);
    }
  }
}