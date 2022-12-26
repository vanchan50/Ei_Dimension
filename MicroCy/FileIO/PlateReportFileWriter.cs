﻿using System;
using System.IO;

namespace DIOS.Core.FileIO
{
  public class PlateReportFileWriter
  {
    private ResultsPublisher _publisher;

    public PlateReportFileWriter(ResultsPublisher publisher)
    {
      _publisher = publisher;
    }

    public void CreateAndWrite(string PlateReportJSON)
    {
      if (!_publisher.IsPlateReportPublishingActive)
      {
        Console.WriteLine("Plate Report Inactive");
        return;
      }

      if (!_publisher.OutputDirectoryExists(_publisher.SUMMARYFOLDERNAME))
        return;

      try
      {
        var fullFilePath = Path.Combine(_publisher.SUMMARYFOLDERNAME, $"Summary_{_publisher.ReportFileName}_{_publisher.Date}.json");
        File.WriteAllText(fullFilePath, PlateReportJSON);
        Console.WriteLine($"Plate Report saved as {fullFilePath}");
      }
      catch
      {
        Console.WriteLine("Failed to create Plate Report");
      }
    }
  }
}
