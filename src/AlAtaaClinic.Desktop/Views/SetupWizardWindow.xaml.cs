using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.ViewModels;

namespace AlAtaaClinic.Desktop.Views;

public partial class SetupWizardWindow : System.Windows.Window
{
    public SetupWizardWindow(SetupWizardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += OnRequestClose;
    }

    public SetupWizardViewModel ViewModel => (SetupWizardViewModel)DataContext;

    private void OnRequestClose(object? sender, bool result)
    {
        DialogResult = result;
    }
}
