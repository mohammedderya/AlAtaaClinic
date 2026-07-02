using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Features.Auth;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.Services;

namespace AlAtaaClinic.Desktop.ViewModels;

public sealed class LoginViewModel : WorkspaceViewModelBase
{
    private readonly IAuthService _authService;
    private readonly CurrentUserSession _session;
    private readonly IThemeService _themeService;
    private string _username = string.Empty;
    private string _password = string.Empty;

    public LoginViewModel(
        IAuthService authService,
        CurrentUserSession session,
        IThemeService themeService,
        IExceptionHandler exceptionHandler)
        : base(exceptionHandler)
    {
        _authService = authService;
        _session = session;
        _themeService = themeService;
        LoginCommand = new AsyncRelayCommand(_ => LoginAsync());
        ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
    }

    public event EventHandler? LoginSucceeded;

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand ToggleThemeCommand { get; }

    private Task LoginAsync()
    {
        return RunAsync(async () =>
        {
            var result = await _authService.LoginAsync(new AlAtaaClinic.Application.Features.Auth.LoginCommand(Username, Password));
            _session.SignIn(result.UserAccountId, result.Username, result.FullNameArabic, result.Permissions);
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        });
    }

    private void ToggleTheme()
    {
        _themeService.ToggleTheme();
    }
}
