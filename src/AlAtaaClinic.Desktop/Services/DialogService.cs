using System.Windows;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.Views.Dialogs;

namespace AlAtaaClinic.Desktop.Services;

public sealed class DialogService : IDialogService
{
    public void ShowInfo(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public bool Confirm(string title, string message)
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    public bool ShowEditor(string title, object viewModel)
    {
        var window = new EditorDialogWindow
        {
            Title = title,
            DataContext = viewModel,
            Owner = WpfApplication.Current.MainWindow
        };

        if (viewModel is DialogViewModelBase dialogViewModel)
        {
            dialogViewModel.RequestClose += (_, result) => window.DialogResult = result;
        }

        return window.ShowDialog() == true;
    }
}
