using DIOS.Core;

namespace DIOS.Application.FileIO;

public class Logger : ILogger
{
  private const string LOGFOLDERNAME = "SystemLogs";
  private const string LOGFILENAME = "EventLog";
  private readonly string _folder;
  private readonly FileStream _fStream;
  private readonly StreamWriter _streamWriter;
  private readonly string _logPath;
  private readonly Func<DateTime, string> _getTimeStamp;

  public Logger(string path, string timeString, Func<DateTime, string> getTimeStamp)
  {
    _folder = Path.Combine(path, LOGFOLDERNAME);
    CheckDir(_folder);

    _logPath = $"{_folder}\\{LOGFILENAME}_{timeString}.txt";

    _fStream = new FileStream(_logPath, FileMode.Create);
    _streamWriter = new StreamWriter(_fStream);
    _streamWriter.AutoFlush = true;
    _getTimeStamp = getTimeStamp;
  }

  ~Logger()
  {
    _streamWriter.Flush();
    _streamWriter.Dispose();
    _fStream.Dispose();
  }

  public void Log(string message)
  {
    var timeStamp = _getTimeStamp.Invoke(DateTime.Now);
    _streamWriter.WriteLine($"{timeStamp}\t{message}");
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
    catch (Exception e)
    {
      Console.Error.WriteLine($"Unable to create directory {folder}");
      Console.Error.WriteLine($"{e.Message}");
      Environment.Exit(1);
    }
  }
}