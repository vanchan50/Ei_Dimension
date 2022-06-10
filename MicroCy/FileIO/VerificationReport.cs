using System;
using System.Collections.Generic;
using System.IO;

namespace DIOS.Core.FileIO
{
  public class VerificationReport
  {
    public DateTime Time;
    public int TotalBeads;
    public double Tolerance1;
    public double Tolerance2;
    public double Tolerance3;
    public bool Passed;
    public readonly List<(int region, double errror)> Test1regions = new List<(int, double)>();
    public double UnclassifiedBeadsPercentage;

    public int Test3HighestUnclassifiedCount;
    public int Test3HighestUnclassifiedCountRegion;

    public int Test3NearestClassifiedCount;
    public int Test3NearestClassifiedCountRegion;
    public double Test3MisclassificationsPercentage;
    private const string HEADER = "Date,Time,Total Beads,Test1 tolerance%,Test2 tolerance%,Test3 tolerance%,Passed,Test1 region #A, %over/under target," +
                                  "Test1 region #B, %over/under target,Test1 region #C, %over/under target,Test1 region #D, %over/under target," +
                                  "Test2 Unclassified beads%, Test3 non-Validation region with highest count" +
                                  "Test3 highest count, Test3 nearest Validation region#,Test3 count of nearest Validation Region";

    public override string ToString()
    {
      if (Test1regions.Count < 4)
        throw new Exception("less than 4 test1 regions selected");
      return $"{Time.ToString("dd.MM.yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"))}," +
             $"{Time.ToString("hh:mm:ss", System.Globalization.CultureInfo.CreateSpecificCulture("en - US"))}," +
             $"{TotalBeads},{Tolerance1},{Tolerance2},{Tolerance3},{Passed},{Test1regions[0].region},{Test1regions[0].errror}," +
             $"{Test1regions[1].region},{Test1regions[1].errror},{Test1regions[2].region},{Test1regions[2].errror}," +
             $"{Test1regions[3].region},{Test1regions[3].errror},{UnclassifiedBeadsPercentage}," +
             $"{Test3HighestUnclassifiedCountRegion},{Test3HighestUnclassifiedCount},{Test3NearestClassifiedCountRegion},{Test3NearestClassifiedCount}";
    }

    public void Publish()
    {
      var path = Device.RootDirectory.FullName + @"\SystemLogs\VerificationLogs.txt";
      if (!File.Exists(path))
      {
        try
        {
          File.WriteAllText(path, HEADER);
          File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
        }
        catch
        {

        }
      }

      var contents = ToString();
      try
      {
        File.AppendAllText(path, contents);
      }
      catch
      {

      }
    }
  }
}
