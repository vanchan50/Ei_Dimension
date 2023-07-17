namespace DIOS.Core.SelfTests
{
  public class SelfTestData
  {
    internal bool ResultReady
    {
      get
      {
        return _pressure != null
               && _startupPressure != null
               && _motorX != null
               && _motorY != null
               && _motorZ != null;
      }
    }

    public float? StartupPressure
    {
      get
      {
        if (_startupPressure > 1.0)
          return _startupPressure;
        return null;
      }
      private set
      {
        _startupPressure = value;
        _logger.Log($"Startup pressure: {value}");
      }
    }

    public float? Pressure
    {
      get
      {
        if (_pressure > _device.MaxPressure) 
          return _pressure;
        return null;
      }
      private set
      {
        _pressure = value;
        _logger.Log($"Pressure: {value}");
      }
    }

    public float? MotorX {
      get
      {
        if (_motorX < 450 || _motorX > 505)
          return _motorX;
        return null;
      }
      set
      {
        _motorX = value;
        _logger.Log($"Motor X: {value}");
      }
    }

    public float? MotorY
    {
      get
      {
        if (_motorY < 450 || _motorY > 505)
          return _motorY;
        return null;
      }
      set
      {
        _motorY = value;
        _logger.Log($"Motor Y: {value}");
      }
    }

    public float? MotorZ
    {
      get
      {
        if (_motorZ < 16 || _motorZ > 21)
          return _motorZ;
        return null;
      }
      set
      {
        _motorZ = value;
        _logger.Log($"Motor Z: {value}");
      }
    }

    private float? _motorX;
    private float? _motorY;
    private float? _motorZ;
    private float? _pressure;
    private float? _startupPressure;
    private bool _startup = true;
    private Device _device;
    private ILogger _logger;

    internal SelfTestData(Device device, ILogger logger)
    {
      _device = device;
      _logger = logger;
    }

    internal void SetPressure(float pressure)
    {
      if (_startup)
      {
        StartupPressure = pressure;
        _startup = false;
        return;
      }
      Pressure = pressure;
    }
    
  }
}
