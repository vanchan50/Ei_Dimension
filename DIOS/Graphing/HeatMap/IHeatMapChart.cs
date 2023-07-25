using System.Windows.Media;
using DevExpress.Xpf.Charts;

namespace Ei_Dimension.Graphing.HeatMap;

internal interface IHeatMapChart
{
  void AddXYPointToHeatMap(SeriesPoint chartPoint, bool LargeXY = false);
  void ChangeHeatMapPointColor(int index, SolidColorBrush brush);
  void ClearHeatMaps();
}