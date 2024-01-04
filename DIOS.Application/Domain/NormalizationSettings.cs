namespace DIOS.Application.Domain;

public class NormalizationSettings
{
  public bool IsEnabled { get; private set; } = true;
  private bool _cache;

  internal NormalizationSettings()
  {
      
  }

  public void Enable()
  {
    IsEnabled = true;
  }

  public void Disable()
  {
    IsEnabled = false;
  }

  internal void SuspendForTheRun()
  {
    _cache = IsEnabled;
    IsEnabled = false;
  }

  public void Restore()
  {
    IsEnabled = _cache;
  }
}