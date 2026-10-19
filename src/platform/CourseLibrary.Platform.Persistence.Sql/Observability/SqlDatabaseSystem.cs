using System.Data.Common;

namespace CourseLibrary.Platform.Persistence.Sql.Observability;

internal static class SqlDatabaseSystem
{
    public static string Resolve(DbConnection? connection)
        => Resolve(connection?.GetType().FullName);

    public static string Resolve(string? providerName)
    {
        if (providerName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
        {
            return SqlActivityTagConstants.PostgreSql;
        }

        if (providerName?.Contains("SqlClient", StringComparison.OrdinalIgnoreCase) == true)
        {
            return SqlActivityTagConstants.SqlServer;
        }

        return SqlActivityTagConstants.Other;
    }
}
