namespace CourseLibrary.Course.Persistence.Observability;

internal static class CosmosActivityTagConstants
{
    public const string DbSystem = "db.system";
    public const string DbNamespace = "db.namespace";
    public const string DbOperationName = "db.operation.name";
    public const string ContainerName = "cosmos.container.name";
    public const string RequestCharge = "cosmos.request_charge";
    public const string ActivityId = "cosmos.activity_id";
    public const string ResultCount = "cosmos.result_count";
    public const string PatchOperationCount = "cosmos.patch_operation_count";
    public const string PageSize = "cosmos.page_size";
    public const string HasContinuationToken = "cosmos.has_continuation_token";
    public const string HasNextPage = "cosmos.has_next_page";
    public const string OperationSuccess = "db.operation.success";
    public const string OperationCancelled = "db.operation.cancelled";
    public const string ErrorType = "error.type";
}
