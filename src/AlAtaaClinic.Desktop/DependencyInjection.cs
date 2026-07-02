using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Desktop.Services;
using AlAtaaClinic.Desktop.ViewModels;
using AlAtaaClinic.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AlAtaaClinic.Desktop;

public static class DependencyInjection
{
    public static IServiceCollection AddDesktopServices(this IServiceCollection services)
    {
        services.AddSingleton<CurrentUserSession>();
        services.AddSingleton<ICurrentUserService>(provider => provider.GetRequiredService<CurrentUserSession>());
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IPrintService, PrintService>();
        services.AddSingleton(TranslationService.Default);
        services.AddSingleton<INavigationService, NavigationService>();
        return services;
    }

    public static IServiceCollection AddDesktopViewModels(this IServiceCollection services)
    {
        services.AddTransient<LoginViewModel>();
        services.AddSingleton<ShellViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<BranchesViewModel>();
        services.AddTransient<DepartmentsViewModel>();
        services.AddTransient<PatientsViewModel>();
        services.AddTransient<PatientHistoryViewModel>();
        services.AddTransient<AppointmentsViewModel>();
        services.AddTransient<VisitsViewModel>();
        services.AddTransient<BillingViewModel>();
        services.AddTransient<CharityCasesViewModel>();
        services.AddTransient<InventoryViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<StaffMembersViewModel>();
        services.AddTransient<DoctorsViewModel>();
        services.AddTransient<SettingsViewModel>();
        return services;
    }

    public static IServiceCollection AddDesktopViews(this IServiceCollection services)
    {
        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();
        return services;
    }
}
