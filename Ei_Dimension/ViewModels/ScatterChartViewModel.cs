using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using Ei_Dimension.Models;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ScatterChartViewModel
  {
    public ScatterData ScttrData { get; set; }
    public virtual ObservableCollection<bool> ScatterSelectorState { get; set; }

    public static ScatterChartViewModel Instance { get; private set; }

    protected ScatterChartViewModel()
    {
      Instance = this;
      ScatterSelectorState = new ObservableCollection<bool> { false, false, false, false, false };
      byte temp = Settings.Default.ScatterGraphSelector;
      if (temp >= 16)
      {
        ScatterSelectorState[4] = true;
        temp -= 16;
      }
      if (temp >= 8)
      {
        ScatterSelectorState[3] = true;
        temp -= 8;
      }
      if (temp >= 4)
      {
        ScatterSelectorState[2] = true;
        temp -= 4;
      }
      if (temp >= 2)
      {
        ScatterSelectorState[1] = true;
        temp -= 2;
      }
      if (temp >= 1)
      {
        ScatterSelectorState[0] = true;
      }
      ScatterData.Create();
    }

    public void ChangeScatterLegend(int num)  //TODO: For buttons
    {
      ScatterSelectorState[num] = !ScatterSelectorState[num];
      var res = 0;
      res += ScatterSelectorState[0] ? 1 : 0;
      res += ScatterSelectorState[1] ? 2 : 0;
      res += ScatterSelectorState[2] ? 4 : 0;
      res += ScatterSelectorState[3] ? 8 : 0;
      res += ScatterSelectorState[4] ? 16 : 0;
      Settings.Default.ScatterGraphSelector = (byte)res;
      Settings.Default.Save();
    }

    public void Scatterprint()
    {
      Views.ScatterChartView.Instance.Print();
    }

    public static ScatterChartViewModel Create()
    {
      return ViewModelSource.Create(() => new ScatterChartViewModel());
    }
  }
}
