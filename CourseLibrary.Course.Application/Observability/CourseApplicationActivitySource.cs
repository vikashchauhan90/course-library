using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Course.Application.Observability;

public sealed class CourseApplicationActivitySource : ActivitySourceBase
{
    private static readonly Lazy<CourseApplicationActivitySource> SharedInstance = new(
        static () => new CourseApplicationActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static CourseApplicationActivitySource Shared => SharedInstance.Value;

    private CourseApplicationActivitySource()
        : base(CourseApplicationActivitySourceConstants.ActivitySourceName)
    {
    }
}
