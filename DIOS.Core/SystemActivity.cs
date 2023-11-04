using System.Collections;
using System.Text;

namespace DIOS.Core;

public class SystemActivity
{
  private StringBuilder _sb = new();
  private BitArray _systemActivity = new(16, false);
  private static readonly string[] SyncElements = { "SHEATH", "SAMPLE_A", "SAMPLE_B", "FLASH", "END_WELL", "VALVES", "X_MOTOR",
    "Y_MOTOR", "Z_MOTOR", "WASH PUMP", "PRESSURE", "WASHING", "FAULT", "ALIGN MOTOR", "MAIN VALVE", "SINGLE STEP" };

  public string DecodeMessage(ushort message)
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
    _sb.Remove(_sb.Length - 2, 2);
    return _sb.ToString();
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