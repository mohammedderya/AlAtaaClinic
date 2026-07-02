using System.Windows;
using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Common.Exceptions;
using AlAtaaClinic.Application.Features.Auth;
using AlAtaaClinic.Desktop.Common;

namespace AlAtaaClinic.Desktop.Views;

public partial class ChangePasswordWindow : Window
{
    private readonly IAuthService _authService;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly long _userAccountId;
    private readonly string _currentPassword;

    public ChangePasswordWindow(
        IAuthService authService,
        IExceptionHandler exceptionHandler,
        long userAccountId,
        string currentPassword)
    {
        InitializeComponent();
        _authService = authService;
        _exceptionHandler = exceptionHandler;
        _userAccountId = userAccountId;
        _currentPassword = currentPassword;
        ChangePasswordCommand = new AsyncRelayCommand(_ => ChangePasswordAsync());
        DataContext = this;
    }

    public ICommand ChangePasswordCommand { get; }

    private async Task ChangePasswordAsync()
    {
        ErrorText.Text = string.Empty;
        var newPassword = NewPasswordInput.Password;
        var confirmPassword = ConfirmPasswordInput.Password;

        if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            ErrorText.Text = "Please enter and confirm your new password.";
            return;
        }

        if (newPassword != confirmPassword)
        {
            ErrorText.Text = "Passwords do not match.";
            return;
        }

        try
        {
            await _authService.ChangePasswordAsync(new ChangePasswordCommand(
                _userAccountId,
                _currentPassword,
                newPassword));

            DialogResult = true;
        }
        catch (ValidationFailedException validation)
        {
            ErrorText.Text = string.Join(" ", validation.Errors.Select(error => error.Message));
        }
        catch (UnauthorizedAppException)
        {
            ErrorText.Text = "Current password is incorrect.";
        }
        catch (Exception ex)
        {
            ErrorText.Text = _exceptionHandler.Handle(ex).Message;
        }
    }
}
