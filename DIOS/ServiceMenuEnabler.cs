namespace Ei_Dimension;

internal class ServiceMenuEnabler
{
  private static int _timerTickcounter;

  public static void Update()
  {
    _timerTickcounter++;
    if (_timerTickcounter > 4)
    {
      ViewModels.MainViewModel.Instance.ServiceVisibilityCheck = 0;
      _timerTickcounter = 0;
    }
  }
}