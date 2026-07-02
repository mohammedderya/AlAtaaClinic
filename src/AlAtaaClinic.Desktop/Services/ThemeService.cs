using System.Windows;

namespace AlAtaaClinic.Desktop.Services;

public sealed class ThemeService : IThemeService
{
    private const string LightTheme = "Light";
    private const string DarkTheme = "Dark";

    public string CurrentTheme { get; private set; } = LightTheme;

    public void ApplyTheme(string themeName)
    {
        var normalizedTheme = IsDark(themeName) ? DarkTheme : LightTheme;
        ReplaceThemeDictionary(normalizedTheme);
        CurrentTheme = normalizedTheme;
    }

    public void ToggleTheme()
    {
        ApplyTheme(CurrentTheme == LightTheme ? DarkTheme : LightTheme);
    }

    private static bool IsDark(string themeName)
    {
        return string.Equals(themeName, DarkTheme, StringComparison.OrdinalIgnoreCase);
    }

    private static void ReplaceThemeDictionary(string themeName)
    {
        var dictionaries = WpfApplication.Current.Resources.MergedDictionaries;
        var oldTheme = dictionaries.FirstOrDefault(IsThemeDictionary);
        if (oldTheme is not null)
        {
            dictionaries.Remove(oldTheme);
        }

        dictionaries.Insert(0, new ResourceDictionary
        {
            Source = new Uri($"/Themes/{themeName}Theme.xaml", UriKind.Relative)
        });
    }

    private static bool IsThemeDictionary(ResourceDictionary dictionary)
    {
        return dictionary.Source?.OriginalString.Contains("Theme.xaml", StringComparison.OrdinalIgnoreCase) == true;
    }
}
