namespace CourseLibrary.Platform.Persistence.Sql.Observability;

public static class SqlActivityTagConstants
{
    public const string DbSystem = "db.system";
    public const string DbNamespace = "db.namespace";
    public const string DbOperationName = "db.operation.name";
    public const string DbTransactionId = "db.transaction.id";
    public const string DbConnectionId = "db.connection.id";
    public const string DurationMilliseconds = "db.operation.duration_ms";
    public const string OperationSuccess = "db.operation.success";
    public const string OperationCancelled = "db.operation.cancelled";
    public const string ErrorType = "error.type";
    public const string DbContextType = "db.context.type";
    public const string EntityType = "db.entity.type";
    public const string EntityState = "db.entity.state";

    public const string SqlServer = "sqlserver";
    public const string ReaderOperation = "reader";
    public const string ScalarOperation = "scalar";
    public const string NonQueryOperation = "non_query";
    public const string ConnectionOpenOperation = "connection.open";
    public const string ConnectionCloseOperation = "connection.close";
    public const string TransactionStartOperation = "transaction.start";
    public const string TransactionCommitOperation = "transaction.commit";
    public const string TransactionRollbackOperation = "transaction.rollback";
    public const string TransactionFailureOperation = "transaction.fail";
    public const string SavepointCreateOperation = "transaction.savepoint.create";
    public const string SavepointRollbackOperation = "transaction.savepoint.rollback";
    public const string SavepointReleaseOperation = "transaction.savepoint.release";
    public const string SaveChangesOperation = "save_changes";
    public const string AuditOperation = "audit";
    public const string ConcurrencyStampOperation = "concurrency_stamp";
}
