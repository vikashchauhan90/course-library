using CourseLibrary.Idp.Infrastructure.Observability.Traces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace CourseLibrary.Idp.Infrastructure.Persistence.Interceptors;


public class TransactionLoggingInterceptor(ILogger<TransactionLoggingInterceptor> logger) : DbTransactionInterceptor, IInterceptor
{

    public override DbTransaction TransactionStarted(DbConnection connection, TransactionEndEventData eventData, DbTransaction result)
    {
        using var activity = ActivitySources.Infrastructure.StartActivity("TransactionStarted", System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag("TransactionId", eventData.TransactionId);
        activity?.SetTag("Connection", connection.Database);
        logger.LogInformation(
            "Transaction started (Id: {TransactionId}) on connection {Connection}",
            eventData.TransactionId,
            connection.Database);
        return base.TransactionStarted(connection, eventData, result);
    }

    public override void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
    {
        using var activity = ActivitySources.Infrastructure.StartActivity("TransactionCommitted", System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag("TransactionId", eventData.TransactionId);
        activity?.SetTag("Duration", eventData.Duration.TotalMilliseconds);
        logger.LogInformation(
            "Transaction committed (Id: {TransactionId}) Duration: {Duration} ms",
            eventData.TransactionId,
            eventData.Duration.TotalMilliseconds);
        base.TransactionCommitted(transaction, eventData);
    }

    public override void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData)
    {
        using var activity = ActivitySources.Infrastructure.StartActivity("TransactionRolledBack", System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag("TransactionId", eventData.TransactionId);
        activity?.SetTag("Duration", eventData.Duration.TotalMilliseconds);
        logger.LogWarning(
            "Transaction rolled back (Id: {TransactionId}) after {Duration} ms",
            eventData.TransactionId,
            eventData.Duration.TotalMilliseconds);
        base.TransactionRolledBack(transaction, eventData);
    }

    public override void TransactionFailed(DbTransaction transaction, TransactionErrorEventData eventData)
    {
        using var activity = ActivitySources.Infrastructure.StartActivity("TransactionFailed", System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag("TransactionId", eventData.TransactionId);
        activity?.SetTag("Exception", eventData.Exception.ToString());
        logger.LogError(
            eventData.Exception,
            "Transaction failed (Id: {TransactionId})",
            eventData.TransactionId);
        base.TransactionFailed(transaction, eventData);
    }

    public override void CreatedSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
        using var activity = ActivitySources.Infrastructure.StartActivity("CreatedSavepoint", System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag("TransactionId", eventData.TransactionId);
        logger.LogDebug(
            "Savepoint created in transaction (Id: {TransactionId})",
            eventData.TransactionId);
        base.CreatedSavepoint(transaction, eventData);
    }

    public override void RolledBackToSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
        using var activity = ActivitySources.Infrastructure.StartActivity("RolledBackToSavepoint", System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag("TransactionId", eventData.TransactionId);
        logger.LogWarning(
            "Rolled back to savepoint in transaction (Id: {TransactionId})",
            eventData.TransactionId);
        base.RolledBackToSavepoint(transaction, eventData);
    }

    public override void ReleasedSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
        using var activity = ActivitySources.Infrastructure.StartActivity("ReleasedSavepoint", System.Diagnostics.ActivityKind.Internal);
        activity?.SetTag("TransactionId", eventData.TransactionId);
        logger.LogDebug(
            "Released savepoint in transaction (Id: {TransactionId})",
            eventData.TransactionId);
        base.ReleasedSavepoint(transaction, eventData);
    }
}