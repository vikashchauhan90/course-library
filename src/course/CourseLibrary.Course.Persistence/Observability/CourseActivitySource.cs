using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Course.Persistence.Observability;

internal sealed class CourseActivitySource : ActivitySourceBase
{
    private static readonly Lazy<CourseActivitySource> SharedInstance = new(
        static () => new CourseActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    internal static CourseActivitySource Shared => SharedInstance.Value;

    private CourseActivitySource()
        : base(CoursePersistenceTelemetry.ActivitySourceName)
    {
    }
}
