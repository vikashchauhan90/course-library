using CourseLibrary.Idp.Infrastructure.Observability.Traces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace CourseLibrary.Idp.Infrastructure.Persistence.Interceptors;


public class ConnectionLoggingInterceptor(ILogger<ConnectionLoggingInterceptor> logger) : DbConnectionInterceptor, IInterceptor
{
    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Infrastructure.StartActivity(
            "Connection Opened",
            System.Diagnostics.ActivityKind.Internal);
        logger.LogDebug(
            "[Connection Opened] {DataSource}/{Database}",
            connection.DataSource,
            connection.Database);

        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    public override async Task ConnectionClosedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData)
    {
        using var activity = ActivitySources.Infrastructure.StartActivity(
            "Connection Closed",
            System.Diagnostics.ActivityKind.Internal);
        logger.LogDebug(
            "[Connection Closed] {DataSource}/{Database}",
            connection.DataSource,
            connection.Database);

        await base.ConnectionClosedAsync(connection, eventData);
    }
}
