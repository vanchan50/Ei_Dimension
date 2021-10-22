using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Ei_Dimension.Models
{
  [POCOViewModel]
  public class HeatMap
  {
    public virtual ObservableCollection<HeatMapData> CurrentMap { get; set; }
    public Dictionary<(int x, int y), int> Dict { get; } = new Dictionary<(int x, int y), int>();

    protected HeatMap()
    {

    }

    public static HeatMap Create()
    {
      return ViewModelSource.Create(() => new HeatMap());
    }
  }
}
