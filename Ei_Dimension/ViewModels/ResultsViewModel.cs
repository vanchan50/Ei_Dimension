using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Ei_Dimension.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Ei_Dimension.ViewModels
{
  [POCOViewModel]
  public class ResultsViewModel
  {
    public virtual ObservableCollection<bool> ScatterSelectorState { get; set; }
    public virtual ObservableCollection<HistogramData> CurrentForwardSsc { get; set; }
    public virtual ObservableCollection<HistogramData> CurrentVioletSsc { get; set; }
    public virtual ObservableCollection<HistogramData> CurrentRedSsc { get; set; }
    public virtual ObservableCollection<HistogramData> CurrentGreenSsc { get; set; }
    public virtual ObservableCollection<HistogramData> CurrentReporter { get; set; }
    public virtual ObservableCollection<HistogramData> DisplayedForwardSsc { get; set; }
    public virtual ObservableCollection<HistogramData> DisplayedVioletSsc { get; set; }
    public virtual ObservableCollection<HistogramData> DisplayedRedSsc { get; set; }
    public virtual ObservableCollection<HistogramData> DisplayedGreenSsc { get; set; }
    public virtual ObservableCollection<HistogramData> DisplayedReporter { get; set; }
    public virtual ObservableCollection<HistogramData> BackingForwardSsc { get; set; }
    public virtual ObservableCollection<HistogramData> BackingVioletSsc { get; set; }
    public virtual ObservableCollection<HistogramData> BackingRedSsc { get; set; }
    public virtual ObservableCollection<HistogramData> BackingGreenSsc { get; set; }
    public virtual ObservableCollection<HistogramData> BackingReporter { get; set; }
    public virtual ObservableCollection<HeatMapData> WorldMap { get; set; }
    public List<HeatMapData> CurrentCL01Map { get; private set; }
    public List<HeatMapData> CurrentCL02Map { get; private set; }
    public List<HeatMapData> CurrentCL03Map { get; private set; }
    public List<HeatMapData> CurrentCL12Map { get; private set; }
    public List<HeatMapData> CurrentCL13Map { get; private set; }
    public List<HeatMapData> CurrentCL23Map { get; private set; }
    public List<HeatMapData> BackingCL01Map { get; private set; }
    public List<HeatMapData> BackingCL02Map { get; private set; }
    public List<HeatMapData> BackingCL03Map { get; private set; }
    public List<HeatMapData> BackingCL12Map { get; private set; }
    public List<HeatMapData> BackingCL13Map { get; private set; }
    public List<HeatMapData> BackingCL23Map { get; private set; }
    public Dictionary<(int x, int y), int> CurrentCL01Dict { get; private set; }
    public Dictionary<(int x, int y), int> CurrentCL02Dict { get; private set; }
    public Dictionary<(int x, int y), int> CurrentCL03Dict { get; private set; }
    public Dictionary<(int x, int y), int> CurrentCL12Dict { get; private set; }
    public Dictionary<(int x, int y), int> CurrentCL13Dict { get; private set; }
    public Dictionary<(int x, int y), int> CurrentCL23Dict { get; private set; }
    public Dictionary<(int x, int y), int> BackingCL01Dict { get; private set; }
    public Dictionary<(int x, int y), int> BackingCL02Dict { get; private set; }
    public Dictionary<(int x, int y), int> BackingCL03Dict { get; private set; }
    public Dictionary<(int x, int y), int> BackingCL12Dict { get; private set; }
    public Dictionary<(int x, int y), int> BackingCL13Dict { get; private set; }
    public Dictionary<(int x, int y), int> BackingCL23Dict { get; private set; }
    public virtual DrawingPlate PlatePictogram { get; set; }
    public virtual System.Windows.Visibility Buttons384Visible { get; set; }
    public virtual System.Windows.Visibility LeftLabel384Visible { get; set; }
    public virtual System.Windows.Visibility RightLabel384Visible { get; set; }
    public virtual System.Windows.Visibility TopLabel384Visible { get; set; }
    public virtual System.Windows.Visibility BottomLabel384Visible { get; set; }
    public virtual ObservableCollection<bool> CornerButtonsChecked { get; set; }
    public virtual ObservableCollection<bool> CLButtonsChecked { get; set; }
    public static ResultsViewModel Instance { get; private set; }

    protected ResultsViewModel()
    {
      ScatterSelectorState = new ObservableCollection<bool> { false, false, false, false, false };
      WorldMap = new ObservableCollection<HeatMapData>();

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
      CurrentForwardSsc = new ObservableCollection<HistogramData>();
      CurrentVioletSsc = new ObservableCollection<HistogramData>();
      CurrentRedSsc = new ObservableCollection<HistogramData>();
      CurrentGreenSsc = new ObservableCollection<HistogramData>();
      CurrentReporter = new ObservableCollection<HistogramData>();
      BackingForwardSsc = new ObservableCollection<HistogramData>();
      BackingVioletSsc = new ObservableCollection<HistogramData>();
      BackingRedSsc = new ObservableCollection<HistogramData>();
      BackingGreenSsc = new ObservableCollection<HistogramData>();
      BackingReporter = new ObservableCollection<HistogramData>();

      CurrentCL01Dict = new Dictionary<(int x, int y), int>();
      CurrentCL02Dict = new Dictionary<(int x, int y), int>();
      CurrentCL03Dict = new Dictionary<(int x, int y), int>();
      CurrentCL12Dict = new Dictionary<(int x, int y), int>();
      CurrentCL13Dict = new Dictionary<(int x, int y), int>();
      CurrentCL23Dict = new Dictionary<(int x, int y), int>();
      BackingCL01Dict = new Dictionary<(int x, int y), int>();
      BackingCL02Dict = new Dictionary<(int x, int y), int>();
      BackingCL03Dict = new Dictionary<(int x, int y), int>();
      BackingCL12Dict = new Dictionary<(int x, int y), int>();
      BackingCL13Dict = new Dictionary<(int x, int y), int>();
      BackingCL23Dict = new Dictionary<(int x, int y), int>();

      for (var i = 0; i < HistogramData.Bins.Length; i++)
      {
        CurrentForwardSsc.Add(new HistogramData(0, HistogramData.Bins[i]));
         CurrentVioletSsc.Add(new HistogramData(0, HistogramData.Bins[i]));
            CurrentRedSsc.Add(new HistogramData(0, HistogramData.Bins[i]));
          CurrentGreenSsc.Add(new HistogramData(0, HistogramData.Bins[i]));
          CurrentReporter.Add(new HistogramData(0, HistogramData.Bins[i]));
                                                   
        BackingForwardSsc.Add(new HistogramData(0, HistogramData.Bins[i]));
         BackingVioletSsc.Add(new HistogramData(0, HistogramData.Bins[i]));
            BackingRedSsc.Add(new HistogramData(0, HistogramData.Bins[i]));
          BackingGreenSsc.Add(new HistogramData(0, HistogramData.Bins[i]));
          BackingReporter.Add(new HistogramData(0, HistogramData.Bins[i]));
      }

      CurrentCL01Map = new List<HeatMapData>();
      CurrentCL02Map = new List<HeatMapData>();
      CurrentCL03Map = new List<HeatMapData>();
      CurrentCL12Map = new List<HeatMapData>();
      CurrentCL13Map = new List<HeatMapData>();
      CurrentCL23Map = new List<HeatMapData>();
      BackingCL01Map = new List<HeatMapData>();
      BackingCL02Map = new List<HeatMapData>();
      BackingCL03Map = new List<HeatMapData>();
      BackingCL12Map = new List<HeatMapData>();
      BackingCL13Map = new List<HeatMapData>();
      BackingCL23Map = new List<HeatMapData>();

      DisplayedForwardSsc = CurrentForwardSsc;
      DisplayedVioletSsc = CurrentVioletSsc;
      DisplayedRedSsc = CurrentRedSsc;
      DisplayedGreenSsc = CurrentGreenSsc;
      DisplayedReporter = CurrentReporter;

      PlatePictogram = DrawingPlate.Create();
      Buttons384Visible = System.Windows.Visibility.Hidden;
      CornerButtonsChecked = new ObservableCollection<bool> { true, false, false, false };
      CLButtonsChecked = new ObservableCollection<bool> { false, false, true, false, false, true, false, false };
      LeftLabel384Visible = System.Windows.Visibility.Visible;
      RightLabel384Visible = System.Windows.Visibility.Hidden;
      TopLabel384Visible = System.Windows.Visibility.Visible;
      BottomLabel384Visible = System.Windows.Visibility.Hidden;
      FillWorldMap(App.Device.RootDirectory.FullName + @"\Config\" + App.Device.ActiveMap.mapName + @"_world.json");
    }

    public static ResultsViewModel Create()
    {
      return ViewModelSource.Create(() => new ResultsViewModel());
    }

    public void CLButtonClick(int CL)
    {
      switch (CL)
      {
        case 0:
          break;
        case 1:
          break;
        case 2:
          break;
        case 3:
          break;
        case 4:
          break;
        case 5:
          break;
        case 6:
          break;
        case 7:
          break;
      }
      if (CL < 4)
      {
        CLButtonsChecked[0] = false;
        CLButtonsChecked[1] = false;
        CLButtonsChecked[2] = false;
        CLButtonsChecked[3] = false;
      }
      else
      {
        CLButtonsChecked[4] = false;
        CLButtonsChecked[5] = false;
        CLButtonsChecked[6] = false;
        CLButtonsChecked[7] = false;
      }
      CLButtonsChecked[CL] = true;
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
      CornerButtonsChecked[corner - 1 ] = true;
      PlatePictogram.ChangeCorner(corner);
    }

    public void ToCurrentButtonClick()
    {
      PlotCurrent();
      PlatePictogram.FollowingCurrentCell = true;

      int tempCorner = 1;
      if (PlatePictogram.CurrentlyReadCell.row < 8)
        tempCorner = PlatePictogram.CurrentlyReadCell.col < 12 ? 1 : 2;
      else
        tempCorner = PlatePictogram.CurrentlyReadCell.col < 12 ? 3 : 4;
      CornerButtonClick(tempCorner);
    }

    public void ClearGraphs(bool current = true)
    {
      if (current)
      {
        for (var i = 0; i < CurrentReporter.Count; i++)
        {
          CurrentForwardSsc[i].Value = 0;
          CurrentVioletSsc[i].Value = 0;
          CurrentRedSsc[i].Value = 0;
          CurrentGreenSsc[i].Value = 0;
          CurrentReporter[i].Value = 0;
        }
        CurrentCL01Map.Clear();
        CurrentCL02Map.Clear();
        CurrentCL03Map.Clear();
        CurrentCL12Map.Clear();
        CurrentCL13Map.Clear();
        CurrentCL23Map.Clear();
        CurrentCL01Dict.Clear();
        CurrentCL02Dict.Clear();
        CurrentCL03Dict.Clear();
        CurrentCL12Dict.Clear();
        CurrentCL13Dict.Clear();
        CurrentCL23Dict.Clear();
      }
      else
      {
        for (var i = 0; i < BackingReporter.Count; i++)
        {
          BackingForwardSsc[i].Value = 0;
          BackingVioletSsc[i].Value = 0;
          BackingRedSsc[i].Value = 0;
          BackingGreenSsc[i].Value = 0;
          BackingReporter[i].Value = 0;
        }
        BackingCL01Map.Clear();
        BackingCL02Map.Clear();
        BackingCL03Map.Clear();
        BackingCL12Map.Clear();
        BackingCL13Map.Clear();
        BackingCL23Map.Clear();
        BackingCL01Dict.Clear();
        BackingCL02Dict.Clear();
        BackingCL03Dict.Clear();
        BackingCL12Dict.Clear();
        BackingCL13Dict.Clear();
        BackingCL23Dict.Clear();
      }
      Views.ResultsView.Instance.ClearPoints();
    }

    public void SelectedCellChanged()
    {
      try
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
        ClearGraphs(false);
        FillAllData();
        PlotCurrent(false);
        PlatePictogram.FollowingCurrentCell = false;
      }
      catch { }
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
    private async Task ParseBeadInfoAsync(string path, List<MicroCy.BeadInfoStruct> beadstructs)
    {
      List<string> LinesInFile = await Core.DataProcessor.GetDataFromFileAsync(path);
      if (LinesInFile.Count == 1 && LinesInFile[0] == " ")
      {
        System.Windows.MessageBox.Show("File is empty");
        return;
      }
      for (var i = 0; i < LinesInFile.Count; i++)
      {
        beadstructs.Add(Core.DataProcessor.ParseRow(LinesInFile[i]));
      }
    }

    public void FillAllData()
    {
      _ = Task.Run(async ()=>
      {
        var path = @"D:\WorkC#\SampleData\Mon Run 2AA11_0.csv";// PlatePictogram.GetSelectedFilePath();
        if (path == null)
          return;
        var beadStructslist = new List<MicroCy.BeadInfoStruct>();
        await ParseBeadInfoAsync(path, beadStructslist);
        _ = Task.Run(() => Core.DataProcessor.BinScatterData(beadStructslist, fromFile: true));
        Core.DataProcessor.BinMapData(beadStructslist, current: false);
        _ = App.Current.Dispatcher.BeginInvoke((Action)(()=>
        {
          Core.DataProcessor.AnalyzeHeatMap(BackingCL12Map);
        }));
      });
    }

    public void PlotCurrent(bool current = true)
    {
      if (current)
      {
        DisplayedForwardSsc = CurrentForwardSsc;
        DisplayedVioletSsc = CurrentVioletSsc;
        DisplayedRedSsc = CurrentRedSsc;
        DisplayedGreenSsc = CurrentGreenSsc;
        DisplayedReporter = CurrentReporter;

        Views.ResultsView.Instance.ClearPoints();
        Core.DataProcessor.AnalyzeHeatMap(CurrentCL12Map);
      }
      else
      {
        DisplayedForwardSsc = BackingForwardSsc;
        DisplayedVioletSsc = BackingVioletSsc;
        DisplayedRedSsc = BackingRedSsc;
        DisplayedGreenSsc = BackingGreenSsc;
        DisplayedReporter = BackingReporter;
      }
    }

    public void FillWorldMap(string path)
    {
      List<(int x, int y)> XYList = null;
      try
      {
        using (System.IO.TextReader reader = new System.IO.StreamReader(path))
        {
          var fileContents = reader.ReadToEnd();
          XYList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<(int, int)>>(fileContents);
        }
      }
      catch { }
      if (XYList != null)
      {
        WorldMap.Clear();
        foreach (var point in XYList)
        {
          WorldMap.Add(new HeatMapData(point.x, point.y));
        }
      }
    }
  }
}