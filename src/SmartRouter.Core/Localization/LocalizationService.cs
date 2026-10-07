using System.ComponentModel;

namespace SmartRouter.Core.Localization;

public class LocalizationService : INotifyPropertyChanged
{
    private static readonly Lazy<LocalizationService> _instance = new(() => new LocalizationService());
    public static LocalizationService Instance => _instance.Value;

    private string _currentLanguage = "fa";

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<string>? LanguageChanged;

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            var normalized = value.Equals("en", StringComparison.OrdinalIgnoreCase) ? "en" : "fa";
            if (_currentLanguage != normalized)
            {
                _currentLanguage = normalized;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null)); // notify all bindings
                LanguageChanged?.Invoke(_currentLanguage);
            }
        }
    }

    public bool IsPersian => _currentLanguage.Equals("fa", StringComparison.OrdinalIgnoreCase);

    public string this[string key] => Strings.Get(key, _currentLanguage);

    public string Get(string key) => Strings.Get(key, _currentLanguage);

    public string Format(string key, params object[] args)
    {
        var raw = Get(key);
        try
        {
            return string.Format(raw, args);
        }
        catch
        {
            return raw;
        }
    }
}
