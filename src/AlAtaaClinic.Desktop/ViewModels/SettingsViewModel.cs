using System.Windows.Input;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.Services;

namespace AlAtaaClinic.Desktop.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;
    private readonly TranslationService _translation;

    public SettingsViewModel(IThemeService themeService, TranslationService translation)
    {
        _themeService = themeService;
        _translation = translation;
        ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
        SwitchLanguageCommand = new RelayCommand(_ => SwitchLanguage());
    }

    public string ThemeName => _themeService.CurrentTheme;
    public string CurrentLanguage => _translation.CurrentLanguageName;
    public ICommand ToggleThemeCommand { get; }
    public ICommand SwitchLanguageCommand { get; }

    private void ToggleTheme()
    {
        _themeService.ToggleTheme();
        OnPropertyChanged(nameof(ThemeName));
    }

    private void SwitchLanguage()
    {
        var current = _translation.CurrentCulture.TwoLetterISOLanguageName;
        _translation.SetLanguage(current == "ar" ? "en" : "ar");
        OnPropertyChanged(nameof(CurrentLanguage));
    }
}
