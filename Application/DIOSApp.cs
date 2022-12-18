using System;
using System.Collections.Generic;
using System.IO;
using DIOS.Core;

namespace DIOSApplication
{ 
  public class DIOSApp
  {
    public MapController MapController { get; }
    public DirectoryInfo RootDirectory { get; private set; }
    public bool RunPlateContinuously { get; set; }

    public DIOSApp()
    {
      SetSystemDirectories();
      MapController = new MapController($"{RootDirectory.FullName}\\Config");
    }
    private void SetSystemDirectories()
    {
      RootDirectory = new DirectoryInfo(Path.Combine(@"C:\Emissioninc", Environment.MachineName));
      List<string> subDirectories = new List<string>(8) { "Config", "WorkOrder", "SavedImages", "Archive", "Result", "Status", "AcquisitionData", "SystemLogs" };
      try
      {
        foreach (var d in subDirectories)
        {
          RootDirectory.CreateSubdirectory(d);
        }
        Directory.CreateDirectory(RootDirectory.FullName + @"\Result" + @"\Summary");
        Directory.CreateDirectory(RootDirectory.FullName + @"\Result" + @"\Detail");
      }
      catch
      {
        Console.WriteLine("Directory Creation Failed");
      }
    }
  }
}