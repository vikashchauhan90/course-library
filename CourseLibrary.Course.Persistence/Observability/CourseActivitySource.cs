using CourseLibrary.Platform.Observability.Traces;

namespace CourseLibrary.Course.Persistence.Observability;

public sealed class CourseActivitySource : ActivitySourceBase
{
    public const string ActivitySourceName = "CourseLibrary.Course.Persistence";

    private static readonly Lazy<CourseActivitySource> SharedInstance = new(
        static () => new CourseActivitySource(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    internal static CourseActivitySource Shared => SharedInstance.Value;

    private CourseActivitySource()
        : base(ActivitySourceName)
    {
    }
}
