using System;
using System.Collections.Generic;
using System.Text;

namespace CourseLibrary.Platform.Caching.Observability;

public static class CacheMetricConstants
{
    public const string MeterName = "CourseLibrary.Platform.Caching";

    public const string LookupsInstrumentName = "course_library.cache.lookups";
    public const string HitsInstrumentName = "course_library.cache.hits";
    public const string MissesInstrumentName = "course_library.cache.misses";

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