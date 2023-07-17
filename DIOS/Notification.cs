using Ei_Dimension.ViewModels;
using System;
using System.Windows;
using System.Windows.Media;

namespace Ei_Dimension
{
  public static class Notification
  {
    private static Brush _errorBackground  = new SolidColorBrush(Color.FromRgb(0xFF, 0x67, 0x00));
    
    private static Brush _successBackground = new SolidColorBrush(Color.FromRgb(0x4B, 0xB5, 0x43));
    public static void Show(string text, Brush background = null, int fontSize = 40)
    {
      NotificationViewModel.Instance.Text[0] = text;
      if (background != null)
        NotificationViewModel.Instance.Background = background;
      NotificationViewModel.Instance.NotificationVisible = Visibility.Visible;
      NotificationViewModel.Instance.FontSize = fontSize;
    }

    public static void Show(string text, Action action1, string actionButton1Text, Brush background = null, int fontSize = 40)
    {
      NotificationViewModel.Instance.Action1 = action1;
      NotificationViewModel.Instance.ActionButtonText[0] = actionButton1Text;
      NotificationViewModel.Instance.ButtonVisible[0] = Visibility.Visible;
      NotificationViewModel.Instance.ButtonVisible[2] = Visibility.Hidden;
      Show(text, background, fontSize);
    }

    public static void Show(string text, Action action1, string actionButton1Text, Action action2, string actionButton2Text, Brush background = null, int fontSize = 40)
    {
      NotificationViewModel.Instance.Action2 = action2;
      NotificationViewModel.Instance.ActionButtonText[1] = actionButton2Text;
      NotificationViewModel.Instance.ButtonVisible[1] = Visibility.Visible;
      Show(text, action1, actionButton1Text, background, fontSize);
    }

    public static void ShowLocalized(string nameofLocalizationString, Brush background = null, int fontSize = 40)
    {
      Show(Language.Resources.ResourceManager.GetString(nameofLocalizationString,
          Language.TranslationSource.Instance.CurrentCulture), background, fontSize);
    }

    public static void ShowLocalized(string nameofLocalizationString, Action action1, string nameofActionButton1Text, Brush background = null, int fontSize = 40)
    {
      NotificationViewModel.Instance.Action1 = action1;
      NotificationViewModel.Instance.ActionButtonText[0] = Language.Resources.ResourceManager.GetString(nameofActionButton1Text,
          Language.TranslationSource.Instance.CurrentCulture);
      NotificationViewModel.Instance.ButtonVisible[0] = Visibility.Visible;
      NotificationViewModel.Instance.ButtonVisible[2] = Visibility.Hidden;
      ShowLocalized(nameofLocalizationString, background, fontSize);
    }

    public static void ShowLocalized(string nameofLocalizationString, Action action1, string nameofActionButton1Text, Action action2, string nameofActionButton2Text, Brush background = null, int fontSize = 40)
    {
      NotificationViewModel.Instance.Action2 = action2;
      NotificationViewModel.Instance.ActionButtonText[1] = Language.Resources.ResourceManager.GetString(nameofActionButton2Text,
          Language.TranslationSource.Instance.CurrentCulture);
      NotificationViewModel.Instance.ButtonVisible[1] = Visibility.Visible;
      ShowLocalized(nameofLocalizationString, action1, nameofActionButton1Text, background, fontSize);
    }

    public static void ShowError(string text, int fontSize = 40)
    {
      Show(text, _errorBackground, fontSize);
    }

    public static void ShowLocalizedError(string nameofLocalizationString, int fontSize = 40)
    {
      Show(Language.Resources.ResourceManager.GetString(nameofLocalizationString,
        Language.TranslationSource.Instance.CurrentCulture), _errorBackground, fontSize);
    }

    public static void ShowLocalizedSuccess(string nameofLocalizationString, int fontSize = 40)
    {
      Show(Language.Resources.ResourceManager.GetString(nameofLocalizationString,
        Language.TranslationSource.Instance.CurrentCulture), _successBackground, fontSize);
    }
  }
}
