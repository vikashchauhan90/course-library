using CourseLibrary.Idp.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Idp.Infrastructure.Persistence.Interceptors;

public sealed class AuditEntityInterceptor(ILogger<AuditEntityInterceptor> logger) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        foreach (var entry in context.ChangeTracker.Entries<IEntityAudit>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    logger.LogDebug("Adding entity of type {EntityType},", entry.Entity.GetType().Name);
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Modified:
                    logger.LogDebug("Updating entity of type {EntityType},", entry.Entity.GetType().Name);
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Deleted:
                    logger.LogDebug("Deleting entity of type {EntityType},", entry.Entity.GetType().Name);
                    entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
                    break;
            }
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}