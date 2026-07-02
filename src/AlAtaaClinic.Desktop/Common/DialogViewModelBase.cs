using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;

namespace AlAtaaClinic.Desktop.Common;

public abstract class DialogViewModelBase : WorkspaceViewModelBase
{
    protected DialogViewModelBase(IExceptionHandler exceptionHandler)
        : base(exceptionHandler)
    {
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        CancelCommand = new RelayCommand(_ => Close(false));
    }

    public event EventHandler<bool>? RequestClose;
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    protected abstract Task SaveAsync();

    protected void Close(bool dialogResult)
    {
        RequestClose?.Invoke(this, dialogResult);
    }
}
