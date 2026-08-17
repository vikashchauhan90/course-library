using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace CourseLibrary.Idp.Infrastructure.Persistence.Interceptors;


public class TransactionLoggingInterceptor(ILogger<TransactionLoggingInterceptor> logger) : DbTransactionInterceptor
{

    public override DbTransaction TransactionStarted(DbConnection connection, TransactionEndEventData eventData, DbTransaction result)
    {
        logger.LogInformation(
            "Transaction started (Id: {TransactionId}) on connection {Connection}",
            eventData.TransactionId,
            connection.Database);
        return base.TransactionStarted(connection, eventData, result);
    }

    public override void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
    {
        logger.LogInformation(
            "Transaction committed (Id: {TransactionId}) Duration: {Duration} ms",
            eventData.TransactionId,
            eventData.Duration.TotalMilliseconds);
        base.TransactionCommitted(transaction, eventData);
    }

    public override void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData)
    {
        logger.LogWarning(
            "Transaction rolled back (Id: {TransactionId}) after {Duration} ms",
            eventData.TransactionId,
            eventData.Duration.TotalMilliseconds);
        base.TransactionRolledBack(transaction, eventData);
    }

    public override void TransactionFailed(DbTransaction transaction, TransactionErrorEventData eventData)
    {
        logger.LogError(
            eventData.Exception,
            "Transaction failed (Id: {TransactionId})",
            eventData.TransactionId);
        base.TransactionFailed(transaction, eventData);
    }

    public override void CreatedSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
        logger.LogDebug(
            "Savepoint created in transaction (Id: {TransactionId})",
            eventData.TransactionId);
        base.CreatedSavepoint(transaction, eventData);
    }

    public override void RolledBackToSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
        logger.LogWarning(
            "Rolled back to savepoint in transaction (Id: {TransactionId})",
            eventData.TransactionId);
        base.RolledBackToSavepoint(transaction, eventData);
    }

    public override void ReleasedSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
        logger.LogDebug(
            "Released savepoint in transaction (Id: {TransactionId})",
            eventData.TransactionId);
        base.ReleasedSavepoint(transaction, eventData);
    }
}