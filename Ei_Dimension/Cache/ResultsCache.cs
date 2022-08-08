using Ei_Dimension.Graphing.HeatMap;

namespace Ei_Dimension.Cache
{
	public class ResultsCache
	{
		private WellResultsCache[,] _cache = new WellResultsCache[16, 24];

		public void Store(byte row, byte col)
		{
			//can be filled with backing data or middle garbage
			//what about Cl0,Cl3? that won't work this way.
			//Maybe store heatmapData and run that on click? resource heavy-ish though
			//initial idea was to switch pointers.
			//Points have no setter -> can't just switch pointers that would be the whole Heatmap, which is dum
			_cache[row, col].CL12 = HeatMapAPI.API.GetCache(MapIndex.CL12);
		}
	}
}