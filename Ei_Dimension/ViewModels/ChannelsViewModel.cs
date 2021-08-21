using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ChannelsViewModel
  {
    public virtual string[] Bias30Parameters { get; set; }
    public virtual string[] TcompBiasParameters { get; set; }
    public virtual string[] TempParameters { get; set; }
    public virtual string[] BackgroundParameters { get; set; }
    public virtual string SiPMTempCoeff { get; set; }
    public virtual ObservableCollection<string> CurrentMapName { get; set; }

    protected ChannelsViewModel()
    {
      CurrentMapName = new ObservableCollection<string> { App.CurrentMap.mapName };

      Bias30Parameters = new string[10];
      Bias30Parameters[0] = "1180";
      Bias30Parameters[1] = "1500";
      Bias30Parameters[2] = "1500";
      Bias30Parameters[4] = "1330";
      Bias30Parameters[5] = "1180";
      Bias30Parameters[6] = "1180";
      Bias30Parameters[7] = "1900";
      TcompBiasParameters = new string[10];
      TempParameters = new string[10];
      BackgroundParameters = new string[10];

    }

    public void UpdateBiasButtonClick()
    {

    }

    public void SaveBiasButtonClick()
    {

    }

    public static ChannelsViewModel Create()
    {
      return ViewModelSource.Create(() => new ChannelsViewModel());
    }
  }
}