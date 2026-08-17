using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace CourseLibrary.Idp.Infrastructure.Persistence.Interceptors;


public class ConnectionLoggingInterceptor(ILogger<ConnectionLoggingInterceptor> logger) : DbConnectionInterceptor
{
    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
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
        logger.LogDebug(
            "[Connection Closed] {DataSource}/{Database}",
            connection.DataSource,
            connection.Database);

        await base.ConnectionClosedAsync(connection, eventData);
    }
}
