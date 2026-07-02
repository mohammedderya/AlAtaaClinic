namespace AlAtaaClinic.Desktop.Services;

public interface INavigationService
{
    event EventHandler<object>? Navigated;
    object? CurrentViewModel { get; }
    void NavigateTo(NavigationKey key);
}
