using System.Collections.ObjectModel;

namespace AlAtaaClinic.Desktop.Common;

public abstract class ViewModelBase : ObservableObject
{
    private bool _isBusy;
    private string? _statusMessage;
    private string? _errorMessage;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ObservableCollection<string> ValidationMessages { get; } = [];

    protected void SetError(string message)
    {
        ErrorMessage = message;
        StatusMessage = null;
    }

    protected void ClearMessages()
    {
        ErrorMessage = null;
        ValidationMessages.Clear();
    }
}
