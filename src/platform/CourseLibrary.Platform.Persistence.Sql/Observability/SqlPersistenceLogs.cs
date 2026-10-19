using Microsoft.Extensions.Logging;

namespace CourseLibrary.Platform.Persistence.Sql.Observability;

internal static partial class SqlPersistenceLogs
{
    [LoggerMessage(6001, LogLevel.Debug, "SQL connection opened for {DataSource}/{Database}.", EventName = nameof(ConnectionOpened))]
    public static partial void ConnectionOpened(this ILogger logger, string dataSource, string database);

    [LoggerMessage(6002, LogLevel.Debug, "SQL connection closed for {DataSource}/{Database}.", EventName = nameof(ConnectionClosed))]
    public static partial void ConnectionClosed(this ILogger logger, string dataSource, string database);

    [LoggerMessage(6003, LogLevel.Error, "EF SQL {Operation} failed after {ElapsedMilliseconds} ms.", EventName = nameof(CommandFailed))]
    public static partial void CommandFailed(
        this ILogger logger,
        Exception exception,
        string operation,
        double elapsedMilliseconds);

    [LoggerMessage(6004, LogLevel.Warning, "EF SQL {Operation} was cancelled after {ElapsedMilliseconds} ms.", EventName = nameof(CommandCancelled))]
    public static partial void CommandCancelled(this ILogger logger, string operation, double elapsedMilliseconds);

    [LoggerMessage(6005, LogLevel.Warning, "EF SQL {Operation} took {ElapsedMilliseconds} ms (slow).", EventName = nameof(CommandSlow))]
    public static partial void CommandSlow(this ILogger logger, string operation, double elapsedMilliseconds);

    [LoggerMessage(6006, LogLevel.Information, "EF SQL {Operation} completed in {ElapsedMilliseconds} ms.", EventName = nameof(CommandCompleted))]
    public static partial void CommandCompleted(this ILogger logger, string operation, double elapsedMilliseconds);

    [LoggerMessage(6007, LogLevel.Information, "SQL transaction {TransactionId} started on {Database}.", EventName = nameof(TransactionStarted))]
    public static partial void TransactionStarted(this ILogger logger, Guid transactionId, string database);

    [LoggerMessage(6008, LogLevel.Information, "SQL transaction {TransactionId} committed in {ElapsedMilliseconds} ms.", EventName = nameof(TransactionCommitted))]
    public static partial void TransactionCommitted(this ILogger logger, Guid transactionId, double elapsedMilliseconds);

    [LoggerMessage(6009, LogLevel.Warning, "SQL transaction {TransactionId} rolled back after {ElapsedMilliseconds} ms.", EventName = nameof(TransactionRolledBack))]
    public static partial void TransactionRolledBack(this ILogger logger, Guid transactionId, double elapsedMilliseconds);

    [LoggerMessage(6010, LogLevel.Error, "SQL transaction {TransactionId} failed.", EventName = nameof(TransactionFailed))]
    public static partial void TransactionFailed(this ILogger logger, Exception exception, Guid transactionId);

    [LoggerMessage(6011, LogLevel.Debug, "SQL savepoint created in transaction {TransactionId}.", EventName = nameof(SavepointCreated))]
    public static partial void SavepointCreated(this ILogger logger, Guid transactionId);

    [LoggerMessage(6012, LogLevel.Warning, "SQL transaction {TransactionId} rolled back to a savepoint.", EventName = nameof(SavepointRolledBack))]
    public static partial void SavepointRolledBack(this ILogger logger, Guid transactionId);

    [LoggerMessage(6013, LogLevel.Debug, "SQL savepoint released in transaction {TransactionId}.", EventName = nameof(SavepointReleased))]
    public static partial void SavepointReleased(this ILogger logger, Guid transactionId);

    [LoggerMessage(6014, LogLevel.Debug, "Adding auditable entity of type {EntityType}.", EventName = nameof(AuditEntityAdded))]
    public static partial void AuditEntityAdded(this ILogger logger, string entityType);

    [LoggerMessage(6015, LogLevel.Debug, "Updating auditable entity of type {EntityType}.", EventName = nameof(AuditEntityUpdated))]
    public static partial void AuditEntityUpdated(this ILogger logger, string entityType);

    [LoggerMessage(6016, LogLevel.Debug, "Deleting auditable entity of type {EntityType}.", EventName = nameof(AuditEntityDeleted))]
    public static partial void AuditEntityDeleted(this ILogger logger, string entityType);

    [LoggerMessage(6017, LogLevel.Debug, "Set concurrency stamp for entity of type {EntityType}.", EventName = nameof(ConcurrencyStampSet))]
    public static partial void ConcurrencyStampSet(this ILogger logger, string entityType);
}
