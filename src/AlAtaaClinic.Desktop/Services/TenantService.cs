using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace AlAtaaClinic.Desktop.Services;

public sealed class TenantService
{
    public const string ConfigFileName = "tenant.config";
    private static readonly Regex SafeDatabaseNamePattern = new("^[a-zA-Z0-9_]+$", RegexOptions.Compiled);
    private readonly AppSettingsService _appSettings;

    public TenantService(AppSettingsService appSettings)
    {
        _appSettings = appSettings;
    }

    public string ServerName { get; set; } = ".";
    public string DatabaseName { get; set; } = "AlAtaaClinic";
    public string ClinicianName { get; set; } = string.Empty;
    public bool UseWindowsAuth { get; set; } = true;
    public string SqlUsername { get; set; } = string.Empty;
    public string SqlPassword { get; set; } = string.Empty;
    public bool IsConfigured { get; private set; }

    public string ConnectionString => BuildConnectionString();

    public void Load()
    {
        try
        {
            ClinicianName = _appSettings.ClinicName;
            var connectionString = _appSettings.ConnectionString;
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                ApplyConnectionString(connectionString);
                IsConfigured = true;
                return;
            }

            LoadLegacyTenantConfig();
        }
        catch
        {
            IsConfigured = false;
        }
    }

    public void NormalizeConnectionSettings()
    {
        if (UseWindowsAuth || ServerName.Contains(','))
        {
            return;
        }

        if (ServerName is "localhost" or "127.0.0.1" or ".")
        {
            ServerName = "localhost,1433";
        }
    }

    public void Save()
    {
        NormalizeConnectionSettings();
        _appSettings.SaveClinicSettings(ClinicianName, ConnectionString);
        IsConfigured = true;
    }

    public bool TestConnection() => DatabaseConnectionValidator.TestConnection(ConnectionString).Success;

    public (bool Success, string? Error) TestConnectionWithDetails()
    {
        var result = DatabaseConnectionValidator.TestConnection(ConnectionString);
        return (result.Success, result.Success ? null : result.Message);
    }

    public bool DatabaseExists()
    {
        try
        {
            var masterConnectionString = BuildMasterConnectionString();
            using var connection = new SqlConnection(masterConnectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = @name";
            cmd.Parameters.AddWithValue("@name", DatabaseName);
            return (int)cmd.ExecuteScalar()! > 0;
        }
        catch
        {
            return false;
        }
    }

    public (bool Success, string? Error) EnsureDatabaseExists()
    {
        if (!SafeDatabaseNamePattern.IsMatch(DatabaseName))
        {
            return (false, "Database name contains invalid characters.");
        }

        var result = DatabaseConnectionValidator.EnsureDatabaseExists(ConnectionString);
        return (result.Success, result.Success ? null : result.Message);
    }

    private void LoadLegacyTenantConfig()
    {
        var path = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
        if (!File.Exists(path))
        {
            IsConfigured = false;
            return;
        }

        var lines = File.ReadAllLines(path);
        foreach (var line in lines)
        {
            var parts = line.Split(['='], 2);
            if (parts.Length != 2)
            {
                continue;
            }

            switch (parts[0].Trim())
            {
                case "Server": ServerName = parts[1].Trim(); break;
                case "Database": DatabaseName = parts[1].Trim(); break;
                case "Clinic": ClinicianName = parts[1].Trim(); break;
                case "UseWindowsAuth": UseWindowsAuth = parts[1].Trim().Equals("true", StringComparison.OrdinalIgnoreCase); break;
                case "SqlUsername": SqlUsername = parts[1].Trim(); break;
                case "SqlPassword": SqlPassword = parts[1].Trim(); break;
            }
        }

        NormalizeConnectionSettings();
        IsConfigured = !string.IsNullOrWhiteSpace(ServerName)
            && !string.IsNullOrWhiteSpace(DatabaseName)
            && (UseWindowsAuth || (!string.IsNullOrWhiteSpace(SqlUsername) && !string.IsNullOrWhiteSpace(SqlPassword)));
    }

    private void ApplyConnectionString(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        ServerName = builder.DataSource;
        DatabaseName = builder.InitialCatalog;
        UseWindowsAuth = builder.IntegratedSecurity;
        SqlUsername = builder.UserID ?? string.Empty;
        SqlPassword = builder.Password ?? string.Empty;
    }

    private string BuildConnectionString()
    {
        return UseWindowsAuth
            ? $"Server={ServerName};Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;Encrypt=True;"
            : $"Server={ServerName};Database={DatabaseName};User ID={SqlUsername};Password={SqlPassword};TrustServerCertificate=True;Encrypt=True;";
    }

    private string BuildMasterConnectionString()
    {
        return UseWindowsAuth
            ? $"Server={ServerName};Database=master;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;"
            : $"Server={ServerName};Database=master;User ID={SqlUsername};Password={SqlPassword};TrustServerCertificate=True;Encrypt=False;";
    }
}
