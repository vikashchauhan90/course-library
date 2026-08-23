using CourseLibrary.Idp.Domain.Abstractions;
using CourseLibrary.Idp.Infrastructure.Observability.Traces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Idp.Infrastructure.Persistence.Interceptors;

public sealed class AuditEntityInterceptor(ILogger<AuditEntityInterceptor> logger) : SaveChangesInterceptor, IInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Infrastructure.StartActivity(
            "AuditEntityInterceptor.SavingChangesAsync",
             System.Diagnostics.ActivityKind.Internal);

        activity?.SetTag("DbContext", eventData.Context?.GetType().Name);
        activity?.SetTag("InterceptionResult", result.ToString());
        var context = eventData.Context;
        if (context == null)
        {
            activity?.SetTag("Error", true);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        foreach (var entry in context.ChangeTracker.Entries<IEntityAudit>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    activity?.SetTag("EntityState", "Added");
                    logger.LogDebug("Adding entity of type {EntityType},", entry.Entity.GetType().Name);
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Modified:
                    activity?.SetTag("EntityState", "Modified");
                    logger.LogDebug("Updating entity of type {EntityType},", entry.Entity.GetType().Name);
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Deleted:
                    activity?.SetTag("EntityState", "Deleted");
                    logger.LogDebug("Deleting entity of type {EntityType},", entry.Entity.GetType().Name);
                    entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
                    break;
            }
        }
        activity?.SetTag("AuditEntityInterceptor", "Completed");
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}