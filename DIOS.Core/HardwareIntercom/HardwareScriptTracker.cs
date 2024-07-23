namespace DIOS.Core.HardwareIntercom;

/// <summary>
/// Helper class to await end of scripts on the hardware side<br></br>
/// Point is to not overflow the 256 command buffer
/// </summary>
internal class HardwareScriptTracker
{
  private readonly ILogger _logger;
  private readonly Mutex _mutex = new();
  private readonly AutoResetEvent _threadBlock = new(false);

  public HardwareScriptTracker(ILogger logger)
  {
    _logger = logger;
  }
  /// <summary>
  /// Blocks the thread, which sent a script, until a signal is received.
  /// The signal comes from one of the "end script" messages, or in case of "Read*" scripts -
  /// from the issuance of the 0xDB command
  /// </summary>
  public void SendScriptAndLock(Action<byte> SendAction, byte command)
  {
#if DEBUG
    _logger.Log($"[SCRIPTTRACKER] Waiting for script: {command:X2}");
#endif
    _mutex.WaitOne();
#if DEBUG
    _logger.Log($"[SCRIPTTRACKER] Executing script: {command:X2}");
#endif
    SendAction(command);
    _threadBlock.WaitOne();
    _mutex.ReleaseMutex();
#if DEBUG
    _logger.Log($"[SCRIPTTRACKER] Script Released: {command:X2}");
#endif
  }

  /// <summary>
  /// IMPORTANT! sending 0xDB must call this. Design of the firmware implies that "Read*"
  /// scripts are not going to end themselves, but by the 0xDB command
  /// </summary>
  /// <param name="command"></param>
  public void SignalScriptEnd(byte command)
  {
    _threadBlock.Set();
#if DEBUG
    _logger.Log($"[SCRIPTTRACKER] Script Signaled: {command:X2}");
#endif
  }
}