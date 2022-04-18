using Ei_Dimension.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ei_Dimension
{
  public class Notification
  {
    public static void Show(string text, System.Windows.Media.Brush background = null, int fontSize = 40)
    {
      NotificationViewModel.Instance.Text[0] = text;
      if (background != null)
        NotificationViewModel.Instance.Background = background;
      NotificationViewModel.Instance.NotificationVisible = Visibility.Visible;
      NotificationViewModel.Instance.FontSize = fontSize;
    }

    public static void Show(string text, Action action1, string actionButton1Text, System.Windows.Media.Brush background = null, int fontSize = 40)
    {
      NotificationViewModel.Instance.Action1 = action1;
      NotificationViewModel.Instance.ActionButtonText[0] = actionButton1Text;
      NotificationViewModel.Instance.ButtonVisible[0] = Visibility.Visible;
      NotificationViewModel.Instance.ButtonVisible[2] = Visibility.Hidden;
      Show(text, background, fontSize);
    }

    public static void Show(string text, Action action1, string actionButton1Text, Action action2, string actionButton2Text, System.Windows.Media.Brush background = null, int fontSize = 40)
    {
      NotificationViewModel.Instance.Action2 = action2;
      NotificationViewModel.Instance.ActionButtonText[1] = actionButton2Text;
      NotificationViewModel.Instance.ButtonVisible[1] = Visibility.Visible;
      Show(text, action1, actionButton1Text, background, fontSize);
    }

    public static void ShowLocalized(string nameofLocalizationString, System.Windows.Media.Brush background = null, int fontSize = 40)
    {
      Show(Language.Resources.ResourceManager.GetString(nameofLocalizationString,
          Language.TranslationSource.Instance.CurrentCulture), background, fontSize);
    }

    public static void ShowLocalized(string nameofLocalizationString, Action action1, string nameofActionButton1Text, System.Windows.Media.Brush background = null, int fontSize = 40)
    {
      NotificationViewModel.Instance.Action1 = action1;
      NotificationViewModel.Instance.ActionButtonText[0] = Language.Resources.ResourceManager.GetString(nameofActionButton1Text,
          Language.TranslationSource.Instance.CurrentCulture);
      NotificationViewModel.Instance.ButtonVisible[0] = Visibility.Visible;
      NotificationViewModel.Instance.ButtonVisible[2] = Visibility.Hidden;
      ShowLocalized(nameofLocalizationString, background, fontSize);
    }

    public static void ShowLocalized(string nameofLocalizationString, Action action1, string nameofActionButton1Text, Action action2, string nameofActionButton2Text, System.Windows.Media.Brush background = null, int fontSize = 40)
    {
      NotificationViewModel.Instance.Action2 = action2;
      NotificationViewModel.Instance.ActionButtonText[1] = Language.Resources.ResourceManager.GetString(nameofActionButton2Text,
          Language.TranslationSource.Instance.CurrentCulture);
      NotificationViewModel.Instance.ButtonVisible[1] = Visibility.Visible;
      ShowLocalized(nameofLocalizationString, action1, nameofActionButton1Text, background, fontSize);
    }
  }
}
