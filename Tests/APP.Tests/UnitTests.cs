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
  public class UnitTests
  {
    [WpfFact]
    public void StartButtonIsEnabledAfterRun()
    {
      var app = new App();
      app.FinishedMeasurementEventHandler(null, EventArgs.Empty);
      Assert.True(MainButtonsViewModel.Instance.StartButtonEnabled);
    }
  }
}
