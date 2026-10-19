namespace CourseLibrary.AspNetCore.OutputCaching.Observability;

public static class OutputCacheActivityTagConstants
{
    public const string Ttl = "cache.ttl";
    public const string Operation = "cache.operation";
    public const string Hit = "cache.hit";
    public const string TagCount = "cache.tag.count";
    public const string OperationSuccess = "cache.operation.success";
    public const string OperationCancelled = "cache.operation.cancelled";
    public const string OperationError = "cache.operation.error";

    public const string GetOperation = "get";
    public const string SetOperation = "set";
    public const string EvictByTagOperation = "evict-by-tag";
}