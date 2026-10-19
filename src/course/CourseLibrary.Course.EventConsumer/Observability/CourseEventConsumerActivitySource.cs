using System.Diagnostics;
using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Course.EventConsumer.Observability;

internal sealed class CourseEventConsumerActivitySource : ActivitySourceBase
{
    private static readonly Lazy<CourseEventConsumerActivitySource> SharedInstance = new(
        static () => new CourseEventConsumerActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static CourseEventConsumerActivitySource Shared => SharedInstance.Value;

    private CourseEventConsumerActivitySource()
        : base(CourseEventConsumerTelemetry.ActivitySourceName)
    {
    }

    public Activity? StartActivity(
        string name,
        ActivityKind kind,
        ActivityContext? parentContext)
        => parentContext is { } context
            ? ActivitySource.StartActivity(name, kind, context)
            : StartActivity(name, kind);
}
