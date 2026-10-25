using CourseLibrary.Platform.Observability;
using CourseLibrary.Platform.Observability.Traces;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CourseLibrary.Idp.Persistence.Observability;

public static class SqlTelemetry
{
    public const string ActivitySourceName = "CourseLibrary.Platform.Persistence.Sql";
    public const string MeterName = ActivitySourceName;
}

public static class SqlActivityTagConstants
{
    public const string Other = "other";
    public const string DbSystem = "db.system";
    public const string DbNamespace = "db.namespace";
    public const string DbOperationName = "db.operation.name";
    public const string DbContextType = "db.context.type";
    public const string DbTransactionId = "db.transaction.id";
    public const string DurationMilliseconds = "duration.ms";
    public const string EntityState = "entity.state";
    public const string EntityType = "entity.type";
    public const string ErrorType = "error.type";
    public const string OperationCancelled = "operation.cancelled";
    public const string OperationSuccess = "operation.success";

    public const string AuditOperation = "audit";
    public const string ConcurrencyStampOperation = "concurrency_stamp";
    public const string ConnectionOpenOperation = "db.connection.open";
    public const string ConnectionCloseOperation = "db.connection.close";
    public const string SaveChangesOperation = "db.save_changes";
    public const string TransactionStartOperation = "db.transaction.start";
    public const string TransactionCommitOperation = "db.transaction.commit";
    public const string TransactionRollbackOperation = "db.transaction.rollback";
    public const string TransactionFailureOperation = "db.transaction.failure";
    public const string SavepointCreateOperation = "db.savepoint.create";
    public const string SavepointRollbackOperation = "db.savepoint.rollback";
    public const string SavepointReleaseOperation = "db.savepoint.release";
    public const string ReaderOperation = "reader";
    public const string ScalarOperation = "scalar";
    public const string NonQueryOperation = "non_query";
}

public sealed class SqlActivitySource : ActivitySourceBase
{
    private static readonly Lazy<SqlActivitySource> SharedSource = new(
        static () => new SqlActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static SqlActivitySource Shared => SharedSource.Value;

    private SqlActivitySource() : base(SqlTelemetry.ActivitySourceName)
    {
    }
}

public static class SqlDatabaseSystem
{
    public static string Resolve(string? providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            return SqlActivityTagConstants.Other;

        if (providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase)
            || providerName.Contains("Postgre", StringComparison.OrdinalIgnoreCase))
            return "postgresql";

        if (providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            return "sqlite";

        if (providerName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
            return "mssql";

        return SqlActivityTagConstants.Other;
    }

    public static string Resolve(DbConnection? connection)
        => Resolve(connection?.GetType().FullName);
}

public sealed class SqlMeter : MeterBase
{
    private static readonly Lazy<SqlMeter> SharedInstance = new(
        static () => new SqlMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    private readonly Histogram<double> _commandDuration;
    private readonly Counter<long> _commandFailures;

    public static SqlMeter Shared => SharedInstance.Value;

    private SqlMeter() : base(SqlTelemetry.MeterName)
    {
        _commandDuration = CreateHistogram<double>(
            "db.client.operation.duration",
            "ms",
            "Duration of SQL commands.");
        _commandFailures = CreateCounter<long>(
            "db.client.operation.failures",
            description: "Number of failed or cancelled SQL commands.");
    }

    public void RecordCommand(string operation, double elapsedMilliseconds, string databaseSystem)
        => _commandDuration.Record(
            elapsedMilliseconds,
            new KeyValuePair<string, object?>("db.operation.name", operation),
            new KeyValuePair<string, object?>("db.system", databaseSystem));

    public void RecordFailure(string operation, string errorType, string databaseSystem)
        => _commandFailures.Add(
            1,
            new KeyValuePair<string, object?>("db.operation.name", operation),
            new KeyValuePair<string, object?>("error.type", errorType),
            new KeyValuePair<string, object?>("db.system", databaseSystem));
}

public static class SqlPersistenceLogs
{
    public static void ConnectionOpened(ILogger logger, string dataSource, string database)
        => logger.LogInformation("Opened database connection to {DataSource}/{Database}.", dataSource, database);

    public static void ConnectionClosed(ILogger logger, string dataSource, string database)
        => logger.LogInformation("Closed database connection to {DataSource}/{Database}.", dataSource, database);

    public static void TransactionStarted(ILogger logger, object? transactionId, string database)
        => logger.LogInformation("Started transaction {TransactionId} for {Database}.", transactionId, database);

    public static void TransactionCommitted(ILogger logger, object? transactionId, double durationMilliseconds)
        => logger.LogInformation("Committed transaction {TransactionId} in {DurationMilliseconds} ms.", transactionId, durationMilliseconds);

    public static void TransactionRolledBack(ILogger logger, object? transactionId, double durationMilliseconds)
        => logger.LogWarning("Rolled back transaction {TransactionId} after {DurationMilliseconds} ms.", transactionId, durationMilliseconds);

    public static void TransactionFailed(ILogger logger, Exception exception, object? transactionId)
        => logger.LogError(exception, "Transaction {TransactionId} failed.", transactionId);

    public static void SavepointCreated(ILogger logger, object? transactionId)
        => logger.LogDebug("Created savepoint for transaction {TransactionId}.", transactionId);

    public static void SavepointRolledBack(ILogger logger, object? transactionId)
        => logger.LogDebug("Rolled back to savepoint for transaction {TransactionId}.", transactionId);

    public static void SavepointReleased(ILogger logger, object? transactionId)
        => logger.LogDebug("Released savepoint for transaction {TransactionId}.", transactionId);

    public static void ConcurrencyStampSet(ILogger logger, string entityType)
        => logger.LogDebug("Set concurrency stamp for {EntityType}.", entityType);

    public static void AuditEntityAdded(ILogger logger, string entityType)
        => logger.LogDebug("Added auditable entity {EntityType}.", entityType);

    public static void AuditEntityUpdated(ILogger logger, string entityType)
        => logger.LogDebug("Updated auditable entity {EntityType}.", entityType);

    public static void AuditEntityDeleted(ILogger logger, string entityType)
        => logger.LogDebug("Deleted auditable entity {EntityType}.", entityType);

    public static void CommandCompleted(ILogger logger, string operation, double durationMilliseconds)
        => logger.LogDebug("Database command {Operation} completed in {DurationMilliseconds} ms.", operation, durationMilliseconds);

    public static void CommandSlow(ILogger logger, string operation, double durationMilliseconds)
        => logger.LogWarning("Database command {Operation} took {DurationMilliseconds} ms.", operation, durationMilliseconds);

    public static void CommandCancelled(ILogger logger, string operation, double durationMilliseconds)
        => logger.LogWarning("Database command {Operation} was cancelled after {DurationMilliseconds} ms.", operation, durationMilliseconds);

    public static void CommandFailed(ILogger logger, Exception exception, string operation, double durationMilliseconds)
        => logger.LogError(exception, "Database command {Operation} failed after {DurationMilliseconds} ms.", operation, durationMilliseconds);
}
