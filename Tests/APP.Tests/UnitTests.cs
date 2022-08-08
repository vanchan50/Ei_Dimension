using System;
using Ei_Dimension;
using Ei_Dimension.ViewModels;
using Xunit;

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
