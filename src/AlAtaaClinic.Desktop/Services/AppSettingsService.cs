using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using AlAtaaClinic.Infrastructure.Persistence;

namespace AlAtaaClinic.Desktop.Services;

public sealed class AppSettingsService
{
    public const string ConnectionStringKey = "ConnectionStrings:ClinicDatabase";
    public const string ClinicNameKey = "Application:ClinicName";
    private const string SettingsFileName = "appsettings.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string SettingsPath => Path.Combine(AppContext.BaseDirectory, SettingsFileName);

    public string? ConnectionString => ReadString(ConnectionStringKey);

    public string ClinicName
    {
        get => ReadString(ClinicNameKey) ?? string.Empty;
        set => WriteString(ClinicNameKey, value);
    }

    public bool HasConnectionString =>
        !string.IsNullOrWhiteSpace(ConnectionString)
        && SqlConnectionStringHelper.IsValid(ConnectionString!);

    public void SaveConnectionString(string connectionString)
    {
        WriteString(ConnectionStringKey, connectionString);
    }

    public void SaveClinicSettings(string clinicName, string connectionString)
    {
        var root = LoadRoot();
        SetNestedValue(root, ConnectionStringKey, connectionString);
        SetNestedValue(root, ClinicNameKey, clinicName);
        SaveRoot(root);
    }

    public void MigrateFromLegacyTenantConfig(TenantService tenantService)
    {
        if (HasConnectionString || !tenantService.IsConfigured)
        {
            return;
        }

        SaveClinicSettings(tenantService.ClinicianName, tenantService.ConnectionString);
    }

    private string? ReadString(string key)
    {
        var root = LoadRoot();
        return GetNestedValue(root, key)?.GetValue<string>();
    }

    private void WriteString(string key, string value)
    {
        var root = LoadRoot();
        SetNestedValue(root, key, value);
        SaveRoot(root);
    }

    private JsonObject LoadRoot()
    {
        if (!File.Exists(SettingsPath))
        {
            return CreateDefaultSettings();
        }

        var json = File.ReadAllText(SettingsPath);
        return JsonNode.Parse(json)?.AsObject() ?? CreateDefaultSettings();
    }

    private static JsonObject CreateDefaultSettings()
    {
        return new JsonObject
        {
            ["ConnectionStrings"] = new JsonObject
            {
                ["ClinicDatabase"] = "Server=.;Database=AlAtaaClinic;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;"
            },
            ["Application"] = new JsonObject
            {
                ["DefaultTheme"] = "Light",
                ["ClinicName"] = "",
                ["Version"] = "1.0.0"
            },
            ["Logging"] = new JsonObject
            {
                ["LogLevel"] = new JsonObject
                {
                    ["Default"] = "Information",
                    ["Microsoft.EntityFrameworkCore"] = "Warning"
                },
                ["File"] = new JsonObject
                {
                    ["Path"] = "logs/alataa-.log",
                    ["RollingInterval"] = "Day",
                    ["RetainedFileCountLimit"] = 30
                }
            },
            ["Serilog"] = new JsonObject
            {
                ["MinimumLevel"] = new JsonObject
                {
                    ["Default"] = "Information",
                    ["Override"] = new JsonObject
                    {
                        ["Microsoft"] = "Warning",
                        ["Microsoft.EntityFrameworkCore"] = "Warning"
                    }
                }
            }
        };
    }

    private void SaveRoot(JsonObject root)
    {
        var directory = Path.GetDirectoryName(SettingsPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(SettingsPath, root.ToJsonString(JsonOptions));
    }

    private static JsonNode? GetNestedValue(JsonObject root, string key)
    {
        var parts = key.Split(':');
        JsonNode? current = root;
        foreach (var part in parts)
        {
            if (current is not JsonObject obj || !obj.TryGetPropertyValue(part, out current))
            {
                return null;
            }
        }

        return current;
    }

    private static void SetNestedValue(JsonObject root, string key, string value)
    {
        var parts = key.Split(':');
        var current = root;
        for (var i = 0; i < parts.Length - 1; i++)
        {
            if (!current.TryGetPropertyValue(parts[i], out var node) || node is not JsonObject child)
            {
                child = new JsonObject();
                current[parts[i]] = child;
            }

            current = child;
        }

        current[parts[^1]] = value;
    }
}
