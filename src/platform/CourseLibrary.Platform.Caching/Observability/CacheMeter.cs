using CourseLibrary.Platform.Observability;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CourseLibrary.Platform.Caching.Observability;

public sealed class CacheMeter : MeterBase
{
    private static readonly Lazy<CacheMeter> SharedInstance = new(
        static () => new CacheMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static CacheMeter Shared => SharedInstance.Value;

    private readonly Counter<long> _lookups;
    private readonly Counter<long> _hits;
    private readonly Counter<long> _misses;

    private CacheMeter()
        : base(CacheTelemetry.MeterName)
    {
        _lookups = CreateCounter<long>(
            CacheMetricConstants.LookupsInstrumentName,
            "{lookup}",
            "Number of cache read lookups.");
        _hits = CreateCounter<long>(
            CacheMetricConstants.HitsInstrumentName,
            "{hit}",
            "Number of cache reads that returned a cached value.");
        _misses = CreateCounter<long>(
            CacheMetricConstants.MissesInstrumentName,
            "{miss}",
            "Number of cache reads that did not return a cached value.");
    }

    public void RecordLookup(string provider, string operation)
        => _lookups.Add(1, CreateTags(provider, operation));

    public void RecordHit(string provider, string operation)
        => _hits.Add(1, CreateTags(provider, operation));

    public void RecordMiss(string provider, string operation)
        => _misses.Add(1, CreateTags(provider, operation));

    private static TagList CreateTags(string provider, string operation)
    {
        var tags = new TagList
        {
            { CacheActivityTagConstants.ProviderTag, provider },
            { CacheActivityTagConstants.OperationTag, operation }
        };

        return tags;
    }
}