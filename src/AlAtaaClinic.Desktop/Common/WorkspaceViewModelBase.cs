using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Common.Exceptions;

namespace AlAtaaClinic.Desktop.Common;

public abstract class WorkspaceViewModelBase : ViewModelBase
{
    private readonly IExceptionHandler _exceptionHandler;

    protected IExceptionHandler ExceptionHandler => _exceptionHandler;

    protected WorkspaceViewModelBase(IExceptionHandler exceptionHandler)
    {
        _exceptionHandler = exceptionHandler;
    }

    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    protected async Task RunAsync(Func<Task> operation)
    {
        try
        {
            IsBusy = true;
            ClearMessages();
            await operation();
        }
        catch (ValidationFailedException exception)
        {
            ShowValidation(exception);
        }
        catch (Exception exception)
        {
            ShowError(exception);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ShowValidation(ValidationFailedException exception)
    {
        ValidationMessages.Clear();
        foreach (var error in exception.Errors)
        {
            ValidationMessages.Add(error.Message);
        }

        ErrorMessage = "Please review the highlighted information.";
    }

    private void ShowError(Exception exception)
    {
        var error = _exceptionHandler.Handle(exception);
        SetError(error.Message);
    }
}
