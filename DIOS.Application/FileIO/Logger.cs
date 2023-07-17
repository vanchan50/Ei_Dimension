using System;
using System.IO;
using DIOS.Core;

namespace DIOS.Application.FileIO
{
  public class Logger : ILogger
  {
    private const string LOGFOLDERNAME = "SystemLogs";
    private const string LOGFILENAME = "EventLog";
    private readonly string _folder;
    private readonly FileStream _fStream;
    private readonly StreamWriter _streamWriter;
    private readonly string _logPath;
    private readonly string _backupLogPath;

    public Logger(string path)
    {
      _folder = Path.Combine(path, LOGFOLDERNAME);
      CheckDir(_folder);

      _logPath = $"{_folder}\\{LOGFILENAME}.txt";
      _backupLogPath = $"{_folder}\\{LOGFILENAME}.bak";
      SetBackup();

      _fStream = new FileStream(_logPath, FileMode.Create);
      _streamWriter = new StreamWriter(_fStream);
      _streamWriter.AutoFlush = true;
    }

   ~Logger()
   {
     _streamWriter.Flush();
     _streamWriter.Dispose();
     _fStream.Dispose();
   }

    public void Log(string message)
    {
      _streamWriter.WriteLine(message);
    }

    public void LogError(string message)
    {
      throw new NotImplementedException();
    }

    private void CheckDir(string folder)
    {
      try
      {
        if (!Directory.Exists(folder))
          Directory.CreateDirectory(folder);
      }
      catch(Exception e)
      {
        Console.Error.WriteLine($"Unable to create directory {folder}");
        Console.Error.WriteLine($"{e.Message}");
        Environment.Exit(1);
      }
    }

    private void SetBackup()
    {
      try
      {
        if (File.Exists(_logPath))
        {
          File.Delete(_backupLogPath);
          File.Move(_logPath, _backupLogPath);
        }
      }
      catch (Exception e)
      {
        Console.Error.WriteLine($"Logger unable to SetBackup");
        Console.Error.WriteLine($"{e.Message}");
        Environment.Exit(1);
      }
    }
  }
}
