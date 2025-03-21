﻿using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using System;
using System.Threading.Tasks;
using DIOS.Application;
using DIOS.Application.FileIO;
using DIOS.Application.FileIO.Calibration;
using DIOS.Core;
using DIOS.Core.HardwareIntercom;
using Ei_Dimension.Graphing.HeatMap;
using System.Collections.Frozen;
using System.Collections.Generic;
using DIOS.Application.Domain;

namespace Ei_Dimension.ViewModels;

[POCOViewModel]
public class CalibrationViewModel
{
  public virtual string SelectedGatingContent { get; set; }
  public byte SelectedGatingIndex { get; set; } = 0;
  public virtual ObservableCollection<DropDownButtonContents> GatingItems { get; }
  public virtual ObservableCollection<string> EventTriggerContents { get; set; }
  public virtual ObservableCollection<string> ClassificationTargetsContents { get; set; } = new(){ "0", "0", "0", "0", "0" };  //init on map changed
  public virtual ObservableCollection<string> CompensationPercentageContent { get; set; }
  public virtual ObservableCollection<string> DNRContents { get; set; } = new(){ "0", "0" };
  public virtual ObservableCollection<string> AttenuationBox { get; set; }
  public byte CalFailsInARow { get; set; } = 0;
  public bool CalJustFailed { get; set; } = true;
  public bool DoPostCalibrationRun { get; set; } = false;
  public virtual string SelectedCl1ClassificatorContent { get; set; }
  public virtual string SelectedCl2ClassificatorContent { get; set; }
  public byte SelectedCl1ClassificatorIndex { get; set; } = 2;
  public byte SelectedCl2ClassificatorIndex { get; set; } = 3;
  public ObservableCollection<DropDownButtonContents> ClClassificatorItems { get; }
  public ObservableCollection<DropDownButtonContents> ReporterItems { get; }
  public virtual string SelectedSensitivityContent { get; set; }
  public byte SelectedSensitivityIndex { get; set; } = 6;
  public virtual string SelectedExChannelContent { get; set; }
  public byte SelectedExChannelIndex { get; set; } = 5;
  public ObservableCollection<string> SelectedReporterContent { get; set; } = new(){string.Empty, string.Empty, string.Empty, string.Empty, };
  public byte[] SelectedReporterIndex { get; set; } = [8, 8, 8, 8];
  public virtual string SelectedCl3rContent { get; set; }
  public byte SelectedCl3rIndex { get; set; } = 8;
  public virtual ObservableCollection<string> Cl3rLevels { get; set; } = new(){"0", "0"};
  public virtual ObservableCollection<bool> SpectraplexReporterEnabledButtonsChecked { get; set; } =
    new() {true, false};

  public static CalibrationViewModel Instance { get; private set; }
  private static readonly FrozenDictionary<byte, string> _paramsDict = new Dictionary<byte, string>()
  {
    [0] = "RedA",
    [1] = "RedB",
    [2] = "RedC",
    [3] = "RedD",
    [4] = "GreenA",
    [5] = "GreenB",
    [6] = "GreenC",
    [7] = "GreenD",
    [8] = "None"
  }.ToFrozenDictionary();
  private static readonly FrozenDictionary<int, Channel?> _channelsDict = new Dictionary<int, Channel?>()
  {
    [0] = Channel.RedA,
    [1] = Channel.RedB,
    [2] = Channel.RedC,
    [3] = Channel.RedD,
    [4] = Channel.GreenA,
    [5] = Channel.GreenB,
    [6] = Channel.GreenC,
    [7] = Channel.GreenD,
    [8] = null
  }.ToFrozenDictionary();
  
  protected CalibrationViewModel()
  {
    var stringSource = Language.TranslationSource.Instance;
    GatingItems = new ObservableCollection<DropDownButtonContents>
    {
      new(stringSource[nameof(Language.Resources.Dropdown_None)], this),
      new(stringSource[nameof(Language.Resources.Dropdown_Green_SSC)], this),
      new(stringSource[nameof(Language.Resources.Dropdown_Red_SSC)], this),
      new(stringSource[nameof(Language.Resources.Dropdown_Green_Red_SSC)], this),
      new(stringSource[nameof(Language.Resources.Dropdown_Rp_bg)], this),
      new(stringSource[nameof(Language.Resources.Dropdown_Green_Rp_bg)], this),
      new(stringSource[nameof(Language.Resources.Dropdown_Red_Rp_bg)], this),
      new(stringSource[nameof(Language.Resources.Dropdown_Green_Red_Rp_bg)], this)
    };
    SelectedGatingContent = GatingItems[SelectedGatingIndex].Content;
    DropDownButtonContents.ResetIndex();
    EventTriggerContents = new()
    {
      "",
      App.DiosApp.MapController.ActiveMap.calParams.minmapssc.ToString(),
      App.DiosApp.MapController.ActiveMap.calParams.maxmapssc.ToString()
    };

    CompensationPercentageContent = new(){ App.DiosApp.Device.ToString() };

    AttenuationBox = new(){ App.DiosApp.MapController.ActiveMap.calParams.att.ToString() };

    ClClassificatorItems = new ObservableCollection<DropDownButtonContents>
    {
      new(stringSource[nameof(Language.Resources.ChannelOffsets_Red_A)], this),
      new(stringSource[nameof(Language.Resources.ChannelOffsets_Red_B)], this),
      new(stringSource[nameof(Language.Resources.ChannelOffsets_Red_C)], this),
      new(stringSource[nameof(Language.Resources.ChannelOffsets_Red_D)], this),
      new(stringSource[nameof(Language.Resources.ChannelOffsets_Green_A)], this),
      new(stringSource[nameof(Language.Resources.ChannelOffsets_Green_B)], this),
      new(stringSource[nameof(Language.Resources.ChannelOffsets_Green_C)], this),
      new(stringSource[nameof(Language.Resources.Channels_Green_D)], this)
    };
    SelectedCl1ClassificatorContent = ClClassificatorItems[SelectedCl1ClassificatorIndex].Content;
    DropDownButtonContents.ResetIndex();
    SelectedCl2ClassificatorContent = ClClassificatorItems[SelectedCl2ClassificatorIndex].Content;
    DropDownButtonContents.ResetIndex();
    SelectedSensitivityContent = ClClassificatorItems[SelectedSensitivityIndex].Content;
    DropDownButtonContents.ResetIndex();
    SelectedExChannelContent = ClClassificatorItems[SelectedExChannelIndex].Content;
    DropDownButtonContents.ResetIndex();

    ReporterItems = new ObservableCollection<DropDownButtonContents>
    {
      new(stringSource[nameof(Language.Resources.ChannelOffsets_Red_A)], this),
      new(stringSource[nameof(Language.Resources.ChannelOffsets_Red_B)], this),
      new(stringSource[nameof(Language.Resources.ChannelOffsets_Red_C)], this),
      new(stringSource[nameof(Language.Resources.ChannelOffsets_Red_D)], this),
      new(stringSource[nameof(Language.Resources.ChannelOffsets_Green_A)], this),
      new(stringSource[nameof(Language.Resources.ChannelOffsets_Green_B)], this),
      new(stringSource[nameof(Language.Resources.ChannelOffsets_Green_C)], this),
      new(stringSource[nameof(Language.Resources.Channels_Green_D)], this),
      new(stringSource[nameof(Language.Resources.Channels_None)], this)
    };
    SelectedReporterContent[0] = ReporterItems[SelectedReporterIndex[0]].Content;
    DropDownButtonContents.ResetIndex();
    SelectedReporterContent[1] = ReporterItems[SelectedReporterIndex[1]].Content;
    DropDownButtonContents.ResetIndex();
    SelectedReporterContent[2] = ReporterItems[SelectedReporterIndex[2]].Content;
    DropDownButtonContents.ResetIndex();
    SelectedReporterContent[3] = ReporterItems[SelectedReporterIndex[3]].Content;
    DropDownButtonContents.ResetIndex();
    SelectedCl3rContent = ReporterItems[SelectedCl3rIndex].Content;
    DropDownButtonContents.ResetIndex();

    Instance = this;
  }

  public static CalibrationViewModel Create()
  {
    return ViewModelSource.Create(() => new CalibrationViewModel());
  }

  public void CalibrationSuccess()
  {
    // make sure to update DeviceParameterType.ChannelBias30C in advance
    //currently done in CalibrationSuccessPostRun
    Action Cancel = DashboardViewModel.Instance.CalModeToggle;
    Action Save = () =>
    {
      var res = App.DiosApp.MapController.SaveCalValsToCurrentMap(new MapCalParameters
      {
        TempGreenD = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[7]),
        TempCl1 = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[5]),
        TempCl2 = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[6]),
        TempCl3 = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[3]),
        TempRedSsc = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[4]),
        TempGreenSsc = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[0]),
        TempRpMaj = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[1]),
        TempRpMin = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[2]),
        Compensation = float.Parse(CompensationPercentageContent[0]),
        Gating = SelectedGatingIndex,
        Height = short.Parse(EventTriggerContents[0]),
        MinSSC = ushort.Parse(EventTriggerContents[1]),
        MaxSSC = ushort.Parse(EventTriggerContents[2]),
        DNRCoef = float.Parse(DNRContents[0]),
        DNRTrans = float.Parse(DNRContents[1]),
        Attenuation = int.Parse(AttenuationBox[0]),
        CL0 = int.Parse(ClassificationTargetsContents[0]),
        CL1 = int.Parse(ClassificationTargetsContents[1]),
        CL2 = int.Parse(ClassificationTargetsContents[2]),
        CL3 = int.Parse(ClassificationTargetsContents[3]),
        RP1 = int.Parse(ClassificationTargetsContents[4]),
        Caldate = DateTime.Now.ToString("dd.MM.yyyy", new System.Globalization.CultureInfo("en-GB")),
        Valdate = null
      });
      if (!res)
      {
        App.Current.Dispatcher.Invoke(() =>
          Notification.Show("Save failed"));
        return;
      }
      DashboardViewModel.Instance.SetCalibrationDate(App.DiosApp.MapController.ActiveMap.caltime);
      Cancel.Invoke();
    };
    Notification.ShowLocalized(nameof(Language.Resources.Calibration_Success), Save, nameof(Language.Resources.Calibration_Save_Calibration_To_Map),
      Cancel, nameof(Language.Resources.Calibration_Cancel_Calibration), Brushes.Green);
  }

  public void CalibrationFailCheck()
  {
    if (++CalFailsInARow >= 3 && CalJustFailed)
    {
      App.Current.Dispatcher.Invoke(() =>
        Notification.ShowLocalizedError(nameof(Language.Resources.Calibration_Fail)));
      App.Current.Dispatcher.Invoke(DashboardViewModel.Instance.CalModeToggle);
      Task.Run(() =>
      {
        var report = FormNewCalibrationReport(false, null);
        Task.Run(() =>
        {
          App.DiosApp.Publisher.CalibrationFile.CreateAndWrite(report);
        });
      });
    }
    else if (CalJustFailed)
      App.Current.Dispatcher.Invoke(() => Notification.ShowLocalizedSuccess(nameof(Language.Resources.Calibration_in_Progress)));
  }

  public void CalibrationSuccessPostRun(object sender, EventArgs e)
  {
    System.Threading.Thread.Sleep(2000);//wait for the end of read procedure
    //make sure to update DeviceParameterType.ChannelBias30C for the calibration report and calibration success
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.GreenA);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.GreenB);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.GreenC);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.RedA);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.RedB);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.RedC);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.RedD);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.GreenD);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.DNRCoefficient);

    DoPostCalibrationRun = true;

    var calibrationWell = WellsSelectViewModel.Instance.MakeCalibrationWell(1024);
    App.Current.Dispatcher.BeginInvoke(async () =>
    {
      await MainButtonsViewModel.Instance.StartButtonClick(calibrationWell);//Run the 1024-bead well
    });
  }

  public void MakeCalMap()
  {
    ResultsViewModel.Instance.WrldMap.CalibrationMap = new();
    int cl1Index = Array.BinarySearch(HeatMapPoint.bins, int.Parse(ClassificationTargetsContents[1]));
    if (cl1Index < 0)
      cl1Index = ~cl1Index;
    int cl2Index = Array.BinarySearch(HeatMapPoint.bins, int.Parse(ClassificationTargetsContents[2]));
    if (cl2Index < 0)
      cl2Index = ~cl2Index;
    for (var i = -5; i < 6; i++)
    {
      var xCoordinateIsInBoundary = cl1Index + i >= 0 && cl1Index + i < 256;
      for (var j = -6; j < 7; j++)
      {
        var yCoordinateIsInBoundary = cl2Index + j >= 0 && cl2Index + j < 256;
        if (Math.Pow(i, 2) + Math.Pow(j, 2) <= 16
            && xCoordinateIsInBoundary && yCoordinateIsInBoundary)
        {
          ResultsViewModel.Instance.WrldMap.CalibrationMap.Add(
            new HeatMapPoint(MapRegion.FromCLSpaceToReal(cl1Index + i, HeatMapPoint.bins),
              MapRegion.FromCLSpaceToReal(cl2Index + j, HeatMapPoint.bins)));
        }
      }
    }
  }

  public CalibrationReport FormNewCalibrationReport(bool isCalibrationSuccesful, ChannelsCalibrationStats multiChannelStats)
  {
    var firmwareVersion = App.DiosApp.Device.FirmwareVersion;
    var appVersion = App.DiosApp.BUILD;
    var dnrCoefficient = float.Parse(DNRContents[0]);
    var dnrTransition = float.Parse(DNRContents[1]);
    var channelConfig = ComponentsViewModel.Instance.SelectedChConfigIndex.ToString();
    var status = isCalibrationSuccesful;
    var report = new CalibrationReport(firmwareVersion, appVersion, dnrCoefficient, dnrTransition, channelConfig, status);

    if (isCalibrationSuccesful is false || multiChannelStats is null)
      return report;

    var temperature = float.Parse(ChannelsViewModel.Instance.TempParameters[0]);
    var bias30 = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[0]);
    var channelStats = multiChannelStats.Greenssc;
    var target = 8500;
    var data = new CalibrationReportData("GreenA", temperature, bias30, target, channelStats);
    report.channelsData.Add(data);

    if (App.DiosApp.Publisher.IsOEMModeActive)
    {
      //OEM case
      temperature = float.Parse(ChannelsViewModel.Instance.TempParameters[1]);
      bias30 = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[1]);
      channelStats = multiChannelStats.Cl1;
      target = int.Parse(ClassificationTargetsContents[1]);//CL1
      data = new CalibrationReportData("GreenB", temperature, bias30, target, channelStats);
      report.channelsData.Add(data);
    }

    temperature = float.Parse(ChannelsViewModel.Instance.TempParameters[2]);
    bias30 = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[2]);
    if (App.DiosApp.Publisher.IsOEMModeActive)
    {
      channelStats = multiChannelStats.Cl2;
      target = int.Parse(ClassificationTargetsContents[2]);//CL2
    }
    else
    {
      channelStats = multiChannelStats.GreenC;
      target = int.Parse(ClassificationTargetsContents[4]);//RP1
    }
    data = new CalibrationReportData("GreenC", temperature, bias30, target, channelStats);
    report.channelsData.Add(data);

    temperature = float.Parse(ChannelsViewModel.Instance.TempParameters[4]);
    bias30 = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[4]);
    channelStats = multiChannelStats.Redssc;
    target = 8500;
    data = new CalibrationReportData("RedB", temperature, bias30, target, channelStats);
    report.channelsData.Add(data);

    if (!App.DiosApp.Publisher.IsOEMModeActive)
    {
      //Non-OEM case
      temperature = float.Parse(ChannelsViewModel.Instance.TempParameters[5]);
      bias30 = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[5]);
      channelStats = multiChannelStats.Cl1;
      target = int.Parse(ClassificationTargetsContents[1]);//CL1
      data = new CalibrationReportData("RedC", temperature, bias30, target, channelStats);
      report.channelsData.Add(data);
    }

    temperature = float.Parse(ChannelsViewModel.Instance.TempParameters[6]);
    bias30 = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[6]);
    if (App.DiosApp.Publisher.IsOEMModeActive)
    {
      channelStats = multiChannelStats.GreenC;
      target = int.Parse(ClassificationTargetsContents[4]);//RP1
    }
    else
    {
      channelStats = multiChannelStats.Cl2;
      target = int.Parse(ClassificationTargetsContents[2]);//CL2
    }
    data = new CalibrationReportData("RedD", temperature, bias30, target, channelStats);
    report.channelsData.Add(data);

    return report;
  }

  public void SaveCalButtonClick()
  {
    UserInputHandler.InputSanityCheck();
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.GreenA);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.GreenB);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.GreenC);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.RedA);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.RedB);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.RedC);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.RedD);
    App.DiosApp.Device.Hardware.RequestParameter(DeviceParameterType.ChannelBias30C, Channel.GreenD);
    System.Threading.Thread.Sleep(1000);
    var res = App.DiosApp.MapController.SaveCalValsToCurrentMap(new MapCalParameters
    {
      TempGreenD = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[7]),
      TempCl1 = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[5]),
      TempCl2 = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[6]),
      TempCl3 = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[3]),
      TempRedSsc = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[4]),
      TempGreenSsc = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[0]),
      TempRpMaj = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[1]),
      TempRpMin = int.Parse(ChannelsViewModel.Instance.Bias30Parameters[2]),
      Compensation = float.Parse(CompensationPercentageContent[0]),
      Gating = SelectedGatingIndex,
      Height = short.Parse(EventTriggerContents[0]),
      MinSSC = ushort.Parse(EventTriggerContents[1]),
      MaxSSC = ushort.Parse(EventTriggerContents[2]),
      DNRCoef = float.Parse(DNRContents[0]),
      DNRTrans = float.Parse(DNRContents[1]),
      Attenuation = int.Parse(AttenuationBox[0]),
      CL0 = int.Parse(ClassificationTargetsContents[0]),
      CL1 = int.Parse(ClassificationTargetsContents[1]),
      CL2 = int.Parse(ClassificationTargetsContents[2]),
      CL3 = int.Parse(ClassificationTargetsContents[3]),
      RP1 = int.Parse(ClassificationTargetsContents[4]),
      Caldate = null,
      Valdate = null
    });
    if (!res)
    {
      App.Current.Dispatcher.Invoke(() =>
        Notification.Show("Save failed"));
      return;
    }

    res = App.DiosApp.MapController.SaveClassificationParametersToCurrentMap(
      _paramsDict[SelectedCl1ClassificatorIndex], _paramsDict[SelectedCl2ClassificatorIndex]);
    if (!res)
    {
      App.Current.Dispatcher.Invoke(() =>
        Notification.Show("Save failed"));
      return;
    }

    res = App.DiosApp.MapController.SaveSensitivityChannelsToCurrentMap(
      _paramsDict[SelectedSensitivityIndex], _paramsDict[SelectedExChannelIndex]);
    if (!res)
    {
      App.Current.Dispatcher.Invoke(() =>
        Notification.Show("Save failed"));
      return;
    }

    res = App.DiosApp.MapController.SaveReporterChannelsToCurrentMap(
      _paramsDict[SelectedReporterIndex[0]], _paramsDict[SelectedReporterIndex[1]],
      _paramsDict[SelectedReporterIndex[2]], _paramsDict[SelectedReporterIndex[3]],
      SpectraplexReporterEnabledButtonsChecked[1]);
    if (!res)
    {
      App.Current.Dispatcher.Invoke(() =>
        Notification.Show("Save failed"));
      return;
    }

    res = App.DiosApp.MapController.SaveCl3rChannelsToCurrentMap(
      _paramsDict[SelectedCl3rIndex], int.Parse(Cl3rLevels[0]), int.Parse(Cl3rLevels[1]));
    if (!res)
    {
      App.Current.Dispatcher.Invoke(() =>
        Notification.Show("Save failed"));
      return;
    }
    var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_CalParameters_Saved),
      Language.TranslationSource.Instance.CurrentCulture);
    Notification.Show($"{msg} {App.DiosApp.MapController.ActiveMap.mapName}");
  }

  public void FocusedBox(int num)
  {
    switch (num)
    {
      case 0:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(CompensationPercentageContent)), this, 0, Views.CalibrationView.Instance.TB0);
        MainViewModel.Instance.NumpadToggleButton(Views.CalibrationView.Instance.TB0);
        break;
      case 1:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(EventTriggerContents)), this, 0, Views.CalibrationView.Instance.TB1);
        MainViewModel.Instance.NumpadToggleButton(Views.CalibrationView.Instance.TB1);
        break;
      case 2:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(EventTriggerContents)), this, 1, Views.CalibrationView.Instance.TB2);
        MainViewModel.Instance.NumpadToggleButton(Views.CalibrationView.Instance.TB2);
        break;
      case 3:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(EventTriggerContents)), this, 2, Views.CalibrationView.Instance.TB3);
        MainViewModel.Instance.NumpadToggleButton(Views.CalibrationView.Instance.TB3);
        break;
      case 4:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(DNRContents)), this, 0, Views.CalibrationView.Instance.TB4);
        MainViewModel.Instance.NumpadToggleButton(Views.CalibrationView.Instance.TB4);
        break;
      case 5:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(DNRContents)), this, 1, Views.CalibrationView.Instance.TB5);
        MainViewModel.Instance.NumpadToggleButton(Views.CalibrationView.Instance.TB5);
        break;
      case 6:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ClassificationTargetsContents)), this, 0, (TextBox)Views.CalibrationView.Instance.targetsSP.Children[0]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.CalibrationView.Instance.targetsSP.Children[0]);
        break;
      case 7:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ClassificationTargetsContents)), this, 1, (TextBox)Views.CalibrationView.Instance.targetsSP.Children[1]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.CalibrationView.Instance.targetsSP.Children[1]);
        break;
      case 8:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ClassificationTargetsContents)), this, 2, (TextBox)Views.CalibrationView.Instance.targetsSP.Children[2]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.CalibrationView.Instance.targetsSP.Children[2]);
        break;
      case 9:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ClassificationTargetsContents)), this, 3, (TextBox)Views.CalibrationView.Instance.targetsSP.Children[3]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.CalibrationView.Instance.targetsSP.Children[3]);
        break;
      case 10:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(ClassificationTargetsContents)), this, 4, (TextBox)Views.CalibrationView.Instance.targetsSP.Children[4]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.CalibrationView.Instance.targetsSP.Children[4]);
        break;
      case 11:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(AttenuationBox)), this, 0, Views.CalibrationView.Instance.TB10);
        MainViewModel.Instance.NumpadToggleButton(Views.CalibrationView.Instance.TB10);
        break;
      case 12:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Cl3rLevels)), this, 0, (TextBox)Views.CalibrationView.Instance.cl3rLevels.Children[0]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.CalibrationView.Instance.cl3rLevels.Children[0]);
        break;
      case 13:
        UserInputHandler.SelectedTextBox = (this.GetType().GetProperty(nameof(Cl3rLevels)), this, 1, (TextBox)Views.CalibrationView.Instance.cl3rLevels.Children[1]);
        MainViewModel.Instance.NumpadToggleButton((TextBox)Views.CalibrationView.Instance.cl3rLevels.Children[1]);
        break;
    }
  }

  public void TextChanged(TextChangedEventArgs e)
  {
    UserInputHandler.InjectToFocusedTextbox(((TextBox)e.Source).Text, true);
  }

  public void DropPress()
  {
    UserInputHandler.InputSanityCheck();
  }

  public void OnMapChanged(MapModel map)
  {
    EventTriggerContents[1] = map.calParams.minmapssc.ToString();
    EventTriggerContents[2] = map.calParams.maxmapssc.ToString();
    AttenuationBox[0] = map.calParams.att.ToString();
    EventTriggerContents[0] = map.calParams.height.ToString();
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.MinSSC, map.calParams.minmapssc);
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.MaxSSC, map.calParams.maxmapssc);
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.Attenuation, map.calParams.att);
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.Height, map.calParams.height);
    GatingItems[map.calParams.gate].Click(0);
    ClClassificatorItems[FindDictKey(map.ClassificationParameter1)].Click(1);
    ClClassificatorItems[FindDictKey(map.ClassificationParameter2)].Click(2);
    ClClassificatorItems[FindDictKey(map.calParams.HiSensChannel)].Click(3);
    ClClassificatorItems[FindDictKey(map.calParams.ExtendedDNRChannel)].Click(4);
    SelectReporterCalculationMode(map.calParams.SpectraplexReporterMode ? 1 : 0);
    ReporterItems[FindDictKey(map.calParams.SPReporterChannel1)].Click(5);
    ReporterItems[FindDictKey(map.calParams.SPReporterChannel2)].Click(6);
    ReporterItems[FindDictKey(map.calParams.SPReporterChannel3)].Click(7);
    ReporterItems[FindDictKey(map.calParams.SPReporterChannel4)].Click(8);
    ReporterItems[FindDictKey(map.calParams.Cl3rChannel)].Click(9);
    Cl3rLevels[0] = map.calParams.Cl3rL1.ToString();
    Cl3rLevels[1] = map.calParams.Cl3rL2.ToString();
    DNRContents[0] = map.calParams.DNRCoef.ToString();
    DNRContents[1] = map.calParams.DNRTrans.ToString();
    App.DiosApp.Compensation = map.calParams.compensation;
    CompensationPercentageContent[0] = map.calParams.compensation.ToString();
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.DNRCoefficient, map.calParams.DNRCoef);
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.DNRTransition, map.calParams.DNRTrans);
    ClassificationTargetsContents[0] = map.calParams.CL0.ToString();
    ClassificationTargetsContents[1] = map.calParams.CL1.ToString();
    ClassificationTargetsContents[2] = map.calParams.CL2.ToString();
    ClassificationTargetsContents[3] = map.calParams.CL3.ToString();
    ClassificationTargetsContents[4] = map.calParams.RP1.ToString();
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationTarget, CalibrationTarget.CL0, map.calParams.CL0);
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationTarget, CalibrationTarget.CL1, map.calParams.CL1);
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationTarget, CalibrationTarget.CL2, map.calParams.CL2);
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationTarget, CalibrationTarget.CL3, map.calParams.CL3);
    App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationTarget, CalibrationTarget.RP1, map.calParams.RP1);
  }

  public void SelectReporterCalculationMode(int mode)
  {
    App.DiosApp._beadProcessor.SpectraPlexEnabled = mode is 1;
    SpectraplexReporterEnabledButtonsChecked[0] = mode is 0;
    SpectraplexReporterEnabledButtonsChecked[1] = mode is 1;
  }

  private int FindDictKey(string param)
  {
    var index = _paramsDict.Values.IndexOf(param);
    if (index < 0)
    {
      throw new ArgumentOutOfRangeException($"\"{param}\" is not found in the channels list");
    }
    return _paramsDict.Keys[index];
  }

  public class DropDownButtonContents : Core.ObservableObject
  {
    public string Content
    {
      get => _content;
      set
      {
        _content = value;
        OnPropertyChanged();
      }
    }
    public byte Index { get; set; }
    private static byte _nextIndex = 0;
    private string _content;
    private static CalibrationViewModel _vm;
    public DropDownButtonContents(string content, CalibrationViewModel vm = null)
    {
      if (_vm == null)
      {
        _vm = vm;
      }
      Content = content;
      Index = _nextIndex++;
    }

    public void Click(int num)
    {
      switch (num)
      {
        case 0:
          _vm.SelectedGatingContent = Content;
          _vm.SelectedGatingIndex = Index;
          App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.CalibrationParameter, CalibrationParameter.ScatterGate, (ushort)(Gate)Index);
          break;
        case 1:
          _vm.SelectedCl1ClassificatorContent = Content;
          _vm.SelectedCl1ClassificatorIndex = Index;
          BeadParamsHelper.ChooseProperClassification(_paramsDict[Index],
            _paramsDict[_vm.SelectedCl2ClassificatorIndex]);
          App.DiosApp.Device.Hardware.SetClassificationChannels(_channelsDict[Index].Value,
            _channelsDict[_vm.SelectedCl2ClassificatorIndex].Value);
          break;
        case 2:
          _vm.SelectedCl2ClassificatorContent = Content;
          _vm.SelectedCl2ClassificatorIndex = Index;
          BeadParamsHelper.ChooseProperClassification(_paramsDict[_vm.SelectedCl1ClassificatorIndex],
            _paramsDict[Index]);
          App.DiosApp.Device.Hardware.SetClassificationChannels(_channelsDict[_vm.SelectedCl1ClassificatorIndex].Value,
            _channelsDict[Index].Value);
          break;
        case 3:
          _vm.SelectedSensitivityContent = Content;
          _vm.SelectedSensitivityIndex = Index;
          //App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.HiSensitivityChannel, (HiSensitivityChannel)Index);
          BeadParamsHelper.ChooseProperSensitivityChannels(_paramsDict[Index],
            _paramsDict[_vm.SelectedExChannelIndex]);
          break;
        case 4:
          _vm.SelectedExChannelContent = Content;
          _vm.SelectedExChannelIndex = Index;
          //App.DiosApp.Device.Hardware.SetParameter(DeviceParameterType.HiSensitivityChannel, (HiSensitivityChannel)Index);
          BeadParamsHelper.ChooseProperSensitivityChannels(_paramsDict[_vm.SelectedSensitivityIndex],
            _paramsDict[Index]);
          break;
        case 5:
          _vm.SelectedReporterContent[0] = Content;
          _vm.SelectedReporterIndex[0] = Index;
          BeadParamsHelper.ChooseProperReporterChannels(_paramsDict[Index], _paramsDict[_vm.SelectedReporterIndex[1]],
            _paramsDict[_vm.SelectedReporterIndex[2]], _paramsDict[_vm.SelectedReporterIndex[3]]);
          App.DiosApp.Device.Hardware.SetSpectraplexReporterChannels(_channelsDict[Index], _channelsDict[_vm.SelectedReporterIndex[1]],
            _channelsDict[_vm.SelectedReporterIndex[2]], _channelsDict[_vm.SelectedReporterIndex[3]],
            _channelsDict[_vm.SelectedCl3rIndex]);
          break;
        case 6:
          _vm.SelectedReporterContent[1] = Content;
          _vm.SelectedReporterIndex[1] = Index;
          BeadParamsHelper.ChooseProperReporterChannels(_paramsDict[_vm.SelectedReporterIndex[0]], _paramsDict[Index],
            _paramsDict[_vm.SelectedReporterIndex[2]], _paramsDict[_vm.SelectedReporterIndex[3]]);
          App.DiosApp.Device.Hardware.SetSpectraplexReporterChannels(_channelsDict[_vm.SelectedReporterIndex[0]], _channelsDict[Index],
            _channelsDict[_vm.SelectedReporterIndex[2]], _channelsDict[_vm.SelectedReporterIndex[3]],
            _channelsDict[_vm.SelectedCl3rIndex]);
          break;
        case 7:
          _vm.SelectedReporterContent[2] = Content;
          _vm.SelectedReporterIndex[2] = Index;
          BeadParamsHelper.ChooseProperReporterChannels(_paramsDict[_vm.SelectedReporterIndex[0]], _paramsDict[_vm.SelectedReporterIndex[1]],
            _paramsDict[Index], _paramsDict[_vm.SelectedReporterIndex[3]]);
          App.DiosApp.Device.Hardware.SetSpectraplexReporterChannels(_channelsDict[_vm.SelectedReporterIndex[0]], _channelsDict[_vm.SelectedReporterIndex[1]],
            _channelsDict[Index], _channelsDict[_vm.SelectedReporterIndex[3]],
            _channelsDict[_vm.SelectedCl3rIndex]);
          break;
        case 8:
          _vm.SelectedReporterContent[3] = Content;
          _vm.SelectedReporterIndex[3] = Index;
          BeadParamsHelper.ChooseProperReporterChannels(_paramsDict[_vm.SelectedReporterIndex[0]], _paramsDict[_vm.SelectedReporterIndex[1]],
            _paramsDict[_vm.SelectedReporterIndex[2]], _paramsDict[Index]);
          App.DiosApp.Device.Hardware.SetSpectraplexReporterChannels(_channelsDict[_vm.SelectedReporterIndex[0]], _channelsDict[_vm.SelectedReporterIndex[1]],
            _channelsDict[_vm.SelectedReporterIndex[2]], _channelsDict[Index],
            _channelsDict[_vm.SelectedCl3rIndex]);
          break;
        case 9:
          _vm.SelectedCl3rContent = Content;
          _vm.SelectedCl3rIndex = Index;
          BeadParamsHelper.ChooseProperCl3rChannels(_paramsDict[Index]);
          App.DiosApp.Device.Hardware.SetSpectraplexReporterChannels(_channelsDict[_vm.SelectedReporterIndex[0]], _channelsDict[_vm.SelectedReporterIndex[1]],
            _channelsDict[_vm.SelectedReporterIndex[2]], _channelsDict[_vm.SelectedReporterIndex[3]],
            _channelsDict[Index]);
          break;
      }
    }

    public static void ResetIndex()
    {
      _nextIndex = 0;
    }
  }
}