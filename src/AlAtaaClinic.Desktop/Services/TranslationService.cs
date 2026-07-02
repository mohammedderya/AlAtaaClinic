using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace AlAtaaClinic.Desktop.Services;

public sealed class TranslationService : INotifyPropertyChanged
{
    private Dictionary<string, string> _strings = [];
    private CultureInfo _culture = new("en");

    public static TranslationService Default { get; } = new();

    public string this[string key] => _strings.TryGetValue(key, out var value) ? value : key;

    public FlowDirection FlowDirection =>
        _culture.TwoLetterISOLanguageName == "ar" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    public CultureInfo CurrentCulture => _culture;

    public string CurrentLanguageName =>
        _culture.TwoLetterISOLanguageName == "ar" ? "العربية" : "English";

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetLanguage(string cultureName)
    {
        _culture = new CultureInfo(cultureName);
        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "Resources", $"strings.{cultureName}.json");
        _strings = LoadStrings(path);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FlowDirection)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguageName)));
    }

    public string Format(string key, params object?[] args)
    {
        var template = this[key];
        return args.Length > 0 ? string.Format(template, args) : template;
    }

    private static Dictionary<string, string> LoadStrings(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
