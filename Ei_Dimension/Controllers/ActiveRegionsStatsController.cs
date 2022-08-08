using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Core;

namespace Ei_Dimension.Controllers
{
	/// <summary>
	/// This class Holds the statistical data for every map region.
	/// The data is split between the Current measurement and Backing (for selected file)
	/// </summary>
	[POCOViewModel]
	public class ActiveRegionsStatsController
	{
		//pointers to storages of mean and count
		public virtual ObservableCollection<string> DisplayedActiveRegionsCount { get; protected set; }
		public virtual ObservableCollection<string> DisplayedActiveRegionsMean { get; protected set; }
		//storage for mean and count of all existing regions. For current reading and backing file select
		public ObservableCollection<string> CurrentCount { get; } = new ObservableCollection<string>();
		public ObservableCollection<string> CurrentMean { get; } = new ObservableCollection<string>();
		public ObservableCollection<string> BackingCount { get; } = new ObservableCollection<string>();
		public ObservableCollection<string> BackingMean { get; } = new ObservableCollection<string>();
		public static ActiveRegionsStatsController Instance
		{
			get
			{
				if (_instance != null)
					return _instance;
				return _instance = ViewModelSource.Create(() => new ActiveRegionsStatsController());
			}
		}

		private static ActiveRegionsStatsController _instance;
		private static bool _activeRegionsUpdateGoing;
		private static readonly RegionReporterResultVolatile _nullWellRegionResult = new RegionReporterResultVolatile();

		protected ActiveRegionsStatsController()
		{
			_nullWellRegionResult.ReporterValues.Capacity = 100000;
		}

		public void DisplayCurrentBeadStats(bool current = true)
		{
			if (current)
			{
				DisplayedActiveRegionsCount = CurrentCount;
				DisplayedActiveRegionsMean = CurrentMean;
				return;
			}
			DisplayedActiveRegionsCount = BackingCount;
			DisplayedActiveRegionsMean = BackingMean;
		}

		public void UpdateCurrentStats()
		{
			if (!_activeRegionsUpdateGoing)
			{
				_activeRegionsUpdateGoing = true;
				var copy = App.Device.Results.MakeDeepCopy();//TODO:hotpath allocations
				var action = App.MapRegions.IsNullRegionActive ? UpdateNullRegionProcedure(copy) : UpdateRegionsProcedure(copy);
				_ = App.Current.Dispatcher.BeginInvoke(action);
			}
		}

		private Action UpdateRegionsProcedure(List<RegionReporterResultVolatile> wellResults)
		{
			return () =>
			{
				ViewModels.ResultsViewModel.Instance.AnalysisMap.ClearData(current: true);
				foreach (var result in wellResults)
				{
					var index = MapRegionsController.GetMapRegionIndex(result.regionNumber);
					if (index < 0)
						continue;
					result.MakeStats(out var count, out var mean);

					CurrentCount[index] = count.ToString();
					CurrentMean[index] = mean.ToString("0.0");

					if (index != 0)
						ViewModels.ResultsViewModel.Instance.AnalysisMap.AddDataPoint(index - 1, mean); // -1 accounts for region = 0
				}

				_activeRegionsUpdateGoing = false;
			};
		}

		private Action UpdateNullRegionProcedure(List<RegionReporterResultVolatile> wellresults)
		{
			foreach (var result in wellresults)
			{
				if (result.ReporterValues.Count > 0)
				{
					_nullWellRegionResult.ReporterValues.InsertRange(_nullWellRegionResult.ReporterValues.Count, result.ReporterValues);
				}
			}

			_nullWellRegionResult.MakeStats(out var count, out var mean);
			_nullWellRegionResult.ReporterValues.Clear();

			return () =>
			{
				ViewModels.ResultsViewModel.Instance.AnalysisMap.ClearData(current: true);

				CurrentCount[0] = count.ToString();
				CurrentMean[0] = mean.ToString("0.0");

				_activeRegionsUpdateGoing = false;
			};
		}

		public void AddNewEntry()
		{
			CurrentCount.Add("0");
			CurrentMean.Add("0");
			BackingCount.Add("0");
			BackingMean.Add("0");
		}

		public void ResetCurrentActiveRegionsDisplayedStats()
		{
			for (var i = 0; i < CurrentCount.Count; i++)
			{
				CurrentCount[i] = "0";
				CurrentMean[i] = "0";
			}
		}

		public void ResetBackingDisplayedStats()
		{
			for (var i = 0; i < BackingCount.Count; i++)
			{
				BackingCount[i] = "0";
				BackingMean[i] = "0";
			}
		}

		public void Clear()
		{
			CurrentCount.Clear();
			CurrentMean.Clear();
			BackingCount.Clear();
			BackingMean.Clear();
		}

	}
}