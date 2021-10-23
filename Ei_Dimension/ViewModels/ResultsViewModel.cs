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
    public virtual ObservableCollection<HeatMapData> WorldMap { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CurrentForwardSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CurrentVioletSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CurrentRedSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CurrentGreenSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CurrentReporter { get; set; }
    public virtual ObservableCollection<HeatMapData> CurrentMap { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> DisplayedForwardSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> DisplayedVioletSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> DisplayedRedSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> DisplayedGreenSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> DisplayedReporter { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> BackingForwardSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> BackingVioletSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> BackingRedSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> BackingGreenSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> BackingReporter { get; set; }
    public virtual ObservableCollection<HeatMapData> BackingMap { get; set; }
    public virtual DrawingPlate PlatePictogram { get; set; }
    public virtual System.Windows.Visibility Buttons384Visible { get; set; }
    public virtual System.Windows.Visibility LeftLabel384Visible { get; set; }
    public virtual System.Windows.Visibility RightLabel384Visible { get; set; }
    public virtual System.Windows.Visibility TopLabel384Visible { get; set; }
    public virtual System.Windows.Visibility BottomLabel384Visible { get; set; }
    public virtual ObservableCollection<bool> CornerButtonsChecked { get; set; }
    public static ResultsViewModel Instance { get; private set; }
    public static int[] HistogramBins { get; private set; }

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
      CurrentForwardSsc = new ObservableCollection<HistogramData<int, int>>();
      CurrentVioletSsc = new ObservableCollection<HistogramData<int, int>>();
      CurrentRedSsc = new ObservableCollection<HistogramData<int, int>>();
      CurrentGreenSsc = new ObservableCollection<HistogramData<int, int>>();
      CurrentReporter = new ObservableCollection<HistogramData<int, int>>();
      BackingForwardSsc = new ObservableCollection<HistogramData<int, int>>();
      BackingVioletSsc = new ObservableCollection<HistogramData<int, int>>();
      BackingRedSsc = new ObservableCollection<HistogramData<int, int>>();
      BackingGreenSsc = new ObservableCollection<HistogramData<int, int>>();
      BackingReporter = new ObservableCollection<HistogramData<int, int>>();

      HistogramBins = Core.DataProcessor.GenerateLogSpace(1, 1000000, 384);
      for (var i = 0; i < HistogramBins.Length; i++)
      {
        CurrentForwardSsc.Add(new HistogramData<int, int>(0, HistogramBins[i]));
         CurrentVioletSsc.Add(new HistogramData<int, int>(0, HistogramBins[i]));
            CurrentRedSsc.Add(new HistogramData<int, int>(0, HistogramBins[i]));
          CurrentGreenSsc.Add(new HistogramData<int, int>(0, HistogramBins[i]));
          CurrentReporter.Add(new HistogramData<int, int>(0, HistogramBins[i]));

        BackingForwardSsc.Add(new HistogramData<int, int>(0, HistogramBins[i]));
         BackingVioletSsc.Add(new HistogramData<int, int>(0, HistogramBins[i]));
            BackingRedSsc.Add(new HistogramData<int, int>(0, HistogramBins[i]));
          BackingGreenSsc.Add(new HistogramData<int, int>(0, HistogramBins[i]));
          BackingReporter.Add(new HistogramData<int, int>(0, HistogramBins[i]));
      }

      CurrentMap = new ObservableCollection<HeatMapData>();
      _ = new HeatMapData(1,1);
      BackingMap = new ObservableCollection<HeatMapData>();

      DisplayedForwardSsc = CurrentForwardSsc;
      DisplayedVioletSsc = CurrentVioletSsc;
      DisplayedRedSsc = CurrentRedSsc;
      DisplayedGreenSsc = CurrentGreenSsc;
      DisplayedReporter = CurrentReporter;

      PlatePictogram = DrawingPlate.Create();
      Buttons384Visible = System.Windows.Visibility.Hidden;
      CornerButtonsChecked = new ObservableCollection<bool> { true, false, false, false };
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

    public void CornerButtonClick(int corner)
    {
      switch (corner) {
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
      Views.ResultsView.Instance.DrawingPlate.UnselectAllCells();
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
        CurrentMap.Clear();
        HeatMapData.Dict.Clear();
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
        BackingMap.Clear();
      }
      Views.ResultsView.Instance.ClearPoints();
    }

    public void SelectedCellChanged()
    {
      try
      {
        var temp = PlatePictogram.GetSelectedCell();
        if (temp.row != -1)
          PlatePictogram.SelectedCell = temp;
        if (temp == PlatePictogram.CurrentlyReadCell)
        {
          ToCurrentButtonClick();
          return;
        }
        ClearGraphs(false);
        FillAllDataAsync();
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

    public void FillAllDataAsync()
    {
      _ = Task.Run(async ()=>
      {
        var path = @"D:\WorkC#\SampleData\Mon Run 2AA11_0.csv";// PlatePictogram.GetSelectedFilePath();
        if (path == null)
          return;
        var beadStructslist = new List<MicroCy.BeadInfoStruct>();
        await ParseBeadInfoAsync(path, beadStructslist);
        Dictionary<(int x, int y), int> Dict = new Dictionary<(int x, int y), int>();
        int index = 0;
        _ = Task.Run(() => Core.DataProcessor.BinData(beadStructslist, fromFile: true));
        foreach (var bead in beadStructslist)
        {
          int x = 0;
          int y = 0;
          for (var i = 0; i < 256; i++)
          {
            if (bead.cl1 <= HeatMapData.bins[i])
            {
              x = i;
              break;
            }
          }
          for (var i = 0; i < 256; i++)
          {
            if (bead.cl2 <= HeatMapData.bins[i])
            {
              y = i;
              break;
            }
          }
          if (!Dict.ContainsKey((x, y)))
          {
            Dict.Add((x, y), index);
            index++;
            BackingMap.Add(new HeatMapData((int)HeatMapData.bins[x], (int)HeatMapData.bins[y]));
          }
          else
          {
            BackingMap[Dict[(x, y)]].A++;
          }
        }
        _ = App.Current.Dispatcher.BeginInvoke((Action)(()=>
        {
          //analyzeheatmap for noncurrentmap, but backing map here
          int max = 0;
          int min = BackingMap[0].A;
          foreach (var p in BackingMap)
          {
            if (p.A > max)
              max = p.A;
            if (p.A < min)
              min = p.A;
          }
          double[] bins = Core.DataProcessor.GenerateLogSpaceD(1, max + 1, 5, true);
          var heat1 = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x0a, 0x6d, 0xaa));
          var heat2 = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x00, 0xcc, 0x49));
          var heat3 = System.Windows.Media.Brushes.Orange;
          var heat4 = System.Windows.Media.Brushes.OrangeRed;
          var heat5 = System.Windows.Media.Brushes.Red;
          for (var i = 0; i < BackingMap.Count; i++)
          {
            if (BackingMap[i].A <= bins[0])
              Views.ResultsView.Instance.AddXYPoint(BackingMap[i].X, BackingMap[i].Y, heat1);
            else if (BackingMap[i].A <= bins[1])
              Views.ResultsView.Instance.AddXYPoint(BackingMap[i].X, BackingMap[i].Y, heat2);
            else if (BackingMap[i].A <= bins[2])
              Views.ResultsView.Instance.AddXYPoint(BackingMap[i].X, BackingMap[i].Y, heat3);
            else if (BackingMap[i].A <= bins[3])
              Views.ResultsView.Instance.AddXYPoint(BackingMap[i].X, BackingMap[i].Y, heat4);
            else if (BackingMap[i].A <= bins[4])
              Views.ResultsView.Instance.AddXYPoint(BackingMap[i].X, BackingMap[i].Y, heat5);
          }
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
        foreach(var p in CurrentMap)
        {
          Views.ResultsView.Instance.AddXYPoint(p.X, p.Y, System.Windows.Media.Brushes.DarkOliveGreen);
        }
        App.AnalyzeHeatMap();
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