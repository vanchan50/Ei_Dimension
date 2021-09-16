using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class DataAnalysisViewModel
  {
    public virtual ObservableCollection<string> MfiItems { get; set; }
    public virtual ObservableCollection<string> CvItems { get; set; }
    public virtual bool CalStatsButtonState { get; set; }

    static public DataAnalysisViewModel Instance;

    protected DataAnalysisViewModel()
    {
      MfiItems = new ObservableCollection<string>();
      CvItems = new ObservableCollection<string>();
      for (var i = 0; i < 10; i++)
      {
        MfiItems.Add("");
        CvItems.Add("");
      }

      CalStatsButtonState = false;

      Instance = this;
    }

    public static DataAnalysisViewModel Create()
    {
      return ViewModelSource.Create(() => new DataAnalysisViewModel());
    }

    public void CalibrationStatisticsButtonClick()
    {
      CalStatsButtonState = !CalStatsButtonState;
      App.Device.CalStats = CalStatsButtonState;
    }
  }
}