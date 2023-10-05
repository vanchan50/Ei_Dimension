namespace DIOS.Core.HardwareIntercom
{
  /// <summary>
  /// Helper class to await end of scripts on the hardware side<br></br>
  /// Point is to not overflow the 256 command buffer
  /// </summary>
  internal class HardwareScriptTracker
  {
    public bool Wait
    {
      get
      {
        return _wait;
      }
      set
      {
        if (value && _wait)
          throw new AccessViolationException("Waiting for two scripts simultaneously");
        _wait = value;
      }
    }
    private bool _wait;
    private readonly object _lock = new();

    /// <summary>
    /// If Input command is not a script - does nothing<br></br>
    /// Else it blocks the calling thread
    /// </summary>
    public void WaitForScriptEndOrDoNothing()
    {
      if (_wait)
      {
        lock (_lock)
        {
          Monitor.Wait(_lock);
        }
        _wait = false;
      }
    }

    public void SignalScriptEnd()
    {
      lock (_lock)
      {
        Monitor.PulseAll(_lock);
      }
    }
  }
}
