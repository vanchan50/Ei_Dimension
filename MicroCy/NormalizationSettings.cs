namespace DIOS.Core
{
	public class NormalizationSettings
	{
		public bool IsEnabled { get; private set; } = true;
		private bool _cache;

		public void Enable()
		{
			IsEnabled = true;
		}

		public void Disable()
		{
			IsEnabled = false;
		}

		internal void SuspendForTheRun()
		{
			_cache = IsEnabled;
			IsEnabled = false;
		}

		internal void Restore()
		{
			IsEnabled = _cache;
		}
	}
}
