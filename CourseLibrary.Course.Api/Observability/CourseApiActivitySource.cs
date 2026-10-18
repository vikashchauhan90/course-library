using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Course.Api.Observability;

internal sealed class CourseApiActivitySource : ActivitySourceBase
{
    private static readonly Lazy<CourseApiActivitySource> SharedInstance = new(
        static () => new CourseApiActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static CourseApiActivitySource Shared => SharedInstance.Value;

    private CourseApiActivitySource()
        : base(CourseApiTelemetry.ActivitySourceName)
    {
    }
}
