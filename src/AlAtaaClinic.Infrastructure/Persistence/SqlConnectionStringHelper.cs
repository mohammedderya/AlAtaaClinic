using Microsoft.Data.SqlClient;

namespace AlAtaaClinic.Infrastructure.Persistence;

public static class SqlConnectionStringHelper
{
    public static string BuildMasterConnectionString(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master",
            Encrypt = false
        };
        return builder.ConnectionString;
    }

    public static string? GetDatabaseName(string connectionString)
    {
        try
        {
            return new SqlConnectionStringBuilder(connectionString).InitialCatalog;
        }
        catch
        {
            return null;
        }
    }

    public static bool IsValidFormat(string connectionString) => IsValid(connectionString);

    public static bool IsValid(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return !string.IsNullOrWhiteSpace(builder.DataSource)
                && !string.IsNullOrWhiteSpace(builder.InitialCatalog);
        }
        catch
        {
            return false;
        }
    }
}
