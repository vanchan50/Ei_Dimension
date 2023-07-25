using System.Collections;

namespace DIOS.Core;

public class SystemActivity
{
  private BitArray _systemActivity = new BitArray(16, false);
  private static readonly string[] SyncElements = { "SHEATH", "SAMPLE_A", "SAMPLE_B", "FLASH", "END_WELL", "VALVES", "X_MOTOR",
    "Y_MOTOR", "Z_MOTOR", "PROXIMITY", "PRESSURE", "WASHING", "FAULT", "ALIGN MOTOR", "MAIN VALVE", "SINGLE STEP" };

  internal void DecodeMessage(ushort message)
  {
    for (var i = 0; i < _systemActivity.Length; i++)
    {
      if ((message & (1 << i)) != 0)
        _systemActivity[i] = true;
      else
        _systemActivity[i] = false;
    }
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