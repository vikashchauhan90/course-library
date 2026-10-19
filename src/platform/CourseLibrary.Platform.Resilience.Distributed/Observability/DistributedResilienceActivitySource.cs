using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Platform.Resilience.Distributed.Observability;

internal sealed class DistributedResilienceActivitySource : ActivitySourceBase
{
    private static readonly Lazy<DistributedResilienceActivitySource> SharedInstance = new(
        static () => new DistributedResilienceActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static DistributedResilienceActivitySource Shared => SharedInstance.Value;

    private DistributedResilienceActivitySource()
        : base(DistributedResilienceTelemetry.ActivitySourceName)
    {
    }
}
