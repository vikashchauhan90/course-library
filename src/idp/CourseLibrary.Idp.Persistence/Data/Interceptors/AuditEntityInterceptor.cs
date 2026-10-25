using CourseLibrary.Idp.Domain.Abstractions;
using CourseLibrary.Idp.Persistence.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Idp.Persistence.Data.Interceptors;

public sealed class AuditEntityInterceptor(ILogger<AuditEntityInterceptor> logger) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        using var activity = SqlActivitySource.Shared.StartActivity(
            SqlActivityTagConstants.SaveChangesOperation,
             System.Diagnostics.ActivityKind.Internal);

        activity?.SetTag(SqlActivityTagConstants.DbOperationName, SqlActivityTagConstants.AuditOperation);
        activity?.SetTag(SqlActivityTagConstants.DbContextType, eventData.Context?.GetType().Name);
        var context = eventData.Context;
        activity?.SetTag(
            SqlActivityTagConstants.DbSystem,
            SqlDatabaseSystem.Resolve(context?.Database.ProviderName));
        if (context == null)
        {
            activity?.SetTag(SqlActivityTagConstants.OperationSuccess, false);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    activity?.SetTag(SqlActivityTagConstants.EntityState, "added");
                    activity?.SetTag(SqlActivityTagConstants.EntityType, entry.Entity.GetType().Name);
                    SqlPersistenceLogs.AuditEntityAdded(logger, entry.Entity.GetType().Name);
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Modified:
                    activity?.SetTag(SqlActivityTagConstants.EntityState, "modified");
                    activity?.SetTag(SqlActivityTagConstants.EntityType, entry.Entity.GetType().Name);
                    SqlPersistenceLogs.AuditEntityUpdated(logger, entry.Entity.GetType().Name);
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Deleted:
                    activity?.SetTag(SqlActivityTagConstants.EntityState, "deleted");
                    activity?.SetTag(SqlActivityTagConstants.EntityType, entry.Entity.GetType().Name);
                    SqlPersistenceLogs.AuditEntityDeleted(logger, entry.Entity.GetType().Name);
                    entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
                    break;
            }
        }
        activity?.SetTag(SqlActivityTagConstants.OperationSuccess, true);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}