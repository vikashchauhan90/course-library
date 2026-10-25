using CourseLibrary.Core.Domain.Abstractions.Security;
using CourseLibrary.Platform.Persistence.Sql.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Idp.Persistence.Data.Interceptors;

public sealed class SecurityEntityInterceptor(ILogger<SecurityEntityInterceptor> logger) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        using var activity = SqlActivitySource.Shared.StartActivity(
            SqlActivityTagConstants.SaveChangesOperation,
            System.Diagnostics.ActivityKind.Internal);

        activity?.SetTag(SqlActivityTagConstants.DbOperationName, SqlActivityTagConstants.ConcurrencyStampOperation);
        activity?.SetTag(SqlActivityTagConstants.DbContextType, eventData.Context?.GetType().Name);
        var context = eventData.Context;
        activity?.SetTag(
            SqlActivityTagConstants.DbSystem,
            SqlDatabaseSystem.Resolve(context?.Database.ProviderName));
        if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        foreach (var entry in context.ChangeTracker.Entries<IConcurrency>())
        {
            if (string.IsNullOrEmpty(entry.Entity.ConcurrencyStamp))
            {
                SqlPersistenceLogs.ConcurrencyStampSet(logger, entry.Entity.GetType().Name);
                entry.Entity.ConcurrencyStamp = Guid.NewGuid().ToString();
                activity?.SetTag(SqlActivityTagConstants.EntityType, entry.Entity.GetType().Name);
            }
        }
        activity?.SetTag(SqlActivityTagConstants.OperationSuccess, true);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
