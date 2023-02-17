using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DIOS.Application;
using DIOS.Core;
using Ei_Dimension.Controllers;
using Ei_Dimension.Graphing;
using Ei_Dimension.Graphing.HeatMap;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ResultsViewModel
  {
    public bool DisplaysCurrentmap { get; private set; }
    public virtual System.Windows.Visibility MultiPlexVisible { get; set; }
    public virtual System.Windows.Visibility SinglePlexVisible { get; set; }
    public virtual System.Windows.Visibility ValidationCoverVisible { get; set; }
    public virtual bool ResultsWaitIndicatorVisibility { get; set; }
    public virtual bool ChartWaitIndicatorVisibility { get; set; }
    public WorldMap WrldMap { get; set; }
    public AnalysysMap AnalysisMap { get; set; }
    public virtual System.Windows.Visibility AnalysisVisible { get; set; }
    public virtual System.Windows.Visibility Analysis2DVisible { get; set; }
    public virtual System.Windows.Visibility Analysis3DVisible { get; set; }
    public virtual ObservableCollection<string> DisplayedMfiItems { get; set; }
    public virtual ObservableCollection<string> DisplayedCvItems { get; set; }
    public virtual ObservableCollection<string> CurrentMfiItems { get; set; }
    public virtual ObservableCollection<string> CurrentCvItems { get; set; }
    public virtual ObservableCollection<string> BackingMfiItems { get; set; }
    public virtual ObservableCollection<string> BackingCvItems { get; set; }
    public virtual string PlexButtonString { get; set; }
    public virtual ObservableCollection<bool> CLButtonsChecked { get; set; }
    public virtual ObservableCollection<string> CLAxis { get; set; }
    public static ResultsViewModel Instance { get; private set; }
    private bool _fillDataActive;
    public const int HIREZDEFINITION = 512;

    private List<ProcessedBead> _cachedBeadStructsForLoadedData = new List<ProcessedBead>(100000);

    protected ResultsViewModel()
    {
      DisplaysCurrentmap = true;
      ResultsWaitIndicatorVisibility = false;
      ChartWaitIndicatorVisibility = false;
      MultiPlexVisible = System.Windows.Visibility.Visible;
      SinglePlexVisible = System.Windows.Visibility.Hidden;
      ValidationCoverVisible = System.Windows.Visibility.Hidden;
      PlexButtonString = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Experiment_Active_Regions),
        Language.TranslationSource.Instance.CurrentCulture);

      Instance = this;

      WrldMap = WorldMap.Create();
      AnalysisMap = AnalysysMap.Create();

      CLButtonsChecked = new ObservableCollection<bool> { false, false, true, false, false, true, false, false };
      CLAxis = new ObservableCollection<string> { "CL1", "CL2" };
      AnalysisVisible = System.Windows.Visibility.Hidden;
      Analysis2DVisible = System.Windows.Visibility.Visible;
      Analysis3DVisible = System.Windows.Visibility.Hidden;

      CurrentMfiItems = new ObservableCollection<string>();
      CurrentCvItems = new ObservableCollection<string>();
      for (var i = 0; i < 10; i++)
      {
        CurrentMfiItems.Add("");
        CurrentCvItems.Add("");
      }
      BackingMfiItems = new ObservableCollection<string>();
      BackingCvItems = new ObservableCollection<string>();
      for (var i = 0; i < 10; i++)
      {
        BackingMfiItems.Add("");
        BackingCvItems.Add("");
      }
      DisplayedMfiItems = CurrentMfiItems;
      DisplayedCvItems = CurrentCvItems;
      _fillDataActive = false;
    }

    public static ResultsViewModel Create()
    {
      return ViewModelSource.Create(() => new ResultsViewModel());
    }

    public void CLButtonClick(int CL)
    {
      if (CL < 4)
      {
        CLButtonsChecked[0] = false;
        CLButtonsChecked[1] = false;
        CLButtonsChecked[2] = false;
        CLButtonsChecked[3] = false;
        CLAxis[1] = $"CL {CL}";
      }
      else
      {
        CLButtonsChecked[4] = false;
        CLButtonsChecked[5] = false;
        CLButtonsChecked[6] = false;
        CLButtonsChecked[7] = false;
        CLAxis[0] = $"CL {CL - 4}";
      }
      CLButtonsChecked[CL] = true;
      SetDisplayedMap();
      _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
        {
          HeatMapAPI.API.ReDraw();
        }));
    }

    public void ClearGraphs(bool current = true)
    {
      ScatterChartViewModel.Instance.ScttrData.ClearData(current);
      HeatMapAPI.API.ClearData(current);
      AnalysisMap.ClearData(current);
      if (!current)
      {
        ActiveRegionsStatsController.Instance.ResetBackingDisplayedStats();
      }
      //else
        //do not ResetCurrentActiveRegionsDisplayedStats() here. This function can be called during runtime -> can loose data
        //ResetCurrentActiveRegionsDisplayedStats() is called in the App.StartingToReadWellEventHandler()
      Views.ResultsView.Instance.ClearHeatMaps();
    }

    private bool ParseBeadInfo(string path, List<ProcessedBead> beadStructs)
    {
      List<string> linesInFile = Core.DataProcessor.GetDataFromFile(path);
      if (linesInFile.Count == 1 && linesInFile[0] == " ")
      {
        Notification.ShowLocalized(nameof(Language.Resources.Notification_Empty_File));
        return false;
      }
      for (var i = 0; i < linesInFile.Count; i++)
      {
        try
        {
          var bs = Core.DataProcessor.ParseRow(linesInFile[i]);
          beadStructs.Add(bs);
        }
        catch(FormatException){}
      }
      return true;
    }

    /// <summary>
    /// Task to fill XY plot with data from file
    /// </summary>
    public void FillAllData()
    {
      if (_fillDataActive)
      {
        Notification.Show("Please wait for the previous well to load");
        return;
      }
      _fillDataActive = true;
      var hiRez = AnalysisVisible == System.Windows.Visibility.Visible;
      _ = Task.Run(() =>
      {
        var path = PlatePictogramViewModel.Instance.PlatePictogram.GetSelectedFilePath();  //@"C:\Emissioninc\KEIZ0R-LEGION\AcquisitionData\rowtest1A1_0.csv";//
        if (!System.IO.File.Exists(path)) //rowtest1A1_0  //BeadAssayA1_19 //val speed test 2E7_0
        {
          Notification.ShowLocalized(  nameof(Language.Resources.Notification_File_Inexistent));
          ResultsWaitIndicatorVisibility = false;
          ChartWaitIndicatorVisibility = false;
          _fillDataActive = false;
          return;
        }
        AnalysisMap.InitBackingWellResults();
        _cachedBeadStructsForLoadedData.Clear();
        if (!ParseBeadInfo(path, _cachedBeadStructsForLoadedData))
        {
          ResultsWaitIndicatorVisibility = false;
          ChartWaitIndicatorVisibility = false;
          _fillDataActive = false;
          return;
        }
        _ = Task.Run(() => Core.DataProcessor.BinScatterData(_cachedBeadStructsForLoadedData, fromFile: true));
        _ = Task.Run(() => Core.DataProcessor.CalculateStatistics(_cachedBeadStructsForLoadedData));
        Core.DataProcessor.BinMapData(_cachedBeadStructsForLoadedData, current: false, hiRez);
        //DisplayedMap.Sort((x, y) => x.A.CompareTo(y.A));
        _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
        {
          try
          {
            HeatMapAPI.API.ReDraw(hiRez);
            AnalysisMap.FillBackingMap();
          }
          catch (Exception e)
          {
            if (Language.TranslationSource.Instance.CurrentCulture.TextInfo.CultureName == "zh-CN")
            {
              Notification.Show($"请将此问题报告给制造商\n {e.Message}");
            }
            else
            {
              Notification.Show($"Something went wrong during File loading.\nPlease report this issue to the manufacturer\n {e.Message}");
            }
          }
          finally
          {
            ChartWaitIndicatorVisibility = false;
            _fillDataActive = false;
          }
        }));
        MainViewModel.Instance.EventCountLocal[0] = _cachedBeadStructsForLoadedData.Count.ToString();
      });
    }

    public void ClearCurrentCalibrationStats()
    {
      for (var i = 0; i < 10; i++)
      {
        CurrentMfiItems[i] = "";
        CurrentCvItems[i] = "";
      }
    }

    public void DecodeCalibrationStats(ChannelsCalibrationStats stats, bool current)
    {
      ObservableCollection<string> mfiItems;
      ObservableCollection<string> cvItems;
      if (current)
      {
        mfiItems = CurrentMfiItems;
        cvItems = CurrentCvItems;
      }
      else
      {
        mfiItems = BackingMfiItems;
        cvItems = BackingCvItems;
      }

      mfiItems[0] = stats.Greenssc.Mean.ToString($"{0:0.0}");
      cvItems[0] = stats.Greenssc.CoeffVar.ToString($"{0:0.00}");

      mfiItems[1] = stats.GreenB.Mean.ToString($"{0:0.0}");
      cvItems[1] = stats.GreenB.CoeffVar.ToString($"{0:0.00}");

      mfiItems[2] = stats.GreenC.Mean.ToString($"{0:0.0}");
      cvItems[2] = stats.GreenC.CoeffVar.ToString($"{0:0.00}");

      mfiItems[3] = stats.Redssc.Mean.ToString($"{0:0.0}");
      cvItems[3] = stats.Redssc.CoeffVar.ToString($"{0:0.00}");

      mfiItems[4] = stats.Cl1.Mean.ToString($"{0:0.0}");
      cvItems[4] = stats.Cl1.CoeffVar.ToString($"{0:0.00}");

      mfiItems[5] = stats.Cl2.Mean.ToString($"{0:0.0}");
      cvItems[5] = stats.Cl2.CoeffVar.ToString($"{0:0.00}");

      mfiItems[6] = stats.Cl3.Mean.ToString($"{0:0.0}");
      cvItems[6] = stats.Cl3.CoeffVar.ToString($"{0:0.00}");

      mfiItems[7] = stats.Violetssc.Mean.ToString($"{0:0.0}");
      cvItems[7] = stats.Violetssc.CoeffVar.ToString($"{0:0.00}");

      mfiItems[8] = stats.Cl0.Mean.ToString($"{0:0.0}");
      cvItems[8] = stats.Cl0.CoeffVar.ToString($"{0:0.00}");

      mfiItems[9] = stats.Fsc.Mean.ToString($"{0:0.0}");
      cvItems[9] = stats.Fsc.CoeffVar.ToString($"{0:0.00}");
    }

    public void PlotCurrent(bool current = true)
    {
      DisplaysCurrentmap = current;
      SetDisplayedMap();
      ScatterChartViewModel.Instance.ScttrData.DisplayCurrent(current);
      if (current)
      {
        Views.PlatePictogramView.Instance.DrawingPlate.UnselectAllCells();
        if (App.MapRegions != null)
        {
          ActiveRegionsStatsController.Instance.DisplayCurrentBeadStats();
        }

        _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
        {
          HeatMapAPI.API.ReDraw();
        }));
        MainViewModel.Instance.EventCountField = MainViewModel.Instance.EventCountCurrent;
        DisplayedMfiItems = CurrentMfiItems;
        DisplayedCvItems = CurrentCvItems;
        return;
      }

      if (App.MapRegions != null)
      {
        ResultsWaitIndicatorVisibility = true;
        ChartWaitIndicatorVisibility = true;
        ActiveRegionsStatsController.Instance.DisplayCurrentBeadStats(current: false);
      }
      MainViewModel.Instance.EventCountField = MainViewModel.Instance.EventCountLocal;
      DisplayedMfiItems = BackingMfiItems;
      DisplayedCvItems = BackingCvItems;
    }

    /// <summary>
    /// Called on Activemap change to fill all world maps with data from files
    /// </summary>
    public void FillWorldMaps()
    {
      _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        WrldMap.InitMaps();
        PlotCurrent(DisplaysCurrentmap);
        ResultsWaitIndicatorVisibility = false;
        ChartWaitIndicatorVisibility = false;
      }));
    }

    private void SetDisplayedMap()
    {
      WrldMap.Flipped = false;
      MapIndex mapIndex = MapIndex.Empty;
      if (CLButtonsChecked[4])
      {
        if (CLButtonsChecked[1])
        {
          mapIndex = MapIndex.CL01;
        }
        else if (CLButtonsChecked[2])
        {
          mapIndex = MapIndex.CL02;
        }
        else if (CLButtonsChecked[3])
        {
          mapIndex = MapIndex.CL03;
        }
        else
        {
          Views.ResultsView.Instance.ClearHeatMaps();
          WrldMap.DisplayedWorldMap.Clear();
          mapIndex = MapIndex.Empty;
        }
      }
      else if (CLButtonsChecked[5])
      {
        if (CLButtonsChecked[0])
        {
          WrldMap.Flipped = true;
          mapIndex = MapIndex.CL01;
        }
        else if (CLButtonsChecked[2])
        {
          mapIndex = MapIndex.CL12;
        }
        else if (CLButtonsChecked[3])
        {
          mapIndex = MapIndex.CL13;
        }
        else
        {
          Views.ResultsView.Instance.ClearHeatMaps();
          WrldMap.DisplayedWorldMap.Clear();
        }
      }
      else if (CLButtonsChecked[6])
      {
        if (CLButtonsChecked[0])
        {
          WrldMap.Flipped = true;
          mapIndex = MapIndex.CL02;
        }
        else if (CLButtonsChecked[1])
        {
          WrldMap.Flipped = true;
          mapIndex = MapIndex.CL12;
        }
        else if (CLButtonsChecked[3])
        {
          mapIndex = MapIndex.CL23;
        }
        else
        {
          Views.ResultsView.Instance.ClearHeatMaps();
          WrldMap.DisplayedWorldMap.Clear();
          mapIndex = MapIndex.Empty;
        }
      }
      else if (CLButtonsChecked[7])
      {
        if (CLButtonsChecked[0])
        {
          WrldMap.Flipped = true;
          mapIndex = MapIndex.CL03;
        }
        else if (CLButtonsChecked[1])
        {
          WrldMap.Flipped = true;
          mapIndex = MapIndex.CL13;
        }
        else if (CLButtonsChecked[2])
        {
          WrldMap.Flipped = true;
          mapIndex = MapIndex.CL23;
        }
        else
        {
          Views.ResultsView.Instance.ClearHeatMaps();
          WrldMap.DisplayedWorldMap.Clear();
          mapIndex = MapIndex.Empty;
        }
      }
      HeatMapAPI.API.ChangeDisplayedMap(mapIndex, DisplaysCurrentmap);
      AnalysisMap.ChangeDisplayedMap(mapIndex, DisplaysCurrentmap);
      WrldMap.DisplayedWmap = mapIndex;
      WrldMap.FillDisplayedWorldMap();
    }

    public void PlexButtonClick()
    {
      if (MultiPlexVisible == System.Windows.Visibility.Visible)
      {
        ShowSinglePlexResults();
        return;
      }
      ShowMultiPlexResults();
    }

    public void ShowSinglePlexResults()
    {
      PlexButtonString = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Experiment_Stats), Language.TranslationSource.Instance.CurrentCulture);
      MultiPlexVisible = System.Windows.Visibility.Hidden;
      SinglePlexVisible = System.Windows.Visibility.Visible;
    }

    public void ShowMultiPlexResults()
    {
      PlexButtonString = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Experiment_Active_Regions), Language.TranslationSource.Instance.CurrentCulture);
      MultiPlexVisible = System.Windows.Visibility.Visible;
      SinglePlexVisible = System.Windows.Visibility.Hidden;
    }

    public void Show3D()
    {
      Analysis3DVisible = System.Windows.Visibility.Visible;
      Analysis2DVisible = System.Windows.Visibility.Hidden;
    }

    public void Show2D()
    {
      Views.ResultsView.Instance.ShowLargeXYPlot();
      Analysis2DVisible = System.Windows.Visibility.Visible;
      Analysis3DVisible = System.Windows.Visibility.Hidden;
    }

    public void ShowAnalysis()
    {
      Views.ResultsView.Instance.Plot2DButton.IsChecked = true;
      AnalysisVisible = System.Windows.Visibility.Visible;
      Show2D();
    }

    public void ShowResults()
    {
      Views.ResultsView.Instance.ShowSmallXYPlot();
      AnalysisVisible = System.Windows.Visibility.Hidden;
      Analysis2DVisible = System.Windows.Visibility.Visible;
      Analysis3DVisible = System.Windows.Visibility.Hidden;
    }

    public void XYprint()
    {
      Views.ResultsView.Instance.PrintXY();
    }

    public void AnalysisPrint()
    {
      Views.ResultsView.Instance.Print3D();
    }
  }
}