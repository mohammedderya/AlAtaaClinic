using System.IO;
using System.Windows;
using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Application;
using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Desktop.ViewModels;
using AlAtaaClinic.Desktop.Views;
using AlAtaaClinic.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AlAtaaClinic.Desktop.Services;

public sealed class ApplicationStartupCoordinator
{
    private readonly AppSettingsService _appSettings = new();
    private readonly LicenseService _licenseService = new();
    private readonly TenantService _tenantService;

    public ApplicationStartupCoordinator()
    {
        _tenantService = new TenantService(_appSettings);
    }

    public async Task<IHost?> RunAsync()
    {
        LogStartup("Application startup initiated.");

        var sqlCheck = SqlServerAvailabilityChecker.Check();
        if (!sqlCheck.IsAvailable)
        {
            ShowError("SQL Server Required", sqlCheck.Message);
            return null;
        }

        _licenseService.LoadLicense();
        _tenantService.Load();

        if (!_appSettings.HasConnectionString && !_tenantService.IsConfigured)
        {
            if (!TryRunSetupWizard())
            {
                return null;
            }
        }
        else
        {
            _appSettings.MigrateFromLegacyTenantConfig(_tenantService);
            _tenantService.Load();
        }

        if (!ValidateLicense())
        {
            return null;
        }

        var connectionString = _appSettings.ConnectionString ?? _tenantService.ConnectionString;
        var validation = DatabaseConnectionValidator.ValidateConnectionString(connectionString);
        if (!validation.Success)
        {
            ShowError("Database Configuration", validation.Message);
            if (!TryRunSetupWizard())
            {
                return null;
            }

            connectionString = _appSettings.ConnectionString ?? _tenantService.ConnectionString;
        }

        var (connected, connectionError) = DatabaseConnectionValidator.TestConnection(connectionString!);
        if (!connected)
        {
            ShowWarning("Database Connection", $"{connectionError}\n\nThe setup wizard will open so you can fix the connection.");
            if (!TryRunSetupWizard())
            {
                return null;
            }

            connectionString = _appSettings.ConnectionString ?? _tenantService.ConnectionString;
            (connected, connectionError) = DatabaseConnectionValidator.TestConnection(connectionString!);
            if (!connected)
            {
                ShowError("Database Connection", connectionError);
                return null;
            }
        }

        var (databaseReady, databaseError) = DatabaseConnectionValidator.EnsureDatabaseExists(connectionString!);
        if (!databaseReady)
        {
            ShowError("Database Setup", databaseError);
            return null;
        }

        var host = CreateHostBuilder(connectionString!, _licenseService);
        await host.StartAsync().ConfigureAwait(true);
        LogStartup("Host started.");

        try
        {
            using var scope = host.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationStartupCoordinator>>();
            logger.LogInformation("Initializing database schema and seed data.");

            var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            LogStartup($"Database initialization failed: {ex}");
            ShowError(
                "Database Initialization",
                "The clinic database could not be initialized.\n\n" +
                "Verify SQL Server Express is running and your connection string in appsettings.json is correct.\n\n" +
                $"Details: {ex.Message}");
            await host.StopAsync().ConfigureAwait(true);
            host.Dispose();
            return null;
        }

        var systemLang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        TranslationService.Default.SetLanguage(systemLang == "ar" ? "ar" : "en");

        using (var scope = host.Services.CreateScope())
        {
            var loginWindow = scope.ServiceProvider.GetRequiredService<LoginWindow>();
            if (loginWindow.ShowDialog() != true)
            {
                await host.StopAsync().ConfigureAwait(true);
                host.Dispose();
                return null;
            }
        }

        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        System.Windows.Application.Current.MainWindow = mainWindow;
        mainWindow.Show();

        LogStartup("Application started successfully.");
        return host;
    }

    private bool ValidateLicense()
    {
        if (_licenseService.IsValid)
        {
            return true;
        }

        var message = _licenseService.IsExpired
            ? "License expired. Please contact support."
            : "Invalid license.";
        ShowWarning("License", message);
        return false;
    }

    private bool TryRunSetupWizard()
    {
        var wizardVm = new SetupWizardViewModel(
            _tenantService,
            _licenseService,
            new Application.Common.ExceptionHandling.ApplicationExceptionHandler());
        var wizard = new SetupWizardWindow(wizardVm);
        wizard.ShowDialog();

        if (!wizardVm.SetupCompleted)
        {
            return false;
        }

        _tenantService.Load();
        return _appSettings.HasConnectionString || _tenantService.IsConfigured;
    }

    private static IHost CreateHostBuilder(string connectionString, LicenseService licenseService)
    {
        var appDir = AppContext.BaseDirectory;
        var logsDir = Path.Combine(appDir, "logs");
        Directory.CreateDirectory(logsDir);

        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, builder) =>
            {
                builder.Sources.Clear();
                builder.SetBasePath(appDir)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            })
            .UseSerilog((context, _, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.File(
                        Path.Combine(logsDir, "alataa-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        shared: true);
            })
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(licenseService);
                services.AddSingleton<ILicenseProvider, LicenseProvider>();
                services.AddApplicationServices();
                services.AddInfrastructureServices(connectionString);
                services.AddDesktopServices();
                services.AddDesktopViewModels();
                services.AddDesktopViews();
            })
            .Build();
    }

    private static void ShowError(string title, string message) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    private static void ShowWarning(string title, string message) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    private static void LogStartup(string message)
    {
        try
        {
            var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logsDir);
            var path = Path.Combine(logsDir, "startup.log");
            File.AppendAllText(path, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
        }
        catch
        {
            // Startup logging must never block application launch.
        }
    }
}
