using System.Windows;
using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Common.Exceptions;
using AlAtaaClinic.Application.Features.Auth;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.Services;

namespace AlAtaaClinic.Desktop.Views;

public partial class LoginWindow : Window
{
    private readonly CurrentUserSession _session;
    private readonly IAuthService _authService;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IThemeService _themeService;

    public LoginWindow(
        IAuthService authService,
        CurrentUserSession session,
        IThemeService themeService,
        IExceptionHandler exceptionHandler)
    {
        InitializeComponent();
        _authService = authService;
        _session = session;
        _themeService = themeService;
        _exceptionHandler = exceptionHandler;
        ToggleThemeCommand = new RelayCommand(_ => _themeService.ToggleTheme());
        DataContext = this;
    }

    public ICommand ToggleThemeCommand { get; }

    private async void SignIn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var username = UsernameInput.Text.Trim();
            var password = PasswordInput.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorText.Text = "Please enter username and password.";
                return;
            }

            var result = await _authService.LoginAsync(new LoginCommand(username, password));

            if (result.MustChangePassword)
            {
                var changePasswordWindow = new ChangePasswordWindow(
                    _authService,
                    _exceptionHandler,
                    result.UserAccountId,
                    password)
                {
                    Owner = this
                };

                if (changePasswordWindow.ShowDialog() != true)
                {
                    ErrorText.Text = "Password change is required before you can sign in.";
                    return;
                }
            }

            _session.SignIn(result.UserAccountId, result.Username, result.FullNameArabic, result.Permissions);
            DialogResult = true;
        }
        catch (UnauthorizedAppException)
        {
            ErrorText.Text = "Invalid username or password.";
        }
        catch (Exception ex)
        {
            ErrorText.Text = _exceptionHandler.Handle(ex).Message;
        }
    }
}
