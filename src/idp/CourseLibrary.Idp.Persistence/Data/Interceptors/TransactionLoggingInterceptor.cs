using CourseLibrary.Idp.Persistence.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace CourseLibrary.Idp.Persistence.Data.Interceptors;


public sealed class TransactionLoggingInterceptor(ILogger<TransactionLoggingInterceptor> logger) : DbTransactionInterceptor
{

    public override DbTransaction TransactionStarted(DbConnection connection, TransactionEndEventData eventData, DbTransaction result)
    {
        using var activity = SqlActivitySource.Shared.StartActivity(
            SqlActivityTagConstants.TransactionStartOperation,
            System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag(
            SqlActivityTagConstants.DbSystem,
            SqlDatabaseSystem.Resolve(connection));
        activity?.SetTag(SqlActivityTagConstants.DbNamespace, connection.Database);
        activity?.SetTag(SqlActivityTagConstants.DbTransactionId, eventData.TransactionId);
        SqlPersistenceLogs.TransactionStarted(logger, eventData.TransactionId, connection.Database);
        return base.TransactionStarted(connection, eventData, result);
    }

    public override void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
    {
        using var activity = SqlActivitySource.Shared.StartActivity(
            SqlActivityTagConstants.TransactionCommitOperation,
            System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag(
            SqlActivityTagConstants.DbSystem,
            SqlDatabaseSystem.Resolve(transaction.Connection));
        activity?.SetTag(SqlActivityTagConstants.DbTransactionId, eventData.TransactionId);
        activity?.SetTag(SqlActivityTagConstants.DurationMilliseconds, eventData.Duration.TotalMilliseconds);
        activity?.SetTag(SqlActivityTagConstants.OperationSuccess, true);
        SqlPersistenceLogs.TransactionCommitted(
            logger,
            eventData.TransactionId,
            eventData.Duration.TotalMilliseconds);
        base.TransactionCommitted(transaction, eventData);
    }

    public override void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData)
    {
        using var activity = SqlActivitySource.Shared.StartActivity(
            SqlActivityTagConstants.TransactionRollbackOperation,
            System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag(
            SqlActivityTagConstants.DbSystem,
            SqlDatabaseSystem.Resolve(transaction.Connection));
        activity?.SetTag(SqlActivityTagConstants.DbTransactionId, eventData.TransactionId);
        activity?.SetTag(SqlActivityTagConstants.DurationMilliseconds, eventData.Duration.TotalMilliseconds);
        activity?.SetTag(SqlActivityTagConstants.OperationSuccess, false);
        SqlPersistenceLogs.TransactionRolledBack(
            logger,
            eventData.TransactionId,
            eventData.Duration.TotalMilliseconds);
        base.TransactionRolledBack(transaction, eventData);
    }

    public override void TransactionFailed(DbTransaction transaction, TransactionErrorEventData eventData)
    {
        using var activity = SqlActivitySource.Shared.StartActivity(
            SqlActivityTagConstants.TransactionFailureOperation,
            System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag(
            SqlActivityTagConstants.DbSystem,
            SqlDatabaseSystem.Resolve(transaction.Connection));
        activity?.SetTag(SqlActivityTagConstants.DbTransactionId, eventData.TransactionId);
        activity?.SetTag(SqlActivityTagConstants.ErrorType, eventData.Exception.GetType().Name);
        activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, eventData.Exception.Message);
        SqlPersistenceLogs.TransactionFailed(
            logger,
            eventData.Exception,
            eventData.TransactionId);
        base.TransactionFailed(transaction, eventData);
    }

    public override void CreatedSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
        using var activity = SqlActivitySource.Shared.StartActivity(
            SqlActivityTagConstants.SavepointCreateOperation,
            System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag(
            SqlActivityTagConstants.DbSystem,
            SqlDatabaseSystem.Resolve(transaction.Connection));
        activity?.SetTag(SqlActivityTagConstants.DbTransactionId, eventData.TransactionId);
        SqlPersistenceLogs.SavepointCreated(logger, eventData.TransactionId);
        base.CreatedSavepoint(transaction, eventData);
    }

    public override void RolledBackToSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
        using var activity = SqlActivitySource.Shared.StartActivity(
            SqlActivityTagConstants.SavepointRollbackOperation,
            System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag(
            SqlActivityTagConstants.DbSystem,
            SqlDatabaseSystem.Resolve(transaction.Connection));
        activity?.SetTag(SqlActivityTagConstants.DbTransactionId, eventData.TransactionId);
        SqlPersistenceLogs.SavepointRolledBack(logger, eventData.TransactionId);
        base.RolledBackToSavepoint(transaction, eventData);
    }

    public override void ReleasedSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
        using var activity = SqlActivitySource.Shared.StartActivity(
            SqlActivityTagConstants.SavepointReleaseOperation,
            System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag(
            SqlActivityTagConstants.DbSystem,
            SqlDatabaseSystem.Resolve(transaction.Connection));
        activity?.SetTag(SqlActivityTagConstants.DbTransactionId, eventData.TransactionId);
        SqlPersistenceLogs.SavepointReleased(logger, eventData.TransactionId);
        base.ReleasedSavepoint(transaction, eventData);
    }
}