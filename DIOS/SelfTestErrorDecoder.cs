using DIOS.Core.SelfTests;

namespace Ei_Dimension
{
  public static class SelfTestErrorDecoder
  {
    public static string Decode(SelfTestData result)
    {
      string errorMessage = null;
      //decode error here
      if (result.StartupPressure != null)
      {
        var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Startup_Overpressure),
          Language.TranslationSource.Instance.CurrentCulture);
        if (errorMessage == null)
          errorMessage = "";
        errorMessage += $"{msg} [{result.StartupPressure}]\n";
      }

      if (result.Pressure != null)
      {
        var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_Overpressure),
          Language.TranslationSource.Instance.CurrentCulture);
        if (errorMessage == null)
          errorMessage = "";
        errorMessage += $"{msg} [{result.Pressure}]\n";
      }

      if (result.MotorX != null)
      {
        var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_MotorX_OutOfPos),
          Language.TranslationSource.Instance.CurrentCulture);
        if (errorMessage == null)
          errorMessage = "";
        errorMessage += $"{msg}\n";
      }

      if (result.MotorY != null)
      {
        var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_MotorY_OutOfPos),
          Language.TranslationSource.Instance.CurrentCulture);
        if (errorMessage == null)
          errorMessage = "";
        errorMessage += $"{msg}\n";
      }

      if (result.MotorZ != null)
      {
        var msg = Language.Resources.ResourceManager.GetString(nameof(Language.Resources.Messages_MotorZ_OutOfPos),
          Language.TranslationSource.Instance.CurrentCulture);
        if (errorMessage == null)
          errorMessage = "";
        errorMessage += $"{msg}\n";
      }

      return errorMessage;
    }
  }
}
