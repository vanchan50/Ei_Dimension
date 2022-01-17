using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Ei_Dimension.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

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
    public virtual ObservableCollection<bool> ScatterSelectorState { get; set; }
    public ScatterData ScttrData { get; set; }
    public WorldMap WrldMap { get; set;}
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
    public List<MicroCy.WellResults> BackingWResults { get; set; }
    public virtual DrawingPlate PlatePictogram { get; set; }
    public virtual System.Windows.Visibility Buttons384Visible { get; set; }
    public virtual System.Windows.Visibility LeftLabel384Visible { get; set; }
    public virtual System.Windows.Visibility RightLabel384Visible { get; set; }
    public virtual System.Windows.Visibility TopLabel384Visible { get; set; }
    public virtual System.Windows.Visibility BottomLabel384Visible { get; set; }
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
    public virtual ObservableCollection<bool> CornerButtonsChecked { get; set; }
    public virtual ObservableCollection<bool> CLButtonsChecked { get; set; }
    public virtual ObservableCollection<string> CLAxis { get; set; }
    public virtual ObservableCollection<string> XYCutOffString { get; set; }
    public virtual ObservableCollection<string> HiSensitivityChannelName { get; set; }
    public int XYCutoff { get; set; }
    public static ResultsViewModel Instance { get; private set; }
    private bool _fillDataActive;
    public const int HIREZDEFINITION = 1024;

    protected ResultsViewModel()
    {
      DisplaysCurrentmap = true;
      ScatterSelectorState = new ObservableCollection<bool> { false, false, false, false, false };
      ResultsWaitIndicatorVisibility = false;
      ChartWaitIndicatorVisibility = false;
      MultiPlexVisible = System.Windows.Visibility.Visible;
      SinglePlexVisible = System.Windows.Visibility.Hidden;
      ValidationCoverVisible = System.Windows.Visibility.Hidden;
      PlexButtonString = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Experiment_Active_Regions), Language.TranslationSource.Instance.CurrentCulture);
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

      Instance = this;

      WorldMap.Create();
      ScatterData.Create();

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
      BackingWResults = new List<MicroCy.WellResults>();

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
      HiSensitivityChannelName = new ObservableCollection<string>
      {
        Language.Resources.ResourceManager.GetString(nameof(Language.Resources.DataAn_Green_Min),
          Language.TranslationSource.Instance.CurrentCulture),
        Language.Resources.ResourceManager.GetString(nameof(Language.Resources.DataAn_Green_Maj),
          Language.TranslationSource.Instance.CurrentCulture)
      };
      if (!Settings.Default.SensitivityChannelB)
        SwapHiSensChannelsStats();
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
      PlatePictogram.ChangeCorner(corner);
    }

    public void ToCurrentButtonClick()
    {
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
      ScttrData.ClearData(current);
      HeatMap.Clear(current);
      if (current)
      {
        CurrentAnalysis01Map.Clear();
        CurrentAnalysis02Map.Clear();
        CurrentAnalysis03Map.Clear();
        CurrentAnalysis12Map.Clear();
        CurrentAnalysis13Map.Clear();
        CurrentAnalysis23Map.Clear();
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
        for (var i = 0; i < App.MapRegions.BackingActiveRegionsCount.Count; i++)
        {
          App.MapRegions.BackingActiveRegionsCount[i] = "0";
          App.MapRegions.BackingActiveRegionsMean[i] = "0";
        }
      }
      Views.ResultsView.Instance.ClearPoints();
    }

    public void SelectedCellChanged()
    {
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

    public void ChangeScatterLegend(int num)  //TODO: For buttons
    {
      ScatterSelectorState[num] = !ScatterSelectorState[num];
      byte res = 0;
      res += ScatterSelectorState[0] ? (byte)1 : (byte)0;
      res += ScatterSelectorState[1] ? (byte)2 : (byte)0;
      res += ScatterSelectorState[2] ? (byte)4 : (byte)0;
      res += ScatterSelectorState[3] ? (byte)8 : (byte)0;
      res += ScatterSelectorState[4] ? (byte)16 : (byte)0;
      Settings.Default.ScatterGraphSelector = res;
      Settings.Default.Save();
    }

    private bool ParseBeadInfo(string path, List<MicroCy.BeadInfoStruct> beadStructs)
    {
      List<string> linesInFile = Core.DataProcessor.GetDataFromFile(path);
      if (linesInFile.Count == 1 && linesInFile[0] == " ")
      {
        Notification.Show("File is empty");
        return false;
      }
      for (var i = 0; i < linesInFile.Count; i++)
      {
        beadStructs.Add(Core.DataProcessor.ParseRow(linesInFile[i]));
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
        Notification.Show("Results loading failed:\nPlease wait for the previous well to load");
        return;
      }
      _fillDataActive = true;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             
      var hiRez = AnalysisVisible == System.Windows.Visibility.Visible;
      _ = Task.Run(() =>
      {
        var path = PlatePictogram.GetSelectedFilePath();  //@"C:\Emissioninc\KEIZ0R-LEGION\AcquisitionData\val speed test 2E7_0.csv"; //
        if (!System.IO.File.Exists(path))
        {
          Notification.Show($"File does not exist.\nPlease report this issue to the manufacturer");
          ResultsWaitIndicatorVisibility = false;
          ChartWaitIndicatorVisibility = false;
          _fillDataActive = false;
          return;
        }
        FillBackingWellResults();
        var beadStructsList = new List<MicroCy.BeadInfoStruct>(100000);
        if(!ParseBeadInfo(path, beadStructsList))
        {
          ResultsWaitIndicatorVisibility = false;
          ChartWaitIndicatorVisibility = false;
          _fillDataActive = false;
          return;
        }
        _ = Task.Run(() => Core.DataProcessor.BinScatterData(beadStructsList, fromFile: true));
        _ = Task.Run(() => Core.DataProcessor.CalculateStatistics(beadStructsList));
        Core.DataProcessor.BinMapData(beadStructsList, current: false, hiRez);
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
            Notification.Show($"Something went wrong during File loading.\nPlease report this issue to the manufacturer\n {e.Message}");
          }
          finally
          {
            ChartWaitIndicatorVisibility = false;
            _fillDataActive = false;
          }
        }));
        MainViewModel.Instance.EventCountLocal[0] = beadStructsList.Count.ToString();
      });
    }

    private void FillBackingWellResults()
    {
      BackingWResults.Clear();
      if (App.MapRegions.ActiveRegionNums.Count > 0)
      {
        foreach (var reg in App.MapRegions.ActiveRegionNums)
        {
          BackingWResults.Add(new MicroCy.WellResults { regionNumber = (ushort)reg });
        }
      }
    }

    private void FillBackingAnalysisMap()
    {
      foreach (var result in BackingWResults)
      {
        var RegionIndex = App.Device.MapCtroller.ActiveMap.regions.FindIndex(r => r.Number == result.regionNumber);
        if (RegionIndex != -1)
        {
          var x = HeatMapData.bins[App.Device.MapCtroller.ActiveMap.regions[RegionIndex].Center.x];
          var y = HeatMapData.bins[App.Device.MapCtroller.ActiveMap.regions[RegionIndex].Center.y];
          lock (BackingAnalysis12Map)
          {
            if (result.RP1vals.Count > 0)
            {
              BackingAnalysis12Map.Add(new DoubleHeatMapData(x, y, result.RP1vals.Average()));
            }
          }
        }
      }
    }

    public void PlotCurrent(bool current = true)
    {
      DisplaysCurrentmap = current;
      SetDisplayedMap();
      ScttrData.DisplayCurrent(current);
      if (current)
      {
        if (App.MapRegions != null)
        {
          App.MapRegions.DisplayedActiveRegionsCount = App.MapRegions.CurrentActiveRegionsCount;
          App.MapRegions.DisplayedActiveRegionsMean = App.MapRegions.CurrentActiveRegionsMean;
        }

        _ = App.Current.Dispatcher.BeginInvoke((Action)(() =>
        {
          Core.DataProcessor.AnalyzeHeatMap();
        }));
        MainViewModel.Instance.EventCountField = MainViewModel.Instance.EventCountCurrent;
        DisplayedMfiItems = CurrentMfiItems;
        DisplayedCvItems = CurrentCvItems;
      }
      else
      {
        if (App.MapRegions != null)
        {
          ResultsWaitIndicatorVisibility = true;
          ChartWaitIndicatorVisibility = true;
          App.MapRegions.DisplayedActiveRegionsCount = App.MapRegions.BackingActiveRegionsCount;
          App.MapRegions.DisplayedActiveRegionsMean = App.MapRegions.BackingActiveRegionsMean;
        }
        MainViewModel.Instance.EventCountField = MainViewModel.Instance.EventCountLocal;
        DisplayedMfiItems = BackingMfiItems;
        DisplayedCvItems = BackingCvItems;
      }
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
      WrldMap.FillDisplayedMap();
    }

    public void PlexButtonClick()
    {
      if (MultiPlexVisible == System.Windows.Visibility.Visible)
      {
        ShowSinglePlexResults();
      }
      else
      {
        ShowMultiPlexResults();
      }
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
      Views.ResultsView.Instance.Plot3DButton.IsChecked = true;
      AnalysisVisible = System.Windows.Visibility.Visible;
      Show3D();
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

    public void Scatterprint()
    {
      Views.ResultsView.Instance.PrintScatter();
    }

    public void AnalysisPrint()
    {
      Views.ResultsView.Instance.Print3D();
    }

    public void SwapHiSensChannelsStats()
    {
      var temps = HiSensitivityChannelName[0];
      HiSensitivityChannelName[0] = HiSensitivityChannelName[1];
      HiSensitivityChannelName[1] = temps;
    }
  }
}