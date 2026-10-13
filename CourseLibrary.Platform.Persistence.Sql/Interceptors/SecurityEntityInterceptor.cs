using CourseLibrary.Core.Domain.Abstractions.Auditing;
using CourseLibrary.Core.Domain.Abstractions.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Platform.Persistence.Sql.Interceptors;

public class SecurityEntityInterceptor(ILogger<SecurityEntityInterceptor> logger) : SaveChangesInterceptor, IInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Infrastructure.StartActivity(
            "SecurityEntityInterceptor.SavingChangesAsync",
            System.Diagnostics.ActivityKind.Internal);

        activity?.SetTag("dbContext", eventData.Context?.GetType().Name);
        var context = eventData.Context;
        if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        foreach (var entry in context.ChangeTracker.Entries<IConcurrency>())
        {
            if (string.IsNullOrEmpty(entry.Entity.ConcurrencyStamp))
            {
                logger.LogDebug("Setting ConcurrencyStamp for entity of type {EntityType},", entry.Entity.GetType().Name);
                entry.Entity.ConcurrencyStamp = Guid.NewGuid().ToString();
                activity?.SetTag("entityType", entry.Entity.GetType().Name);
                activity?.SetTag("concurrencyStamp", entry.Entity.ConcurrencyStamp);
            }
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
