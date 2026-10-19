using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Course.Client.Observability;

internal sealed class CourseClientActivitySource : ActivitySourceBase
{
    private static readonly Lazy<CourseClientActivitySource> SharedInstance = new(
        static () => new CourseClientActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static CourseClientActivitySource Shared => SharedInstance.Value;

    private CourseClientActivitySource()
        : base(CourseClientTelemetry.ActivitySourceName)
    {
    }
}