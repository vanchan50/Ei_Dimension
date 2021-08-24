using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Ei_Dimension.Language
{
  public class TranslationSource : INotifyPropertyChanged
  {
    private readonly ResourceManager resManager = Resources.ResourceManager;
    private CultureInfo _currentCulture = null;
    public static TranslationSource Instance { get; } = new TranslationSource();

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
          var @event = PropertyChanged;
          if (@event != null)
          {
            @event.Invoke(this, new PropertyChangedEventArgs(string.Empty));
          }
        }
      }
    }
  }
}
