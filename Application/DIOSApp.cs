using System;
using System.Collections.Generic;
using System.IO;
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
    public SystemControl Control { get; set; } = SystemControl.Manual;
    public Termination TerminationType { get; set; } = Termination.MinPerRegion;
    public int TotalBeadsToCapture { get; set; }
    public int MinPerRegion { get; set; }
    public WorkOrder WorkOrder { get; set; }
    public bool RunPlateContinuously { get; set; }
    public Verificator Verificator { get; }
    public readonly string BUILD = "1.5.2.2";
    public ILogger Logger { get; }

    public DIOSApp()
    {
      SetSystemDirectories();
      Logger = new Logger(RootDirectory.FullName);
      MapController = new MapController($"{RootDirectory.FullName}\\Config", Logger);
      Publisher = new ResultsPublisher(RootDirectory.FullName, Logger);
      Device = new Device(new USBConnection(Logger), Logger);
      Results = new RunResults(Device, this);
      Verificator = new Verificator(Logger);
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
  }
}