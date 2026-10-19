using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Platform.Resilience.Observability;

internal sealed class ResilienceActivitySource : ActivitySourceBase
{
    private static readonly Lazy<ResilienceActivitySource> SharedInstance = new(
        static () => new ResilienceActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static ResilienceActivitySource Shared => SharedInstance.Value;

    private ResilienceActivitySource()
        : base(ResilienceTelemetry.ActivitySourceName)
    {
    }
}
