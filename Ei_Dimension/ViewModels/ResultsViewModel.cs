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
    public virtual ObservableCollection<HistogramData<int, int>> CurrentForwardSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CurrentVioletSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CurrentRedSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CurrentGreenSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CurrentReporter { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CurrentCL0 { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CurrentCL1 { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CurrentCL2 { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> CurrentCL3 { get; set; }
    public virtual ObservableCollection<HeatMapData> CurrentMap { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> DisplayedForwardSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> DisplayedVioletSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> DisplayedRedSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> DisplayedGreenSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> DisplayedReporter { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> DisplayedCL0 { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> DisplayedCL1 { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> DisplayedCL2 { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> DisplayedCL3 { get; set; }
    public virtual ObservableCollection<HeatMapData> DisplayedMap { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> BackingForwardSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> BackingVioletSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> BackingRedSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> BackingGreenSsc { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> BackingReporter { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> BackingCL0 { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> BackingCL1 { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> BackingCL2 { get; set; }
    public virtual ObservableCollection<HistogramData<int, int>> BackingCL3 { get; set; }
    public virtual ObservableCollection<HeatMapData> BackingMap { get; set; }
    public virtual DrawingPlate PlatePictogram { get; set; }
    public virtual System.Windows.Visibility Buttons384Visible { get; set; }
    public static ResultsViewModel Instance { get; private set; }

    protected ResultsViewModel()
    {
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

      Instance = this;
      CurrentForwardSsc = new ObservableCollection<HistogramData<int, int>>();
      CurrentVioletSsc = new ObservableCollection<HistogramData<int, int>>();
      CurrentRedSsc = new ObservableCollection<HistogramData<int, int>>();
      CurrentGreenSsc = new ObservableCollection<HistogramData<int, int>>();
      CurrentReporter = new ObservableCollection<HistogramData<int, int>>();
      CurrentCL0 = new ObservableCollection<HistogramData<int, int>>();
      CurrentCL1 = new ObservableCollection<HistogramData<int, int>>();
      CurrentCL2 = new ObservableCollection<HistogramData<int, int>>();
      CurrentCL3 = new ObservableCollection<HistogramData<int, int>>();
      BackingForwardSsc = new ObservableCollection<HistogramData<int, int>>();
      BackingVioletSsc = new ObservableCollection<HistogramData<int, int>>();
      BackingRedSsc = new ObservableCollection<HistogramData<int, int>>();
      BackingGreenSsc = new ObservableCollection<HistogramData<int, int>>();
      BackingReporter = new ObservableCollection<HistogramData<int, int>>();
      BackingCL0 = new ObservableCollection<HistogramData<int, int>>();
      BackingCL1 = new ObservableCollection<HistogramData<int, int>>();
      BackingCL2 = new ObservableCollection<HistogramData<int, int>>();
      BackingCL3 = new ObservableCollection<HistogramData<int, int>>();

      var bins = Core.DataProcessor.GenerateLogSpace(1, 1000000, 384);
      for (var i = 0; i < bins.Length; i++)
      {
        CurrentForwardSsc.Add(new HistogramData<int, int>(0, bins[i]));
         CurrentVioletSsc.Add(new HistogramData<int, int>(0, bins[i]));
            CurrentRedSsc.Add(new HistogramData<int, int>(0, bins[i]));
          CurrentGreenSsc.Add(new HistogramData<int, int>(0, bins[i]));
          CurrentReporter.Add(new HistogramData<int, int>(0, bins[i]));
               CurrentCL0.Add(new HistogramData<int, int>(0, bins[i]));
               CurrentCL1.Add(new HistogramData<int, int>(0, bins[i]));
               CurrentCL2.Add(new HistogramData<int, int>(0, bins[i]));
               CurrentCL3.Add(new HistogramData<int, int>(0, bins[i]));

        BackingForwardSsc.Add(new HistogramData<int, int>(0, bins[i]));
         BackingVioletSsc.Add(new HistogramData<int, int>(0, bins[i]));
            BackingRedSsc.Add(new HistogramData<int, int>(0, bins[i]));
          BackingGreenSsc.Add(new HistogramData<int, int>(0, bins[i]));
          BackingReporter.Add(new HistogramData<int, int>(0, bins[i]));
               BackingCL0.Add(new HistogramData<int, int>(0, bins[i]));
               BackingCL1.Add(new HistogramData<int, int>(0, bins[i]));
               BackingCL2.Add(new HistogramData<int, int>(0, bins[i]));
               BackingCL3.Add(new HistogramData<int, int>(0, bins[i]));
      }

      CurrentMap = new ObservableCollection<HeatMapData>();
      BackingMap = new ObservableCollection<HeatMapData>();
      PlotCurrent();

      PlatePictogram = DrawingPlate.Create();
      Buttons384Visible = System.Windows.Visibility.Hidden;
    }

    public static ResultsViewModel Create()
    {
      return ViewModelSource.Create(() => new ResultsViewModel());
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
      }
    }

    public void SelectedCellChanged()
    {
      try
      {
        ClearGraphs(false);
        FillAllDataAsync();
        PlotCurrent(false);
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
    public async Task ParseBeadInfoAsync(string path, List<MicroCy.BeadInfoStruct> beadstructs)
    {
      List<string> LinesInFile = await Core.DataProcessor.GetDataFromFileAsync(path);
      for (var i = 0; i < LinesInFile.Count; i++)
      {
        beadstructs.Add(Core.DataProcessor.ParseRow(LinesInFile[i]));
      }
    }

    public async void FillAllDataAsync()
    {
      //PlatePictogram.SelectedCell();    --start searching for file here
      var beadStructslist = new List<MicroCy.BeadInfoStruct>();
      await ParseBeadInfoAsync(@"C:\Users\Admin\Desktop\BeadAssayA1_2.csv", beadStructslist);
      foreach (var bead in beadStructslist)
      {
        Core.DataProcessor.BinData(bead, fromFile: true);
      }
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
        DisplayedCL0 = CurrentCL0;
        DisplayedCL1 = CurrentCL1;
        DisplayedCL2 = CurrentCL2;
        DisplayedCL3 = CurrentCL3;
        DisplayedMap = CurrentMap;
      }
      else
      {
        DisplayedForwardSsc = BackingForwardSsc;
        DisplayedVioletSsc = BackingVioletSsc;
        DisplayedRedSsc = BackingRedSsc;
        DisplayedGreenSsc = BackingGreenSsc;
        DisplayedReporter = BackingReporter;
        DisplayedCL0 = BackingCL0;
        DisplayedCL1 = BackingCL1;
        DisplayedCL2 = BackingCL2;
        DisplayedCL3 = BackingCL3;
        DisplayedMap = BackingMap;
      }
    }
  }
}