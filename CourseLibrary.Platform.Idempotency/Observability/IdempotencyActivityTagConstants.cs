namespace CourseLibrary.Platform.Idempotency.Observability;

internal static class IdempotencyActivityTagConstants
{
    public const string Operation = "idempotency.operation";
    public const string Outcome = "idempotency.outcome";
    public const string Ttl = "idempotency.ttl";
    public const string Found = "idempotency.found";
    public const string Acquired = "idempotency.acquired";
    public const string Status = "idempotency.status";
    public const string Cancelled = "idempotency.cancelled";
    public const string Error = "idempotency.error";

    public const string GetOperation = "get";
    public const string AcquireOperation = "try-acquire";
    public const string StoreOperation = "store";
    public const string RemoveOperation = "remove";
}