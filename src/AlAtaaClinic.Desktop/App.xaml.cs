using System.IO;
using System.Windows;
using AlAtaaClinic.Desktop.Services;
using Microsoft.Extensions.Hosting;

namespace AlAtaaClinic.Desktop;

public partial class App : System.Windows.Application
{
    private IHost? _host;
    private static string CrashLogPath => Path.Combine(AppContext.BaseDirectory, "logs", "crash.log");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (_, args) =>
        {
            WriteCrashLog($"Dispatcher: {args.Exception}");
            MessageBox.Show(
                args.Exception.Message,
                "Unexpected Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };

        _ = RunAsync();
    }

    private async Task RunAsync()
    {
        try
        {
            var coordinator = new ApplicationStartupCoordinator();
            _host = await coordinator.RunAsync().ConfigureAwait(true);
            if (_host is null)
            {
                Shutdown();
            }
        }
        catch (Exception ex) when (ex is not HostAbortedException)
        {
            WriteCrashLog($"RunAsync: {ex}");
            MessageBox.Show(
                $"The application could not start.\n\n{ex.Message}",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync().ConfigureAwait(true);
            _host.Dispose();
        }

        base.OnExit(e);
    }

    private static void WriteCrashLog(string content)
    {
        try
        {
            var logsDir = Path.GetDirectoryName(CrashLogPath);
            if (!string.IsNullOrEmpty(logsDir))
            {
                Directory.CreateDirectory(logsDir);
            }

            File.WriteAllText(CrashLogPath, content);
        }
        catch
        {
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "crash.log"), content);
        }
    }
}
