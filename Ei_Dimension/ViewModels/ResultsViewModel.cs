using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Ei_Dimension.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DIOS.Core;
using Ei_Dimension.Controllers;

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
    public virtual ObservableCollection<DoubleHeatMapData> DisplayedAnalysisMap { get; set; }
    public virtual ObservableCollection<DoubleHeatMapData> CurrentAnalysis01Map { get; set; }
    public virtual ObservableCollection<DoubleHeatMapData> CurrentAnalysis02Map { get; set; }
    public virtual ObservableCollection<DoubleHeatMapData> CurrentAnalysis03Map { get; set; }
    public virtual ObservableCollection<DoubleHeatMapData> CurrentAnalysis12Map { get; set; }
    public virtual ObservableCollection<DoubleHeatMapData> CurrentAnalysis13Map { get; set; }
    public virtual ObservableCollection<DoubleHeatMapData> CurrentAnalysis23Map { get; set; }
    public virtual ObservableCollection<DoubleHeatMapData> BackingAnalysis01Map { get; set; }
    public virtual ObservableCollection<DoubleHeatMapData> BackingAnalysis02Map { get; set; }
    public virtual ObservableCollection<DoubleHeatMapData> BackingAnalysis03Map { get; set; }
    public virtual ObservableCollection<DoubleHeatMapData> BackingAnalysis12Map { get; set; }
    public virtual ObservableCollection<DoubleHeatMapData> BackingAnalysis13Map { get; set; }
    public virtual ObservableCollection<DoubleHeatMapData> BackingAnalysis23Map { get; set; }
    public List<RegionReporterResult> BackingWResults { get; set; }
    public virtual DrawingPlate PlatePictogram { get; set; }
    public virtual System.Windows.Visibility Buttons384Visible { get; set; }
    public virtual System.Windows.Visibility LeftLabel384Visible { get; set; }
    public virtual System.Windows.Visibility RightLabel384Visible { get; set; }
    public virtual System.Windows.Visibility TopLabel384Visible { get; set; }
    public virtual System.Windows.Visibility BottomLabel384Visible { get; set; }
    public virtual System.Windows.Visibility AnalysisVisible { get; set; }
    public virtual System.Windows.Visibility Analysis2DVisible { get; set; }
    public virtual System.Windows.Visibility Analysis3DVisible { get; set; }
    public virtual System.Windows.Visibility PlatePictogramIsCovered { get; set; }
    public virtual ObservableCollection<string> DisplayedMfiItems { get; set; }
    public virtual ObservableCollection<string> DisplayedCvItems { get; set; }
    public virtual ObservableCollection<string> CurrentMfiItems { get; set; }
    public virtual ObservableCollection<string> CurrentCvItems { get; set; }
    public virtual ObservableCollection<string> BackingMfiItems { get; set; }
    public virtual ObservableCollection<string> BackingCvItems { get; set; }
    public virtual string PlexButtonString { get; set; }
    public virtual ObservableCollection<bool> CornerButtonsChecked { get; set; }
    public virtual ObservableCollection<bool> CLButtonsChecked { get; set; }
    public virtual ObservableCollection<string> CLAxis { get; set; }
    public virtual ObservableCollection<string> XYCutOffString { get; set; }
    public int XYCutoff { get; set; }
    public static ResultsViewModel Instance { get; private set; }
    private bool _fillDataActive;
    public const int HIREZDEFINITION = 512;

    private List<BeadInfoStruct> _cachedBeadStructsForLoadedData = new List<BeadInfoStruct>(100000);

    protected ResultsViewModel()
    {
      DisplaysCurrentmap = true;
      ResultsWaitIndicatorVisibility = false;
      ChartWaitIndicatorVisibility = false;
      MultiPlexVisible = System.Windows.Visibility.Visible;
      SinglePlexVisible = System.Windows.Visibility.Hidden;
      ValidationCoverVisible = System.Windows.Visibility.Hidden;
      PlexButtonString = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Experiment_Active_Regions), Language.TranslationSource.Instance.CurrentCulture);

      Instance = this;

      WorldMap.Create();

      CurrentAnalysis01Map = new ObservableCollection<DoubleHeatMapData>();
      CurrentAnalysis02Map = new ObservableCollection<DoubleHeatMapData>();
      CurrentAnalysis03Map = new ObservableCollection<DoubleHeatMapData>();
      CurrentAnalysis12Map = new ObservableCollection<DoubleHeatMapData>();
      CurrentAnalysis13Map = new ObservableCollection<DoubleHeatMapData>();
      CurrentAnalysis23Map = new ObservableCollection<DoubleHeatMapData>();
      BackingAnalysis01Map = new ObservableCollection<DoubleHeatMapData>();
      BackingAnalysis02Map = new ObservableCollection<DoubleHeatMapData>();
      BackingAnalysis03Map = new ObservableCollection<DoubleHeatMapData>();
      BackingAnalysis12Map = new ObservableCollection<DoubleHeatMapData>();
      BackingAnalysis13Map = new ObservableCollection<DoubleHeatMapData>();
      BackingAnalysis23Map = new ObservableCollection<DoubleHeatMapData>();
      BackingWResults = new List<RegionReporterResult>();

      DisplayedAnalysisMap = CurrentAnalysis12Map;

      PlatePictogram = DrawingPlate.Create();
      Buttons384Visible = System.Windows.Visibility.Hidden;
      CornerButtonsChecked = new ObservableCollection<bool> { true, false, false, false };
      CLButtonsChecked = new ObservableCollection<bool> { false, false, true, false, false, true, false, false };
      CLAxis = new ObservableCollection<string> { "CL1", "CL2" };
      LeftLabel384Visible = System.Windows.Visibility.Visible;
      RightLabel384Visible = System.Windows.Visibility.Hidden;
      TopLabel384Visible = System.Windows.Visibility.Visible;
      BottomLabel384Visible = System.Windows.Visibility.Hidden;
      AnalysisVisible = System.Windows.Visibility.Hidden;
      Analysis2DVisible = System.Windows.Visibility.Visible;
      Analysis3DVisible = System.Windows.Visibility.Hidden;
      PlatePictogramIsCovered = System.Windows.Visibility.Hidden;

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
      XYCutoff = Settings.Default.XYCutOff;
      XYCutOffString = new ObservableCollection<string> { XYCutoff.ToString() };
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
          Core.DataProcessor.AnalyzeHeatMap();
        }));
    }

    public void CornerButtonClick(int corner)
    {
      var changed = PlatePictogram.ChangeCorner(corner);
      if (!changed)
        return;
      //adjust labels
      switch (corner)
      {
        case 1:
          LeftLabel384Visible = System.Windows.Visibility.Visible;
          RightLabel384Visible = System.Windows.Visibility.Hidden;
          TopLabel384Visible = System.Windows.Visibility.Visible;
          BottomLabel384Visible = System.Windows.Visibility.Hidden;
          break;
        case 2:
          LeftLabel384Visible = System.Windows.Visibility.Hidden;
          RightLabel384Visible = System.Windows.Visibility.Visible;
          TopLabel384Visible = System.Windows.Visibility.Visible;
          BottomLabel384Visible = System.Windows.Visibility.Hidden;
          break;
        case 3:
          LeftLabel384Visible = System.Windows.Visibility.Visible;
          RightLabel384Visible = System.Windows.Visibility.Hidden;
          TopLabel384Visible = System.Windows.Visibility.Hidden;
          BottomLabel384Visible = System.Windows.Visibility.Visible;
          break;
        case 4:
          LeftLabel384Visible = System.Windows.Visibility.Hidden;
          RightLabel384Visible = System.Windows.Visibility.Visible;
          TopLabel384Visible = System.Windows.Visibility.Hidden;
          BottomLabel384Visible = System.Windows.Visibility.Visible;
          break;
      }
      Views.ResultsView.Instance.DrawingPlate.UnselectAllCells();
      CornerButtonsChecked[0] = false;
      CornerButtonsChecked[1] = false;
      CornerButtonsChecked[2] = false;
      CornerButtonsChecked[3] = false;
      CornerButtonsChecked[corner - 1] = true;
    }

    public void ToCurrentButtonClick()
    {
      if (App.Device.IsMeasurementGoing)
        return;
      #if DEBUG
      Console.WriteLine(new System.Diagnostics.StackTrace());
      #endif

      PlotCurrent();

      int tempCorner = 1;
      if (PlatePictogram.CurrentlyReadCell.row < 8)
        tempCorner = PlatePictogram.CurrentlyReadCell.col < 12 ? 1 : 2;
      else
        tempCorner = PlatePictogram.CurrentlyReadCell.col < 12 ? 3 : 4;
      CornerButtonClick(tempCorner);
    }

    public void ClearGraphs(bool current = true)
    {
      ScatterChartViewModel.Instance.ScttrData.ClearData(current);
      HeatMap.Clear(current);
      if (current)
      {
        CurrentAnalysis01Map.Clear();
        CurrentAnalysis02Map.Clear();
        CurrentAnalysis03Map.Clear();
        CurrentAnalysis12Map.Clear();
        CurrentAnalysis13Map.Clear();
        CurrentAnalysis23Map.Clear();
        //do not ResetCurrentActiveRegionsDisplayedStats() here. This function can be called during runtime -> can loose data
        //ResetCurrentActiveRegionsDisplayedStats() is called in the App.StartingToReadWellEventHandler()
      }
      else
      {
        BackingAnalysis01Map.Clear();
        BackingAnalysis02Map.Clear();
        BackingAnalysis03Map.Clear();
        lock (BackingAnalysis12Map)
        {
          BackingAnalysis12Map.Clear();
        }
        BackingAnalysis13Map.Clear();
        BackingAnalysis23Map.Clear();
        ActiveRegionsStatsController.Instance.ResetBackingDisplayedStats();
      }
      Views.ResultsView.Instance.ClearPoints();
    }

    public void SelectedCellChanged()
    {
      if (App.Device.IsMeasurementGoing)
        return;
      var temp = PlatePictogram.GetSelectedCell();
      if (temp.row == -1)
        return;
      PlatePictogram.SelectedCell = temp;
      if (temp == PlatePictogram.CurrentlyReadCell)
      {
        ToCurrentButtonClick();
        return;
      }
      PlotCurrent(false);
      ClearGraphs(false);
      FillAllData();
    }

    private bool ParseBeadInfo(string path, List<BeadInfoStruct> beadStructs)
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
        var path = PlatePictogram.GetSelectedFilePath();  //@"C:\Emissioninc\KEIZ0R-LEGION\AcquisitionData\rowtest1A1_0.csv";//
        if (!System.IO.File.Exists(path)) //rowtest1A1_0  //BeadAssayA1_19 //val speed test 2E7_0
        {
          Notification.ShowLocalized(  nameof(Language.Resources.Notification_File_Inexistent));
          ResultsWaitIndicatorVisibility = false;
          ChartWaitIndicatorVisibility = false;
          _fillDataActive = false;
          return;
        }
        InitBackingWellResults();
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
            Core.DataProcessor.AnalyzeHeatMap(hiRez);
            FillBackingAnalysisMap();
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

    private void InitBackingWellResults()
    {
      BackingWResults.Clear();
      if (MapRegionsController.ActiveRegionNums.Count > 0)
      {
        foreach (var reg in MapRegionsController.ActiveRegionNums)
        {
          if(reg == 0)
            continue;
          BackingWResults.Add(new RegionReporterResult { regionNumber = (ushort)reg });
        }
      }
    }

    private void FillBackingAnalysisMap()
    {
      foreach (var result in BackingWResults)
      {
        if (App.Device.MapCtroller.ActiveMap.Regions.TryGetValue(result.regionNumber, out var region))
        {
          var x = HeatMapData.bins[region.Center.x];
          var y = HeatMapData.bins[region.Center.y];
          lock (BackingAnalysis12Map)
          {
            if (result.ReporterValues.Count > 0)
            {
              BackingAnalysis12Map.Add(new DoubleHeatMapData(x, y, result.ReporterValues.Average()));
            }
          }
        }
      }
    }

    public void PlotCurrent(bool current = true)
    {
      DisplaysCurrentmap = current;
      SetDisplayedMap();
      ScatterChartViewModel.Instance.ScttrData.DisplayCurrent(current);
      if (current)
      {
        Views.ResultsView.Instance.DrawingPlate.UnselectAllCells();
        if (App.MapRegions != null)
        {
          ActiveRegionsStatsController.Instance.DisplayCurrentBeadStats();
        }

        _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
        {
          Core.DataProcessor.AnalyzeHeatMap();
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
          if (DisplaysCurrentmap)
          {
            DisplayedAnalysisMap = CurrentAnalysis01Map;
          }
          else
          {
            DisplayedAnalysisMap = BackingAnalysis01Map;
          }
          mapIndex = MapIndex.CL01;
        }
        else if (CLButtonsChecked[2])
        {
          if (DisplaysCurrentmap)
          {
            DisplayedAnalysisMap = CurrentAnalysis02Map;
          }
          else
          {
            DisplayedAnalysisMap = BackingAnalysis02Map;
          }
          mapIndex = MapIndex.CL02;
        }
        else if (CLButtonsChecked[3])
        {
          if (DisplaysCurrentmap)
          {
            DisplayedAnalysisMap = CurrentAnalysis03Map;
          }
          else
          {
            DisplayedAnalysisMap = BackingAnalysis03Map;
          }
          mapIndex = MapIndex.CL03;
        }
        else
        {
          DisplayedAnalysisMap = null;
          Views.ResultsView.Instance.ClearPoints();
          WrldMap.DisplayedWorldMap.Clear();
          mapIndex = MapIndex.Empty;
        }
      }
      else if (CLButtonsChecked[5])
      {
        if (CLButtonsChecked[0])
        {
          if (DisplaysCurrentmap)
          {
            DisplayedAnalysisMap = CurrentAnalysis01Map;
          }
          else
          {
            DisplayedAnalysisMap = BackingAnalysis01Map;
          }
          WrldMap.Flipped = true;
          mapIndex = MapIndex.CL01;
        }
        else if (CLButtonsChecked[2])
        {
          if (DisplaysCurrentmap)
          {
            DisplayedAnalysisMap = CurrentAnalysis12Map;
          }
          else
          {
            DisplayedAnalysisMap = BackingAnalysis12Map;
          }
          mapIndex = MapIndex.CL12;
        }
        else if (CLButtonsChecked[3])
        {
          if (DisplaysCurrentmap)
          {
            DisplayedAnalysisMap = CurrentAnalysis13Map;
          }
          else
          {
            DisplayedAnalysisMap = BackingAnalysis13Map;
          }
          mapIndex = MapIndex.CL13;
        }
        else
        {
          DisplayedAnalysisMap = null;
          Views.ResultsView.Instance.ClearPoints();
          WrldMap.DisplayedWorldMap.Clear();
        }
      }
      else if (CLButtonsChecked[6])
      {
        if (CLButtonsChecked[0])
        {
          if (DisplaysCurrentmap)
          {
            DisplayedAnalysisMap = CurrentAnalysis02Map;
          }
          else
          {
            DisplayedAnalysisMap = BackingAnalysis02Map;
          }
          WrldMap.Flipped = true;
          mapIndex = MapIndex.CL02;
        }
        else if (CLButtonsChecked[1])
        {
          if (DisplaysCurrentmap)
          {
            DisplayedAnalysisMap = CurrentAnalysis12Map;
          }
          else
          {
            DisplayedAnalysisMap = BackingAnalysis12Map;
          }
          WrldMap.Flipped = true;
          mapIndex = MapIndex.CL12;
        }
        else if (CLButtonsChecked[3])
        {
          if (DisplaysCurrentmap)
          {
            DisplayedAnalysisMap = CurrentAnalysis23Map;
          }
          else
          {
            DisplayedAnalysisMap = BackingAnalysis23Map;
          }
          mapIndex = MapIndex.CL23;
        }
        else
        {
          DisplayedAnalysisMap = null;
          Views.ResultsView.Instance.ClearPoints();
          WrldMap.DisplayedWorldMap.Clear();
          mapIndex = MapIndex.Empty;
        }
      }
      else if (CLButtonsChecked[7])
      {
        if (CLButtonsChecked[0])
        {
          if (DisplaysCurrentmap)
          {
            DisplayedAnalysisMap = CurrentAnalysis03Map;
          }
          else
          {
            DisplayedAnalysisMap = BackingAnalysis03Map;
          }
          WrldMap.Flipped = true;
          mapIndex = MapIndex.CL03;
        }
        else if (CLButtonsChecked[1])
        {
          if (DisplaysCurrentmap)
          {
            DisplayedAnalysisMap = CurrentAnalysis13Map;
          }
          else
          {
            DisplayedAnalysisMap = BackingAnalysis13Map;
          }
          WrldMap.Flipped = true;
          mapIndex = MapIndex.CL13;
        }
        else if (CLButtonsChecked[2])
        {
          if (DisplaysCurrentmap)
          {
            DisplayedAnalysisMap = CurrentAnalysis23Map;
          }
          else
          {
            DisplayedAnalysisMap = BackingAnalysis23Map;
          }
          WrldMap.Flipped = true;
          mapIndex = MapIndex.CL23;
        }
        else
        {
          DisplayedAnalysisMap = null;
          Views.ResultsView.Instance.ClearPoints();
          WrldMap.DisplayedWorldMap.Clear();
          mapIndex = MapIndex.Empty;
        }
      }
      HeatMap.Display(mapIndex, DisplaysCurrentmap);
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