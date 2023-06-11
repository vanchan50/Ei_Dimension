﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DIOS.Application.Domain;
using DIOS.Application.FileIO;
using DIOS.Core;

namespace DIOS.Application
{ 
  public class DIOSApp
  {
    public Device Device { get; }
    public MapController MapController { get; }
    public DirectoryInfo RootDirectory { get; } = new DirectoryInfo(Path.Combine(@"C:\Emissioninc", Environment.MachineName));
    public ResultsPublisher Publisher { get; }
    public RunResults Results { get; }
    public ResultsProcessor ResultsProc { get; }  //TODO make private
    public SystemControl Control { get; set; } = SystemControl.Manual;
    public ReadTerminator Terminator => ResultsProc.Terminator;
    public WorkOrder WorkOrder { get; set; }
    public bool RunPlateContinuously { get; set; }
    public Verificator Verificator { get; }
    public readonly string BUILD = Assembly.GetCallingAssembly().GetName().Version.ToString();
    public ILogger Logger { get; }

    public DIOSApp()
    {
      SetSystemDirectories();
      Logger = new Logger(RootDirectory.FullName);
      MapController = new MapController($"{RootDirectory.FullName}\\Config", Logger);
      Publisher = new ResultsPublisher(RootDirectory.FullName, Logger);
      Device = new Device(new USBConnection(Logger), Logger);
      Results = new RunResults(Device, this);
      ResultsProc = new ResultsProcessor(Device, Results);
      Verificator = new Verificator(Logger);
    }

    public void StartOperation(IReadOnlyCollection<int> regions, IReadOnlyCollection<Well> wells)
    {
      Results.SetupRunRegions(regions);
      Publisher.ResultsFile.MakeNew();
      Results.StartNewPlateReport();
      Logger.Log(Publisher.ReportActivePublishingFlags());
      Device.StartOperation(wells, Results.OutputBeadsCollector);
      ResultsProc.StartBeadProcessing();//call after StartOperation, so IsMeasurementGoing == true
    }

    private void SetSystemDirectories()
    {
      List<string> subDirectories = new List<string> { "Config", "WorkOrder", "SavedImages", "Archive", "Status" };
      try
      {
        foreach (var d in subDirectories)
        {
          RootDirectory.CreateSubdirectory(d);
        }
        Directory.CreateDirectory(RootDirectory.FullName + @"\Result" + @"\Detail");
      }
      catch
      {
        Console.WriteLine("Directory Creation Failed");
      }
    }

    public WellType GetWellStateForPictogram()
    {
      var type = WellType.Success;

      //feature only for Normal mode, MinPerRegion Termination
      if (Device.Mode != OperationMode.Normal
          || Terminator.TerminationType != Termination.MinPerRegion)
        return type;

      var lacking = Results.MinPerRegionAchieved(Terminator.MinPerRegion);
      //not achieved
      if (lacking < 0)
      {
        //if lacking more then 25% of minperregion beads 
        if (-lacking > Terminator.MinPerRegion * 0.25)
        {
          type = WellType.Fail;
        }
        else
        {
          type = WellType.LightFail;
        }
      }
      return type;
    }

    public void SaveWellFiles()
    {
      _ = Task.Run(() =>
      {
        Results.MakeWellStats();
        Publisher.ResultsFile.AppendAndWrite(Results.PublishWellStats()); //makewellstats should be awaited only for this method
        Publisher.DoSomethingWithWorkOrder();
      });
      Publisher.BeadEventFile.CreateAndWrite(Results.PublishBeadEvents());
    }
  }
}