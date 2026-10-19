namespace CourseLibrary.Platform.Caching.Observability;

public static class CacheActivityTagConstants
{
    public const string Key = "cache.key";
    public const string Tag = "cache.tag";
    public const string Ttl = "cache.ttl";
    public const string Operation = "cache.operation";
    public const string Hit = "cache.hit";
    public const string Added = "cache.added";
    public const string OperationSuccess = "cache.operation.success";
    public const string OperationCancelled = "cache.operation.cancelled";
    public const string OperationError = "cache.operation.error";

    public const string ProviderTag = "cache.provider";
    public const string OperationTag = "cache.operation";
    public const string FusionCacheProvider = "fusion";
    public const string MemoryCacheProvider = "memory";
    public const string RedisCacheProvider = "redis";
    public const string HybridCacheProvider = "hybrid";

    public const string GetOperation = "get";
    public const string GetOrCreateOperation = "get-or-create";
    public const string SetOperation = "set";
    public const string RemoveOperation = "remove";
    public const string RemoveByTagOperation = "remove-by-tag";
    public const string TryAddOperation = "try-add";
}