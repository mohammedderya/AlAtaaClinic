using Microsoft.Data.SqlClient;

namespace AlAtaaClinic.Desktop.Services;

public static class DatabaseConnectionValidator
{
    public static (bool Success, string Message) ValidateConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return (false, "Database connection string is missing.\n\nOpen the setup wizard and configure SQL Server Express connection settings in appsettings.json.");
        }

        try
        {
            _ = new SqlConnectionStringBuilder(connectionString);
        }
        catch (Exception ex)
        {
            return (false, $"The connection string in appsettings.json is invalid.\n\nDetails: {ex.Message}");
        }

        return (true, string.Empty);
    }

    public static (bool Success, string Message) TestConnection(string connectionString)
    {
        var formatCheck = ValidateConnectionString(connectionString);
        if (!formatCheck.Success)
        {
            return formatCheck;
        }

        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            return (true, string.Empty);
        }
        catch (SqlException ex)
        {
            return (false, TranslateSqlException(ex));
        }
        catch (Exception ex)
        {
            return (false, $"Could not connect to the database.\n\nDetails: {ex.Message}");
        }
    }

    public static (bool Success, string Message) EnsureDatabaseExists(string connectionString)
    {
        var databaseName = Infrastructure.Persistence.SqlConnectionStringHelper.GetDatabaseName(connectionString);
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            return (false, "The connection string must include a database name.");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(databaseName, "^[a-zA-Z0-9_]+$"))
        {
            return (false, "Database name contains invalid characters. Use letters, numbers, and underscores only.");
        }

        try
        {
            var masterConnectionString = Infrastructure.Persistence.SqlConnectionStringHelper.BuildMasterConnectionString(connectionString);
            using var connection = new SqlConnection(masterConnectionString);
            connection.Open();

            using var existsCommand = connection.CreateCommand();
            existsCommand.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = @name";
            existsCommand.Parameters.AddWithValue("@name", databaseName);
            var exists = (int)existsCommand.ExecuteScalar()! > 0;
            if (exists)
            {
                return (true, string.Empty);
            }

            using var createCommand = connection.CreateCommand();
            createCommand.CommandText = $"CREATE DATABASE [{databaseName.Replace("]", "]]")}]";
            createCommand.ExecuteNonQuery();
            return (true, string.Empty);
        }
        catch (SqlException ex)
        {
            return (false, TranslateSqlException(ex));
        }
        catch (Exception ex)
        {
            return (false, $"Could not create the clinic database.\n\nDetails: {ex.Message}");
        }
    }

    private static string TranslateSqlException(SqlException ex)
    {
        return ex.Number switch
        {
            -2 or 258 => "The SQL Server connection timed out. Verify that SQL Server Express is installed, running, and reachable.",
            2 or 53 or 10061 => "SQL Server was not found. Install SQL Server Express or verify the server name in appsettings.json (default: .).",
            18456 => "SQL Server login failed. Check Windows Authentication or SQL username/password in appsettings.json.",
            4060 => "The clinic database does not exist and could not be opened. The application will attempt to create it on first launch.",
            40615 or 40501 => "SQL Server is temporarily unavailable. Wait a moment and try again.",
            _ => $"Database error ({ex.Number}): {ex.Message}"
        };
    }
}
