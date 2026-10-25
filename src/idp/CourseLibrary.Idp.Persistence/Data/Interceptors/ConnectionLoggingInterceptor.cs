using CourseLibrary.Platform.Persistence.Sql.Observability;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace CourseLibrary.Idp.Persistence.Data.Interceptors;


public sealed class ConnectionLoggingInterceptor(ILogger<ConnectionLoggingInterceptor> logger) : DbConnectionInterceptor
{
    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        using var activity = SqlActivitySource.Shared.StartActivity(
            SqlActivityTagConstants.ConnectionOpenOperation,
            System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag(
            SqlActivityTagConstants.DbSystem,
            SqlDatabaseSystem.Resolve(connection));
        activity?.SetTag(SqlActivityTagConstants.DbNamespace, connection.Database);
        SqlPersistenceLogs.ConnectionOpened(
            logger,
            connection.DataSource,
            connection.Database);

        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    public override async Task ConnectionClosedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData)
    {
        using var activity = SqlActivitySource.Shared.StartActivity(
            SqlActivityTagConstants.ConnectionCloseOperation,
            System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag(
            SqlActivityTagConstants.DbSystem,
            SqlDatabaseSystem.Resolve(connection));
        activity?.SetTag(SqlActivityTagConstants.DbNamespace, connection.Database);
        SqlPersistenceLogs.ConnectionClosed(
            logger,
            connection.DataSource,
            connection.Database);

        await base.ConnectionClosedAsync(connection, eventData);
    }
}
