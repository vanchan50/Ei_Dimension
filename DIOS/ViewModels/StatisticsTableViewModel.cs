using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DIOS.Application;
using System.Collections.ObjectModel;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class StatisticsTableViewModel
  {
    public virtual ObservableCollection<string> DisplayedMfiItems { get; set; }
    public virtual ObservableCollection<string> DisplayedCvItems { get; set; }
    public virtual ObservableCollection<string> CurrentMfiItems { get; set; } = new();
    public virtual ObservableCollection<string> CurrentMedianItems { get; set; } = new();
    public virtual ObservableCollection<string> CurrentPeakItems { get; set; } = new();
    public virtual ObservableCollection<string> CurrentCvItems { get; set; } = new();
    public virtual ObservableCollection<string> BackingMfiItems { get; set; } = new();
    public virtual ObservableCollection<string> BackingMedianItems { get; set; } = new();
    public virtual ObservableCollection<string> BackingPeakItems { get; set; } = new();
    public virtual ObservableCollection<string> BackingCvItems { get; set; } = new();
    public virtual ObservableCollection<string> StatisticsLabels { get; set; }
    public static StatisticsTableViewModel Instance { get; private set; }
    private int _displayedStatsType = 0;

    protected StatisticsTableViewModel()
    {
      for (var i = 0; i < 10; i++)
      {
        CurrentMfiItems.Add("");
        CurrentMedianItems.Add("");
        CurrentPeakItems.Add("");
        CurrentCvItems.Add("");

        BackingMfiItems.Add("");
        BackingMedianItems.Add("");
        BackingPeakItems.Add("");
        BackingCvItems.Add("");
      }

      DisplayedMfiItems = CurrentMfiItems;
      DisplayedCvItems = CurrentCvItems;

      StatisticsLabels = new()
      {
        Language.Resources.Channels_Green_A,
        Language.Resources.Channels_Green_B,
        Language.Resources.Channels_Green_C,
        Language.Resources.DataAn_Red_SSC,
        Language.Resources.CL1,
        Language.Resources.CL2,
        Language.Resources.CL3,
        Language.Resources.DataAn_Violet_SSC,
        Language.Resources.CL0,
        Language.Resources.Channels_ExternalPMT
      };
      Instance = this;
    }

    public static StatisticsTableViewModel Create()
    {
      return ViewModelSource.Create(() => new StatisticsTableViewModel());
    }

    public void ClearCurrentCalibrationStats()
    {
      for (var i = 0; i < 10; i++)
      {
        CurrentMfiItems[i] = "";
        CurrentMedianItems[i] = "";
        CurrentPeakItems[i] = "";
        CurrentCvItems[i] = "";
      }
    }

    public void DecodeCalibrationStats(ChannelsCalibrationStats stats, ChannelsHistogramPeaks peaks, bool current)
    {
      ObservableCollection<string> mfiItems;
      ObservableCollection<string> medianItems;
      ObservableCollection<string> peakItems;
      ObservableCollection<string> cvItems;
      if (current)
      {
        mfiItems = CurrentMfiItems;
        medianItems = CurrentMedianItems;
        peakItems = CurrentPeakItems;
        cvItems = CurrentCvItems;
      }
      else
      {
        mfiItems = BackingMfiItems;
        medianItems = BackingMedianItems;
        peakItems = BackingPeakItems;
        cvItems = BackingCvItems;
      }

      mfiItems[0] = stats.Greenssc.Mean.ToString($"{0:0.0}");
      medianItems[0] = stats.Greenssc.Median.ToString($"{0:0.0}");
      peakItems[0] = peaks.GreenA;
      cvItems[0] = stats.Greenssc.CoeffVar.ToString($"{0:0.00}");

      mfiItems[1] = stats.GreenB.Mean.ToString($"{0:0.0}");
      medianItems[1] = stats.GreenB.Median.ToString($"{0:0.0}");
      peakItems[1] = peaks.GreenB;
      cvItems[1] = stats.GreenB.CoeffVar.ToString($"{0:0.00}");

      mfiItems[2] = stats.GreenC.Mean.ToString($"{0:0.0}");
      medianItems[2] = stats.GreenC.Median.ToString($"{0:0.0}");
      peakItems[2] = peaks.GreenC;
      cvItems[2] = stats.GreenC.CoeffVar.ToString($"{0:0.00}");

      mfiItems[3] = stats.Redssc.Mean.ToString($"{0:0.0}");
      medianItems[3] = stats.Redssc.Median.ToString($"{0:0.0}");
      peakItems[3] = peaks.Redssc;
      cvItems[3] = stats.Redssc.CoeffVar.ToString($"{0:0.00}");

      mfiItems[4] = stats.Cl1.Mean.ToString($"{0:0.0}");
      medianItems[4] = stats.Cl1.Median.ToString($"{0:0.0}");
      peakItems[4] = peaks.Cl1;
      cvItems[4] = stats.Cl1.CoeffVar.ToString($"{0:0.00}");

      mfiItems[5] = stats.Cl2.Mean.ToString($"{0:0.0}");
      medianItems[5] = stats.Cl2.Median.ToString($"{0:0.0}");
      peakItems[5] = peaks.Cl2;
      cvItems[5] = stats.Cl2.CoeffVar.ToString($"{0:0.00}");

      mfiItems[6] = stats.Cl3.Mean.ToString($"{0:0.0}");
      medianItems[6] = stats.Cl3.Median.ToString($"{0:0.0}");
      peakItems[6] = peaks.Cl3;
      cvItems[6] = stats.Cl3.CoeffVar.ToString($"{0:0.00}");

      mfiItems[7] = stats.Violetssc.Mean.ToString($"{0:0.0}");
      medianItems[7] = stats.Violetssc.Median.ToString($"{0:0.0}");
      peakItems[7] = peaks.Violetssc;
      cvItems[7] = stats.Violetssc.CoeffVar.ToString($"{0:0.00}");

      mfiItems[8] = stats.Cl0.Mean.ToString($"{0:0.0}");
      medianItems[8] = stats.Cl0.Median.ToString($"{0:0.0}");
      peakItems[8] = peaks.Cl0;
      cvItems[8] = stats.Cl0.CoeffVar.ToString($"{0:0.00}");

      mfiItems[9] = stats.Fsc.Mean.ToString($"{0:0.0}");
      medianItems[9] = stats.Fsc.Median.ToString($"{0:0.0}");
      peakItems[9] = peaks.Fsc;
      cvItems[9] = stats.Fsc.CoeffVar.ToString($"{0:0.00}");
    }

    public void DisplayStatsTypeChange(int type)
    {
      //0 mean(mfi)
      //1 median
      //2 peak
      _displayedStatsType = type;
      SelectDisplayedStatsType();
    }

    public void SelectDisplayedStatsType()
    {
      
      switch (_displayedStatsType)
      {
        case 0:
          DisplayedMfiItems = ResultsViewModel.Instance.DisplaysCurrentmap ? CurrentMfiItems : BackingMfiItems;
          break;
        case 1:
          DisplayedMfiItems = ResultsViewModel.Instance.DisplaysCurrentmap ? CurrentMedianItems : BackingMedianItems;
          break;
        case 2:
          DisplayedMfiItems = ResultsViewModel.Instance.DisplaysCurrentmap ? CurrentPeakItems : BackingPeakItems;
          break;
      }
      DisplayedCvItems = ResultsViewModel.Instance.DisplaysCurrentmap ? CurrentCvItems : BackingCvItems;
    }
  }
}
