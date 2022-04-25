using System.Reflection;
using System;
using System.Collections.Generic;
using System.Windows;
using Ei_Dimension.ViewModels;
using DIOS.Core;
using System.IO;
using System.Configuration;
using Ei_Dimension.Controllers;

namespace Ei_Dimension
{
  public partial class App : Application
  {
    public static (PropertyInfo prop, object VM) NumpadShow { get; set; }
    public static (PropertyInfo prop, object VM) KeyboardShow { get; set; }
    public static Device Device { get; private set; }
    public static MapRegionsController MapRegions { get; set; }
    public static bool _nextWellWarning;

    private static bool _workOrderPending;

    public App()
    {
      CorruptSettingsChecker();
      InitApp(null);
    }

    public App(Device device)
    {
      CorruptSettingsChecker();
      InitApp(device);
    }

    private static void CorruptSettingsChecker()
    {
      try
      {
        Settings.Default.Reload();
        var value = Settings.Default.DefaultMap;
      }
      catch (ConfigurationErrorsException ex)
      {
        string filename = ((ConfigurationErrorsException)ex.InnerException).Filename;
        File.Delete(filename);
        StartupFinalizer.SettingsWiped = true;
        MessageBox.Show("Settings file is Corrupted.\n\nPlease Restart the Application.");
        Environment.Exit(1);
      }
    }

    private static void SetupDevice()
    {
      if (Device == null)
        throw new Exception("Device not initialized");

      if (Settings.Default.DefaultMap > Device.MapCtroller.MapList.Count - 1)
      {
        try
        {
          Device.MapCtroller.ActiveMap = Device.MapCtroller.MapList[0];
        }
        catch
        {
          throw new Exception($"Could not find Maps in {Device.RootDirectory.FullName + @"\Config" } folder");
        }
      }
      else
      {
        Device.MapCtroller.ActiveMap = Device.MapCtroller.MapList[Settings.Default.DefaultMap];
      }
      Device.Control = (SystemControl)Settings.Default.SystemControl;
      Device.Everyevent = Settings.Default.Everyevent;
      Device.RMeans = Settings.Default.RMeans;
      Device.PlateReportActive = Settings.Default.PlateReport;
      Device.TerminationType = (Termination)Settings.Default.EndRead;
      Device.MinPerRegion = Settings.Default.MinPerRegion;
      Device.BeadsToCapture = Settings.Default.BeadsToCapture;
      Device.OnlyClassified = Settings.Default.OnlyClassifed;
      Device.SensitivityChannel = Settings.Default.SensitivityChannelB? HiSensitivityChannel.B: HiSensitivityChannel.C;
      Device.ReporterScaling = Settings.Default.ReporterScaling;
      Device.MainCommand("Set Property", code: 0xbf, parameter: (ushort)Device.MapCtroller.ActiveMap.calParams.att);
      if (!System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
        Device.MainCommand("Startup");
      Device.HdnrTrans = Device.MapCtroller.ActiveMap.calParams.DNRTrans;
      Device.Compensation = Device.MapCtroller.ActiveMap.calParams.compensation;
      Device.MainCommand("Set Property", code: 0x97, parameter: 1170);  //set current limit of aligner motors if leds are off
    }

    public static int GetMapIndex(string MapName)
    {
      int i = 0;
      for (; i < Device.MapCtroller.MapList.Count; i++)
      {
        if (Device.MapCtroller.MapList[i].mapName == MapName)
          break;
      }
      if (i == Device.MapCtroller.MapList.Count)
        i = -1;
      return i;
    }

    public static void SetActiveMap(string mapName)
    {
      for (var i = 0; i < Device.MapCtroller.MapList.Count; i++)
      {
        if (Device.MapCtroller.MapList[i].mapName == mapName)
        {
          Device.MapCtroller.ActiveMap = Device.MapCtroller.MapList[i];
          Settings.Default.DefaultMap = i;
          Settings.Default.Save();
          break;
        }
      }
      ResultsViewModel.Instance.FillWorldMaps();

      var CaliVM = CalibrationViewModel.Instance;
      if (CaliVM != null)
      {
        CaliVM.EventTriggerContents[1] = Device.MapCtroller.ActiveMap.calParams.minmapssc.ToString();
        Device.MainCommand("Set Property", code: 0xce, parameter: (ushort)Device.MapCtroller.ActiveMap.calParams.minmapssc);
        CaliVM.EventTriggerContents[2] = Device.MapCtroller.ActiveMap.calParams.maxmapssc.ToString();
        Device.MainCommand("Set Property", code: 0xcf, parameter: (ushort)Device.MapCtroller.ActiveMap.calParams.maxmapssc);
        CaliVM.AttenuationBox[0] = Device.MapCtroller.ActiveMap.calParams.att.ToString();
        Device.MainCommand("Set Property", code: 0xbf, parameter: (ushort)Device.MapCtroller.ActiveMap.calParams.att);


        CaliVM.EventTriggerContents[0] = Device.MapCtroller.ActiveMap.calParams.height.ToString();
        Device.MainCommand("Set Property", code: 0xcd, parameter: Device.MapCtroller.ActiveMap.calParams.height);
        CaliVM.CompensationPercentageContent[0] = Device.MapCtroller.ActiveMap.calParams.compensation.ToString();
        Device.Compensation = Device.MapCtroller.ActiveMap.calParams.compensation;
        CaliVM.DNRContents[0] = Device.MapCtroller.ActiveMap.calParams.DNRCoef.ToString();
        Device.HDnrCoef = Device.MapCtroller.ActiveMap.calParams.DNRCoef;
        CaliVM.DNRContents[1] = Device.MapCtroller.ActiveMap.calParams.DNRTrans.ToString();
        Device.HdnrTrans = Device.MapCtroller.ActiveMap.calParams.DNRTrans;
        CaliVM.ClassificationTargetsContents[0] = Device.MapCtroller.ActiveMap.calParams.CL0.ToString();
        Device.MainCommand("Set Property", code: 0x8b, parameter: (ushort)Device.MapCtroller.ActiveMap.calParams.CL0);
        CaliVM.ClassificationTargetsContents[1] = Device.MapCtroller.ActiveMap.calParams.CL1.ToString();
        Device.MainCommand("Set Property", code: 0x8c, parameter: (ushort)Device.MapCtroller.ActiveMap.calParams.CL1);
        CaliVM.ClassificationTargetsContents[2] = Device.MapCtroller.ActiveMap.calParams.CL2.ToString();
        Device.MainCommand("Set Property", code: 0x8d, parameter: (ushort)Device.MapCtroller.ActiveMap.calParams.CL2);
        CaliVM.ClassificationTargetsContents[3] = Device.MapCtroller.ActiveMap.calParams.CL3.ToString();
        Device.MainCommand("Set Property", code: 0x8e, parameter: (ushort)Device.MapCtroller.ActiveMap.calParams.CL3);
        CaliVM.ClassificationTargetsContents[4] = Device.MapCtroller.ActiveMap.calParams.RP1.ToString();
        Device.MainCommand("Set Property", code: 0x8f, parameter: (ushort)Device.MapCtroller.ActiveMap.calParams.RP1);
        CaliVM.GatingItems[Device.MapCtroller.ActiveMap.calParams.gate].Click();
      }

      var ChannelsVM = ChannelsViewModel.Instance;
      if (ChannelsVM != null)
      {
        ChannelsVM.Bias30Parameters[0] = Device.MapCtroller.ActiveMap.calgssc.ToString();
        Device.MainCommand("Set Property", code: 0x28, parameter: (ushort)Device.MapCtroller.ActiveMap.calgssc);
        ChannelsVM.Bias30Parameters[1] = Device.MapCtroller.ActiveMap.calrpmaj.ToString();
        Device.MainCommand("Set Property", code: 0x29, parameter: (ushort)Device.MapCtroller.ActiveMap.calrpmaj);
        ChannelsVM.Bias30Parameters[2] = Device.MapCtroller.ActiveMap.calrpmin.ToString();
        Device.MainCommand("Set Property", code: 0x2a, parameter: (ushort)Device.MapCtroller.ActiveMap.calrpmin);
        ChannelsVM.Bias30Parameters[3] = Device.MapCtroller.ActiveMap.calcl3.ToString();
        Device.MainCommand("Set Property", code: 0x2c, parameter: (ushort)Device.MapCtroller.ActiveMap.calcl3);
        ChannelsVM.Bias30Parameters[4] = Device.MapCtroller.ActiveMap.calrssc.ToString();
        Device.MainCommand("Set Property", code: 0x2d, parameter: (ushort)Device.MapCtroller.ActiveMap.calrssc);
        ChannelsVM.Bias30Parameters[5] = Device.MapCtroller.ActiveMap.calcl1.ToString();
        Device.MainCommand("Set Property", code: 0x2e, parameter: (ushort)Device.MapCtroller.ActiveMap.calcl1);
        ChannelsVM.Bias30Parameters[6] = Device.MapCtroller.ActiveMap.calcl2.ToString();
        Device.MainCommand("Set Property", code: 0x2f, parameter: (ushort)Device.MapCtroller.ActiveMap.calcl2);
        ChannelsVM.Bias30Parameters[7] = Device.MapCtroller.ActiveMap.calvssc.ToString();
        Device.MainCommand("Set Property", code: 0x25, parameter: (ushort)Device.MapCtroller.ActiveMap.calvssc);
        ChannelsVM.Bias30Parameters[8] = Device.MapCtroller.ActiveMap.calcl0.ToString();
        Device.MainCommand("Set Property", code: 0x26, parameter: (ushort)Device.MapCtroller.ActiveMap.calcl0);
        ChannelsVM.Bias30Parameters[9] = Device.MapCtroller.ActiveMap.calfsc.ToString();
        Device.MainCommand("Set Property", code: 0x24, parameter: (ushort)Device.MapCtroller.ActiveMap.calfsc);
      }

      var DashVM = DashboardViewModel.Instance;
      if (DashVM != null)
      {
        if (Device.MapCtroller.ActiveMap.validation)
        {
          DashVM.CalValModeEnabled = true;
          DashVM.CaliDateBox[0] = Device.MapCtroller.ActiveMap.caltime;
          DashVM.ValidDateBox[0] = Device.MapCtroller.ActiveMap.valtime;
        }
        else
        {
          DashVM.CalValModeEnabled = false;
          DashVM.CaliDateBox[0] = null;
          DashVM.ValidDateBox[0] = null;
        }
      }

      bool Warning = false;
      if (Device.MapCtroller.ActiveMap.validation)
      {
        var valDate = DateTime.Parse(Device.MapCtroller.ActiveMap.valtime, new System.Globalization.CultureInfo("en-GB"));
        switch (Settings.Default.VerificationWarningIndex)
        {
          case 0:
            Warning = valDate.AddDays(1) < DateTime.Today;
            break;
          case 1:
            Warning = valDate.AddDays(7) < DateTime.Today;
            break;
          case 2:
            Warning = valDate.AddMonths(1) < DateTime.Today;
            break;
          case 3:
            Warning = valDate.AddMonths(3) < DateTime.Today;
            break;
          case 4:
            Warning = valDate.AddYears(1) < DateTime.Today;
            break;
        }
      }
      DashVM.VerificationWarningVisible = Warning? Visibility.Visible : Visibility.Hidden;

    }

    public static void SetSystemControl(byte num)
    {
      Device.Control = (SystemControl)num;
      Settings.Default.SystemControl = num;
      Settings.Default.Save();
    }

    public static void SetTerminationType(byte num)
    {
      Device.TerminationType = (Termination)num;
      Settings.Default.EndRead = num;
      Settings.Default.Save();
    }

    public static void LockMapSelection()
    {
      Views.DashboardView.Instance.MapSelectr.IsEnabled = false;
      Views.CalibrationView.Instance.MapSelectr.IsEnabled = false;
      Views.VerificationView.Instance.MapSelectr.IsEnabled = false;
      Views.ChannelsView.Instance.MapSelectr.IsEnabled = false;
    }

    public static void UnlockMapSelection()
    {
      Views.DashboardView.Instance.MapSelectr.IsEnabled = true;
      Views.CalibrationView.Instance.MapSelectr.IsEnabled = true;
      Views.VerificationView.Instance.MapSelectr.IsEnabled = true;
      Views.ChannelsView.Instance.MapSelectr.IsEnabled = true;
    }

    public static void HideNumpad()
    {
      UserInputHandler.InputSanityCheck();
      NumpadShow.prop.SetValue(NumpadShow.VM, Visibility.Hidden);
    }

    public static void HideKeyboard()
    {
      UserInputHandler.InputSanityCheck();
      KeyboardShow.prop.SetValue(KeyboardShow.VM, Visibility.Hidden);
    }

    public static void UnfocusUIElement()
    {
      if (Ei_Dimension.MainWindow.Instance == null)
        return;
      System.Windows.Input.FocusManager.SetFocusedElement(System.Windows.Input.FocusManager.GetFocusScope(Ei_Dimension.MainWindow.Instance), null);
      System.Windows.Input.Keyboard.ClearFocus();
    }

    /*
    private static void WorkOrderHandler()
    {
      //see if work order is available
      if (DashboardViewModel.Instance.SelectedSystemControlIndex != 0)
      {
        if (Device.IsNewWorkOrder())
        {
          DashboardViewModel.Instance.WorkOrder[0] = Device.WorkOrderName;
          _workOrderPending = true;
        }
      }
      if (_workOrderPending)
      {
        if (DashboardViewModel.Instance.SelectedSystemControlIndex == 1)  //no barcode required so allow start
        {
          MainButtonsViewModel.Instance.StartButtonEnabled = true;
          _workOrderPending = false;
        }
        else if (DashboardViewModel.Instance.SelectedSystemControlIndex == 2)    //barcode required
        {
          //if (videogoing & (video != null))
          //{
          //  BarcodeResult plateGUID = BarcodeReader.QuicklyReadOneBarcode(video, BarcodeEncoding.PDF417, true);
          //  if (plateGUID != null)
          //    pltNumbertb.Text = plateGUID.Value;
          //}
          //else
          //  DashboardViewModel.Instance.ValidateBCodeButtonEnabled = true;
          //  if (DashboardViewModel.Instance.WorkOrder[0] == pltNumbertb.Text)
          //  {
          //    DashboardViewModel.Instance.StartButtonEnabled = true;
          //    _workOrderPending = false;
          //    Device.MainCommand("Set Property", code: 0x17); //leds off
          //    DashboardViewModel.Instance.ValidateBCodeButtonEnabled = false;
          //  }
          //  else
          //  {
          //    //  pltNumbertb.BackColor = Color.Red;
          //  }
        }
      }
    }
    */
    
    private static void StartingToReadWellEventHandler(object sender, ReadingWellEventArgs e)
    {
      MultiTube.GetModifiedWellIndexes(e, out var row, out var col);

      ResultsViewModel.Instance.PlatePictogramIsCovered = Visibility.Visible; //TODO: temporary solution

      ResultsViewModel.Instance.PlatePictogram.CurrentlyReadCell = (row, col);
      ResultsViewModel.Instance.PlatePictogram.ChangeState(row, col, Models.WellType.NowReading, GetWarningState(), FilePath: e.FilePath);
      
      App.Current.Dispatcher.Invoke(() =>
      {
        ResultsViewModel.Instance.CornerButtonClick(Models.DrawingPlate.CalculateCorner(row, col));
        ResultsViewModel.Instance.ClearGraphs();
      });
      ActiveRegionsStatsController.Instance.ResetCurrentActiveRegionsDisplayedStats();
    }

    private static Models.WellWarningState GetWarningState()
    {
      if (_nextWellWarning)
      {
        _nextWellWarning = false;
        return Models.WellWarningState.YellowWarning;
      }
      return Models.WellWarningState.OK;
    }

    public static void FinishedReadingWellEventHandler(object sender, ReadingWellEventArgs e)
    {
      var type = GetWellStateForPictogram();
      
      MultiTube.GetModifiedWellIndexes(e, out var row, out var col);
      MultiTube.Proceed();

      ResultsViewModel.Instance.PlatePictogramIsCovered = Visibility.Hidden; //TODO: temporary solution

      ResultsViewModel.Instance.PlatePictogram.ChangeState(row, col, type);
      SavePlateState();
      if (Device.Control == SystemControl.WorkOrder)
      {
        App.Current.Dispatcher.Invoke(() => DashboardViewModel.Instance.WorkOrder[0] = ""); //actually questionable if not in workorder operation
      }
      LogBeadsFromFirmware();
    }

    private static Models.WellType GetWellStateForPictogram()
    {
      var type = Models.WellType.Success;
      if (Device.Mode == OperationMode.Normal && Device.TerminationType == Termination.MinPerRegion)
      {
        var lacking = Device.Results.MinPerRegionAchieved();
        //not achieved
        if (lacking < 0)
        {
          //if lacking more then 25% of minperregion beads 
          if (-lacking > Device.MinPerRegion * 0.25)
          {
            type = Models.WellType.Fail;
          }
          else
          {
            type = Models.WellType.LightFail;
          }
        }
      }
      return type;
    }

    public static void FinishedMeasurementEventHandler(object sender, EventArgs e)
    {
      MainButtonsViewModel.Instance.StartButtonEnabled = true;
      ResultsViewModel.Instance.PlatePictogram.CurrentlyReadCell = (-1, -1);
      switch (Device.Mode)
      {
        case OperationMode.Normal:
          break;
        case OperationMode.Calibration:
          if (++CalibrationViewModel.Instance.CalFailsInARow >= 3 && CalibrationViewModel.Instance.CalJustFailed)
          {
            Notification.ShowLocalized(nameof(Language.Resources.Calibration_Fail), System.Windows.Media.Brushes.Red);
            App.Current.Dispatcher.Invoke(DashboardViewModel.Instance.CalModeToggle);
          }
          else if (CalibrationViewModel.Instance.CalJustFailed)
            Notification.ShowLocalized(nameof(Language.Resources.Calibration_in_Progress), System.Windows.Media.Brushes.Green);
          break;
        case OperationMode.Verification:
          if (VerificationViewModel.AnalyzeVerificationResults(out var errorMsg))
          {
            _ = Current.Dispatcher.BeginInvoke((Action)VerificationViewModel.VerificationSuccess);
          }
          else
          {
            Notification.Show(errorMsg, System.Windows.Media.Brushes.Red, 26);
            //Notification.ShowLocalized(nameof(Language.Resources.Validation_Fail), System.Windows.Media.Brushes.Red);
          }
          break;
      }
    }

    public static void NewStatsAvailableEventHandler(object sender, StatsEventArgs e)
    {
      _ = Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        for (var i = 0; i < 10; i++)
        {
          ResultsViewModel.Instance.CurrentMfiItems[i] = e.GStats[i].mfi.ToString($"{0:0.0}");
          ResultsViewModel.Instance.CurrentCvItems[i] = e.GStats[i].cv.ToString($"{0:0.00}");
          ChannelOffsetViewModel.Instance.AverageBg[i] = e.AvgBg[i].ToString($"{0:0.00}");
        }
      }));
    }

    public static void SetLogOutput()
    {
      Device.Publisher.OutDirCheck();
      if (!Directory.Exists(Device.Publisher.Outdir + "\\SystemLogs"))
        Directory.CreateDirectory(Device.Publisher.Outdir + "\\SystemLogs");
      string logPath = Path.Combine(Path.Combine(@"C:\Emissioninc", Environment.MachineName), "SystemLogs", "EventLog");
      string logFilePath = logPath + ".txt";
    
      string backFilePath = logPath + ".bak";
      if (File.Exists(logFilePath))
      {
        File.Delete(backFilePath);
        File.Move(logFilePath, backFilePath);
      }
      var fs = new FileStream(logFilePath, FileMode.Create);
      var logWriter = new StreamWriter(fs);
      logWriter.AutoFlush = true;
      Console.SetOut(logWriter);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      DevExpress.Xpf.Core.ApplicationThemeHelper.ApplicationThemeName = DevExpress.Xpf.Core.Theme.Office2019ColorfulName;
      base.OnStartup(e);
    }

    public static void SavePlateState()
    {
      //overwrite the whole thing
      try
      {
        string contents = ResultsViewModel.Instance.PlatePictogram.GetSerializedPlate();
        File.WriteAllText($"{Device.RootDirectory.FullName}\\Status\\StatusFile.json", contents);
      }
      catch(Exception e)
      {
        Notification.Show($"Problem with status file save, Please report this issue to the Manufacturer {e.Message}");
      }
    }

    public static void InitSTab(string tabname)
    {
      //Removing this can lead to unforseen crucial bugs in instrument operation. If so - do with extra care
      //one example is a check in CommandLists.Readertab for changed plate parameter,which could happen in manual well selection in motors tab
      List<byte> list;
      switch (tabname)
      {
        case "readertab":
          list = CommandLists.Readertab;
          break;
        case "reportingtab":
          list = CommandLists.Reportingtab;
          break;
        case "calibtab":
          list = CommandLists.Calibtab;
          break;
        case "channeltab":
          list = CommandLists.Channeltab;
          break;
        case "motorstab":
          list = CommandLists.Motorstab;
          break;
        case "componentstab":
          list = CommandLists.Componentstab;
          break;
        default:
          return;
      }
      foreach (byte Code in list)
      {
        Device.MainCommand("Get Property", code: Code);
      }
    }

    public void OnNewWorkOrder(object sender, FileSystemEventArgs e)
    {
      var name = Path.GetFileNameWithoutExtension(e.Name);
      ResultsPublisher.WorkOrderPath = e.FullPath;
      if (!ParseWorkOrder())
        return;
      
      _workOrderPending = true;
      DashboardViewModel.Instance.WorkOrder[0] = name;  //check for already existing one 
      // if WO already selected -> allow start. else the WO checking action should perform the same check
      if (DashboardViewModel.Instance.SelectedSystemControlIndex == 1)  //no barcode required so allow start
      {
        MainButtonsViewModel.Instance.StartButtonEnabled = true;
        _workOrderPending = false;  //questionable logic. can check on non empty textbox
      }
    }

    public static void CheckAvailableWorkOrders()
    {
      string[] fileEntries = Directory.GetFiles($"{Device.RootDirectory.FullName}\\WorkOrder", "*.txt");
      if (fileEntries.Length == 0)
        return;
      var name = Path.GetFileNameWithoutExtension(fileEntries[0]);
      ResultsPublisher.WorkOrderPath = fileEntries[0];
      int i = 1;
      while (!ParseWorkOrder())
      {
        if (i < fileEntries.Length)
        {
          ResultsPublisher.WorkOrderPath = fileEntries[i];
          name = Path.GetFileNameWithoutExtension(fileEntries[i]);
          i++;
        }
        else
          return;
      }

      DashboardViewModel.Instance.WorkOrder[0] = name;  //should be first succesfully parsed
      _workOrderPending = true;
      // if WO already selected -> allow start. else the WO checking action should perform the same check
      if (DashboardViewModel.Instance.SelectedSystemControlIndex == 1)  //no barcode required so allow start
      {
        MainButtonsViewModel.Instance.StartButtonEnabled = true;
        _workOrderPending = false;
      }
    }

    private static bool ParseWorkOrder()
    {
      try
      {
        using (TextReader reader = new StreamReader(ResultsPublisher.WorkOrderPath))
        {
          var contents = reader.ReadToEnd();
          Device.WorkOrder = Newtonsoft.Json.JsonConvert.DeserializeObject<WorkOrder>(contents);
        }
      }
      catch
      {
        return false;
      }
      return true;
    }

    //TODO:make it a property of Device that calls maincommand on GET
    private static void LogBeadsFromFirmware()
    {
      Device.MainCommand("Get FProperty", code: 0x06);  //get totalbeads from firmware
      Console.WriteLine($"[Report] FW:SW {MainViewModel.Instance.TotalBeadsInFirmware} : {MainViewModel.Instance.EventCountCurrent}");
    }

    private void InitApp(Device device)
    {
      Device = device ?? new Device(new USBConnection());

      if (Directory.Exists(Settings.Default.LastOutFolder))
        Device.Publisher.Outdir = Settings.Default.LastOutFolder;
      else
      {
        Settings.Default.LastOutFolder = Device.Publisher.Outdir;
        Settings.Default.Save();
      }
      SetLogOutput();
      SetupDevice();
      Device.StartingToReadWell += StartingToReadWellEventHandler;
      Device.FinishedReadingWell += FinishedReadingWellEventHandler;
      Device.FinishedMeasurement += FinishedMeasurementEventHandler;
      Device.NewStatsAvailable += NewStatsAvailableEventHandler;
      ResultsPublisher.Outfilename = Settings.Default.SaveFileName;
      _workOrderPending = false;
      _nextWellWarning = false;
      var watcher = new FileSystemWatcher($"{Device.RootDirectory.FullName}\\WorkOrder");
      watcher.NotifyFilter = NotifyFilters.FileName;
      watcher.Filter = "*.txt";
      watcher.EnableRaisingEvents = true;
      watcher.Created += OnNewWorkOrder;
    }
  }
}