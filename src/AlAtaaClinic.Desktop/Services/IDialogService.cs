namespace AlAtaaClinic.Desktop.Services;

public interface IDialogService
{
    void ShowInfo(string title, string message);
    void ShowError(string title, string message);
    bool Confirm(string title, string message);
    bool ShowEditor(string title, object viewModel);
}
