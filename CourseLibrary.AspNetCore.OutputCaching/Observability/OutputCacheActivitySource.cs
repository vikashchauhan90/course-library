using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.AspNetCore.OutputCaching.Observability;

internal sealed class OutputCacheActivitySource : ActivitySourceBase
{
    private static readonly Lazy<OutputCacheActivitySource> SharedInstance = new(
        static () => new OutputCacheActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static OutputCacheActivitySource Shared => SharedInstance.Value;

    private OutputCacheActivitySource()
        : base(OutputCacheTelemetry.ActivitySourceName)
    {
    }
}
