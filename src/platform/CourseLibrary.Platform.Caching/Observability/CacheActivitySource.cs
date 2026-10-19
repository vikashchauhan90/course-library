using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Platform.Caching.Observability;

internal sealed class CacheActivitySource : ActivitySourceBase
{
    private static readonly Lazy<CacheActivitySource> SharedInstance = new(
        static () => new CacheActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static CacheActivitySource Shared => SharedInstance.Value;

    private CacheActivitySource()
        : base(CacheTelemetry.ActivitySourceName)
    {
    }
}
