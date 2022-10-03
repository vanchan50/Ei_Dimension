using System.Reflection;
using System;
using System.Collections.Generic;
using System.Windows;
using Ei_Dimension.ViewModels;
using DIOS.Core;
using System.IO;
using System.Configuration;
using System.Text;
using Ei_Dimension.Cache;
using Ei_Dimension.Controllers;

namespace Ei_Dimension
{
  public partial class App : Application
  {
    public static (PropertyInfo prop, object VM) NumpadShow { get; set; }
    public static (PropertyInfo prop, object VM) KeyboardShow { get; set; }
    public static Device Device { get; private set; }
    public static ResultsCache Cache { get; } = new ResultsCache();
    public static MapRegionsController MapRegions { get; set; }
    public static bool _nextWellWarning;
    public static bool MakeLegacyPlateReport { get; set; } = Settings.Default.LegacyPlateReport;

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
          Device.MapCtroller.SetMap(Device.MapCtroller.MapList[0]);
        }
        catch
        {
          throw new Exception($"Could not find Maps in {Device.RootDirectory.FullName + @"\Config" } folder");
        }
      }
      else
      {
        try
        {
          var map = Device.MapCtroller.MapList[Settings.Default.DefaultMap];
          Device.MapCtroller.SetMap(map);
        }
        catch
        {
          throw new Exception($"Problem with Maps in {Device.RootDirectory.FullName + @"\Config"} folder");
        }
        finally
        {
          Device.MapCtroller.SetMap(Device.MapCtroller.MapList[0]);
        }
      }
      Device.Control = (SystemControl)Settings.Default.SystemControl;
      Device.SaveIndividualBeadEvents = Settings.Default.Everyevent;
      Device.RMeans = Settings.Default.RMeans;
      Device.Publisher.MakePlateReport = Settings.Default.PlateReport;
      Device.TerminationType = (Termination)Settings.Default.EndRead;
      Device.MinPerRegion = Settings.Default.MinPerRegion;
      Device.BeadsToCapture = Settings.Default.BeadsToCapture;
      Device.OnlyClassifiedInBeadEventFile = Settings.Default.OnlyClassifed;
      Device.SensitivityChannel = Settings.Default.SensitivityChannelB ? HiSensitivityChannel.GreenB : HiSensitivityChannel.GreenC;
      Device.ReporterScaling = Settings.Default.ReporterScaling;
      Device.MainCommand("Set Property", code: 0xbf, parameter: (ushort)Device.MapCtroller.ActiveMap.calParams.att);
      Device.HdnrTrans = Device.MapCtroller.ActiveMap.calParams.DNRTrans;
      Device.Compensation = Device.MapCtroller.ActiveMap.calParams.compensation;
      Device.MainCommand("Set Property", code: 0x97, parameter: 1170);  //set current limit of aligner motors if leds are off
      Device.MaxPressure = Settings.Default.MaxPressure;
    }

    public static int GetMapIndex(string mapName)
    {
      var mapList = Device.MapCtroller.MapList;
      int i = 0;
      for (; i < mapList.Count; i++)
      {
        if (mapList[i].mapName == mapName)
          break;
      }
      if (i == mapList.Count)
        i = -1;
      return i;
    }

    public static void SetActiveMap(string mapName)
    {
      for (var i = 0; i < Device.MapCtroller.MapList.Count; i++)
      {
        if (Device.MapCtroller.MapList[i].mapName == mapName)
        {
          Device.MapCtroller.SetMap(Device.MapCtroller.MapList[i]);
          Settings.Default.DefaultMap = i;
          Settings.Default.Save();
          break;
        }
      }
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

    public static void Export(DevExpress.Xpf.Charts.ChartControlBase chart, in int dpi)
    {
      var options = new DevExpress.XtraPrinting.ImageExportOptions
      {
        TextRenderingMode = DevExpress.XtraPrinting.TextRenderingMode.SingleBitPerPixelGridFit,
        Resolution = dpi,
        Format = new System.Drawing.Imaging.ImageFormat(System.Drawing.Imaging.ImageFormat.Png.Guid)
      };
      string date = DateTime.Now.ToString("dd.MM.yyyy.hh-mm-ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
      try
      {
        App.Device.Publisher.OutDirCheck();
        if (!Directory.Exists(App.Device.Publisher.Outdir + "\\SavedImages"))
          Directory.CreateDirectory(App.Device.Publisher.Outdir + "\\SavedImages");
        chart.ExportToImage(App.Device.Publisher.Outdir + @"\SavedImages\" + date + ".png", options);
      }
      catch
      {
        App.Current.Dispatcher.Invoke(() =>
          Notification.Show("Save failed"));
      }
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
    
    private void StartingToReadWellEventHandler(object sender, ReadingWellEventArgs e)
    {
      MultiTube.GetModifiedWellIndexes(e, out var row, out var col);

      //ResultsViewModel.Instance.PlatePictogramIsCovered = Visibility.Visible; //TODO: temporary solution

      PlatePictogramViewModel.Instance.PlatePictogram.CurrentlyReadCell = (row, col);
      PlatePictogramViewModel.Instance.PlatePictogram.ChangeState(row, col, Models.WellType.NowReading, GetWarningState(), FilePath: e.FilePath);
      
      App.Current.Dispatcher.Invoke(() =>
      {
        PlatePictogramViewModel.Instance.CornerButtonClick(Models.DrawingPlate.CalculateCorner(row, col));
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

    public void FinishedReadingWellEventHandler(object sender, ReadingWellEventArgs e)
    {
      var type = GetWellStateForPictogram();
      
      MultiTube.GetModifiedWellIndexes(e, out var row, out var col, proceed:true);

      //Cache.Store(row, col);
      //ResultsViewModel.Instance.PlatePictogramIsCovered = Visibility.Hidden; //TODO: temporary solution

      PlatePictogramViewModel.Instance.PlatePictogram.CurrentlyReadCell = (-1, -1);
      PlatePictogramViewModel.Instance.PlatePictogram.ChangeState(row, col, type);
      SavePlateState();
      if (Device.Control == SystemControl.WorkOrder)
      {
        App.Current.Dispatcher.Invoke(() => DashboardViewModel.Instance.WorkOrder[0] = ""); //actually questionable if not in workorder operation
      }
    }

    private static Models.WellType GetWellStateForPictogram()
    {
      var type = Models.WellType.Success;

      if (Device.Mode != OperationMode.Normal
          || Device.TerminationType != Termination.MinPerRegion)
        return type;

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
      return type;
    }

    public void FinishedMeasurementEventHandler(object sender, EventArgs e)
    {
      MainButtonsViewModel.Instance.StartButtonEnabled = true;
      PlatePictogramViewModel.Instance.PlatePictogram.CurrentlyReadCell = (-1, -1);
      switch (Device.Mode)
      {
        case OperationMode.Normal:
          break;
        case OperationMode.Calibration:
          if (++CalibrationViewModel.Instance.CalFailsInARow >= 3 && CalibrationViewModel.Instance.CalJustFailed)
          {
            Current.Dispatcher.Invoke(() => Notification.ShowLocalizedError(nameof(Language.Resources.Calibration_Fail)));
            Current.Dispatcher.Invoke(DashboardViewModel.Instance.CalModeToggle);
          }
          else if (CalibrationViewModel.Instance.CalJustFailed)
            Current.Dispatcher.Invoke(() => Notification.ShowLocalizedSuccess(nameof(Language.Resources.Calibration_in_Progress)));
          break;
        case OperationMode.Verification:
          if (VerificationViewModel.AnalyzeVerificationResults(out var errorMsg))
          {
            _ = Current.Dispatcher.BeginInvoke((Action)VerificationViewModel.VerificationSuccess);
          }
          else
          {
            Current.Dispatcher.Invoke(()=>Notification.ShowError(errorMsg, 26));
          }
          Verificator.PublishReport();
          Current.Dispatcher.Invoke(DashboardViewModel.Instance.ValModeToggle);
          //Notification.ShowLocalizedError(nameof(Language.Resources.Validation_Fail));
          break;
      }

      OutputLegacyReport();
    }

    public void NewStatsAvailableEventHandler(object sender, StatsEventArgs e)
    {
      _ = Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        ResultsViewModel.Instance.DecodeCalibrationStats(e.Stats, current:true);
        ChannelOffsetViewModel.Instance.DecodeBackgroundStats(e.BgStats);
      }));
    }

    public void BeadConcentrationEventHandler(object sender, int value)
    {
      _ = Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        MainViewModel.Instance.SetBeadConcentrationMonitorValue(value);
      }));
    }

    public void MapChangedEventHandler(object sender, CustomMap map)
    {
      _ = Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        ResultsViewModel.Instance.FillWorldMaps();

        if (CalibrationViewModel.Instance != null)
          CalibrationViewModel.Instance.OnMapChanged(map);

        if (ChannelsViewModel.Instance != null)
          ChannelsViewModel.Instance.OnMapChanged(map);

        if (DashboardViewModel.Instance != null)
          DashboardViewModel.Instance.OnMapChanged(map);

        if (NormalizationViewModel.Instance != null)
          NormalizationViewModel.Instance.OnMapChanged(map);
      }));
    }

    public void OutputLegacyReport()
    {
      if (!MakeLegacyPlateReport)
      {
        Console.WriteLine("Legacy Plate Report Inactive");
        return;
      }

      var bldr = new StringBuilder();
      bldr.AppendLine("Program,\"DIOS\"");
      bldr.AppendLine($"Build,\"{Program.BUILD}\",Firmware,\"{Device.FirmwareVersion}\"");
      bldr.AppendLine($"Date,\"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"))}\"\n");

      bldr.AppendLine($"Instrument,\"{Environment.MachineName}\"");
      bldr.AppendLine($"Session,\"{Device.Publisher.Outfilename}\"\n\n\n\n\n\n\n");

      bldr.AppendLine($"Samples,\"{WellsSelectViewModel.Instance.CurrentTableSize}\"\n");
      var header = MapRegionsController.GetLegacyReportHeader();
      bldr.Append(Device.Results.PlateReport.LegacyReport(header));


      string rfilename = Device.Control == SystemControl.Manual ? Device.Publisher.Outfilename : Device.WorkOrder.plateID.ToString();
      var directoryName = $"{Device.Publisher.Outdir}\\AcquisitionData";
      try
      {
        if (!Directory.Exists(directoryName))
          _ = Directory.CreateDirectory(directoryName);
      }
      catch
      {
        Console.WriteLine($"Failed to create {directoryName}");
        return;
      }

      try
      {
        var fileName = $"{directoryName}" +
                       "\\LxResults_" + rfilename + "_" + Device.Publisher.Date + ".csv";
        using (TextWriter jwriter = new StreamWriter(fileName))
        {
          jwriter.Write(bldr.ToString());
          Console.WriteLine($"Legacy Plate Report saved as {fileName}");
        }
      }
      catch
      {
        Console.WriteLine("Failed to create Legacy Plate Report");
      }
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
        string contents = PlatePictogramViewModel.Instance.PlatePictogram.GetSerializedPlate();
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
      Device.Publisher.WorkOrderPath = e.FullPath;
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
      Device.Publisher.WorkOrderPath = fileEntries[0];
      int i = 1;
      while (!ParseWorkOrder())
      {
        if (i < fileEntries.Length)
        {
          Device.Publisher.WorkOrderPath = fileEntries[i];
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
        using (TextReader reader = new StreamReader(Device.Publisher.WorkOrderPath))
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

    private void InitApp(Device device)
    {
      StatisticsExtension.TailDiscardPercentage = Settings.Default.StatisticsTailDiscardPercentage;
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
      Device.MapCtroller.ChangedActiveMap += MapChangedEventHandler;
      Device.BeadConcentrationStatusUpdate += BeadConcentrationEventHandler;
      Device.Publisher.Outfilename = Settings.Default.SaveFileName;
      _workOrderPending = false;
      _nextWellWarning = false;
      var watcher = new FileSystemWatcher($"{Device.RootDirectory.FullName}\\WorkOrder");
      watcher.NotifyFilter = NotifyFilters.FileName;
      watcher.Filter = "*.txt";
      watcher.EnableRaisingEvents = true;
      watcher.Created += OnNewWorkOrder;

      if (!System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
        Device.StartSelfTest();
    }
  }
}