using System.Collections;
using System.Text;

namespace DIOS.Core;

public class SystemActivity
{
  private ILogger _logger;
  private StringBuilder _sb = new();
  private BitArray _systemActivity = new(16, false);
  private static readonly string[] SyncElements = { "SHEATH", "SAMPLE_A", "SAMPLE_B", "FLASH", "END_WELL", "VALVES", "X_MOTOR",
    "Y_MOTOR", "Z_MOTOR", "WASH PUMP", "PRESSURE", "WASHING", "FAULT", "ALIGN MOTOR", "MAIN VALVE", "SINGLE STEP" };

  public SystemActivity(ILogger logger)
  {
    _logger = logger;
  }

  internal void DecodeMessage(ushort message)
  {
    _sb.Clear()
      .Append("System Monitor: ");
    for (var i = 0; i < _systemActivity.Length; i++)
    {
      if ((message & (1 << i)) is not 0)
      {
        _systemActivity[i] = true;
        _sb.Append($"{SyncElements[i]}, ");
      }
      else
        _systemActivity[i] = false;
    }
    _sb.Remove(_sb.Length - 1, 2);
    _logger.Log(_sb.ToString());
  }

  internal bool ContainsWashing()
  {
    return _systemActivity[11];
  }

  public bool IsBusy()
  {
    for (var i = 0; i < _systemActivity.Length; i++)
    {
      if (_systemActivity[i]) //if any activity is going on - dismiss
        return true;
    }
    return false;
  }
}