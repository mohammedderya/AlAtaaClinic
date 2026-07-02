using System.ServiceProcess;
using Microsoft.Win32;

namespace AlAtaaClinic.Desktop.Services;

public static class SqlServerAvailabilityChecker
{
    private static readonly string[] ExpressServiceNames =
    [
        "MSSQLSERVER",
        "MSSQL$SQLEXPRESS",
        "MSSQL$SQLEXPRESS01",
        "MSSQL$MSSQLSERVER"
    ];

    public static SqlServerCheckResult Check()
    {
        var instances = GetInstalledInstances();
        var runningServices = GetRunningSqlServices();

        if (instances.Count == 0 && runningServices.Count == 0)
        {
            return SqlServerCheckResult.NotInstalled(
                "SQL Server Express was not detected on this computer.\n\n" +
                "Please install SQL Server Express before running AlAtaa Clinic:\n" +
                "1. Download SQL Server Express from Microsoft.\n" +
                "2. Run the installer and choose the default instance (SQLEXPRESS).\n" +
                "3. Enable Mixed Mode or Windows Authentication as needed.\n" +
                "4. Restart this application after installation completes.");
        }

        if (runningServices.Count == 0)
        {
            return SqlServerCheckResult.NotRunning(
                "SQL Server is installed but the database service is not running.\n\n" +
                "Open Services (services.msc) and start the SQL Server service, " +
                "then launch AlAtaa Clinic again.");
        }

        return SqlServerCheckResult.Available(instances, runningServices);
    }

    private static IReadOnlyList<string> GetInstalledInstances()
    {
        var instances = new List<string>();
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL");
            if (key is null)
            {
                return instances;
            }

            foreach (var name in key.GetValueNames())
            {
                instances.Add(name);
            }
        }
        catch
        {
            // Registry access may be restricted; rely on service detection below.
        }

        return instances;
    }

    private static IReadOnlyList<string> GetRunningSqlServices()
    {
        var running = new List<string>();
        foreach (var serviceName in ExpressServiceNames)
        {
            try
            {
                using var service = new ServiceController(serviceName);
                if (service.Status == ServiceControllerStatus.Running)
                {
                    running.Add(serviceName);
                }
            }
            catch
            {
                // Service does not exist on this machine.
            }
        }

        return running;
    }
}

public sealed class SqlServerCheckResult
{
    public bool IsAvailable { get; init; }
    public bool IsInstalled { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<string> Instances { get; init; } = [];
    public IReadOnlyList<string> RunningServices { get; init; } = [];

    public static SqlServerCheckResult Available(
        IReadOnlyList<string> instances,
        IReadOnlyList<string> runningServices) =>
        new()
        {
            IsAvailable = true,
            IsInstalled = true,
            Instances = instances,
            RunningServices = runningServices
        };

    public static SqlServerCheckResult NotInstalled(string message) =>
        new() { IsAvailable = false, IsInstalled = false, Message = message };

    public static SqlServerCheckResult NotRunning(string message) =>
        new() { IsAvailable = false, IsInstalled = true, Message = message };
}
