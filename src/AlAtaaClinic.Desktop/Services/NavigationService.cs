using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.ViewModels;
using AlAtaaClinic.Desktop.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace AlAtaaClinic.Desktop.Services;

public sealed class NavigationService : INavigationService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private IServiceScope? _currentScope;

    public NavigationService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public event EventHandler<object>? Navigated;
    public object? CurrentViewModel { get; private set; }

    public void NavigateTo(NavigationKey key)
    {
        _currentScope?.Dispose();
        _currentScope = _scopeFactory.CreateScope();
        CurrentViewModel = Resolve(key, _currentScope.ServiceProvider);
        Navigated?.Invoke(this, CurrentViewModel);
        if (CurrentViewModel is WorkspaceViewModelBase workspace)
        {
            _ = workspace.InitializeAsync();
        }
    }

    public void Dispose()
    {
        _currentScope?.Dispose();
    }

    private static object Resolve(NavigationKey key, IServiceProvider serviceProvider)
    {
        return key switch
        {
            NavigationKey.Dashboard => serviceProvider.GetRequiredService<DashboardViewModel>(),
            NavigationKey.Branches => serviceProvider.GetRequiredService<BranchesViewModel>(),
            NavigationKey.Departments => serviceProvider.GetRequiredService<DepartmentsViewModel>(),
            NavigationKey.Patients => serviceProvider.GetRequiredService<PatientsViewModel>(),
            NavigationKey.StaffMembers => serviceProvider.GetRequiredService<StaffMembersViewModel>(),
            NavigationKey.Doctors => serviceProvider.GetRequiredService<DoctorsViewModel>(),
            NavigationKey.PatientHistory => serviceProvider.GetRequiredService<PatientHistoryViewModel>(),
            NavigationKey.Appointments => serviceProvider.GetRequiredService<AppointmentsViewModel>(),
            NavigationKey.Visits => serviceProvider.GetRequiredService<VisitsViewModel>(),
            NavigationKey.Billing => serviceProvider.GetRequiredService<BillingViewModel>(),
            NavigationKey.Charity => serviceProvider.GetRequiredService<CharityCasesViewModel>(),
            NavigationKey.Inventory => serviceProvider.GetRequiredService<InventoryViewModel>(),
            NavigationKey.Reports => serviceProvider.GetRequiredService<ReportsViewModel>(),
            NavigationKey.Settings => serviceProvider.GetRequiredService<SettingsViewModel>(),
            _ => serviceProvider.GetRequiredService<DashboardViewModel>()
        };
    }
}
