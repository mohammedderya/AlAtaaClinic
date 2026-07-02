using System.Windows;
using AlAtaaClinic.Desktop.ViewModels;

namespace AlAtaaClinic.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
