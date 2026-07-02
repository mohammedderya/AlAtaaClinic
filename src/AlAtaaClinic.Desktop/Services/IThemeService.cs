namespace AlAtaaClinic.Desktop.Services;

public interface IThemeService
{
    string CurrentTheme { get; }
    void ApplyTheme(string themeName);
    void ToggleTheme();
}
