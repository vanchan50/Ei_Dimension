using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Ei_Dimension.Language;

public class TranslationSource : INotifyPropertyChanged
{
  private static readonly ResourceManager resManager = Resources.ResourceManager;
  private static CultureInfo _currentCulture;

  public static TranslationSource Instance => _instance;

  public event PropertyChangedEventHandler PropertyChanged;

  public string this[string key] => resManager.GetString(key, _currentCulture);

  public CultureInfo CurrentCulture
  {
    get => _currentCulture;
    set
    {
      if (_currentCulture != value)
      {
        _currentCulture = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
      }
    }
  }

  private static readonly TranslationSource _instance = new();
}